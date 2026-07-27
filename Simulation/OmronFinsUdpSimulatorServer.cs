using System.Net;
using System.Net.Sockets;
using Dreamine.PLC.Core.Memory;

namespace Dreamine.PLC.Omron.Fins.Simulation;

/// <summary>
/// \if KO
/// <para>로컬 및 PC 간 테스트를 위한 최소 FINS/UDP 시뮬레이터 서버를 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides a minimal FINS/UDP simulator server for local and cross-PC tests.</para>
/// \endif
/// </summary>
public sealed class OmronFinsUdpSimulatorServer : IAsyncDisposable
{
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
    /// <para>memory 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the memory value.</para>
    /// \endif
    /// </summary>
    private readonly InMemoryPlcMemory _memory;
    /// <summary>
    /// \if KO
    /// <para>protocol 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the protocol value.</para>
    /// \endif
    /// </summary>
    private readonly OmronFinsSimulatorProtocol _protocol;
    /// <summary>
    /// \if KO
    /// <para>udp Client 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the udp client value.</para>
    /// \endif
    /// </summary>
    private UdpClient? _udpClient;
    /// <summary>
    /// \if KO
    /// <para>cts 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the cts value.</para>
    /// \endif
    /// </summary>
    private CancellationTokenSource? _cts;
    /// <summary>
    /// \if KO
    /// <para>receive Task 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the receive task value.</para>
    /// \endif
    /// </summary>
    private Task? _receiveTask;

    /// <summary>
    /// \if KO
    /// <para>새 메모리를 사용해 <see cref="T:Dreamine.PLC.Omron.Fins.Simulation.OmronFinsUdpSimulatorServer" /> 클래스의 새 인스턴스를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes a new instance of <see cref="T:Dreamine.PLC.Omron.Fins.Simulation.OmronFinsUdpSimulatorServer" /> using new PLC memory.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>바인딩 및 자동 응답 서버 옵션입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The server binding and automatic-response options.</para>
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
    public OmronFinsUdpSimulatorServer(OmronFinsSimulatorServerOptions options)
        : this(options, new InMemoryPlcMemory())
    {
    }

