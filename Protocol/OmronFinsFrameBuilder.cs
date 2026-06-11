using Dreamine.PLC.Abstractions.Devices;
using Dreamine.PLC.Omron.Fins.Devices;
using Dreamine.PLC.Omron.Fins.Options;

namespace Dreamine.PLC.Omron.Fins.Protocol;

/// <summary>
/// Builds Omron FINS command frames for memory area read/write operations.
/// </summary>
public sealed class OmronFinsFrameBuilder
{
    private int _sid;

    /// <summary>
    /// Builds a FINS memory area read frame.
    /// </summary>
    /// <param name="options">The FINS connection options.</param>
    /// <param name="address">The start PLC address.</param>
    /// <param name="count">The number of elements to read.</param>
    /// <param name="bitAccess">Whether the request targets bit access.</param>
    /// <returns>The request frame.</returns>
    public byte[] BuildMemoryAreaRead(
        OmronFinsConnectionOptions options,
        PlcAddress address,
        int count,
        bool bitAccess)
    {
        var command = BuildMemoryCommandBody(OmronFinsCommand.MemoryAreaRead, address, count, bitAccess, null);
        return BuildFrame(options, command);
    }

    /// <summary>
    /// Builds a FINS memory area write frame for word values.
    /// </summary>
    /// <param name="options">The FINS connection options.</param>
    /// <param name="address">The start PLC address.</param>
    /// <param name="values">The word values.</param>
    /// <returns>The request frame.</returns>
    public byte[] BuildMemoryAreaWriteWords(
        OmronFinsConnectionOptions options,
        PlcAddress address,
        IReadOnlyList<short> values)
    {
        var payload = new byte[values.Count * 2];
        for (var i = 0; i < values.Count; i++)
        {
            OmronFinsEndian.WriteInt16(payload, i * 2, values[i]);
        }

        var command = BuildMemoryCommandBody(OmronFinsCommand.MemoryAreaWrite, address, values.Count, false, payload);
        return BuildFrame(options, command);
    }

    /// <summary>
    /// Builds a FINS memory area write frame for bit values.
    /// </summary>
    /// <param name="options">The FINS connection options.</param>
    /// <param name="address">The start PLC address.</param>
    /// <param name="values">The bit values.</param>
    /// <returns>The request frame.</returns>
    public byte[] BuildMemoryAreaWriteBits(
        OmronFinsConnectionOptions options,
        PlcAddress address,
        IReadOnlyList<bool> values)
    {
        var payload = values.Select(value => value ? (byte)1 : (byte)0).ToArray();
        var command = BuildMemoryCommandBody(OmronFinsCommand.MemoryAreaWrite, address, values.Count, true, payload);
        return BuildFrame(options, command);
    }

    private byte[] BuildFrame(OmronFinsConnectionOptions options, byte[] command)
    {
        var frame = new byte[10 + command.Length];
        frame[0] = 0x80;
        frame[1] = 0x00;
        frame[2] = 0x02;
        frame[3] = options.DestinationNetwork;
        frame[4] = options.DestinationNode;
        frame[5] = options.DestinationUnit;
        frame[6] = options.SourceNetwork;
        frame[7] = options.SourceNode;
        frame[8] = options.SourceUnit;
        frame[9] = unchecked((byte)Interlocked.Increment(ref _sid));
        Buffer.BlockCopy(command, 0, frame, 10, command.Length);
        return frame;
    }

    private static byte[] BuildMemoryCommandBody(
        OmronFinsCommand command,
        PlcAddress address,
        int count,
        bool bitAccess,
        byte[]? payload)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "The FINS point count must be greater than zero.");
        }

        if (address.Offset is < 0 or > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(address),
                address.Offset,
                "The FINS address offset must be between 0 and 65535.");
        }

        if (count > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                count,
                "The FINS point count must be between 1 and 65535.");
        }

        var areaCode = OmronFinsMemoryAreaMapper.Map(address, bitAccess);
        var bodyLength = 8 + (payload?.Length ?? 0);
        var body = new byte[bodyLength];
        var commandValue = (ushort)command;

        OmronFinsEndian.WriteUInt16(body, 0, commandValue);
        body[2] = areaCode;
        OmronFinsEndian.WriteUInt16(body, 3, (ushort)address.Offset);
        body[5] = (byte)(address.BitOffset ?? 0);
        OmronFinsEndian.WriteUInt16(body, 6, (ushort)count);

        if (payload is not null)
        {
            Buffer.BlockCopy(payload, 0, body, 8, payload.Length);
        }

        return body;
    }
}
