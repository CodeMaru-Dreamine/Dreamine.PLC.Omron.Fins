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
}
