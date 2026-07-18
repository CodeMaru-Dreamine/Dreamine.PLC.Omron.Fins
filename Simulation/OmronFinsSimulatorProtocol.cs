using Dreamine.PLC.Abstractions.Devices;
using Dreamine.PLC.Abstractions.Results;
using Dreamine.PLC.Core.Memory;
using Dreamine.PLC.Omron.Fins.Devices;
using Dreamine.PLC.Omron.Fins.Protocol;

namespace Dreamine.PLC.Omron.Fins.Simulation;

/// <summary>
/// \if KO
/// <para>메모리 읽기·쓰기 테스트를 위한 최소 Omron FINS 시뮬레이터 프로토콜을 실행합니다.</para>
/// \endif
/// \if EN
/// <para>Executes a minimal Omron FINS simulator protocol for memory read and write tests.</para>
/// \endif
/// </summary>
public sealed class OmronFinsSimulatorProtocol
{
    /// <summary>
    /// \if KO
    /// <para>memory 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the memory value.</para>
    /// \endif
    /// </summary>
    private readonly InMemoryPlcMemory _memory;
    /// <summary>
    /// \if KO
    /// <para>options 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the options value.</para>
    /// \endif
    /// </summary>
    private readonly OmronFinsSimulatorServerOptions _options;

