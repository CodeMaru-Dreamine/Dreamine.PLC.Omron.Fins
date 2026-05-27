namespace Dreamine.PLC.Omron.Fins.Options;

/// <summary>
/// Represents Omron FINS connection options.
/// </summary>
public sealed class OmronFinsConnectionOptions
{
    /// <summary>
    /// Gets or sets the target host.
    /// </summary>
    public string Host { get; set; } = "127.0.0.1";

    /// <summary>
    /// Gets or sets the target port. The default FINS port is 9600.
    /// </summary>
    public int Port { get; set; } = 9600;

    /// <summary>
    /// Gets or sets the transport type.
    /// </summary>
    public OmronFinsTransportType TransportType { get; set; } = OmronFinsTransportType.Udp;

    /// <summary>
    /// Gets or sets the connect timeout in milliseconds.
    /// </summary>
    public int ConnectTimeoutMs { get; set; } = 3000;

    /// <summary>
    /// Gets or sets the receive timeout in milliseconds.
    /// </summary>
    public int ReceiveTimeoutMs { get; set; } = 3000;

    /// <summary>
    /// Gets or sets the retry count.
    /// </summary>
    public int RetryCount { get; set; } = 1;

    /// <summary>
    /// Gets or sets the destination network address.
    /// </summary>
    public byte DestinationNetwork { get; set; } = 0x00;

    /// <summary>
    /// Gets or sets the destination node address.
    /// </summary>
    public byte DestinationNode { get; set; } = 0x00;

    /// <summary>
    /// Gets or sets the destination unit address.
    /// </summary>
    public byte DestinationUnit { get; set; } = 0x00;

    /// <summary>
    /// Gets or sets the source network address.
    /// </summary>
    public byte SourceNetwork { get; set; } = 0x00;

    /// <summary>
    /// Gets or sets the source node address.
    /// </summary>
    public byte SourceNode { get; set; } = 0x01;

    /// <summary>
    /// Gets or sets the source unit address.
    /// </summary>
    public byte SourceUnit { get; set; } = 0x00;
}
