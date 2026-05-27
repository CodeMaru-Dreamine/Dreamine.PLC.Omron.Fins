using System.Net;
using System.Net.Sockets;
using Dreamine.PLC.Core.Memory;
using Dreamine.PLC.Omron.Fins.Protocol;

namespace Dreamine.PLC.Omron.Fins.Simulation;

/// <summary>
/// Provides a minimal FINS/TCP simulator server for local and cross-PC tests.
/// </summary>
public sealed class OmronFinsTcpSimulatorServer : IAsyncDisposable
{
    private readonly OmronFinsSimulatorServerOptions _options;
    private readonly InMemoryPlcMemory _memory;
    private readonly OmronFinsSimulatorProtocol _protocol;
    private readonly List<TcpClient> _clients = [];
    private readonly object _syncRoot = new();
    private TcpListener? _listener;
    private CancellationTokenSource? _cts;
    private Task? _acceptTask;

    /// <summary>
    /// Initializes a new instance of the <see cref="OmronFinsTcpSimulatorServer"/> class.
    /// </summary>
    /// <param name="options">The server options.</param>
    public OmronFinsTcpSimulatorServer(OmronFinsSimulatorServerOptions options)
        : this(options, new InMemoryPlcMemory())
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="OmronFinsTcpSimulatorServer"/> class.
    /// </summary>
    /// <param name="options">The server options.</param>
    /// <param name="memory">The shared PLC memory.</param>
    public OmronFinsTcpSimulatorServer(OmronFinsSimulatorServerOptions options, InMemoryPlcMemory memory)
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
    public bool IsRunning => _listener is not null;

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
        _listener = new TcpListener(address, _options.Port);
        _listener.Start();
        _acceptTask = AcceptLoopAsync(_cts.Token);
        StatusChanged?.Invoke(this, $"FINS TCP simulator listening on {_options.Host}:{_options.Port}.");
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
        _listener?.Stop();
        _listener = null;

        lock (_syncRoot)
        {
            foreach (var client in _clients)
            {
                client.Dispose();
            }

            _clients.Clear();
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

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested && _listener is not null)
        {
            var client = await _listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
            lock (_syncRoot)
            {
                _clients.Add(client);
            }

            _ = Task.Run(() => ClientLoopAsync(client, cancellationToken), cancellationToken);
        }
    }

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
