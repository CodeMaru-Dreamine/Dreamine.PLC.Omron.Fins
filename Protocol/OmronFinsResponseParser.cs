using Dreamine.PLC.Abstractions.Results;

namespace Dreamine.PLC.Omron.Fins.Protocol;

/// <summary>
/// Parses Omron FINS memory area responses.
/// </summary>
public sealed class OmronFinsResponseParser
{
    /// <summary>
    /// Extracts payload bytes from a FINS response frame.
    /// </summary>
    /// <param name="frame">The FINS response frame.</param>
    /// <returns>The payload extraction result.</returns>
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
    /// Parses bit values from FINS payload bytes.
    /// </summary>
    /// <param name="payload">The payload bytes.</param>
    /// <param name="count">The expected bit count.</param>
    /// <returns>The parsed bit values.</returns>
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
    /// Parses word values from FINS payload bytes.
    /// </summary>
    /// <param name="payload">The payload bytes.</param>
    /// <param name="count">The expected word count.</param>
    /// <returns>The parsed word values.</returns>
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
