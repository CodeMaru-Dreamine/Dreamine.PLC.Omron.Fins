namespace Dreamine.PLC.Omron.Fins.Simulation;

/// <summary>
/// \if KO
/// <para>Omron FINS 시뮬레이터 서버 옵션을 나타냅니다.</para>
/// \endif
/// \if EN
/// <para>Represents options for the Omron FINS simulator server.</para>
/// \endif
/// </summary>
public sealed class OmronFinsSimulatorServerOptions
{
    /// <summary>
    /// \if KO
    /// <para>서버가 바인딩할 호스트를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the host to which the server binds.</para>
    /// \endif
    /// </summary>
    public string Host { get; set; } = "0.0.0.0";

    /// <summary>
    /// \if KO
    /// <para>서버가 바인딩할 포트를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the port to which the server binds.</para>
    /// \endif
    /// </summary>
    public int Port { get; set; } = 9600;

    /// <summary>
    /// \if KO
    /// <para>D100 단일 워드 쓰기에서 D101 자동 응답을 생성할지 여부를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets whether D100 single-word writes produce an automatic D101 response.</para>
    /// \endif
    /// </summary>
    public bool EnableAutoWordResponse { get; set; } = true;

    /// <summary>
    /// \if KO
    /// <para>자동 응답을 시작하는 메모리 오프셋을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the memory offset that triggers an automatic response.</para>
    /// \endif
    /// </summary>
    public int AutoResponseTriggerOffset { get; set; } = 100;

    /// <summary>
    /// \if KO
    /// <para>자동 응답을 기록할 메모리 오프셋을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the memory offset to which the automatic response is written.</para>
    /// \endif
    /// </summary>
    public int AutoResponseOffset { get; set; } = 101;

    /// <summary>
    /// \if KO
    /// <para>자동 응답 값에 더할 증분을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the increment added to the automatic response value.</para>
    /// \endif
    /// </summary>
    public int AutoResponseIncrement { get; set; } = 1;
}
