<<<<<<< HEAD
using Dreamine.PLC.Abstractions.Results;

=======
>>>>>>> main
namespace Dreamine.PLC.Omron.Fins.Transport;

/// <summary>
/// Provides a test transport for Omron FINS adapter unit tests.
/// </summary>
public sealed class FakeOmronFinsTransport : IOmronFinsTransport
{
    private readonly Queue<byte[]> _responses = new();

    /// <inheritdoc />
<<<<<<< HEAD
    public bool IsConnected { get; private set; }
=======
    public bool IsReady { get; private set; }
>>>>>>> main

    /// <summary>
    /// Gets the sent request frames.
    /// </summary>
<<<<<<< HEAD
    public List<byte[]> SentRequests { get; } = [];
=======
    public List<byte[]> SentRequests { get; } = new();
>>>>>>> main

    /// <summary>
    /// Adds a response frame to be returned by the next request.
    /// </summary>
    /// <param name="response">The response frame.</param>
    public void EnqueueResponse(byte[] response)
    {
        _responses.Enqueue(response);
    }

    /// <inheritdoc />
<<<<<<< HEAD
    public Task<PlcResult> ConnectAsync(CancellationToken cancellationToken = default)
    {
        IsConnected = true;
        return Task.FromResult(PlcResult.Success());
    }

    /// <inheritdoc />
    public Task<PlcResult> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        IsConnected = false;
        return Task.FromResult(PlcResult.Success());
    }

    /// <inheritdoc />
    public Task<PlcResult<byte[]>> SendAndReceiveAsync(
        IReadOnlyList<byte> requestFrame,
        int receiveTimeoutMs,
        int retryCount,
        CancellationToken cancellationToken = default)
    {
        SentRequests.Add(requestFrame.ToArray());
        if (_responses.Count == 0)
        {
            return Task.FromResult(PlcResult<byte[]>.Failure("No fake FINS response has been queued."));
        }

        return Task.FromResult(PlcResult<byte[]>.Success(_responses.Dequeue()));
=======
    public Task OpenAsync(CancellationToken cancellationToken)
    {
        IsReady = true;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task CloseAsync(CancellationToken cancellationToken)
    {
        IsReady = false;
        return Task.CompletedTask;
    }

    /// <inheritdoc />
    public Task<byte[]> SendAndReceiveAsync(byte[] request, CancellationToken cancellationToken)
    {
        SentRequests.Add(request.ToArray());
        if (_responses.Count == 0)
        {
            throw new InvalidOperationException("No fake FINS response has been queued.");
        }

        return Task.FromResult(_responses.Dequeue());
>>>>>>> main
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
<<<<<<< HEAD
        IsConnected = false;
=======
        IsReady = false;
>>>>>>> main
        return ValueTask.CompletedTask;
    }
}
