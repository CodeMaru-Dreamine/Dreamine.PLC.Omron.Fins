namespace Dreamine.PLC.Omron.Fins.Transport;

/// <summary>
/// Defines transport-level send/receive behavior for Omron FINS communication.
/// </summary>
public interface IOmronFinsTransport : IAsyncDisposable
{
    /// <summary>
    /// Gets whether the transport is ready to send requests.
    /// </summary>
    bool IsReady { get; }

    /// <summary>
    /// Opens the transport.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task OpenAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Closes the transport.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A task that represents the asynchronous operation.</returns>
    Task CloseAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Sends a request frame and receives a response frame.
    /// </summary>
    /// <param name="request">The request frame.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The response frame.</returns>
    Task<byte[]> SendAndReceiveAsync(byte[] request, CancellationToken cancellationToken);
}
