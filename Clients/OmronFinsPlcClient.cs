using Dreamine.PLC.Abstractions.Devices;
using Dreamine.PLC.Abstractions.Results;
using Dreamine.PLC.Core.Clients;
using Dreamine.PLC.Omron.Fins.Options;
using Dreamine.PLC.Omron.Fins.Protocol;
using Dreamine.PLC.Omron.Fins.Transport;

namespace Dreamine.PLC.Omron.Fins.Clients;

/// <summary>
/// \if KO
/// <para>Dreamine PLC 스택을 위한 Omron FINS PLC 클라이언트 구현을 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides an Omron FINS PLC client implementation for the Dreamine PLC stack.</para>
/// \endif
/// </summary>
public sealed class OmronFinsPlcClient : PlcClientBase
{
    /// <summary>
    /// \if KO
    /// <para>options 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the options value.</para>
    /// \endif
    /// </summary>
    private readonly OmronFinsConnectionOptions _options;
    /// <summary>
    /// \if KO
    /// <para>transport 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the transport value.</para>
    /// \endif
    /// </summary>
    private readonly IOmronFinsTransport _transport;
    /// <summary>
    /// \if KO
    /// <para>frame Builder 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the frame builder value.</para>
    /// \endif
    /// </summary>
    private readonly OmronFinsFrameBuilder _frameBuilder;
    /// <summary>
    /// \if KO
    /// <para>response Parser 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the response parser value.</para>
    /// \endif
    /// </summary>
    private readonly OmronFinsResponseParser _responseParser;

    /// <summary>
    /// \if KO
    /// <para>옵션에 맞는 기본 전송을 사용해 <see cref="T:Dreamine.PLC.Omron.Fins.Clients.OmronFinsPlcClient" /> 클래스의 새 인스턴스를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes a new instance of <see cref="T:Dreamine.PLC.Omron.Fins.Clients.OmronFinsPlcClient" /> using the default transport selected by the options.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>전송 형식과 네트워크 설정을 포함하는 FINS 연결 옵션입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The FINS connection options containing transport and network settings.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="options"/>가 <see langword="null"/>일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="options"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// \if KO
    /// <para>지원되지 않는 전송 형식이 지정된 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when an unsupported transport type is configured.</para>
    /// \endif
    /// </exception>
    public OmronFinsPlcClient(OmronFinsConnectionOptions options)
        : this(options, CreateTransport(options), new OmronFinsFrameBuilder(), new OmronFinsResponseParser())
    {
    }

