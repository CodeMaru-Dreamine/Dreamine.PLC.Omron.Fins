<<<<<<< HEAD
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
=======
namespace Dreamine.PLC.Omron.Fins.Protocol;

/// <summary>
/// Parses minimal FINS response frames used by the adapter boundary.
/// </summary>
public static class OmronFinsResponseParser
{
    /// <summary>
    /// Extracts the FINS command response payload.
    /// </summary>
    /// <param name="response">The response frame.</param>
    /// <returns>The command payload after the end code.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the response indicates a FINS error.</exception>
    public static byte[] ExtractPayload(byte[] response)
    {
        if (response.Length < 14)
        {
            throw new InvalidOperationException("Invalid FINS response length.");
        }

        var commandOffset = 10;
        var endCode = (response[commandOffset + 2] << 8) | response[commandOffset + 3];
        if (endCode != 0)
        {
            throw new InvalidOperationException($"FINS command failed. EndCode=0x{endCode:X4}.");
        }

        return response.Skip(commandOffset + 4).ToArray();
    }

    /// <summary>
    /// Parses word values from a FINS memory area read response payload.
    /// </summary>
    /// <param name="payload">The response payload.</param>
    /// <returns>The parsed word values.</returns>
    public static short[] ParseWords(byte[] payload)
    {
        var values = new short[payload.Length / 2];
        for (var i = 0; i < values.Length; i++)
        {
            values[i] = unchecked((short)((payload[i * 2] << 8) | payload[(i * 2) + 1]));
        }

        return values;
    }

    /// <summary>
    /// Parses bit values from a FINS memory area read response payload.
    /// </summary>
    /// <param name="payload">The response payload.</param>
    /// <returns>The parsed bit values.</returns>
    public static bool[] ParseBits(byte[] payload)
    {
        return payload.Select(value => value != 0).ToArray();
>>>>>>> main
    }
}
