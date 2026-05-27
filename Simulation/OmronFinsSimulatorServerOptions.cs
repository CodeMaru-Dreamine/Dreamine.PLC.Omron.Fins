namespace Dreamine.PLC.Omron.Fins.Simulation;

/// <summary>
/// Represents options for the Omron FINS simulator server.
/// </summary>
public sealed class OmronFinsSimulatorServerOptions
{
    /// <summary>
    /// Gets or sets the bind host.
    /// </summary>
    public string Host { get; set; } = "0.0.0.0";

    /// <summary>
    /// Gets or sets the bind port.
    /// </summary>
    public int Port { get; set; } = 9600;

    /// <summary>
    /// Gets or sets whether D100 single-word writes should produce an automatic D101 response.
    /// </summary>
    public bool EnableAutoWordResponse { get; set; } = true;

    /// <summary>
    /// Gets or sets the auto-response trigger offset.
    /// </summary>
    public int AutoResponseTriggerOffset { get; set; } = 100;

    /// <summary>
    /// Gets or sets the auto-response write offset.
    /// </summary>
    public int AutoResponseOffset { get; set; } = 101;

    /// <summary>
    /// Gets or sets the auto-response increment.
    /// </summary>
    public int AutoResponseIncrement { get; set; } = 1;
}
