using Dreamine.PLC.Abstractions.Devices;
using Dreamine.PLC.Omron.Fins.Devices;
using Dreamine.PLC.Omron.Fins.Options;
using Dreamine.PLC.Omron.Fins.Protocol;

namespace Dreamine.PLC.Omron.Fins.Tests;

public sealed class ProtocolTests
{
    [Fact]
    public void Endian_RoundTripsSignedAndUnsignedValues()
    {
        Span<byte> buffer = stackalloc byte[8];

        OmronFinsEndian.WriteUInt16(buffer, 0, 0xABCD);
        OmronFinsEndian.WriteInt16(buffer, 2, -1234);
        OmronFinsEndian.WriteInt32(buffer, 4, unchecked((int)0x89ABCDEF));

        Assert.Equal((ushort)0xABCD, OmronFinsEndian.ReadUInt16(buffer, 0));
        Assert.Equal((short)-1234, OmronFinsEndian.ReadInt16(buffer, 2));
        Assert.Equal(unchecked((int)0x89ABCDEF), OmronFinsEndian.ReadInt32(buffer, 4));
    }

    [Theory]
    [InlineData(PlcDeviceType.D, false, OmronFinsMemoryAreaCode.DmWord)]
    [InlineData(PlcDeviceType.D, true, OmronFinsMemoryAreaCode.DmBit)]
    [InlineData(PlcDeviceType.W, false, OmronFinsMemoryAreaCode.WorkWord)]
    [InlineData(PlcDeviceType.W, true, OmronFinsMemoryAreaCode.WorkBit)]
    [InlineData(PlcDeviceType.R, false, OmronFinsMemoryAreaCode.HoldingWord)]
    [InlineData(PlcDeviceType.R, true, OmronFinsMemoryAreaCode.HoldingBit)]
    [InlineData(PlcDeviceType.M, false, OmronFinsMemoryAreaCode.CioWord)]
    [InlineData(PlcDeviceType.X, true, OmronFinsMemoryAreaCode.CioBit)]
    [InlineData(PlcDeviceType.Y, false, OmronFinsMemoryAreaCode.CioWord)]
    public void MemoryAreaMapper_MapsSupportedDevices(PlcDeviceType type, bool bitAccess, byte expected)
    {
        var actual = OmronFinsMemoryAreaMapper.Map(new PlcAddress(type, 10), bitAccess);

        Assert.Equal(expected, actual);
        Assert.Equal(bitAccess, OmronFinsMemoryAreaMapper.IsBitArea(actual));
        Assert.NotEqual(PlcDeviceType.Unknown, OmronFinsMemoryAreaMapper.ToDeviceType(actual));
    }

    [Fact]
    public void MemoryAreaMapper_RejectsUnsupportedDevice()
    {
        Assert.Throws<NotSupportedException>(
            () => OmronFinsMemoryAreaMapper.Map(new PlcAddress(PlcDeviceType.Unknown, 0), false));
        Assert.Equal(PlcDeviceType.Unknown, OmronFinsMemoryAreaMapper.ToDeviceType(0xFF));
        Assert.False(OmronFinsMemoryAreaMapper.IsBitArea(0xFF));
    }

    [Fact]
    public void FrameBuilder_BuildsReadAndWriteFrames()
    {
        var builder = new OmronFinsFrameBuilder();
        var options = CreateOptions();

        var read = builder.BuildMemoryAreaRead(options, new PlcAddress(PlcDeviceType.D, 100), 3, false);
        var words = builder.BuildMemoryAreaWriteWords(
            options,
            new PlcAddress(PlcDeviceType.D, 200),
            new short[] { -1, 0x1234 });
        var bits = builder.BuildMemoryAreaWriteBits(
            options,
            new PlcAddress(PlcDeviceType.W, 12, 3),
            new[] { true, false, true });

        Assert.Equal(18, read.Length);
        Assert.Equal(new byte[] { 1, 2, 3, 4, 5, 6 }, read[3..9]);
        Assert.Equal((byte)1, read[9]);
        Assert.Equal((ushort)OmronFinsCommand.MemoryAreaRead, OmronFinsEndian.ReadUInt16(read, 10));
        Assert.Equal(OmronFinsMemoryAreaCode.DmWord, read[12]);
        Assert.Equal((ushort)100, OmronFinsEndian.ReadUInt16(read, 13));
        Assert.Equal((ushort)3, OmronFinsEndian.ReadUInt16(read, 16));

        Assert.Equal((byte)2, words[9]);
        Assert.Equal((ushort)OmronFinsCommand.MemoryAreaWrite, OmronFinsEndian.ReadUInt16(words, 10));
        Assert.Equal(new byte[] { 0xFF, 0xFF, 0x12, 0x34 }, words[18..]);

        Assert.Equal((byte)3, bits[9]);
        Assert.Equal(OmronFinsMemoryAreaCode.WorkBit, bits[12]);
        Assert.Equal((byte)3, bits[15]);
        Assert.Equal(new byte[] { 1, 0, 1 }, bits[18..]);
    }

