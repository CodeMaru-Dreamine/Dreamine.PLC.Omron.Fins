namespace Dreamine.PLC.Omron.Fins.Protocol;

/// <summary>
<<<<<<< HEAD
/// Defines Omron FINS memory area code constants.
/// </summary>
public static class OmronFinsMemoryAreaCode
=======
/// Defines Omron FINS memory area codes used by the adapter boundary.
/// </summary>
public enum OmronFinsMemoryAreaCode : byte
>>>>>>> main
{
    /// <summary>
    /// CIO bit area.
    /// </summary>
<<<<<<< HEAD
    public const byte CioBit = 0x30;

    /// <summary>
    /// Work bit area.
    /// </summary>
    public const byte WorkBit = 0x31;

    /// <summary>
    /// Holding bit area.
    /// </summary>
    public const byte HoldingBit = 0x32;
=======
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
>>>>>>> main

    /// <summary>
    /// DM bit area.
    /// </summary>
<<<<<<< HEAD
    public const byte DmBit = 0x02;
=======
    DataMemoryBit = 0x02,
>>>>>>> main

    /// <summary>
    /// CIO word area.
    /// </summary>
<<<<<<< HEAD
    public const byte CioWord = 0xB0;

    /// <summary>
    /// Work word area.
    /// </summary>
    public const byte WorkWord = 0xB1;

    /// <summary>
    /// Holding word area.
    /// </summary>
    public const byte HoldingWord = 0xB2;
=======
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
>>>>>>> main

    /// <summary>
    /// DM word area.
    /// </summary>
<<<<<<< HEAD
    public const byte DmWord = 0x82;
=======
    DataMemoryWord = 0x82
>>>>>>> main
}
