namespace Dreamine.PLC.Omron.Fins.Protocol;

/// <summary>
/// Defines Omron FINS memory area code constants.
/// </summary>
public static class OmronFinsMemoryAreaCode
{
    /// <summary>
    /// CIO bit area.
    /// </summary>
    public const byte CioBit = 0x30;

    /// <summary>
    /// Work bit area.
    /// </summary>
    public const byte WorkBit = 0x31;

    /// <summary>
    /// Holding bit area.
    /// </summary>
    public const byte HoldingBit = 0x32;

    /// <summary>
    /// DM bit area.
    /// </summary>
    public const byte DmBit = 0x02;

    /// <summary>
    /// CIO word area.
    /// </summary>
    public const byte CioWord = 0xB0;

    /// <summary>
    /// Work word area.
    /// </summary>
    public const byte WorkWord = 0xB1;

    /// <summary>
    /// Holding word area.
    /// </summary>
    public const byte HoldingWord = 0xB2;

    /// <summary>
    /// DM word area.
    /// </summary>
    public const byte DmWord = 0x82;
}
