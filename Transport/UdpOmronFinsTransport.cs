using System.Net;
using System.Net.Sockets;
using Dreamine.PLC.Omron.Fins.Options;

namespace Dreamine.PLC.Omron.Fins.Transport;

/// <summary>
/// Provides UDP transport for Omron FINS communication.
/// </summary>
public sealed class UdpOmronFinsTransport : IOmronFinsTransport
{
    private readonly OmronFinsConnectionOptions _options;
    private UdpClient? _client;
    private IPEndPoint? _endpoint;

    /// <summary>
    /// Initializes a new instance of the <see cref="UdpOmronFinsTransport"/> class.
    /// </summary>
    /// <param name="options">The connection options.</param>
    public UdpOmronFinsTransport(OmronFinsConnectionOptions options)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc />
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
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await CloseAsync(CancellationToken.None).ConfigureAwait(false);
    }
}
