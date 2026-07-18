using System.Net;
using System.Net.Sockets;
using Dreamine.PLC.Abstractions.Results;
using Dreamine.PLC.Omron.Fins.Options;

namespace Dreamine.PLC.Omron.Fins.Transport;

/// <summary>
/// \if KO
/// <para>Omron FINS 통신을 위한 UDP 전송을 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides UDP transport for Omron FINS communication.</para>
/// \endif
/// </summary>
public sealed class UdpOmronFinsTransport : IOmronFinsTransport
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
    /// <para>udp Client 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the udp client value.</para>
    /// \endif
    /// </summary>
    private UdpClient? _udpClient;
    /// <summary>
    /// \if KO
    /// <para>remote End Point 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the remote end point value.</para>
    /// \endif
    /// </summary>
    private IPEndPoint? _remoteEndPoint;
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
    /// <para><see cref="T:Dreamine.PLC.Omron.Fins.Transport.UdpOmronFinsTransport" /> 클래스의 새 인스턴스를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Initializes a new instance of <see cref="T:Dreamine.PLC.Omron.Fins.Transport.UdpOmronFinsTransport" />.</para>
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
    public UdpOmronFinsTransport(OmronFinsConnectionOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// \if KO
    /// <para>UDP 클라이언트와 원격 끝점이 준비되었는지 여부를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets whether the UDP client and remote endpoint are ready.</para>
    /// \endif
    /// </summary>
    public bool IsConnected => _udpClient is not null && _remoteEndPoint is not null;

    /// <summary>
    /// \if KO
    /// <para>대상 호스트를 확인하고 연결된 UDP 클라이언트를 준비합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Resolves the target host and prepares a connected UDP client.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>DNS 확인과 잠금 대기를 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels DNS resolution and lock acquisition.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>성공 또는 유효성·DNS·소켓 오류를 포함하는 연결 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing success or a validation, DNS, or socket error.</para>
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

            if (_options.Port is <= 0 or > 65535)
            {
                return PlcResult.Failure($"Invalid FINS UDP port: {_options.Port}.");
            }

            var addresses = await Dns.GetHostAddressesAsync(_options.Host, cancellationToken).ConfigureAwait(false);
            if (addresses.Length == 0)
            {
                return PlcResult.Failure($"Failed to resolve FINS host: {_options.Host}.");
            }

            _remoteEndPoint = new IPEndPoint(addresses[0], _options.Port);
            _udpClient = new UdpClient();
            _udpClient.Connect(_remoteEndPoint);
            return PlcResult.Success();
        }
        catch (OperationCanceledException ex)
        {
            CloseCore();
            return PlcResult.Failure(ex.Message);
        }
        catch (Exception ex)
        {
            CloseCore();
            return PlcResult.Failure(ex.Message);
        }
        finally
        {
            _syncLock.Release();
        }
    }

    /// <summary>
    /// \if KO
    /// <para>UDP 클라이언트를 닫고 원격 끝점 정보를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Closes the UDP client and clears the remote endpoint information.</para>
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
            CloseCore();
            return PlcResult.Success();
        }
        finally
        {
            _syncLock.Release();
        }
    }

    /// <summary>
    /// \if KO
    /// <para>원시 FINS UDP 프레임을 보내고 구성된 횟수만큼 응답 수신을 재시도합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Sends a raw FINS UDP frame and retries response reception for the configured number of attempts.</para>
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
    /// <para>각 시도의 밀리초 단위 수신 제한 시간입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The receive timeout in milliseconds for each attempt.</para>
    /// \endif
    /// </param>
    /// <param name="retryCount">
    /// \if KO
    /// <para>총 시도 횟수이며 1보다 작으면 한 번 시도합니다.</para>
    /// \endif
    /// \if EN
    /// <para>The total attempt count; values below one result in one attempt.</para>
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
    /// <para>수신된 원시 FINS 응답 또는 마지막 전송 오류를 포함하는 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing the received raw FINS response or the last transport error.</para>
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
    /// <remarks>
    /// \if KO
    /// <para>송수신 중 발생한 취소도 재시도 대상으로 포착되며 모든 시도가 끝나면 실패 결과로 반환됩니다.</para>
    /// \endif
    /// \if EN
    /// <para>Cancellation raised during I/O is captured as an attempt failure and returned as a failure result after all attempts.</para>
    /// \endif
    /// </remarks>
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
            if (_udpClient is null)
            {
                return PlcResult<byte[]>.Failure("The FINS UDP transport is not connected.");
            }

            var request = requestFrame as byte[] ?? requestFrame.ToArray();
            var attempts = Math.Max(1, retryCount);
            Exception? lastException = null;

            for (var attempt = 0; attempt < attempts; attempt++)
            {
                try
                {
                    using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
                    timeoutCts.CancelAfter(receiveTimeoutMs);

                    await _udpClient.SendAsync(request, timeoutCts.Token).ConfigureAwait(false);
                    var result = await _udpClient.ReceiveAsync(timeoutCts.Token).ConfigureAwait(false);
                    return PlcResult<byte[]>.Success(result.Buffer);
                }
                catch (OperationCanceledException ex)
                {
                    lastException = ex;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                }
            }

            return PlcResult<byte[]>.Failure(lastException?.Message ?? "FINS UDP receive timeout.");
        }
        finally
        {
            _syncLock.Release();
        }
    }

    /// <summary>
    /// \if KO
    /// <para>UDP 연결을 닫고 동기화 자원을 비동기로 정리합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Closes the UDP connection and asynchronously disposes synchronization resources.</para>
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
    /// <para>현재 UDP 클라이언트를 닫고 연결 상태를 초기화합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Closes the current UDP client and clears the connection state.</para>
    /// \endif
    /// </summary>
    private void CloseCore()
    {
        _udpClient?.Dispose();
        _udpClient = null;
        _remoteEndPoint = null;
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
