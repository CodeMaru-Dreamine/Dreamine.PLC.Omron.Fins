namespace Dreamine.PLC.Omron.Fins.Options;

/// <summary>
/// Defines the Omron FINS transport type.
/// </summary>
public enum OmronFinsTransportType
{
    /// <summary>
    /// Uses FINS/TCP transport.
    /// </summary>
    Tcp = 0,

    /// <summary>
    /// Uses FINS/UDP transport.
    /// </summary>
    Udp = 1
}
