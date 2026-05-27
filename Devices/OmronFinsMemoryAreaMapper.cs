using Dreamine.PLC.Abstractions.Devices;
using Dreamine.PLC.Omron.Fins.Protocol;

namespace Dreamine.PLC.Omron.Fins.Devices;

/// <summary>
/// Maps Dreamine PLC device types to Omron FINS memory area codes.
/// </summary>
public static class OmronFinsMemoryAreaMapper
{
    /// <summary>
    /// Maps a PLC address to a FINS memory area code.
    /// </summary>
    /// <param name="address">The PLC address.</param>
<<<<<<< HEAD
    /// <param name="bitAccess">Whether bit access is requested.</param>
    /// <returns>The FINS memory area code.</returns>
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
    /// Maps a FINS memory area code to a Dreamine PLC device type.
    /// </summary>
    /// <param name="areaCode">The FINS memory area code.</param>
    /// <returns>The PLC device type.</returns>
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
    /// Returns whether the area code represents bit access.
    /// </summary>
    /// <param name="areaCode">The FINS memory area code.</param>
    /// <returns><c>true</c> if the area code represents bit access; otherwise, <c>false</c>.</returns>
    public static bool IsBitArea(byte areaCode)
    {
        return areaCode is OmronFinsMemoryAreaCode.CioBit
            or OmronFinsMemoryAreaCode.WorkBit
            or OmronFinsMemoryAreaCode.HoldingBit
            or OmronFinsMemoryAreaCode.DmBit;
    }
=======
    /// <param name="bitAccess">Whether the operation is bit-level access.</param>
    /// <returns>The mapped FINS memory area code.</returns>
    /// <exception cref="NotSupportedException">Thrown when the device type is not supported.</exception>
    public static OmronFinsMemoryAreaCode Map(PlcAddress address, bool bitAccess)
    {
        return address.DeviceType switch
        {
            PlcDeviceType.D => bitAccess ? OmronFinsMemoryAreaCode.DataMemoryBit : OmronFinsMemoryAreaCode.DataMemoryWord,
            PlcDeviceType.M => bitAccess ? OmronFinsMemoryAreaCode.CioBit : OmronFinsMemoryAreaCode.CioWord,
            PlcDeviceType.W => bitAccess ? OmronFinsMemoryAreaCode.WorkBit : OmronFinsMemoryAreaCode.WorkWord,
            PlcDeviceType.R => bitAccess ? OmronFinsMemoryAreaCode.HoldingBit : OmronFinsMemoryAreaCode.HoldingWord,
            _ => throw new NotSupportedException($"Unsupported FINS device type: {address.DeviceType}.")
        };
    }
>>>>>>> main
}
