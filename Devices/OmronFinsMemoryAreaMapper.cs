using Dreamine.PLC.Abstractions.Devices;
using Dreamine.PLC.Omron.Fins.Protocol;

namespace Dreamine.PLC.Omron.Fins.Devices;

/// <summary>
/// \if KO
/// <para>Dreamine PLC 장치 형식과 Omron FINS 메모리 영역 코드를 상호 매핑합니다.</para>
/// \endif
/// \if EN
/// <para>Maps between Dreamine PLC device types and Omron FINS memory-area codes.</para>
/// \endif
/// </summary>
public static class OmronFinsMemoryAreaMapper
{
    /// <summary>
    /// \if KO
    /// <para>PLC 주소를 FINS 메모리 영역 코드로 매핑합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Maps a PLC address to a FINS memory-area code.</para>
    /// \endif
    /// </summary>
    /// <param name="address">
    /// \if KO
    /// <para>매핑할 PLC 주소입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The PLC address to map.</para>
    /// \endif
    /// </param>
    /// <param name="bitAccess">
    /// \if KO
    /// <para>비트 접근 코드가 필요한지 여부입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Whether a bit-access code is requested.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>대응하는 FINS 메모리 영역 코드입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The corresponding FINS memory-area code.</para>
    /// \endif
    /// </returns>
    /// <exception cref="NotSupportedException">
    /// \if KO
    /// <para>PLC 장치 형식을 FINS가 지원하지 않을 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the PLC device type is unsupported by FINS.</para>
    /// \endif
    /// </exception>
    public static byte Map(PlcAddress address, bool bitAccess)
    {
        return address.DeviceType switch
        {
            PlcDeviceType.D => bitAccess ? OmronFinsMemoryAreaCode.DmBit : OmronFinsMemoryAreaCode.DmWord,
            PlcDeviceType.W => bitAccess ? OmronFinsMemoryAreaCode.WorkBit : OmronFinsMemoryAreaCode.WorkWord,
            PlcDeviceType.R => bitAccess ? OmronFinsMemoryAreaCode.HoldingBit : OmronFinsMemoryAreaCode.HoldingWord,
            PlcDeviceType.M => bitAccess ? OmronFinsMemoryAreaCode.CioBit : OmronFinsMemoryAreaCode.CioWord,
            PlcDeviceType.X => bitAccess ? OmronFinsMemoryAreaCode.CioBit : OmronFinsMemoryAreaCode.CioWord,
            PlcDeviceType.Y => bitAccess ? OmronFinsMemoryAreaCode.CioBit : OmronFinsMemoryAreaCode.CioWord,
            _ => throw new NotSupportedException($"Unsupported FINS device type: {address.DeviceType}.")
        };
    }

    /// <summary>
    /// \if KO
    /// <para>FINS 메모리 영역 코드를 Dreamine PLC 장치 형식으로 매핑합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Maps a FINS memory-area code to a Dreamine PLC device type.</para>
    /// \endif
    /// </summary>
    /// <param name="areaCode">
    /// \if KO
    /// <para>매핑할 FINS 메모리 영역 코드입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The FINS memory-area code to map.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>대응하는 PLC 장치 형식이며 알 수 없는 코드는 <see cref="PlcDeviceType.Unknown"/>입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The corresponding PLC device type, or <see cref="PlcDeviceType.Unknown"/> for an unrecognized code.</para>
    /// \endif
    /// </returns>
    public static PlcDeviceType ToDeviceType(byte areaCode)
    {
        return areaCode switch
        {
            OmronFinsMemoryAreaCode.DmBit or OmronFinsMemoryAreaCode.DmWord => PlcDeviceType.D,
            OmronFinsMemoryAreaCode.WorkBit or OmronFinsMemoryAreaCode.WorkWord => PlcDeviceType.W,
            OmronFinsMemoryAreaCode.HoldingBit or OmronFinsMemoryAreaCode.HoldingWord => PlcDeviceType.R,
            OmronFinsMemoryAreaCode.CioBit or OmronFinsMemoryAreaCode.CioWord => PlcDeviceType.M,
            _ => PlcDeviceType.Unknown
        };
    }

    /// <summary>
    /// \if KO
    /// <para>메모리 영역 코드가 비트 접근을 나타내는지 확인합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Determines whether the memory-area code represents bit access.</para>
    /// \endif
    /// </summary>
    /// <param name="areaCode">
    /// \if KO
    /// <para>검사할 FINS 메모리 영역 코드입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The FINS memory-area code to inspect.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>비트 영역이면 <see langword="true"/>, 아니면 <see langword="false"/>입니다.</para>
    /// \endif
    /// \if EN
    /// <para><see langword="true"/> for a bit area; otherwise, <see langword="false"/>.</para>
    /// \endif
    /// </returns>
    public static bool IsBitArea(byte areaCode)
    {
        return areaCode is OmronFinsMemoryAreaCode.CioBit
            or OmronFinsMemoryAreaCode.WorkBit
            or OmronFinsMemoryAreaCode.HoldingBit
            or OmronFinsMemoryAreaCode.DmBit;
    }
}
