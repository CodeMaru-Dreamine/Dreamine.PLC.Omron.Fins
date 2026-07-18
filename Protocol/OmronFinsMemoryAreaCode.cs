namespace Dreamine.PLC.Omron.Fins.Protocol;

/// <summary>
/// \if KO
/// <para>Omron FINS 메모리 영역 코드 상수를 정의합니다.</para>
/// \endif
/// \if EN
/// <para>Defines Omron FINS memory-area code constants.</para>
/// \endif
/// </summary>
public static class OmronFinsMemoryAreaCode
{
    /// <summary>
    /// \if KO
    /// <para>CIO 비트 영역입니다.</para>
    /// \endif
    /// \if EN
    /// <para>CIO bit area.</para>
    /// \endif
    /// </summary>
    public const byte CioBit = 0x30;

    /// <summary>
    /// \if KO
    /// <para>작업 비트 영역입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Work bit area.</para>
    /// \endif
    /// </summary>
    public const byte WorkBit = 0x31;

    /// <summary>
    /// \if KO
    /// <para>홀딩 비트 영역입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Holding bit area.</para>
    /// \endif
    /// </summary>
    public const byte HoldingBit = 0x32;

    /// <summary>
    /// \if KO
    /// <para>DM 비트 영역입니다.</para>
    /// \endif
    /// \if EN
    /// <para>DM bit area.</para>
    /// \endif
    /// </summary>
    public const byte DmBit = 0x02;

    /// <summary>
    /// \if KO
    /// <para>CIO 워드 영역입니다.</para>
    /// \endif
    /// \if EN
    /// <para>CIO word area.</para>
    /// \endif
    /// </summary>
    public const byte CioWord = 0xB0;

    /// <summary>
    /// \if KO
    /// <para>작업 워드 영역입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Work word area.</para>
    /// \endif
    /// </summary>
    public const byte WorkWord = 0xB1;

    /// <summary>
    /// \if KO
    /// <para>홀딩 워드 영역입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Holding word area.</para>
    /// \endif
    /// </summary>
    public const byte HoldingWord = 0xB2;

    /// <summary>
    /// \if KO
    /// <para>DM 워드 영역입니다.</para>
    /// \endif
    /// \if EN
    /// <para>DM word area.</para>
    /// \endif
    /// </summary>
    public const byte DmWord = 0x82;
}
