namespace Dreamine.PLC.Omron.Fins.Protocol;

/// <summary>
/// Provides big-endian conversion helpers used by FINS frames.
/// </summary>
public static class OmronFinsEndian
{
    /// <summary>
    /// Reads a big-endian unsigned 16-bit integer.
    /// </summary>
    /// <param name="buffer">The source buffer.</param>
    /// <param name="offset">The source offset.</param>
    /// <returns>The unsigned 16-bit integer.</returns>
    public static ushort ReadUInt16(ReadOnlySpan<byte> buffer, int offset)
    {
        return (ushort)((buffer[offset] << 8) | buffer[offset + 1]);
    }

    /// <summary>
    /// Reads a big-endian signed 16-bit integer.
    /// </summary>
    /// <param name="buffer">The source buffer.</param>
    /// <param name="offset">The source offset.</param>
    /// <returns>The signed 16-bit integer.</returns>
    public static short ReadInt16(ReadOnlySpan<byte> buffer, int offset)
    {
        return unchecked((short)ReadUInt16(buffer, offset));
    }

    /// <summary>
    /// Reads a big-endian signed 32-bit integer.
    /// </summary>
    /// <param name="buffer">The source buffer.</param>
    /// <param name="offset">The source offset.</param>
    /// <returns>The signed 32-bit integer.</returns>
    public static int ReadInt32(ReadOnlySpan<byte> buffer, int offset)
    {
        return (buffer[offset] << 24) | (buffer[offset + 1] << 16) | (buffer[offset + 2] << 8) | buffer[offset + 3];
    }

    /// <summary>
    /// Writes a big-endian unsigned 16-bit integer.
    /// </summary>
    /// <param name="buffer">The destination buffer.</param>
    /// <param name="offset">The destination offset.</param>
    /// <param name="value">The value.</param>
    public static void WriteUInt16(Span<byte> buffer, int offset, ushort value)
    {
        buffer[offset] = (byte)((value >> 8) & 0xFF);
        buffer[offset + 1] = (byte)(value & 0xFF);
    }

    /// <summary>
    /// Writes a big-endian signed 16-bit integer.
    /// </summary>
    /// <param name="buffer">The destination buffer.</param>
    /// <param name="offset">The destination offset.</param>
    /// <param name="value">The value.</param>
    public static void WriteInt16(Span<byte> buffer, int offset, short value)
    {
        WriteUInt16(buffer, offset, unchecked((ushort)value));
    }

    /// <summary>
    /// Writes a big-endian signed 32-bit integer.
    /// </summary>
    /// <param name="buffer">The destination buffer.</param>
    /// <param name="offset">The destination offset.</param>
    /// <param name="value">The value.</param>
    public static void WriteInt32(Span<byte> buffer, int offset, int value)
    {
        buffer[offset] = (byte)((value >> 24) & 0xFF);
        buffer[offset + 1] = (byte)((value >> 16) & 0xFF);
        buffer[offset + 2] = (byte)((value >> 8) & 0xFF);
        buffer[offset + 3] = (byte)(value & 0xFF);
    }
}
