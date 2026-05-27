namespace Dreamine.PLC.Omron.Fins.Protocol;

/// <summary>
/// Defines supported Omron FINS command codes.
/// </summary>
public enum OmronFinsCommand : ushort
{
    /// <summary>
    /// FINS memory area read command.
    /// </summary>
    MemoryAreaRead = 0x0101,

    /// <summary>
    /// FINS memory area write command.
    /// </summary>
    MemoryAreaWrite = 0x0102
}