    /// <summary>
    /// \if KO
    /// <para><see cref="T:Dreamine.PLC.Omron.Fins.Simulation.OmronFinsSimulatorProtocol" /> 클래스의 새 인스턴스를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes a new instance of <see cref="T:Dreamine.PLC.Omron.Fins.Simulation.OmronFinsSimulatorProtocol" />.</para>
    /// \endif
    /// </summary>
    /// <param name="memory">
    /// \if KO
    /// <para>요청 간에 공유할 PLC 메모리입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The PLC memory shared across requests.</para>
    /// \endif
    /// </param>
    /// <param name="options">
    /// \if KO
    /// <para>자동 응답 동작을 포함하는 시뮬레이터 옵션입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The simulator options, including automatic-response behavior.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="memory"/> 또는 <paramref name="options"/>가 <see langword="null"/>일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="memory"/> or <paramref name="options"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    public OmronFinsSimulatorProtocol(InMemoryPlcMemory memory, OmronFinsSimulatorServerOptions options)
    {
        _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// \if KO
    /// <para>시뮬레이터 상태 또는 자동 응답 정보가 변경될 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Occurs when simulator status or automatic-response information changes.</para>
    /// \endif
    /// </summary>
    public event EventHandler<string>? StatusChanged;

    /// <summary>
    /// \if KO
    /// <para>원시 FINS 요청 프레임을 처리하고 대응하는 원시 응답 프레임을 반환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Handles a raw FINS request frame and returns the corresponding raw response frame.</para>
    /// \endif
    /// </summary>
    /// <param name="requestFrame">
    /// \if KO
    /// <para>처리할 원시 FINS 요청 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The raw FINS request frame to process.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>성공 페이로드 또는 FINS 종료 코드를 포함하는 원시 응답 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A raw response frame containing a success payload or FINS end code.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="requestFrame"/>이 <see langword="null"/>일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="requestFrame"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
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

    /// <summary>
    /// \if KO
    /// <para>구문 분석된 메모리 영역 읽기 요청을 공유 메모리에서 실행합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Executes a parsed memory-area read request against shared memory.</para>
    /// \endif
    /// </summary>
    /// <param name="requestFrame">
    /// \if KO
    /// <para>응답 헤더 생성에 사용할 원본 요청 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The original request frame used to construct the response header.</para>
    /// \endif
    /// </param>
    /// <param name="request">
    /// \if KO
    /// <para>구문 분석된 읽기 요청입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The parsed read request.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>비트 또는 워드 페이로드를 포함하는 FINS 응답입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A FINS response containing a bit or word payload.</para>
    /// \endif
    /// </returns>
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

    /// <summary>
    /// \if KO
    /// <para>구문 분석된 메모리 영역 쓰기 요청을 공유 메모리에 실행합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Executes a parsed memory-area write request against shared memory.</para>
    /// \endif
    /// </summary>
    /// <param name="requestFrame">
    /// \if KO
    /// <para>응답 헤더 생성에 사용할 원본 요청 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The original request frame used to construct the response header.</para>
    /// \endif
    /// </param>
    /// <param name="request">
    /// \if KO
    /// <para>구문 분석된 쓰기 요청입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The parsed write request.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>성공 또는 유효성·메모리 오류 종료 코드를 포함하는 FINS 응답입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A FINS response containing success or a validation or memory-error end code.</para>
    /// \endif
    /// </returns>
    /// <exception cref="OverflowException">
    /// \if KO
    /// <para>워드 개수에 2를 곱한 예상 데이터 길이가 정수 범위를 벗어날 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when doubling the word count for the expected data length exceeds the integer range.</para>
    /// \endif
    /// </exception>
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

    /// <summary>
    /// \if KO
    /// <para>구성된 트리거 주소의 단일 워드 쓰기에 자동 응답 값을 적용합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Applies an automatic response value for a single-word write to the configured trigger address.</para>
    /// \endif
    /// </summary>
    /// <param name="writeAddress">
    /// \if KO
    /// <para>원래 쓰기 대상 주소입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The original write destination address.</para>
    /// \endif
    /// </param>
    /// <param name="values">
    /// \if KO
    /// <para>기록된 워드 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The word values that were written.</para>
    /// \endif
    /// </param>
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

    /// <summary>
    /// \if KO
    /// <para>원시 FINS 요청 프레임의 명령·영역·주소·개수·데이터를 구문 분석합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Parses the command, area, address, count, and data from a raw FINS request frame.</para>
    /// \endif
    /// </summary>
    /// <param name="frame">
    /// \if KO
    /// <para>구문 분석할 요청 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The request frame to parse.</para>
    /// \endif
    /// </param>
    /// <param name="request">
    /// \if KO
    /// <para>성공 시 구문 분석된 요청을 받습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Receives the parsed request on success.</para>
    /// \endif
    /// </param>
    /// <param name="errorMessage">
    /// \if KO
    /// <para>실패 시 진단 메시지를 받습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Receives a diagnostic message on failure.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>프레임이 유효하고 지원되는 메모리 영역을 사용하면 <see langword="true"/>입니다.</para>
    /// \endif
    /// \if EN
    /// <para><see langword="true"/> when the frame is valid and uses a supported memory area.</para>
    /// \endif
    /// </returns>
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

    /// <summary>
    /// \if KO
    /// <para>요청 프레임을 기반으로 지정한 종료 코드의 오류 응답을 생성합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds an error response with the specified end code from a request frame.</para>
    /// \endif
    /// </summary>
    /// <param name="requestFrame">
    /// \if KO
    /// <para>응답 주소와 명령을 가져올 요청 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The request frame from which response addresses and command are derived.</para>
    /// \endif
    /// </param>
    /// <param name="endCode">
    /// \if KO
    /// <para>응답에 기록할 FINS 종료 코드입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The FINS end code to write to the response.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>페이로드가 없는 FINS 오류 응답입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A FINS error response without a payload.</para>
    /// \endif
    /// </returns>
    private static byte[] BuildErrorResponse(byte[] requestFrame, ushort endCode)
    {
        var command = requestFrame.Length >= 12 ? OmronFinsEndian.ReadUInt16(requestFrame, 10) : (ushort)0;
        return BuildResponse(requestFrame, command, endCode, []);
    }

    /// <summary>
    /// \if KO
    /// <para>요청의 주소를 반전하고 명령·종료 코드·페이로드를 포함하는 FINS 응답을 생성합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds a FINS response by reversing request addresses and appending the command, end code, and payload.</para>
    /// \endif
    /// </summary>
    /// <param name="requestFrame">
    /// \if KO
    /// <para>헤더 값을 가져올 원본 요청 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The original request frame from which header values are derived.</para>
    /// \endif
    /// </param>
    /// <param name="command">
    /// \if KO
    /// <para>응답에 에코할 명령 코드입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The command code to echo in the response.</para>
    /// \endif
    /// </param>
    /// <param name="endCode">
    /// \if KO
    /// <para>FINS 종료 코드입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The FINS end code.</para>
    /// \endif
    /// </param>
    /// <param name="payload">
    /// \if KO
    /// <para>응답 뒤에 추가할 페이로드입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The payload to append to the response.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>생성된 원시 FINS 응답 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The generated raw FINS response frame.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="requestFrame"/> 또는 <paramref name="payload"/>가 <see langword="null"/>일 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when <paramref name="requestFrame"/> or <paramref name="payload"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
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

    /// <summary>
    /// \if KO
    /// <para>구문 분석된 FINS 메모리 요청을 나타냅니다.</para>
    /// \endif
    /// \if EN
    /// <para>Represents a parsed FINS memory request.</para>
    /// \endif
    /// </summary>
    /// <param name="Command">
    /// \if KO
    /// <para>FINS 명령 코드입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The FINS command code.</para>
    /// \endif
    /// </param>
    /// <param name="AreaCode">
    /// \if KO
    /// <para>메모리 영역 코드입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The memory-area code.</para>
    /// \endif
    /// </param>
    /// <param name="Address">
    /// \if KO
    /// <para>변환된 PLC 주소입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The mapped PLC address.</para>
    /// \endif
    /// </param>
    /// <param name="Count">
    /// \if KO
    /// <para>처리할 포인트 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The number of points to process.</para>
    /// \endif
    /// </param>
    /// <param name="Data">
    /// \if KO
    /// <para>요청 데이터 페이로드입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The request data payload.</para>
    /// \endif
    /// </param>
    private readonly record struct FinsRequest(
        ushort Command,
        byte AreaCode,
        PlcAddress Address,
        int Count,
        byte[] Data)
    {
        /// <summary>
        /// \if KO
        /// <para>요청 영역이 비트 접근을 나타내는지 여부를 가져옵니다.</para>
        /// \endif
        /// \if EN
        /// <para>Gets whether the requested area represents bit access.</para>
        /// \endif
        /// </summary>
        public bool IsBitAccess => OmronFinsMemoryAreaMapper.IsBitArea(AreaCode);
    }
}
