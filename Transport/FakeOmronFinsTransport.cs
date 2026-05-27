using Dreamine.PLC.Abstractions.Results;

namespace Dreamine.PLC.Omron.Fins.Transport;

/// <summary>
/// Provides a test transport for Omron FINS adapter unit tests.
/// </summary>
public sealed class FakeOmronFinsTransport : IOmronFinsTransport
{
    private readonly Queue<byte[]> _responses = new();

    /// <inheritdoc />
    public bool IsConnected { get; private set; }

    /// <summary>
    /// Gets the sent request frames.
    /// </summary>
    public List<byte[]> SentRequests { get; } = [];

    /// <summary>
    /// Adds a response frame to be returned by the next request.
    /// </summary>
    /// <param name="response">The response frame.</param>
    public void EnqueueResponse(byte[] response)
    {
        _responses.Enqueue(response);
    }

    /// <inheritdoc />
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
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        IsConnected = false;
        return ValueTask.CompletedTask;
    }
}
