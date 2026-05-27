namespace Dreamine.PLC.Omron.Fins.Options;

/// <summary>
<<<<<<< HEAD
/// Represents Omron FINS connection options.
=======
/// Represents connection options for Omron FINS communication.
>>>>>>> main
/// </summary>
public sealed class OmronFinsConnectionOptions
{
    /// <summary>
<<<<<<< HEAD
    /// Gets or sets the target host.
=======
    /// Gets or sets the PLC host address.
>>>>>>> main
    /// </summary>
    public string Host { get; set; } = "127.0.0.1";

    /// <summary>
<<<<<<< HEAD
    /// Gets or sets the target port. The default FINS port is 9600.
=======
    /// Gets or sets the FINS port number.
>>>>>>> main
    /// </summary>
    public int Port { get; set; } = 9600;

    /// <summary>
    /// Gets or sets the transport type.
    /// </summary>
    public OmronFinsTransportType TransportType { get; set; } = OmronFinsTransportType.Udp;

    /// <summary>
<<<<<<< HEAD
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
=======
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
>>>>>>> main
}
