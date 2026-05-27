using System.Net.Sockets;
using Dreamine.PLC.Omron.Fins.Options;

namespace Dreamine.PLC.Omron.Fins.Transport;

/// <summary>
/// Provides TCP transport boundary for Omron FINS communication.
/// </summary>
public sealed class TcpOmronFinsTransport : IOmronFinsTransport
{
    private readonly OmronFinsConnectionOptions _options;
    private TcpClient? _client;
    private NetworkStream? _stream;

    /// <summary>
    /// Initializes a new instance of the <see cref="TcpOmronFinsTransport"/> class.
    /// </summary>
    /// <param name="options">The connection options.</param>
    public TcpOmronFinsTransport(OmronFinsConnectionOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
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
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await CloseAsync(CancellationToken.None).ConfigureAwait(false);
    }
}
