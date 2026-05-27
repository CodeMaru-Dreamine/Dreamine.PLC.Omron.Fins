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
    }
}
