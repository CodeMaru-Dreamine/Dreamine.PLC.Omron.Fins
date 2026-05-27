using Dreamine.PLC.Abstractions.Devices;
using Dreamine.PLC.Omron.Fins.Devices;
using Dreamine.PLC.Omron.Fins.Options;

namespace Dreamine.PLC.Omron.Fins.Protocol;

/// <summary>
/// Builds minimal FINS command frames for memory area read/write operations.
/// </summary>
public static class OmronFinsFrameBuilder
{
    /// <summary>
    /// Builds a FINS memory area read frame.
    /// </summary>
    /// <param name="options">The FINS connection options.</param>
    /// <param name="address">The start PLC address.</param>
    /// <param name="count">The number of elements to read.</param>
    /// <param name="bitAccess">Whether the request targets bit access.</param>
    /// <returns>The request frame.</returns>
    public static byte[] BuildMemoryAreaRead(
        OmronFinsConnectionOptions options,
        PlcAddress address,
        int count,
        bool bitAccess)
    {
        var command = BuildMemoryCommandBody(OmronFinsCommand.MemoryAreaRead, address, count, bitAccess, null);
        return BuildUdpFrame(options, command);
    }

    /// <summary>
    /// Builds a FINS memory area write frame for word values.
    /// </summary>
    /// <param name="options">The FINS connection options.</param>
    /// <param name="address">The start PLC address.</param>
    /// <param name="values">The word values.</param>
    /// <returns>The request frame.</returns>
    public static byte[] BuildMemoryAreaWriteWords(
        OmronFinsConnectionOptions options,
        PlcAddress address,
        IReadOnlyList<short> values)
    {
        var payload = new byte[values.Count * 2];
        for (var i = 0; i < values.Count; i++)
        {
            payload[i * 2] = (byte)((values[i] >> 8) & 0xFF);
            payload[(i * 2) + 1] = (byte)(values[i] & 0xFF);
        }

        var command = BuildMemoryCommandBody(OmronFinsCommand.MemoryAreaWrite, address, values.Count, false, payload);
        return BuildUdpFrame(options, command);
    }

    /// <summary>
    /// Builds a FINS memory area write frame for bit values.
    /// </summary>
    /// <param name="options">The FINS connection options.</param>
    /// <param name="address">The start PLC address.</param>
    /// <param name="values">The bit values.</param>
    /// <returns>The request frame.</returns>
    public static byte[] BuildMemoryAreaWriteBits(
        OmronFinsConnectionOptions options,
        PlcAddress address,
        IReadOnlyList<bool> values)
    {
        var payload = values.Select(value => value ? (byte)1 : (byte)0).ToArray();
        var command = BuildMemoryCommandBody(OmronFinsCommand.MemoryAreaWrite, address, values.Count, true, payload);
        return BuildUdpFrame(options, command);
    }

    private static byte[] BuildUdpFrame(OmronFinsConnectionOptions options, byte[] command)
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
        frame[9] = 0x01;
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
        var areaCode = OmronFinsMemoryAreaMapper.Map(address, bitAccess);
        var bodyLength = 8 + (payload?.Length ?? 0);
        var body = new byte[bodyLength];
        var commandValue = (ushort)command;

        body[0] = (byte)((commandValue >> 8) & 0xFF);
        body[1] = (byte)(commandValue & 0xFF);
        body[2] = (byte)areaCode;
        body[3] = (byte)((address.Offset >> 8) & 0xFF);
        body[4] = (byte)(address.Offset & 0xFF);
        body[5] = (byte)(address.BitOffset ?? 0);
        body[6] = (byte)((count >> 8) & 0xFF);
        body[7] = (byte)(count & 0xFF);

        if (payload is not null)
        {
            Buffer.BlockCopy(payload, 0, body, 8, payload.Length);
        }

        return body;
    }
}
