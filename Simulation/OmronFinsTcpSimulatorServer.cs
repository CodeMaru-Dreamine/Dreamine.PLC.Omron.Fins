using System.Net;
using System.Net.Sockets;
using Dreamine.PLC.Core.Memory;
using Dreamine.PLC.Omron.Fins.Protocol;

namespace Dreamine.PLC.Omron.Fins.Simulation;

/// <summary>
/// \if KO
/// <para>로컬 및 PC 간 테스트를 위한 최소 FINS/TCP 시뮬레이터 서버를 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides a minimal FINS/TCP simulator server for local and cross-PC tests.</para>
/// \endif
/// </summary>
public sealed class OmronFinsTcpSimulatorServer : IAsyncDisposable
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
    /// <para>clients 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the clients value.</para>
    /// \endif
    /// </summary>
    private readonly List<TcpClient> _clients = [];
    /// <summary>
    /// \if KO
    /// <para>sync Root 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the sync root value.</para>
    /// \endif
    /// </summary>
    private readonly object _syncRoot = new();
    /// <summary>
    /// \if KO
    /// <para>listener 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the listener value.</para>
    /// \endif
    /// </summary>
    private TcpListener? _listener;
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
    /// <para>accept Task 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the accept task value.</para>
    /// \endif
    /// </summary>
    private Task? _acceptTask;

    /// <summary>
    /// \if KO
    /// <para>새 메모리를 사용해 <see cref="T:Dreamine.PLC.Omron.Fins.Simulation.OmronFinsTcpSimulatorServer" /> 클래스의 새 인스턴스를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes a new instance of <see cref="T:Dreamine.PLC.Omron.Fins.Simulation.OmronFinsTcpSimulatorServer" /> using new PLC memory.</para>
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
    public OmronFinsTcpSimulatorServer(OmronFinsSimulatorServerOptions options)
        : this(options, new InMemoryPlcMemory())
    {
    }

    /// <summary>
    /// \if KO
    /// <para>지정한 공유 메모리를 사용해 <see cref="T:Dreamine.PLC.Omron.Fins.Simulation.OmronFinsTcpSimulatorServer" /> 클래스의 새 인스턴스를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes a new instance of <see cref="T:Dreamine.PLC.Omron.Fins.Simulation.OmronFinsTcpSimulatorServer" /> using the specified shared memory.</para>
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
    public OmronFinsTcpSimulatorServer(OmronFinsSimulatorServerOptions options, InMemoryPlcMemory memory)
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
    /// <para>TCP 수신기가 생성되어 서버가 실행 중인지 여부를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets whether the TCP listener exists and the server is running.</para>
    /// \endif
    /// </summary>
    public bool IsRunning => _listener is not null;

    /// <summary>
    /// \if KO
    /// <para>구성된 주소와 포트에서 TCP 수신을 시작하고 연결 수락 루프를 실행합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Starts TCP listening at the configured address and port and runs the accept loop.</para>
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
    /// <para>주소 또는 포트에서 수신을 시작할 수 없을 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when listening cannot start at the address or port.</para>
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
        _listener = new TcpListener(address, _options.Port);
        _listener.Start();
        _acceptTask = AcceptLoopAsync(_cts.Token);
        StatusChanged?.Invoke(this, $"FINS TCP simulator listening on {_options.Host}:{_options.Port}.");
        return Task.CompletedTask;
    }

    /// <summary>
    /// \if KO
    /// <para>TCP 수신기와 모든 클라이언트를 닫고 수락 루프를 종료합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stops the TCP listener, closes all clients, and terminates the accept loop.</para>
    /// \endif
    /// </summary>
    /// <returns>
    /// \if KO
    /// <para>연결과 수락 루프의 종료 및 자원 정리를 나타내는 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task representing connection and accept-loop termination and cleanup.</para>
    /// \endif
    /// </returns>
    public async Task StopAsync()
    {
        if (!IsRunning)
        {
            return;
        }

        _cts?.Cancel();
        _listener?.Stop();
        _listener = null;

        TcpClient[] clients;
        lock (_syncRoot)
        {
            clients = _clients.ToArray();
            _clients.Clear();
        }

        foreach (var client in clients)
        {
            client.Dispose();
        }

        if (_acceptTask is not null)
        {
            try
            {
                await _acceptTask.ConfigureAwait(false);
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
        _acceptTask = null;
        StatusChanged?.Invoke(this, "FINS TCP simulator stopped.");
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
    /// <para>TCP 클라이언트 연결을 수락하고 각각의 처리 루프를 백그라운드에서 시작합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Accepts TCP client connections and starts a background processing loop for each client.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>연결 수락을 중지하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that stops accepting connections.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>수락 루프 수명 동안 실행되는 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task that runs for the lifetime of the accept loop.</para>
    /// \endif
    /// </returns>
    /// <exception cref="SocketException">
    /// \if KO
    /// <para>취소 이외의 이유로 클라이언트 수락이 실패할 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when accepting a client fails for a reason other than cancellation.</para>
    /// \endif
    /// </exception>
    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _listener is not null)
        {
            TcpClient client;

            try
            {
                client = await _listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (ObjectDisposedException)
            {
                break;
            }
            catch (SocketException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }

            lock (_syncRoot)
            {
                _clients.Add(client);
            }

            _ = Task.Run(() => ClientLoopAsync(client, cancellationToken), cancellationToken);
        }
    }

    /// <summary>
    /// \if KO
    /// <para>단일 TCP 클라이언트의 FINS/TCP 요청을 수신·처리·응답합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Receives, processes, and responds to FINS/TCP requests for one TCP client.</para>
    /// \endif
    /// </summary>
    /// <param name="client">
    /// \if KO
    /// <para>처리할 연결된 TCP 클라이언트입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The connected TCP client to process.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>클라이언트 루프를 중지하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that stops the client loop.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>클라이언트 연결 수명 동안 실행되는 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task that runs for the lifetime of the client connection.</para>
    /// \endif
    /// </returns>
    private async Task ClientLoopAsync(TcpClient client, CancellationToken cancellationToken)
    {
        try
        {
            using (client)
            {
                var stream = client.GetStream();
                while (!cancellationToken.IsCancellationRequested && client.Connected)
                {
                    var packet = await ReceiveFinsTcpPacketAsync(stream, cancellationToken).ConfigureAwait(false);
                    var request = OmronFinsTcpPacket.Extract(packet);
                    var response = _protocol.HandleRequest(request);
                    var responsePacket = OmronFinsTcpPacket.Wrap(response);
                    await stream.WriteAsync(responsePacket, cancellationToken).ConfigureAwait(false);
                    await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
                }
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (IOException)
        {
        }
        catch (SocketException)
        {
        }
        catch (ObjectDisposedException)
        {
        }
        catch (Exception ex)
        {
            StatusChanged?.Invoke(this, $"FINS TCP client error: {ex.Message}");
        }
        finally
        {
            lock (_syncRoot)
            {
                _clients.Remove(client);
            }
        }
    }

    /// <summary>
    /// \if KO
    /// <para>FINS/TCP 헤더와 선언된 본문 길이를 기준으로 패킷 하나를 정확히 수신합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Receives exactly one packet based on the FINS/TCP header and declared body length.</para>
    /// \endif
    /// </summary>
    /// <param name="stream">
    /// \if KO
    /// <para>읽을 클라이언트 네트워크 스트림입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The client network stream to read.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>수신을 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels receiving.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>헤더와 본문을 포함한 완전한 FINS/TCP 패킷입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The complete FINS/TCP packet containing header and body.</para>
    /// \endif
    /// </returns>
    /// <exception cref="InvalidOperationException">
    /// \if KO
    /// <para>헤더의 패킷 길이가 8보다 작을 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the packet length declared in the header is less than eight.</para>
    /// \endif
    /// </exception>
    /// <exception cref="IOException">
    /// \if KO
    /// <para>완전한 패킷을 받기 전에 클라이언트가 연결을 닫으면 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the client closes before a complete packet is received.</para>
    /// \endif
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>수신이 취소되면 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when receiving is canceled.</para>
    /// \endif
    /// </exception>
    private static async Task<byte[]> ReceiveFinsTcpPacketAsync(NetworkStream stream, CancellationToken cancellationToken)
    {
        var header = new byte[16];
        await ReadExactlyAsync(stream, header, cancellationToken).ConfigureAwait(false);
        var length = OmronFinsEndian.ReadInt32(header, 4);
        if (length < 8)
        {
            throw new InvalidOperationException($"Invalid FINS/TCP packet length: {length}.");
        }

        var body = new byte[length - 8];
        await ReadExactlyAsync(stream, body, cancellationToken).ConfigureAwait(false);
        var packet = new byte[header.Length + body.Length];
        Buffer.BlockCopy(header, 0, packet, 0, header.Length);
        Buffer.BlockCopy(body, 0, packet, header.Length, body.Length);
        return packet;
    }

    /// <summary>
    /// \if KO
    /// <para>네트워크 스트림에서 대상 버퍼를 끝까지 채웁니다.</para>
    /// \endif
    /// \if EN
    /// <para>Fills the destination buffer completely from the network stream.</para>
    /// \endif
    /// </summary>
    /// <param name="stream">
    /// \if KO
    /// <para>읽을 네트워크 스트림입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The network stream to read.</para>
    /// \endif
    /// </param>
    /// <param name="buffer">
    /// \if KO
    /// <para>완전히 채울 대상 버퍼입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The destination buffer to fill completely.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>읽기를 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels reading.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>비동기 읽기 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task representing the asynchronous read operation.</para>
    /// \endif
    /// </returns>
    /// <exception cref="IOException">
    /// \if KO
    /// <para>버퍼를 채우기 전에 클라이언트가 연결을 닫으면 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the client closes before the buffer is filled.</para>
    /// \endif
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>읽기가 취소되면 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when reading is canceled.</para>
    /// \endif
    /// </exception>
    private static async Task ReadExactlyAsync(NetworkStream stream, byte[] buffer, CancellationToken cancellationToken)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            var read = await stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset), cancellationToken).ConfigureAwait(false);
            if (read == 0)
            {
                throw new IOException("The FINS TCP client closed the connection.");
            }

            offset += read;
        }
    }
}
