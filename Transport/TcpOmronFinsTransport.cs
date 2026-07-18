using System.Net.Sockets;
using Dreamine.PLC.Abstractions.Results;
using Dreamine.PLC.Omron.Fins.Options;
using Dreamine.PLC.Omron.Fins.Protocol;

namespace Dreamine.PLC.Omron.Fins.Transport;

/// <summary>
/// \if KO
/// <para>Omron FINS 통신을 위한 TCP 전송을 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides TCP transport for Omron FINS communication.</para>
/// \endif
/// </summary>
public sealed class TcpOmronFinsTransport : IOmronFinsTransport
{
    /// <summary>
    /// \if KO
    /// <para>sync Lock 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the sync lock value.</para>
    /// \endif
    /// </summary>
    private readonly SemaphoreSlim _syncLock = new(1, 1);
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
    /// <para>tcp Client 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the tcp client value.</para>
    /// \endif
    /// </summary>
    private TcpClient? _tcpClient;
    /// <summary>
    /// \if KO
    /// <para>stream 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the stream value.</para>
    /// \endif
    /// </summary>
    private NetworkStream? _stream;
    /// <summary>
    /// \if KO
    /// <para>disposed 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the disposed value.</para>
    /// \endif
    /// </summary>
    private bool _disposed;

    /// <summary>
    /// \if KO
    /// <para><see cref="T:Dreamine.PLC.Omron.Fins.Transport.TcpOmronFinsTransport" /> 클래스의 새 인스턴스를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes a new instance of <see cref="T:Dreamine.PLC.Omron.Fins.Transport.TcpOmronFinsTransport" />.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>대상과 제한 시간을 지정하는 FINS 연결 옵션입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The FINS connection options specifying the endpoint and timeouts.</para>
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
    public TcpOmronFinsTransport(OmronFinsConnectionOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// \if KO
    /// <para>TCP 클라이언트가 연결되고 네트워크 스트림이 준비되었는지 여부를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets whether the TCP client is connected and its network stream is ready.</para>
    /// \endif
    /// </summary>
    public bool IsConnected => _tcpClient?.Connected == true && _stream is not null;

    /// <summary>
    /// \if KO
    /// <para>구성된 제한 시간 안에 FINS TCP 대상에 연결합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Connects to the configured FINS TCP endpoint within the configured timeout.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>연결 및 잠금 대기를 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels connection and lock acquisition.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>연결 성공 또는 포착된 네트워크·제한 시간 오류를 포함하는 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing success or a captured network or timeout error.</para>
    /// \endif
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>동기화 잠금을 기다리는 동안 취소되면 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when cancellation occurs while waiting for the synchronization lock.</para>
    /// \endif
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// \if KO
    /// <para>전송 또는 동기화 잠금이 이미 정리된 경우 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when the transport or synchronization lock has already been disposed.</para>
    /// \endif
    /// </exception>
    public async Task<PlcResult> ConnectAsync(CancellationToken cancellationToken = default)
    {
        await _syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            if (IsConnected)
            {
                return PlcResult.Success();
            }

            await CloseCoreAsync().ConfigureAwait(false);
            _tcpClient = new TcpClient { NoDelay = true };

            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(_options.ConnectTimeoutMs);
            await _tcpClient.ConnectAsync(_options.Host, _options.Port, timeoutCts.Token).ConfigureAwait(false);
            _stream = _tcpClient.GetStream();
            return PlcResult.Success();
        }
        catch (OperationCanceledException ex)
        {
            await CloseCoreAsync().ConfigureAwait(false);
            return PlcResult.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            await CloseCoreAsync().ConfigureAwait(false);
            return PlcResult.Failure(ex.Message);
        }
        finally
        {
            _syncLock.Release();
        }
    }

