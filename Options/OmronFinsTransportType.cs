namespace Dreamine.PLC.Omron.Fins.Options;

/// <summary>
/// Defines the transport type used by the Omron FINS adapter.
/// </summary>
public enum OmronFinsTransportType
{
    /// <summary>
    /// Uses FINS/UDP transport.
    /// </summary>
    Udp = 0,

    /// <summary>
    /// Uses FINS/TCP transport.
    /// </summary>
    Tcp = 1
}
