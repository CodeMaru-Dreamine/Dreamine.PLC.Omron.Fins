using System.Net;
using System.Net.Sockets;
<<<<<<< HEAD
using Dreamine.PLC.Abstractions.Results;
=======
>>>>>>> main
using Dreamine.PLC.Omron.Fins.Options;

namespace Dreamine.PLC.Omron.Fins.Transport;

/// <summary>
/// Provides UDP transport for Omron FINS communication.
/// </summary>
public sealed class UdpOmronFinsTransport : IOmronFinsTransport
{
<<<<<<< HEAD
    private readonly SemaphoreSlim _syncLock = new(1, 1);
    private readonly OmronFinsConnectionOptions _options;
    private UdpClient? _udpClient;
    private IPEndPoint? _remoteEndPoint;
    private bool _disposed;
=======
    private readonly OmronFinsConnectionOptions _options;
    private UdpClient? _client;
    private IPEndPoint? _endpoint;
>>>>>>> main

    /// <summary>
    /// Initializes a new instance of the <see cref="UdpOmronFinsTransport"/> class.
    /// </summary>
<<<<<<< HEAD
    /// <param name="options">The FINS connection options.</param>
=======
    /// <param name="options">The connection options.</param>
>>>>>>> main
    public UdpOmronFinsTransport(OmronFinsConnectionOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
<<<<<<< HEAD
    public bool IsConnected => _udpClient is not null && _remoteEndPoint is not null;

    /// <inheritdoc />
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

    /// <inheritdoc />
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

    /// <inheritdoc />
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
=======
    public bool IsReady => _client is not null && _endpoint is not null;

    /// <inheritdoc />
    public Task OpenAsync(CancellationToken cancellationToken)
    {
        _endpoint = new IPEndPoint(IPAddress.Parse(_options.Host), _options.Port);
        _client = new UdpClient();
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task CloseAsync(CancellationToken cancellationToken)
    {
        _client?.Dispose();
        _client = null;
        _endpoint = null;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<byte[]> SendAndReceiveAsync(byte[] request, CancellationToken cancellationToken)
    {
        if (_client is null || _endpoint is null)
        {
            throw new InvalidOperationException("FINS UDP transport is not open.");
        }

        Exception? lastException = null;
        var retryCount = Math.Max(1, _options.RetryCount);

        for (var attempt = 0; attempt < retryCount; attempt++)
        {
            try
            {
                await _client.SendAsync(request, request.Length, _endpoint).ConfigureAwait(false);

                using var timeout = new CancellationTokenSource(_options.TimeoutMs);
                using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
                var result = await _client.ReceiveAsync(linked.Token).ConfigureAwait(false);
                return result.Buffer;
            }
            catch (Exception ex) when (ex is OperationCanceledException or SocketException)
            {
                lastException = ex;
            }
        }

        throw new TimeoutException("FINS UDP request timed out.", lastException);
>>>>>>> main
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
<<<<<<< HEAD
        if (_disposed)
        {
            return;
        }

        await DisconnectAsync().ConfigureAwait(false);
        _syncLock.Dispose();
        _disposed = true;
        GC.SuppressFinalize(this);
    }

    private void CloseCore()
    {
        _udpClient?.Dispose();
        _udpClient = null;
        _remoteEndPoint = null;
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
=======
        await CloseAsync(CancellationToken.None).ConfigureAwait(false);
>>>>>>> main
    }
}
