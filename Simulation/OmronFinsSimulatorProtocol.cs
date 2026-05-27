using Dreamine.PLC.Abstractions.Devices;
using Dreamine.PLC.Abstractions.Results;
using Dreamine.PLC.Core.Memory;
using Dreamine.PLC.Omron.Fins.Devices;
using Dreamine.PLC.Omron.Fins.Protocol;

namespace Dreamine.PLC.Omron.Fins.Simulation;

/// <summary>
/// Executes a minimal Omron FINS simulator protocol for memory read/write tests.
/// </summary>
public sealed class OmronFinsSimulatorProtocol
{
    private readonly InMemoryPlcMemory _memory;
    private readonly OmronFinsSimulatorServerOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="OmronFinsSimulatorProtocol"/> class.
    /// </summary>
    /// <param name="memory">The shared PLC memory.</param>
    /// <param name="options">The simulator options.</param>
    public OmronFinsSimulatorProtocol(InMemoryPlcMemory memory, OmronFinsSimulatorServerOptions options)
    {
        _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Occurs when the simulator status changes.
    /// </summary>
    public event EventHandler<string>? StatusChanged;

    /// <summary>
    /// Handles a raw FINS request frame and returns a raw FINS response frame.
    /// </summary>
    /// <param name="requestFrame">The raw FINS request frame.</param>
    /// <returns>The raw FINS response frame.</returns>
    public byte[] HandleRequest(IReadOnlyList<byte> requestFrame)
    {
        ArgumentNullException.ThrowIfNull(requestFrame);
        var request = requestFrame as byte[] ?? requestFrame.ToArray();

        if (!TryParseRequest(request, out var parsed, out var errorMessage))
        {
            StatusChanged?.Invoke(this, errorMessage ?? "Invalid FINS request.");
            return BuildErrorResponse(request, 0x1101);
        }

        return parsed.Command switch
        {
            (ushort)OmronFinsCommand.MemoryAreaRead => HandleRead(request, parsed),
            (ushort)OmronFinsCommand.MemoryAreaWrite => HandleWrite(request, parsed),
            _ => BuildErrorResponse(request, 0x0101)
        };
    }

    private byte[] HandleRead(byte[] requestFrame, FinsRequest request)
    {
        if (request.IsBitAccess)
        {
            var readResult = _memory.ReadBits(request.Address, request.Count);
            if (!readResult.IsSuccess || readResult.Value is null)
            {
                return BuildErrorResponse(requestFrame, 0x2101);
            }

            var payload = readResult.Value.Select(value => value ? (byte)1 : (byte)0).ToArray();
            return BuildResponse(requestFrame, request.Command, 0, payload);
        }

        var wordResult = _memory.ReadWords(request.Address, request.Count);
        if (!wordResult.IsSuccess || wordResult.Value is null)
        {
            return BuildErrorResponse(requestFrame, 0x2101);
        }

        var wordPayload = new byte[wordResult.Value.Length * 2];
        for (var index = 0; index < wordResult.Value.Length; index++)
        {
            OmronFinsEndian.WriteInt16(wordPayload, index * 2, wordResult.Value[index]);
        }

        return BuildResponse(requestFrame, request.Command, 0, wordPayload);
    }

    private byte[] HandleWrite(byte[] requestFrame, FinsRequest request)
    {
        if (request.IsBitAccess)
        {
            if (request.Data.Length < request.Count)
            {
                return BuildErrorResponse(requestFrame, 0x1101);
            }

            var values = new bool[request.Count];
            for (var index = 0; index < request.Count; index++)
            {
                values[index] = request.Data[index] != 0;
            }

            var writeResult = _memory.WriteBits(request.Address, values);
            if (!writeResult.IsSuccess)
            {
                return BuildErrorResponse(requestFrame, 0x2101);
            }
        }
        else
        {
            var expectedLength = checked(request.Count * 2);
            if (request.Data.Length < expectedLength)
            {
                return BuildErrorResponse(requestFrame, 0x1101);
            }

            var values = new short[request.Count];
            for (var index = 0; index < request.Count; index++)
            {
                values[index] = OmronFinsEndian.ReadInt16(request.Data, index * 2);
            }

            var writeResult = _memory.WriteWords(request.Address, values);
            if (!writeResult.IsSuccess)
            {
                return BuildErrorResponse(requestFrame, 0x2101);
            }

            ApplyAutoWordResponse(request.Address, values);
        }

        return BuildResponse(requestFrame, request.Command, 0, []);
    }

    private void ApplyAutoWordResponse(PlcAddress writeAddress, IReadOnlyList<short> values)
    {
        if (!_options.EnableAutoWordResponse || values.Count != 1)
        {
            return;
        }

        if (writeAddress.DeviceType != PlcDeviceType.D || writeAddress.Offset != _options.AutoResponseTriggerOffset)
        {
            return;
        }

        var rawResponseValue = values[0] + _options.AutoResponseIncrement;
        if (rawResponseValue is < short.MinValue or > short.MaxValue)
        {
            StatusChanged?.Invoke(this, $"FINS auto response skipped: value overflow. value={rawResponseValue}");
            return;
        }

        var responseValue = (short)rawResponseValue;
        var responseAddress = new PlcAddress(PlcDeviceType.D, _options.AutoResponseOffset);
        _memory.WriteWords(responseAddress, [responseValue]);
        StatusChanged?.Invoke(this, $"FINS auto response: D{_options.AutoResponseOffset}={responseValue}");
    }

    private static bool TryParseRequest(byte[] frame, out FinsRequest request, out string? errorMessage)
    {
        request = default;
        errorMessage = null;

        if (frame.Length < 18)
        {
            errorMessage = $"The FINS request frame is too short. Length={frame.Length}.";
            return false;
        }

        var command = OmronFinsEndian.ReadUInt16(frame, 10);
        var areaCode = frame[12];
        var offset = OmronFinsEndian.ReadUInt16(frame, 13);
        var bitOffset = frame[15];
        var count = OmronFinsEndian.ReadUInt16(frame, 16);
        var data = frame.Length > 18 ? frame[18..] : [];
        var deviceType = OmronFinsMemoryAreaMapper.ToDeviceType(areaCode);

        if (deviceType == PlcDeviceType.Unknown)
        {
            errorMessage = $"Unsupported FINS memory area code: 0x{areaCode:X2}.";
            return false;
        }

        request = new FinsRequest(
            command,
            areaCode,
            new PlcAddress(deviceType, offset, OmronFinsMemoryAreaMapper.IsBitArea(areaCode) ? bitOffset : null),
            count,
            data);
        return true;
    }

    private static byte[] BuildErrorResponse(byte[] requestFrame, ushort endCode)
    {
        var command = requestFrame.Length >= 12 ? OmronFinsEndian.ReadUInt16(requestFrame, 10) : (ushort)0;
        return BuildResponse(requestFrame, command, endCode, []);
    }

    private static byte[] BuildResponse(byte[] requestFrame, ushort command, ushort endCode, IReadOnlyList<byte> payload)
    {
        var response = new byte[14 + payload.Count];
        response[0] = requestFrame.Length > 0 ? requestFrame[0] : (byte)0xC0;
        response[1] = requestFrame.Length > 1 ? requestFrame[1] : (byte)0x00;
        response[2] = requestFrame.Length > 2 ? requestFrame[2] : (byte)0x02;

        response[3] = requestFrame.Length > 6 ? requestFrame[6] : (byte)0x00;
        response[4] = requestFrame.Length > 7 ? requestFrame[7] : (byte)0x00;
        response[5] = requestFrame.Length > 8 ? requestFrame[8] : (byte)0x00;
        response[6] = requestFrame.Length > 3 ? requestFrame[3] : (byte)0x00;
        response[7] = requestFrame.Length > 4 ? requestFrame[4] : (byte)0x00;
        response[8] = requestFrame.Length > 5 ? requestFrame[5] : (byte)0x00;
        response[9] = requestFrame.Length > 9 ? requestFrame[9] : (byte)0x00;

        OmronFinsEndian.WriteUInt16(response, 10, command);
        OmronFinsEndian.WriteUInt16(response, 12, endCode);

        for (var index = 0; index < payload.Count; index++)
        {
            response[14 + index] = payload[index];
        }

        return response;
    }

    private readonly record struct FinsRequest(
        ushort Command,
        byte AreaCode,
        PlcAddress Address,
        int Count,
        byte[] Data)
    {
        public bool IsBitAccess => OmronFinsMemoryAreaMapper.IsBitArea(AreaCode);
    }
}
