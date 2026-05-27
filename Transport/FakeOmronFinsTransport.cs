namespace Dreamine.PLC.Omron.Fins.Transport;

/// <summary>
/// Provides a test transport for Omron FINS adapter unit tests.
/// </summary>
public sealed class FakeOmronFinsTransport : IOmronFinsTransport
{
    private readonly Queue<byte[]> _responses = new();

    /// <inheritdoc />
    public bool IsReady { get; private set; }

    /// <summary>
    /// Gets the sent request frames.
    /// </summary>
    public List<byte[]> SentRequests { get; } = new();

    /// <summary>
    /// Adds a response frame to be returned by the next request.
    /// </summary>
    /// <param name="response">The response frame.</param>
    public void EnqueueResponse(byte[] response)
    {
        _responses.Enqueue(response);
    }

    /// <inheritdoc />
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
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        IsReady = false;
        return ValueTask.CompletedTask;
    }
}