    /// <summary>
    /// \if KO
    /// <para>제공된 구성 요소를 사용해 <see cref="T:Dreamine.PLC.Omron.Fins.Clients.OmronFinsPlcClient" /> 클래스의 새 인스턴스를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes a new instance of <see cref="T:Dreamine.PLC.Omron.Fins.Clients.OmronFinsPlcClient" /> using the supplied components.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>FINS 연결 옵션입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The FINS connection options.</para>
    /// \endif
    /// </param>
    /// <param name="transport">
    /// \if KO
    /// <para>원시 프레임을 송수신할 FINS 전송입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The FINS transport used to send and receive raw frames.</para>
    /// \endif
    /// </param>
    /// <param name="frameBuilder">
    /// \if KO
    /// <para>메모리 명령 프레임 생성기입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The memory-command frame builder.</para>
    /// \endif
    /// </param>
    /// <param name="responseParser">
    /// \if KO
    /// <para>응답 페이로드 구문 분석기입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The response-payload parser.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para>인수 중 하나가 <see langword="null"/>일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when any argument is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    public OmronFinsPlcClient(
        OmronFinsConnectionOptions options,
        IOmronFinsTransport transport,
        OmronFinsFrameBuilder frameBuilder,
        OmronFinsResponseParser responseParser)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _transport = transport ?? throw new ArgumentNullException(nameof(transport));
        _frameBuilder = frameBuilder ?? throw new ArgumentNullException(nameof(frameBuilder));
        _responseParser = responseParser ?? throw new ArgumentNullException(nameof(responseParser));
    }

    /// <summary>
    /// \if KO
    /// <para>이 클라이언트가 사용하는 Omron FINS 연결 옵션을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the Omron FINS connection options used by this client.</para>
    /// \endif
    /// </summary>
    public OmronFinsConnectionOptions Options => _options;

    /// <summary>
    /// \if KO
    /// <para>구성된 FINS 전송을 연결합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Connects the configured FINS transport.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>연결 및 전송 잠금 대기를 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels connection and transport-lock acquisition.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>전송 연결 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing the transport connection result.</para>
    /// \endif
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>전송 잠금 대기 중 취소되면 구현체에서 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown by the transport when cancellation occurs during lock acquisition.</para>
    /// \endif
    /// </exception>
    protected override Task<PlcResult> ConnectCoreAsync(CancellationToken cancellationToken)
    {
        return _transport.ConnectAsync(cancellationToken);
    }

    /// <summary>
    /// \if KO
    /// <para>구성된 FINS 전송의 연결을 해제합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Disconnects the configured FINS transport.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>전송 잠금 대기를 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels transport-lock acquisition.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>전송 연결 해제 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing the transport disconnection result.</para>
    /// \endif
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>전송 잠금 대기 중 취소되면 구현체에서 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown by the transport when cancellation occurs during lock acquisition.</para>
    /// \endif
    /// </exception>
    protected override Task<PlcResult> DisconnectCoreAsync(CancellationToken cancellationToken)
    {
        return _transport.DisconnectAsync(cancellationToken);
    }

    /// <summary>
    /// \if KO
    /// <para>FINS 메모리 영역에서 비트 값을 읽고 응답 페이로드를 구문 분석합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Reads bit values from a FINS memory area and parses the response payload.</para>
    /// \endif
    /// </summary>
    /// <param name="address">
    /// \if KO
    /// <para>읽기를 시작할 PLC 주소입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The PLC address at which to begin reading.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>읽을 비트 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The number of bits to read.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>전송 작업을 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels the transport operation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>비트 배열 또는 프레임·전송·응답 오류를 포함하는 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing the bit array or a frame, transport, or response error.</para>
    /// \endif
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>전송 잠금 대기 중 취소되면 구현체에서 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown by the transport when cancellation occurs during lock acquisition.</para>
    /// \endif
    /// </exception>
    protected override async Task<PlcResult<bool[]>> ReadBitsCoreAsync(
        PlcAddress address,
        int count,
        CancellationToken cancellationToken)
    {
        byte[] request;
        try
        {
            request = _frameBuilder.BuildMemoryAreaRead(_options, address, count, bitAccess: true);
        }
        catch (Exception ex)
        {
            return PlcResult<bool[]>.Failure(ex.Message);
        }

        var responseResult = await _transport.SendAndReceiveAsync(
            request,
            _options.ReceiveTimeoutMs,
            _options.RetryCount,
            cancellationToken).ConfigureAwait(false);

        if (!responseResult.IsSuccess || responseResult.Value is null)
        {
            return PlcResult<bool[]>.Failure(responseResult.Message ?? "Failed to receive FINS bit read response.", responseResult.ErrorCode);
        }

        var payloadResult = _responseParser.ExtractPayload(responseResult.Value);
        if (!payloadResult.IsSuccess || payloadResult.Value is null)
        {
            return PlcResult<bool[]>.Failure(payloadResult.Message ?? "Failed to parse FINS bit read payload.", payloadResult.ErrorCode);
        }

        return _responseParser.ParseBits(payloadResult.Value, count);
    }

    /// <summary>
    /// \if KO
    /// <para>FINS 메모리 영역에서 워드 값을 읽고 응답 페이로드를 구문 분석합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Reads word values from a FINS memory area and parses the response payload.</para>
    /// \endif
    /// </summary>
    /// <param name="address">
    /// \if KO
    /// <para>읽기를 시작할 PLC 주소입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The PLC address at which to begin reading.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>읽을 워드 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The number of words to read.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>전송 작업을 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels the transport operation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>워드 배열 또는 프레임·전송·응답 오류를 포함하는 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing the word array or a frame, transport, or response error.</para>
    /// \endif
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>전송 잠금 대기 중 취소되면 구현체에서 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown by the transport when cancellation occurs during lock acquisition.</para>
    /// \endif
    /// </exception>
    protected override async Task<PlcResult<short[]>> ReadWordsCoreAsync(
        PlcAddress address,
        int count,
        CancellationToken cancellationToken)
    {
        byte[] request;
        try
        {
            request = _frameBuilder.BuildMemoryAreaRead(_options, address, count, bitAccess: false);
        }
        catch (Exception ex)
        {
            return PlcResult<short[]>.Failure(ex.Message);
        }

        var responseResult = await _transport.SendAndReceiveAsync(
            request,
            _options.ReceiveTimeoutMs,
            _options.RetryCount,
            cancellationToken).ConfigureAwait(false);

        if (!responseResult.IsSuccess || responseResult.Value is null)
        {
            return PlcResult<short[]>.Failure(responseResult.Message ?? "Failed to receive FINS word read response.", responseResult.ErrorCode);
        }

        var payloadResult = _responseParser.ExtractPayload(responseResult.Value);
        if (!payloadResult.IsSuccess || payloadResult.Value is null)
        {
            return PlcResult<short[]>.Failure(payloadResult.Message ?? "Failed to parse FINS word read payload.", payloadResult.ErrorCode);
        }

        return _responseParser.ParseWords(payloadResult.Value, count);
    }

    /// <summary>
    /// \if KO
    /// <para>비트 값을 FINS 메모리 영역에 쓰고 응답 종료 코드를 확인합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes bit values to a FINS memory area and validates the response end code.</para>
    /// \endif
    /// </summary>
    /// <param name="address">
    /// \if KO
    /// <para>쓰기를 시작할 PLC 주소입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The PLC address at which to begin writing.</para>
    /// \endif
    /// </param>
    /// <param name="values">
    /// \if KO
    /// <para>쓸 비트 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The bit values to write.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>전송 작업을 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels the transport operation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>성공 또는 프레임·전송·응답 오류를 포함하는 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing success or a frame, transport, or response error.</para>
    /// \endif
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>전송 잠금 대기 중 취소되면 구현체에서 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown by the transport when cancellation occurs during lock acquisition.</para>
    /// \endif
    /// </exception>
    protected override async Task<PlcResult> WriteBitsCoreAsync(
        PlcAddress address,
        IReadOnlyList<bool> values,
        CancellationToken cancellationToken)
    {
        byte[] request;
        try
        {
            request = _frameBuilder.BuildMemoryAreaWriteBits(_options, address, values);
        }
        catch (Exception ex)
        {
            return PlcResult.Failure(ex.Message);
        }

        var responseResult = await _transport.SendAndReceiveAsync(
            request,
            _options.ReceiveTimeoutMs,
            _options.RetryCount,
            cancellationToken).ConfigureAwait(false);

        if (!responseResult.IsSuccess || responseResult.Value is null)
        {
            return PlcResult.Failure(responseResult.Message ?? "Failed to receive FINS bit write response.", responseResult.ErrorCode);
        }

        var payloadResult = _responseParser.ExtractPayload(responseResult.Value);
        return payloadResult.IsSuccess
            ? PlcResult.Success()
            : PlcResult.Failure(payloadResult.Message ?? "Failed to parse FINS bit write response.", payloadResult.ErrorCode);
    }

    /// <summary>
    /// \if KO
    /// <para>워드 값을 FINS 메모리 영역에 쓰고 응답 종료 코드를 확인합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Writes word values to a FINS memory area and validates the response end code.</para>
    /// \endif
    /// </summary>
    /// <param name="address">
    /// \if KO
    /// <para>쓰기를 시작할 PLC 주소입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The PLC address at which to begin writing.</para>
    /// \endif
    /// </param>
    /// <param name="values">
    /// \if KO
    /// <para>쓸 워드 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The word values to write.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>전송 작업을 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels the transport operation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>성공 또는 프레임·전송·응답 오류를 포함하는 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing success or a frame, transport, or response error.</para>
    /// \endif
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>전송 잠금 대기 중 취소되면 구현체에서 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown by the transport when cancellation occurs during lock acquisition.</para>
    /// \endif
    /// </exception>
    protected override async Task<PlcResult> WriteWordsCoreAsync(
        PlcAddress address,
        IReadOnlyList<short> values,
        CancellationToken cancellationToken)
    {
        byte[] request;
        try
        {
            request = _frameBuilder.BuildMemoryAreaWriteWords(_options, address, values);
        }
        catch (Exception ex)
        {
            return PlcResult.Failure(ex.Message);
        }

        var responseResult = await _transport.SendAndReceiveAsync(
            request,
            _options.ReceiveTimeoutMs,
            _options.RetryCount,
            cancellationToken).ConfigureAwait(false);

        if (!responseResult.IsSuccess || responseResult.Value is null)
        {
            return PlcResult.Failure(responseResult.Message ?? "Failed to receive FINS word write response.", responseResult.ErrorCode);
        }

        var payloadResult = _responseParser.ExtractPayload(responseResult.Value);
        return payloadResult.IsSuccess
            ? PlcResult.Success()
            : PlcResult.Failure(payloadResult.Message ?? "Failed to parse FINS word write response.", payloadResult.ErrorCode);
    }

    /// <summary>
    /// \if KO
    /// <para>기본 PLC 클라이언트와 소유한 FINS 전송을 비동기로 정리합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Asynchronously disposes the base PLC client and owned FINS transport.</para>
    /// \endif
    /// </summary>
    /// <returns>
    /// \if KO
    /// <para>비동기 정리 작업을 나타내는 값 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A value task representing asynchronous disposal.</para>
    /// \endif
    /// </returns>
    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync().ConfigureAwait(false);
        await _transport.DisposeAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// \if KO
    /// <para>연결 옵션에 지정된 형식에 맞는 기본 FINS 전송을 생성합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Creates the default FINS transport matching the type selected in the connection options.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>전송 형식과 네트워크 설정을 포함하는 연결 옵션입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The connection options containing transport and network settings.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>TCP 또는 UDP FINS 전송 인스턴스입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A TCP or UDP FINS transport instance.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="options"/>가 <see langword="null"/>일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="options"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// \if KO
    /// <para>지원되지 않는 전송 형식이 지정된 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when an unsupported transport type is configured.</para>
    /// \endif
    /// </exception>
    private static IOmronFinsTransport CreateTransport(OmronFinsConnectionOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        return options.TransportType switch
        {
            OmronFinsTransportType.Tcp => new TcpOmronFinsTransport(options),
            OmronFinsTransportType.Udp => new UdpOmronFinsTransport(options),
            _ => throw new ArgumentOutOfRangeException(nameof(options), options.TransportType, "Unsupported Omron FINS transport type.")
        };
    }
}