    /// <summary>
    /// \if KO
    /// <para>지정한 공유 메모리를 사용해 <see cref="T:Dreamine.PLC.Omron.Fins.Simulation.OmronFinsUdpSimulatorServer" /> 클래스의 새 인스턴스를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes a new instance of <see cref="T:Dreamine.PLC.Omron.Fins.Simulation.OmronFinsUdpSimulatorServer" /> using the specified shared memory.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>바인딩 및 자동 응답 서버 옵션입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The server binding and automatic-response options.</para>
    /// \endif
    /// </param>
    /// <param name="memory">
    /// \if KO
    /// <para>요청 간에 공유할 PLC 메모리입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The PLC memory shared across requests.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="options"/> 또는 <paramref name="memory"/>가 <see langword="null"/>일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="options"/> or <paramref name="memory"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    public OmronFinsUdpSimulatorServer(OmronFinsSimulatorServerOptions options, InMemoryPlcMemory memory)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        _protocol = new OmronFinsSimulatorProtocol(_memory, _options);
        _protocol.StatusChanged += (_, message) => StatusChanged?.Invoke(this, message);
    }

    /// <summary>
    /// \if KO
    /// <para>서버 상태 또는 프로토콜 상태가 변경될 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Occurs when server or protocol status changes.</para>
    /// \endif
    /// </summary>
    public event EventHandler<string>? StatusChanged;

    /// <summary>
    /// \if KO
    /// <para>UDP 소켓이 생성되어 서버가 실행 중인지 여부를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets whether the UDP socket exists and the server is running.</para>
    /// \endif
    /// </summary>
    public bool IsRunning => _udpClient is not null;

    /// <summary>
    /// \if KO
    /// <para>구성된 주소와 포트에 UDP 소켓을 바인딩하고 수신 루프를 시작합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Binds a UDP socket to the configured address and port and starts the receive loop.</para>
    /// \endif
    /// </summary>
    /// <returns>
    /// \if KO
    /// <para>서버가 이미 실행 중이거나 시작이 완료되면 완료되는 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task completed when the server is already running or startup is complete.</para>
    /// \endif
    /// </returns>
    /// <exception cref="SocketException">
    /// \if KO
    /// <para>주소 또는 포트에 UDP 소켓을 바인딩할 수 없을 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when the UDP socket cannot bind to the address or port.</para>
    /// \endif
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// \if KO
    /// <para>구성된 포트가 유효한 범위를 벗어날 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when the configured port is outside the valid range.</para>
    /// \endif
    /// </exception>
    public Task StartAsync()
    {
        if (IsRunning)
        {
            return Task.CompletedTask;
        }

        var address = ParseAddress(_options.Host);

        _cts = new CancellationTokenSource();
        _udpClient = new UdpClient(new IPEndPoint(address, _options.Port));
        _receiveTask = ReceiveLoopAsync(_cts.Token);
        StatusChanged?.Invoke(this, $"FINS UDP simulator listening on {_options.Host}:{_options.Port}.");
        return Task.CompletedTask;
    }

    /// <summary>
    /// \if KO
    /// <para>UDP 소켓과 수신 루프를 중지하고 서버 자원을 정리합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stops the UDP socket and receive loop and releases server resources.</para>
    /// \endif
    /// </summary>
    /// <returns>
    /// \if KO
    /// <para>수신 루프의 종료와 자원 정리를 나타내는 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task representing receive-loop termination and resource cleanup.</para>
    /// \endif
    /// </returns>
    public async Task StopAsync()
    {
        if (!IsRunning)
        {
            return;
        }

        if (_cts is not null)
            await _cts.CancelAsync().ConfigureAwait(false);
        _udpClient?.Dispose();
        _udpClient = null;

        if (_receiveTask is not null)
        {
            try
            {
                await _receiveTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException)
            {
            }
        }

        _cts?.Dispose();
        _cts = null;
        _receiveTask = null;
        StatusChanged?.Invoke(this, "FINS UDP simulator stopped.");
    }

    /// <summary>
    /// \if KO
    /// <para>서버를 중지해 비동기로 정리합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Asynchronously disposes the server by stopping it.</para>
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
    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
    }

    /// <summary>
    /// \if KO
    /// <para>구성된 바인딩 호스트를 IP 주소로 변환하며 와일드카드와 잘못된 값은 모든 인터페이스로 처리합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Converts a configured binding host to an IP address, treating wildcards and invalid values as all interfaces.</para>
    /// \endif
    /// </summary>
    /// <param name="host">
    /// \if KO
    /// <para>변환할 호스트 또는 와일드카드 문자열입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The host or wildcard string to convert.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>구문 분석된 주소 또는 <see cref="IPAddress.Any"/>입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The parsed address or <see cref="IPAddress.Any"/>.</para>
    /// \endif
    /// </returns>
    private static IPAddress ParseAddress(string host)
    {
        if (string.IsNullOrWhiteSpace(host) || host == "0.0.0.0" || host == "*" || host == "+")
        {
            return IPAddress.Any;
        }

        return IPAddress.TryParse(host, out var address) ? address : IPAddress.Any;
    }

    /// <summary>
    /// \if KO
    /// <para>UDP 요청을 수신해 프로토콜로 처리하고 원격 끝점에 응답하는 루프를 실행합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Runs the loop that receives UDP requests, processes them through the protocol, and replies to each remote endpoint.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>수신 루프를 중지하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that stops the receive loop.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>수신 루프 수명 동안 실행되는 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task that runs for the lifetime of the receive loop.</para>
    /// \endif
    /// </returns>
    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _udpClient is not null)
        {
            try
            {
                var request = await _udpClient.ReceiveAsync(cancellationToken).ConfigureAwait(false);
                var response = _protocol.HandleRequest(request.Buffer);
                await _udpClient.SendAsync(response, response.Length, request.RemoteEndPoint).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (SocketException ex)
            {
                StatusChanged?.Invoke(this, $"FINS UDP socket error: {ex.Message}");
            }
            catch (Exception ex)
            {
                StatusChanged?.Invoke(this, $"FINS UDP server error: {ex.Message}");
            }
        }
    }
}
