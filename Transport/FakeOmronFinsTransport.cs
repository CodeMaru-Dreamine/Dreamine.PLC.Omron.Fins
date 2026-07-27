using Dreamine.PLC.Abstractions.Results;

namespace Dreamine.PLC.Omron.Fins.Transport;

/// <summary>
/// \if KO
/// <para>Omron FINS 어댑터 단위 테스트를 위한 메모리 기반 가짜 전송을 제공합니다.</para>
/// \endif
/// \if EN
/// <para>Provides an in-memory fake transport for Omron FINS adapter unit tests.</para>
/// \endif
/// </summary>
public sealed class FakeOmronFinsTransport : IOmronFinsTransport
{
    /// <summary>
    /// \if KO
    /// <para>responses 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the responses value.</para>
    /// \endif
    /// </summary>
    private readonly Queue<byte[]> _responses = new();

    /// <summary>
    /// \if KO
    /// <para>가짜 전송이 논리적으로 연결되었는지 여부를 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets whether the fake transport is logically connected.</para>
    /// \endif
    /// </summary>
    public bool IsConnected { get; private set; }

    /// <summary>
    /// \if KO
    /// <para>이 전송으로 보낸 요청 프레임을 가져옵니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets the request frames sent through this transport.</para>
    /// \endif
    /// </summary>
    public List<byte[]> SentRequests { get; } = [];

    /// <summary>
    /// \if KO
    /// <para>다음 요청에서 반환할 응답 프레임을 큐에 추가합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Enqueues a response frame to return for the next request.</para>
    /// \endif
    /// </summary>
    /// <param name="response">
    /// \if KO
    /// <para>큐에 추가할 응답 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The response frame to enqueue.</para>
    /// \endif
    /// </param>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="response"/>가 <see langword="null"/>일 때 큐 구현에서 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown by the queue implementation when <paramref name="response"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    public void EnqueueResponse(byte[] response)
    {
        _responses.Enqueue(response);
    }

    /// <summary>
    /// \if KO
    /// <para>가짜 전송을 연결 상태로 표시합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Marks the fake transport as connected.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>호환성을 위한 취소 토큰이며 이 구현에서는 사용하지 않습니다.</para>
    /// \endif
    /// \if EN
    /// <para>A compatibility cancellation token that is not observed by this implementation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>성공 연결 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing a successful connection result.</para>
    /// \endif
    /// </returns>
    public Task<PlcResult> ConnectAsync(CancellationToken cancellationToken = default)
    {
        IsConnected = true;
        return Task.FromResult(PlcResult.Success());
    }

    /// <summary>
    /// \if KO
    /// <para>가짜 전송을 연결 해제 상태로 표시합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Marks the fake transport as disconnected.</para>
    /// \endif
    /// </summary>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>호환성을 위한 취소 토큰이며 이 구현에서는 사용하지 않습니다.</para>
    /// \endif
    /// \if EN
    /// <para>A compatibility cancellation token that is not observed by this implementation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>성공 연결 해제 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing a successful disconnection result.</para>
    /// \endif
    /// </returns>
    public Task<PlcResult> DisconnectAsync(CancellationToken cancellationToken = default)
    {
        IsConnected = false;
        return Task.FromResult(PlcResult.Success());
    }

    /// <summary>
    /// \if KO
    /// <para>요청을 기록하고 큐의 다음 가짜 FINS 응답을 반환합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Records a request and returns the next queued fake FINS response.</para>
    /// \endif
    /// </summary>
    /// <param name="requestFrame">
    /// \if KO
    /// <para>기록할 원시 FINS 요청 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The raw FINS request frame to record.</para>
    /// \endif
    /// </param>
    /// <param name="receiveTimeoutMs">
    /// \if KO
    /// <para>호환성을 위한 수신 제한 시간이며 이 구현에서는 사용하지 않습니다.</para>
    /// \endif
    /// \if EN
    /// <para>A compatibility receive timeout that is not used by this implementation.</para>
    /// \endif
    /// </param>
    /// <param name="retryCount">
    /// \if KO
    /// <para>호환성을 위한 재시도 횟수이며 이 구현에서는 사용하지 않습니다.</para>
    /// \endif
    /// \if EN
    /// <para>A compatibility retry count that is not used by this implementation.</para>
    /// \endif
    /// </param>
    /// <param name="cancellationToken">
    /// \if KO
    /// <para>호환성을 위한 취소 토큰이며 이 구현에서는 사용하지 않습니다.</para>
    /// \endif
    /// \if EN
    /// <para>A compatibility cancellation token that is not observed by this implementation.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>큐의 다음 응답 또는 응답 없음 오류를 포함하는 결과 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>A task containing the next queued response or a no-response error.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="requestFrame"/>이 <see langword="null"/>일 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when <paramref name="requestFrame"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
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

    /// <summary>
    /// \if KO
    /// <para>가짜 전송을 연결 해제 상태로 만들고 비동기 정리를 완료합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Marks the fake transport as disconnected and completes asynchronous disposal.</para>
    /// \endif
    /// </summary>
    /// <returns>
    /// \if KO
    /// <para>이미 완료된 값 작업입니다.</para>
    /// \endif
    /// \if EN
    /// <para>An already-completed value task.</para>
    /// \endif
    /// </returns>
    public ValueTask DisposeAsync()
    {
        IsConnected = false;
        return ValueTask.CompletedTask;
    }
}
