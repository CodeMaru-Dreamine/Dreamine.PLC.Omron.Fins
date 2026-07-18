using System.Text;

namespace Dreamine.PLC.Omron.Fins.Protocol;

/// <summary>
/// \if KO
/// <para>FINS/TCP 패킷을 래핑하고 추출하는 최소 도우미를 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides minimal helpers for wrapping and extracting FINS/TCP packets.</para>
/// \endif
/// </summary>
public static class OmronFinsTcpPacket
{
    /// <summary>
    /// \if KO
    /// <para>Signature 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the signature value.</para>
    /// \endif
    /// </summary>
    private static readonly byte[] Signature = Encoding.ASCII.GetBytes("FINS");

    /// <summary>
    /// \if KO
    /// <para>원시 FINS 명령 프레임을 FINS/TCP 패킷으로 래핑합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Wraps a raw FINS command frame in a FINS/TCP packet.</para>
    /// \endif
    /// </summary>
    /// <param name="finsFrame">
    /// \if KO
    /// <para>래핑할 원시 FINS 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The raw FINS frame to wrap.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>FINS/TCP 헤더가 추가된 새 패킷입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A new packet containing the FINS/TCP header.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="finsFrame"/>이 <see langword="null"/>일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="finsFrame"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
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
    /// \if KO
    /// <para>FINS/TCP 패킷에서 원시 FINS 프레임을 추출합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Extracts the raw FINS frame from a FINS/TCP packet.</para>
    /// \endif
    /// </summary>
    /// <param name="packet">
    /// \if KO
    /// <para>검사하고 추출할 FINS/TCP 패킷입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The FINS/TCP packet to validate and extract.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>헤더가 제거된 원시 FINS 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The raw FINS frame without the TCP header.</para>
    /// \endif
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// \if KO
    /// <para>패킷이 너무 짧거나 서명이 올바르지 않거나 TCP 오류 코드를 포함할 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the packet is too short, has an invalid signature, or contains a TCP error code.</para>
    /// \endif
    /// </exception>
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
