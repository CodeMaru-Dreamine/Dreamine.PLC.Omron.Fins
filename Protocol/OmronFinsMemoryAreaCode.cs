namespace Dreamine.PLC.Omron.Fins.Protocol;

/// <summary>
/// Defines Omron FINS memory area codes used by the adapter boundary.
/// </summary>
public enum OmronFinsMemoryAreaCode : byte
{
    /// <summary>
    /// CIO bit area.
    /// </summary>
    CioBit = 0x30,

    /// <summary>
    /// WR bit area.
    /// </summary>
    WorkBit = 0x31,

    /// <summary>
    /// HR bit area.
    /// </summary>
    HoldingBit = 0x32,

    /// <summary>
    /// AR bit area.
    /// </summary>
    AuxiliaryBit = 0x33,

    /// <summary>
    /// DM bit area.
    /// </summary>
    DataMemoryBit = 0x02,

    /// <summary>
    /// CIO word area.
    /// </summary>
    CioWord = 0xB0,

    /// <summary>
    /// WR word area.
    /// </summary>
    WorkWord = 0xB1,

    /// <summary>
    /// HR word area.
    /// </summary>
    HoldingWord = 0xB2,

    /// <summary>
    /// AR word area.
    /// </summary>
    AuxiliaryWord = 0xB3,

    /// <summary>
    /// DM word area.
    /// </summary>
    DataMemoryWord = 0x82
}