    [Theory]
    [InlineData(1, 0)]
    [InlineData(1, 65536)]
    [InlineData(-1, 1)]
    [InlineData(65536, 1)]
    public void FrameBuilder_RejectsInvalidRanges(int offset, int count)
    {
        var builder = new OmronFinsFrameBuilder();

        Assert.Throws<ArgumentOutOfRangeException>(
            () => builder.BuildMemoryAreaRead(
                new OmronFinsConnectionOptions(),
                new PlcAddress(PlcDeviceType.D, offset),
                count,
                false));
    }

    [Fact]
    public void ResponseParser_HandlesSuccessAndFailures()
    {
        var parser = new OmronFinsResponseParser();
        var response = BuildResponse(0, 0x12, 0x34, 1, 0, 1);

        var payload = parser.ExtractPayload(response);
        var words = parser.ParseWords(payload.Value!, 1);
        var bits = parser.ParseBits(payload.Value!, 3);

        Assert.True(payload.IsSuccess);
        Assert.Equal((short)0x1234, words.Value![0]);
        Assert.Equal(new[] { true, true, true }, bits.Value);
        Assert.False(parser.ExtractPayload(new byte[3]).IsSuccess);
        Assert.False(parser.ExtractPayload(BuildResponse(0x2101)).IsSuccess);
        Assert.False(parser.ParseWords(new byte[1], 1).IsSuccess);
        Assert.False(parser.ParseBits(new byte[1], 2).IsSuccess);
    }

    [Fact]
    public void TcpPacket_WrapsAndExtractsFrames()
    {
        byte[] frame = [0x80, 0, 2, 1, 2, 3];

        var packet = OmronFinsTcpPacket.Wrap(frame);

        Assert.Equal("FINS", System.Text.Encoding.ASCII.GetString(packet, 0, 4));
        Assert.Equal(frame.Length + 8, OmronFinsEndian.ReadInt32(packet, 4));
        Assert.Equal(frame, OmronFinsTcpPacket.Extract(packet));
        Assert.Throws<InvalidOperationException>(() => OmronFinsTcpPacket.Extract(new byte[4]));

        packet[0] = 0;
        Assert.Throws<InvalidOperationException>(() => OmronFinsTcpPacket.Extract(packet));

        packet[0] = (byte)'F';
        OmronFinsEndian.WriteInt32(packet, 12, 5);
        Assert.Throws<InvalidOperationException>(() => OmronFinsTcpPacket.Extract(packet));
    }

    internal static OmronFinsConnectionOptions CreateOptions() => new()
    {
        DestinationNetwork = 1,
        DestinationNode = 2,
        DestinationUnit = 3,
        SourceNetwork = 4,
        SourceNode = 5,
        SourceUnit = 6
    };

    internal static byte[] BuildResponse(ushort endCode, params byte[] payload)
    {
        var response = new byte[14 + payload.Length];
        response[0] = 0xC0;
        response[2] = 0x02;
        OmronFinsEndian.WriteUInt16(response, 10, (ushort)OmronFinsCommand.MemoryAreaRead);
        OmronFinsEndian.WriteUInt16(response, 12, endCode);
        payload.CopyTo(response, 14);
        return response;
    }
}
