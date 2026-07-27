using Dreamine.PLC.Abstractions.Results;

namespace Dreamine.PLC.Omron.Fins.Protocol;

/// <summary>
/// \if KO
/// <para>Omron FINS 메모리 영역 응답을 구문 분석합니다.</para>
/// \endif
/// \if EN
/// <para>Parses Omron FINS memory-area responses.</para>
/// \endif
/// </summary>
public sealed class OmronFinsResponseParser
{
    /// <summary>
    /// \if KO
    /// <para>FINS 응답 프레임을 검증하고 페이로드 바이트를 추출합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Validates a FINS response frame and extracts its payload bytes.</para>
    /// \endif
    /// </summary>
    /// <param name="frame">
    /// \if KO
    /// <para>검사할 FINS 응답 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The FINS response frame to inspect.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>추출된 페이로드 또는 프레임·종료 코드 오류를 포함하는 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A result containing the extracted payload or a frame or end-code error.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="frame"/>이 <see langword="null"/>일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="frame"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    public PlcResult<byte[]> ExtractPayload(IReadOnlyList<byte> frame)
    {
        ArgumentNullException.ThrowIfNull(frame);

        if (frame.Count < 14)
        {
            return PlcResult<byte[]>.Failure($"The FINS response frame is too short. Length={frame.Count}.");
        }

        var buffer = frame as byte[] ?? frame.ToArray();
        var endCode = OmronFinsEndian.ReadUInt16(buffer, 12);
        if (endCode != 0)
        {
            return PlcResult<byte[]>.Failure($"FINS end code indicates failure: 0x{endCode:X4}.", endCode);
        }

        return PlcResult<byte[]>.Success(buffer[14..]);
    }

    /// <summary>
    /// \if KO
    /// <para>FINS 페이로드 바이트에서 비트 값을 구문 분석합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Parses bit values from FINS payload bytes.</para>
    /// \endif
    /// </summary>
    /// <param name="payload">
    /// \if KO
    /// <para>구문 분석할 페이로드 바이트입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The payload bytes to parse.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>기대하는 비트 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The expected number of bits.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>구문 분석된 비트 값 또는 길이 오류를 포함하는 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A result containing the parsed bit values or a length error.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="payload"/>가 <see langword="null"/>일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="payload"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    public PlcResult<bool[]> ParseBits(IReadOnlyList<byte> payload, int count)
    {
        ArgumentNullException.ThrowIfNull(payload);

        if (payload.Count < count)
        {
            return PlcResult<bool[]>.Failure($"The FINS bit payload is too short. Expected={count}, Actual={payload.Count}.");
        }

        var values = new bool[count];
        for (var index = 0; index < count; index++)
        {
            values[index] = payload[index] != 0;
        }

        return PlcResult<bool[]>.Success(values);
    }

    /// <summary>
    /// \if KO
    /// <para>FINS 페이로드 바이트에서 빅 엔디언 워드 값을 구문 분석합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Parses big-endian word values from FINS payload bytes.</para>
    /// \endif
    /// </summary>
    /// <param name="payload">
    /// \if KO
    /// <para>구문 분석할 페이로드 바이트입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The payload bytes to parse.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>기대하는 워드 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The expected number of words.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>구문 분석된 워드 값 또는 길이 오류를 포함하는 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A result containing the parsed word values or a length error.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="payload"/>가 <see langword="null"/>일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="payload"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    /// <exception cref="OverflowException">
    /// \if KO
    /// <para><paramref name="count"/>에 2를 곱한 값이 <see cref="int"/> 범위를 벗어날 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when multiplying <paramref name="count"/> by two exceeds the range of <see cref="int"/>.</para>
    /// \endif
    /// </exception>
    public PlcResult<short[]> ParseWords(IReadOnlyList<byte> payload, int count)
    {
        ArgumentNullException.ThrowIfNull(payload);

        var expectedLength = checked(count * 2);
        if (payload.Count < expectedLength)
        {
            return PlcResult<short[]>.Failure($"The FINS word payload is too short. Expected={expectedLength}, Actual={payload.Count}.");
        }

        var buffer = payload as byte[] ?? payload.ToArray();
        var values = new short[count];
        for (var index = 0; index < count; index++)
        {
            values[index] = OmronFinsEndian.ReadInt16(buffer, index * 2);
        }

        return PlcResult<short[]>.Success(values);
    }
}
