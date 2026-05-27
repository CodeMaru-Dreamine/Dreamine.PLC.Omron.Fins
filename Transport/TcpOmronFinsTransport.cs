using System.Net.Sockets;
<<<<<<< HEAD
using Dreamine.PLC.Abstractions.Results;
using Dreamine.PLC.Omron.Fins.Options;
using Dreamine.PLC.Omron.Fins.Protocol;
=======
using Dreamine.PLC.Omron.Fins.Options;
>>>>>>> main

namespace Dreamine.PLC.Omron.Fins.Transport;

/// <summary>
<<<<<<< HEAD
/// Provides TCP transport for Omron FINS communication.
/// </summary>
public sealed class TcpOmronFinsTransport : IOmronFinsTransport
{
    private readonly SemaphoreSlim _syncLock = new(1, 1);
    private readonly OmronFinsConnectionOptions _options;
    private TcpClient? _tcpClient;
    private NetworkStream? _stream;
    private bool _disposed;
=======
/// Provides TCP transport boundary for Omron FINS communication.
/// </summary>
public sealed class TcpOmronFinsTransport : IOmronFinsTransport
{
    private readonly OmronFinsConnectionOptions _options;
    private TcpClient? _client;
    private NetworkStream? _stream;
>>>>>>> main

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpOmronFinsTransport"/> class.
    /// </summary>
<<<<<<< HEAD
    /// <param name="options">The FINS connection options.</param>
=======
    /// <param name="options">The connection options.</param>
>>>>>>> main
    public TcpOmronFinsTransport(OmronFinsConnectionOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
<<<<<<< HEAD
    public bool IsConnected => _tcpClient?.Connected == true && _stream is not null;

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

    /// <inheritdoc />
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
=======
    public bool IsReady => _client?.Connected == true && _stream is not null;

    /// <inheritdoc />
    public async Task OpenAsync(CancellationToken cancellationToken)
    {
        _client = new TcpClient();
        using var timeout = new CancellationTokenSource(_options.TimeoutMs);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        await _client.ConnectAsync(_options.Host, _options.Port, linked.Token).ConfigureAwait(false);
        _stream = _client.GetStream();
    }

    /// <inheritdoc />
    public Task CloseAsync(CancellationToken cancellationToken)
    {
        _stream?.Dispose();
        _client?.Dispose();
        _stream = null;
        _client = null;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public async Task<byte[]> SendAndReceiveAsync(byte[] request, CancellationToken cancellationToken)
    {
        if (_stream is null)
        {
            throw new InvalidOperationException("FINS TCP transport is not open.");
        }

        await _stream.WriteAsync(request, cancellationToken).ConfigureAwait(false);
        await _stream.FlushAsync(cancellationToken).ConfigureAwait(false);

        var buffer = new byte[4096];
        using var timeout = new CancellationTokenSource(_options.TimeoutMs);
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeout.Token);
        var length = await _stream.ReadAsync(buffer, linked.Token).ConfigureAwait(false);
        return buffer.Take(length).ToArray();
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

    private async Task CloseCoreAsync()
    {
        _stream?.Dispose();
        _tcpClient?.Dispose();
        _stream = null;
        _tcpClient = null;
        await Task.CompletedTask.ConfigureAwait(false);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
=======
        await CloseAsync(CancellationToken.None).ConfigureAwait(false);
>>>>>>> main
    }
}
