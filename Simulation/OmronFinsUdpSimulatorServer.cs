using System.Net;
using System.Net.Sockets;
using Dreamine.PLC.Core.Memory;

namespace Dreamine.PLC.Omron.Fins.Simulation;

/// <summary>
/// Provides a minimal FINS/UDP simulator server for local and cross-PC tests.
/// </summary>
public sealed class OmronFinsUdpSimulatorServer : IAsyncDisposable
{
    private readonly OmronFinsSimulatorServerOptions _options;
    private readonly InMemoryPlcMemory _memory;
    private readonly OmronFinsSimulatorProtocol _protocol;
    private UdpClient? _udpClient;
    private CancellationTokenSource? _cts;
    private Task? _receiveTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="OmronFinsUdpSimulatorServer"/> class.
    /// </summary>
    /// <param name="options">The server options.</param>
    public OmronFinsUdpSimulatorServer(OmronFinsSimulatorServerOptions options)
        : this(options, new InMemoryPlcMemory())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OmronFinsUdpSimulatorServer"/> class.
    /// </summary>
    /// <param name="options">The server options.</param>
    /// <param name="memory">The shared PLC memory.</param>
    public OmronFinsUdpSimulatorServer(OmronFinsSimulatorServerOptions options, InMemoryPlcMemory memory)
    {
        _options = options ?? throw new ArgumentNullException(nameof(options));
        _memory = memory ?? throw new ArgumentNullException(nameof(memory));
        _protocol = new OmronFinsSimulatorProtocol(_memory, _options);
        _protocol.StatusChanged += (_, message) => StatusChanged?.Invoke(this, message);
    }

    /// <summary>
    /// Occurs when the server status changes.
    /// </summary>
    public event EventHandler<string>? StatusChanged;

    /// <summary>
    /// Gets whether the server is running.
    /// </summary>
    public bool IsRunning => _udpClient is not null;

    /// <summary>
    /// Starts the server.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
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
    /// Stops the server.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation.</returns>
    public async Task StopAsync()
    {
        if (!IsRunning)
        {
            return;
        }

        _cts?.Cancel();
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

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await StopAsync().ConfigureAwait(false);
    }

    private static IPAddress ParseAddress(string host)
    {
        if (string.IsNullOrWhiteSpace(host) || host == "0.0.0.0" || host == "*" || host == "+")
        {
            return IPAddress.Any;
        }

        return IPAddress.TryParse(host, out var address) ? address : IPAddress.Any;
    }

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
