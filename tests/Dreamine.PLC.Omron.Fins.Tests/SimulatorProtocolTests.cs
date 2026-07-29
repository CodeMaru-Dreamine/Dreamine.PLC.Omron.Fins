using Dreamine.PLC.Abstractions.Devices;
using Dreamine.PLC.Core.Memory;
using Dreamine.PLC.Omron.Fins.Options;
using Dreamine.PLC.Omron.Fins.Protocol;
using Dreamine.PLC.Omron.Fins.Simulation;

namespace Dreamine.PLC.Omron.Fins.Tests;

public sealed class SimulatorProtocolTests
{
    [Fact]
    public void Constructor_RejectsNullDependencies()
    {
        Assert.Throws<ArgumentNullException>(
            () => new OmronFinsSimulatorProtocol(null!, new OmronFinsSimulatorServerOptions()));
        Assert.Throws<ArgumentNullException>(
            () => new OmronFinsSimulatorProtocol(new InMemoryPlcMemory(), null!));
    }

    [Fact]
    public void HandleRequest_WritesAndReadsWords()
    {
        var memory = new InMemoryPlcMemory();
        var protocol = new OmronFinsSimulatorProtocol(memory, new OmronFinsSimulatorServerOptions());
        var builder = new OmronFinsFrameBuilder();
        var address = new PlcAddress(PlcDeviceType.D, 100);

        var writeResponse = protocol.HandleRequest(
            builder.BuildMemoryAreaWriteWords(new OmronFinsConnectionOptions(), address, [12, -5]));
        var readResponse = protocol.HandleRequest(
            builder.BuildMemoryAreaRead(new OmronFinsConnectionOptions(), address, 2, false));

        Assert.Equal((ushort)0, OmronFinsEndian.ReadUInt16(writeResponse, 12));
        Assert.Equal((ushort)0, OmronFinsEndian.ReadUInt16(readResponse, 12));
        Assert.Equal((short)12, OmronFinsEndian.ReadInt16(readResponse, 14));
        Assert.Equal((short)-5, OmronFinsEndian.ReadInt16(readResponse, 16));
    }

    [Fact]
    public void HandleRequest_WritesAndReadsBits()
    {
        var protocol = new OmronFinsSimulatorProtocol(
            new InMemoryPlcMemory(),
            new OmronFinsSimulatorServerOptions());
        var builder = new OmronFinsFrameBuilder();
        var address = new PlcAddress(PlcDeviceType.W, 5, 2);

        var writeResponse = protocol.HandleRequest(
            builder.BuildMemoryAreaWriteBits(new OmronFinsConnectionOptions(), address, [true, false, true]));
        var readResponse = protocol.HandleRequest(
            builder.BuildMemoryAreaRead(new OmronFinsConnectionOptions(), address, 3, true));

        Assert.Equal((ushort)0, OmronFinsEndian.ReadUInt16(writeResponse, 12));
        Assert.Equal(new byte[] { 1, 0, 1 }, readResponse[14..]);
    }

    [Fact]
    public void HandleRequest_AppliesConfiguredAutoResponse()
    {
        var memory = new InMemoryPlcMemory();
        var options = new OmronFinsSimulatorServerOptions
        {
            EnableAutoWordResponse = true,
            AutoResponseTriggerOffset = 10,
            AutoResponseOffset = 11,
            AutoResponseIncrement = 2
        };
        var messages = new List<string>();
        var protocol = new OmronFinsSimulatorProtocol(memory, options);
        protocol.StatusChanged += (_, message) => messages.Add(message);
        var builder = new OmronFinsFrameBuilder();

        protocol.HandleRequest(builder.BuildMemoryAreaWriteWords(
            new OmronFinsConnectionOptions(),
            new PlcAddress(PlcDeviceType.D, 10),
            [40]));

        var result = memory.ReadWords(new PlcAddress(PlcDeviceType.D, 11), 1);
        Assert.Equal((short)42, result.Value![0]);
        Assert.Contains(messages, message => message.Contains("D11=42", StringComparison.Ordinal));
    }

    [Fact]
    public void HandleRequest_ReportsInvalidAndUnsupportedFrames()
    {
        var messages = new List<string>();
        var protocol = new OmronFinsSimulatorProtocol(
            new InMemoryPlcMemory(),
            new OmronFinsSimulatorServerOptions());
        protocol.StatusChanged += (_, message) => messages.Add(message);

        var shortResponse = protocol.HandleRequest([1, 2, 3]);
        var unsupportedArea = new byte[18];
        OmronFinsEndian.WriteUInt16(unsupportedArea, 10, (ushort)OmronFinsCommand.MemoryAreaRead);
        unsupportedArea[12] = 0xFF;
        var areaResponse = protocol.HandleRequest(unsupportedArea);
        var unsupportedCommand = new OmronFinsFrameBuilder().BuildMemoryAreaRead(
            new OmronFinsConnectionOptions(),
            new PlcAddress(PlcDeviceType.D, 0),
            1,
            false);
        OmronFinsEndian.WriteUInt16(unsupportedCommand, 10, 0x9999);
        var commandResponse = protocol.HandleRequest(unsupportedCommand);

        Assert.Equal((ushort)0x1101, OmronFinsEndian.ReadUInt16(shortResponse, 12));
        Assert.Equal((ushort)0x1101, OmronFinsEndian.ReadUInt16(areaResponse, 12));
        Assert.Equal((ushort)0x0101, OmronFinsEndian.ReadUInt16(commandResponse, 12));
        Assert.Equal(2, messages.Count);
    }

    [Fact]
    public void HandleRequest_RejectsTruncatedWritePayload()
    {
        var protocol = new OmronFinsSimulatorProtocol(
            new InMemoryPlcMemory(),
            new OmronFinsSimulatorServerOptions());
        var builder = new OmronFinsFrameBuilder();
        var wordWrite = builder.BuildMemoryAreaWriteWords(
            new OmronFinsConnectionOptions(),
            new PlcAddress(PlcDeviceType.D, 0),
            [1, 2]);
        var bitWrite = builder.BuildMemoryAreaWriteBits(
            new OmronFinsConnectionOptions(),
            new PlcAddress(PlcDeviceType.W, 0),
            [true, false]);

        var wordResponse = protocol.HandleRequest(wordWrite[..^1]);
        var bitResponse = protocol.HandleRequest(bitWrite[..^1]);

        Assert.Equal((ushort)0x1101, OmronFinsEndian.ReadUInt16(wordResponse, 12));
        Assert.Equal((ushort)0x1101, OmronFinsEndian.ReadUInt16(bitResponse, 12));
    }

    [Fact]
    public void HandleRequest_SkipsOverflowingAutoResponse()
    {
        var messages = new List<string>();
        var protocol = new OmronFinsSimulatorProtocol(
            new InMemoryPlcMemory(),
            new OmronFinsSimulatorServerOptions
            {
                EnableAutoWordResponse = true,
                AutoResponseTriggerOffset = 1,
                AutoResponseOffset = 2,
                AutoResponseIncrement = 1
            });
        protocol.StatusChanged += (_, message) => messages.Add(message);
        var request = new OmronFinsFrameBuilder().BuildMemoryAreaWriteWords(
            new OmronFinsConnectionOptions(),
            new PlcAddress(PlcDeviceType.D, 1),
            [short.MaxValue]);

        var response = protocol.HandleRequest(request);

        Assert.Equal((ushort)0, OmronFinsEndian.ReadUInt16(response, 12));
        Assert.Contains(messages, message => message.Contains("overflow", StringComparison.OrdinalIgnoreCase));
    }
}
