namespace Dreamine.PLC.Omron.Fins.Options;

/// <summary>
/// \if KO
/// <para>Omron FINS 전송 형식을 정의합니다.</para>
/// \endif
/// \if EN
/// <para>Defines the Omron FINS transport type.</para>
/// \endif
/// </summary>
public enum OmronFinsTransportType
{
    /// <summary>
    /// \if KO
    /// <para>FINS/TCP 전송을 사용합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Uses FINS/TCP transport.</para>
    /// \endif
    /// </summary>
    Tcp = 0,

    /// <summary>
    /// \if KO
    /// <para>FINS/UDP 전송을 사용합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Uses FINS/UDP transport.</para>
    /// \endif
    /// </summary>
    Udp = 1
}
