using Dreamine.PLC.Abstractions.Results;

namespace Dreamine.PLC.Omron.Fins.Transport;

/// <summary>
/// \if KO
/// <para>Omron FINS 통신을 위한 전송 계층 계약을 정의합니다.</para>
/// \endif
/// \if EN
/// <para>Defines the transport-layer contract for Omron FINS communication.</para>
/// \endif
/// </summary>
public interface IOmronFinsTransport : IAsyncDisposable
{
    /// <summary>
    /// \if KO
    /// <para>전송 계층이 논리적으로 연결되었거나 통신 준비가 되었는지 여부를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets whether the transport is logically connected or ready for communication.</para>
    /// \endif
    /// </summary>
    bool IsConnected { get; }

    /// <summary>
    /// \if KO
    /// <para>전송 계층을 연결하거나 엽니다.</para>
    /// \endif
    /// \if EN
    /// <para>Connects or opens the transport.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>연결 작업을 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels the connection operation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>PLC 연결 작업 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The PLC connection operation result.</para>
    /// \endif
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>작업이 취소되면 구현체가 발생시킬 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown by an implementation when the operation is canceled.</para>
    /// \endif
    /// </exception>
    Task<PlcResult> ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// \if KO
    /// <para>전송 계층의 연결을 해제하거나 닫습니다.</para>
    /// \endif
    /// \if EN
    /// <para>Disconnects or closes the transport.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>연결 해제 작업을 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels the disconnection operation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>PLC 연결 해제 작업 결과입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The PLC disconnection operation result.</para>
    /// \endif
    /// </returns>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>작업이 취소되면 구현체가 발생시킬 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown by an implementation when the operation is canceled.</para>
    /// \endif
    /// </exception>
    Task<PlcResult> DisconnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// \if KO
    /// <para>원시 FINS 프레임을 전송하고 원시 응답 프레임을 수신합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Sends a raw FINS frame and receives a raw FINS response frame.</para>
    /// \endif
    /// </summary>
    /// <param name="requestFrame">
    /// \if KO
    /// <para>전송할 원시 FINS 요청 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The raw FINS request frame to send.</para>
    /// \endif
    /// </param>
    /// <param name="receiveTimeoutMs">
    /// \if KO
    /// <para>밀리초 단위 수신 제한 시간입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The receive timeout in milliseconds.</para>
    /// \endif
    /// </param>
    /// <param name="retryCount">
    /// \if KO
    /// <para>실패 후 재시도 횟수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The number of retries after a failed attempt.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>송수신 작업을 취소하는 토큰입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A token that cancels the send-and-receive operation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>원시 FINS 응답 프레임 또는 전송 오류를 포함하는 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing the raw FINS response frame or a transport error.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="requestFrame"/>이 <see langword="null"/>이면 구현체가 발생시킬 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown by an implementation when <paramref name="requestFrame"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    /// <exception cref="OperationCanceledException">
    /// \if KO
    /// <para>작업이 취소되면 구현체가 발생시킬 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown by an implementation when the operation is canceled.</para>
    /// \endif
    /// </exception>
    Task<PlcResult<byte[]>> SendAndReceiveAsync(
        IReadOnlyList<byte> requestFrame,
        int receiveTimeoutMs,
        int retryCount,
        CancellationToken cancellationToken = default);
}
