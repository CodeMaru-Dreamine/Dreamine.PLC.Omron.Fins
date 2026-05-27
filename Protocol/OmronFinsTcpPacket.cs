using System.Text;

namespace Dreamine.PLC.Omron.Fins.Protocol;

/// <summary>
/// Provides minimal FINS/TCP packet wrapping helpers.
/// </summary>
public static class OmronFinsTcpPacket
{
    private static readonly byte[] Signature = Encoding.ASCII.GetBytes("FINS");

    /// <summary>
    /// Wraps a raw FINS command frame into a FINS/TCP packet.
    /// </summary>
    /// <param name="finsFrame">The raw FINS frame.</param>
    /// <returns>The FINS/TCP packet.</returns>
    public static byte[] Wrap(IReadOnlyList<byte> finsFrame)
    {
        ArgumentNullException.ThrowIfNull(finsFrame);

        var packet = new byte[16 + finsFrame.Count];
        Buffer.BlockCopy(Signature, 0, packet, 0, Signature.Length);
        OmronFinsEndian.WriteInt32(packet, 4, 8 + finsFrame.Count);
        OmronFinsEndian.WriteInt32(packet, 8, 2);
        OmronFinsEndian.WriteInt32(packet, 12, 0);

        for (var index = 0; index < finsFrame.Count; index++)
        {
            packet[16 + index] = finsFrame[index];
        }

        return packet;
    }

    /// <summary>
    /// Extracts the raw FINS frame from a FINS/TCP packet.
    /// </summary>
    /// <param name="packet">The FINS/TCP packet.</param>
    /// <returns>The raw FINS frame.</returns>
    public static byte[] Extract(ReadOnlySpan<byte> packet)
    {
        if (packet.Length < 16)
        {
            throw new InvalidOperationException($"FINS/TCP packet is too short. Length={packet.Length}.");
        }

        if (packet[0] != Signature[0] || packet[1] != Signature[1] || packet[2] != Signature[2] || packet[3] != Signature[3])
        {
            throw new InvalidOperationException("Invalid FINS/TCP signature.");
        }

        var errorCode = OmronFinsEndian.ReadInt32(packet, 12);
        if (errorCode != 0)
        {
            throw new InvalidOperationException($"FINS/TCP error: 0x{errorCode:X8}.");
        }

        return packet[16..].ToArray();
    }
}