    /// <summary>
    /// \if KO
    /// <para>네트워크 스트림과 TCP 클라이언트를 닫아 FINS 연결을 해제합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Disconnects FINS by closing the network stream and TCP client.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>잠금 대기를 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels lock acquisition.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>연결 해제 성공 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing a successful disconnection result.</para>
    /// \endif
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>동기화 잠금을 기다리는 동안 취소되면 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when cancellation occurs while waiting for the synchronization lock.</para>
    /// \endif
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// \if KO
    /// <para>동기화 잠금이 이미 정리된 경우 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when the synchronization lock has already been disposed.</para>
    /// \endif
    /// </exception>
    public async Task<PlcResult> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        await _syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await CloseCoreAsync().ConfigureAwait(false);
            return PlcResult.Success();
        }
        finally
        {
            _syncLock.Release();
        }
    }

    /// <summary>
    /// \if KO
    /// <para>원시 FINS 프레임을 TCP 패킷으로 전송하고 완전한 응답 패킷을 수신합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Sends a raw FINS frame as a TCP packet and receives one complete response packet.</para>
    /// \endif
    /// </summary>
    /// <param name="requestFrame">
    /// \if KO
    /// <para>전송할 원시 FINS 요청 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The raw FINS request frame to send.</para>
    /// \endif
    /// </param>
    /// <param name="receiveTimeoutMs">
    /// \if KO
    /// <para>밀리초 단위 송수신 제한 시간입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The send-and-receive timeout in milliseconds.</para>
    /// \endif
    /// </param>
    /// <param name="retryCount">
    /// \if KO
    /// <para>인터페이스 호환성을 위한 재시도 횟수이며 TCP 구현에서는 사용하지 않습니다.</para>
    /// \endif
    /// \if EN
    /// <para>A retry count retained for interface compatibility; this TCP implementation does not use it.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>잠금과 송수신을 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels lock acquisition and I/O.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>원시 FINS 응답 프레임 또는 연결·전송·프로토콜 오류를 포함하는 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing the raw FINS response frame or a connection, transport, or protocol error.</para>
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
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>동기화 잠금을 기다리는 동안 취소되면 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when cancellation occurs while waiting for the synchronization lock.</para>
    /// \endif
    /// </exception>
    /// <exception cref="ObjectDisposedException">
    /// \if KO
    /// <para>전송 또는 동기화 잠금이 이미 정리된 경우 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when the transport or synchronization lock has already been disposed.</para>
    /// \endif
    /// </exception>
    public async Task<PlcResult<byte[]>> SendAndReceiveAsync(
        IReadOnlyList<byte> requestFrame,
        int receiveTimeoutMs,
        int retryCount,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(requestFrame);

        await _syncLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            ThrowIfDisposed();
            if (_stream is null || _tcpClient is null || !_tcpClient.Connected)
            {
                return PlcResult<byte[]>.Failure("The FINS TCP transport is not connected.");
            }

            var packet = OmronFinsTcpPacket.Wrap(requestFrame);
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(receiveTimeoutMs);

            await _stream.WriteAsync(packet, timeoutCts.Token).ConfigureAwait(false);
            await _stream.FlushAsync(timeoutCts.Token).ConfigureAwait(false);

            var responsePacket = await ReceiveFinsTcpPacketAsync(_stream, timeoutCts.Token).ConfigureAwait(false);
            return PlcResult<byte[]>.Success(OmronFinsTcpPacket.Extract(responsePacket));
        }
        catch (OperationCanceledException ex)
        {
            return PlcResult<byte[]>.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            return PlcResult<byte[]>.Failure(ex.Message);
        }
        finally
        {
            _syncLock.Release();
        }
    }

    /// <summary>
    /// \if KO
    /// <para>연결을 닫고 동기화 자원을 비동기로 정리합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Closes the connection and asynchronously disposes synchronization resources.</para>
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
    /// <exception cref="ObjectDisposedException">
    /// \if KO
    /// <para>동시 정리로 동기화 잠금이 이미 정리된 경우 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when concurrent disposal has already disposed the synchronization lock.</para>
    /// \endif
    /// </exception>
    public async ValueTask DisposeAsync()
    {
        if (_disposed)
        {
            return;
        }

        await DisconnectAsync().ConfigureAwait(false);
        _syncLock.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
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
    /// <para>읽을 연결된 네트워크 스트림입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The connected network stream to read.</para>
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
    /// <para>완전한 패킷을 받기 전에 원격 끝점이 연결을 닫으면 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the remote endpoint closes before a complete packet is received.</para>
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

        var bodyLength = length - 8;
        var body = new byte[bodyLength];
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
    /// <para>버퍼를 채우기 전에 원격 끝점이 연결을 닫으면 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the remote endpoint closes before the buffer is filled.</para>
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
                throw new IOException("The remote FINS endpoint closed the TCP connection.");
            }

            offset += read;
        }
    }

    /// <summary>
    /// \if KO
    /// <para>현재 네트워크 스트림과 TCP 클라이언트를 닫고 참조를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Closes the current network stream and TCP client and clears their references.</para>
    /// \endif
    /// </summary>
    /// <returns>
    /// \if KO
    /// <para>닫기 작업을 나타내는 완료 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A completed task representing the close operation.</para>
    /// \endif
    /// </returns>
    private async Task CloseCoreAsync()
    {
        _stream?.Dispose();
        _tcpClient?.Dispose();
        _stream = null;
        _tcpClient = null;
        await Task.CompletedTask.ConfigureAwait(false);
    }

    /// <summary>
    /// \if KO
    /// <para>이 전송 인스턴스가 아직 정리되지 않았는지 확인합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Verifies that this transport instance has not been disposed.</para>
    /// \endif
    /// </summary>
    /// <exception cref="ObjectDisposedException">
    /// \if KO
    /// <para>인스턴스가 이미 정리된 경우 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the instance has already been disposed.</para>
    /// \endif
    /// </exception>
    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
    }
}
