<<<<<<< HEAD
using Dreamine.PLC.Abstractions.Results;

namespace Dreamine.PLC.Omron.Fins.Transport;

/// <summary>
/// Defines the transport boundary for Omron FINS communication.
=======
namespace Dreamine.PLC.Omron.Fins.Transport;

/// <summary>
/// Defines transport-level send/receive behavior for Omron FINS communication.
>>>>>>> main
/// </summary>
public interface IOmronFinsTransport : IAsyncDisposable
{
    /// <summary>
<<<<<<< HEAD
    /// Gets whether the transport is logically connected or ready.
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// Connects or opens the transport.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The PLC operation result.</returns>
    Task<PlcResult> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Disconnects or closes the transport.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The PLC operation result.</returns>
    Task<PlcResult> DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends a raw FINS frame and receives a raw FINS response frame.
    /// </summary>
    /// <param name="requestFrame">The raw FINS request frame.</param>
    /// <param name="receiveTimeoutMs">The receive timeout in milliseconds.</param>
    /// <param name="retryCount">The retry count.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The raw FINS response frame.</returns>
    Task<PlcResult<byte[]>> SendAndReceiveAsync(
        IReadOnlyList<byte> requestFrame,
        int receiveTimeoutMs,
        int retryCount,
        CancellationToken cancellationToken = default);
=======
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
>>>>>>> main
}
