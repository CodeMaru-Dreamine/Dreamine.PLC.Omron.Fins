namespace Dreamine.PLC.Omron.Fins.Protocol;

/// <summary>
/// \if KO
/// <para>FINS 프레임에서 사용하는 빅 엔디언 변환 도우미를 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides big-endian conversion helpers used by FINS frames.</para>
/// \endif
/// </summary>
public static class OmronFinsEndian
{
    /// <summary>
    /// \if KO
    /// <para>빅 엔디언 부호 없는 16비트 정수를 읽습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Reads a big-endian unsigned 16-bit integer.</para>
    /// \endif
    /// </summary>
    /// <param name="buffer">
    /// \if KO
    /// <para>원본 버퍼입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The source buffer.</para>
    /// \endif
    /// </param>
    /// <param name="offset">
    /// \if KO
    /// <para>읽기를 시작할 0부터 시작하는 오프셋입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The zero-based offset at which to begin reading.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>읽은 부호 없는 16비트 정수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The unsigned 16-bit integer read from the buffer.</para>
    /// \endif
    /// </returns>
    /// <exception cref="IndexOutOfRangeException">
    /// \if KO
    /// <para>오프셋에서 2바이트를 읽을 수 없을 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when two bytes cannot be read at the specified offset.</para>
    /// \endif
    /// </exception>
    public static ushort ReadUInt16(ReadOnlySpan<byte> buffer, int offset)
    {
        return (ushort)((buffer[offset] << 8) | buffer[offset + 1]);
    }

    /// <summary>
    /// \if KO
    /// <para>빅 엔디언 부호 있는 16비트 정수를 읽습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Reads a big-endian signed 16-bit integer.</para>
    /// \endif
    /// </summary>
    /// <param name="buffer">
    /// \if KO
    /// <para>원본 버퍼입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The source buffer.</para>
    /// \endif
    /// </param>
    /// <param name="offset">
    /// \if KO
    /// <para>읽기를 시작할 0부터 시작하는 오프셋입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The zero-based offset at which to begin reading.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>읽은 부호 있는 16비트 정수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The signed 16-bit integer read from the buffer.</para>
    /// \endif
    /// </returns>
    /// <exception cref="IndexOutOfRangeException">
    /// \if KO
    /// <para>오프셋에서 2바이트를 읽을 수 없을 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when two bytes cannot be read at the specified offset.</para>
    /// \endif
    /// </exception>
    public static short ReadInt16(ReadOnlySpan<byte> buffer, int offset)
    {
        return unchecked((short)ReadUInt16(buffer, offset));
    }

    /// <summary>
    /// \if KO
    /// <para>빅 엔디언 부호 있는 32비트 정수를 읽습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Reads a big-endian signed 32-bit integer.</para>
    /// \endif
    /// </summary>
    /// <param name="buffer">
    /// \if KO
    /// <para>원본 버퍼입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The source buffer.</para>
    /// \endif
    /// </param>
    /// <param name="offset">
    /// \if KO
    /// <para>읽기를 시작할 0부터 시작하는 오프셋입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The zero-based offset at which to begin reading.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>읽은 부호 있는 32비트 정수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The signed 32-bit integer read from the buffer.</para>
    /// \endif
    /// </returns>
    /// <exception cref="IndexOutOfRangeException">
    /// \if KO
    /// <para>오프셋에서 4바이트를 읽을 수 없을 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when four bytes cannot be read at the specified offset.</para>
    /// \endif
    /// </exception>
    public static int ReadInt32(ReadOnlySpan<byte> buffer, int offset)
    {
        return (buffer[offset] << 24) | (buffer[offset + 1] << 16) | (buffer[offset + 2] << 8) | buffer[offset + 3];
    }

    /// <summary>
    /// \if KO
    /// <para>부호 없는 16비트 정수를 빅 엔디언으로 씁니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes an unsigned 16-bit integer in big-endian order.</para>
    /// \endif
    /// </summary>
    /// <param name="buffer">
    /// \if KO
    /// <para>대상 버퍼입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The destination buffer.</para>
    /// \endif
    /// </param>
    /// <param name="offset">
    /// \if KO
    /// <para>쓰기를 시작할 0부터 시작하는 오프셋입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The zero-based offset at which to begin writing.</para>
    /// \endif
    /// </param>
    /// <param name="value">
    /// \if KO
    /// <para>쓸 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The value to write.</para>
    /// \endif
    /// </param>
    /// <exception cref="IndexOutOfRangeException">
    /// \if KO
    /// <para>오프셋에 2바이트를 쓸 수 없을 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when two bytes cannot be written at the specified offset.</para>
    /// \endif
    /// </exception>
    public static void WriteUInt16(Span<byte> buffer, int offset, ushort value)
    {
        buffer[offset] = (byte)((value >> 8) & 0xFF);
        buffer[offset + 1] = (byte)(value & 0xFF);
    }

    /// <summary>
    /// \if KO
    /// <para>부호 있는 16비트 정수를 빅 엔디언으로 씁니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes a signed 16-bit integer in big-endian order.</para>
    /// \endif
    /// </summary>
    /// <param name="buffer">
    /// \if KO
    /// <para>대상 버퍼입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The destination buffer.</para>
    /// \endif
    /// </param>
    /// <param name="offset">
    /// \if KO
    /// <para>쓰기를 시작할 0부터 시작하는 오프셋입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The zero-based offset at which to begin writing.</para>
    /// \endif
    /// </param>
    /// <param name="value">
    /// \if KO
    /// <para>쓸 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The value to write.</para>
    /// \endif
    /// </param>
    /// <exception cref="IndexOutOfRangeException">
    /// \if KO
    /// <para>오프셋에 2바이트를 쓸 수 없을 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when two bytes cannot be written at the specified offset.</para>
    /// \endif
    /// </exception>
    public static void WriteInt16(Span<byte> buffer, int offset, short value)
    {
        WriteUInt16(buffer, offset, unchecked((ushort)value));
    }

    /// <summary>
    /// \if KO
    /// <para>부호 있는 32비트 정수를 빅 엔디언으로 씁니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes a signed 32-bit integer in big-endian order.</para>
    /// \endif
    /// </summary>
    /// <param name="buffer">
    /// \if KO
    /// <para>대상 버퍼입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The destination buffer.</para>
    /// \endif
    /// </param>
    /// <param name="offset">
    /// \if KO
    /// <para>쓰기를 시작할 0부터 시작하는 오프셋입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The zero-based offset at which to begin writing.</para>
    /// \endif
    /// </param>
    /// <param name="value">
    /// \if KO
    /// <para>쓸 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The value to write.</para>
    /// \endif
    /// </param>
    /// <exception cref="IndexOutOfRangeException">
    /// \if KO
    /// <para>오프셋에 4바이트를 쓸 수 없을 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when four bytes cannot be written at the specified offset.</para>
    /// \endif
    /// </exception>
    public static void WriteInt32(Span<byte> buffer, int offset, int value)
    {
        buffer[offset] = (byte)((value >> 24) & 0xFF);
        buffer[offset + 1] = (byte)((value >> 16) & 0xFF);
        buffer[offset + 2] = (byte)((value >> 8) & 0xFF);
        buffer[offset + 3] = (byte)(value & 0xFF);
    }
}
