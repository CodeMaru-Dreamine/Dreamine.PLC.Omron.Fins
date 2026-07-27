namespace Dreamine.PLC.Omron.Fins.Protocol;

/// <summary>
/// \if KO
/// <para>지원되는 Omron FINS 명령 코드를 정의합니다.</para>
/// \endif
/// \if EN
/// <para>Defines supported Omron FINS command codes.</para>
/// \endif
/// </summary>
public enum OmronFinsCommand : ushort
{
    /// <summary>
    /// \if KO
    /// <para>메모리 영역 읽기 명령입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Memory-area read command.</para>
    /// \endif
    /// </summary>
    MemoryAreaRead = 0x0101,

    /// <summary>
    /// \if KO
    /// <para>메모리 영역 쓰기 명령입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Memory-area write command.</para>
    /// \endif
    /// </summary>
    MemoryAreaWrite = 0x0102
}
