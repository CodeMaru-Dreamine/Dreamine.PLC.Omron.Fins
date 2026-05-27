namespace Dreamine.PLC.Omron.Fins.Options;

/// <summary>
/// Represents connection options for Omron FINS communication.
/// </summary>
public sealed class OmronFinsConnectionOptions
{
    /// <summary>
    /// Gets or sets the PLC host address.
    /// </summary>
    public string Host { get; set; } = "127.0.0.1";

    /// <summary>
    /// Gets or sets the FINS port number.
    /// </summary>
    public int Port { get; set; } = 9600;

    /// <summary>
    /// Gets or sets the transport type.
    /// </summary>
    public OmronFinsTransportType TransportType { get; set; } = OmronFinsTransportType.Udp;

    /// <summary>
    /// Gets or sets the local FINS network number.
    /// </summary>
    public byte SourceNetwork { get; set; }

    /// <summary>
    /// Gets or sets the local FINS node number.
    /// </summary>
    public byte SourceNode { get; set; } = 1;

    /// <summary>
    /// Gets or sets the local FINS unit number.
    /// </summary>
    public byte SourceUnit { get; set; }

    /// <summary>
    /// Gets or sets the destination FINS network number.
    /// </summary>
    public byte DestinationNetwork { get; set; }

    /// <summary>
    /// Gets or sets the destination FINS node number.
    /// </summary>
    public byte DestinationNode { get; set; } = 1;

    /// <summary>
    /// Gets or sets the destination FINS unit number.
    /// </summary>
    public byte DestinationUnit { get; set; }

    /// <summary>
    /// Gets or sets the request timeout in milliseconds.
    /// </summary>
    public int TimeoutMs { get; set; } = 3000;

    /// <summary>
    /// Gets or sets the retry count used for request/response operations.
    /// </summary>
    public int RetryCount { get; set; } = 1;
}
