namespace Dreamine.PLC.Omron.Fins.Options;

/// <summary>
/// \if KO
/// <para>Omron FINS 연결 옵션을 나타냅니다.</para>
/// \endif
/// \if EN
/// <para>Represents Omron FINS connection options.</para>
/// \endif
/// </summary>
public sealed class OmronFinsConnectionOptions
{
    /// <summary>
    /// \if KO
    /// <para>대상 PLC 호스트를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the target PLC host.</para>
    /// \endif
    /// </summary>
    public string Host { get; set; } = "127.0.0.1";

    /// <summary>
    /// \if KO
    /// <para>대상 포트를 가져오거나 설정합니다. 기본 FINS 포트는 9600입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the target port. The default FINS port is 9600.</para>
    /// \endif
    /// </summary>
    public int Port { get; set; } = 9600;

    /// <summary>
    /// \if KO
    /// <para>사용할 FINS 전송 형식을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the FINS transport type to use.</para>
    /// \endif
    /// </summary>
    public OmronFinsTransportType TransportType { get; set; } = OmronFinsTransportType.Udp;

    /// <summary>
    /// \if KO
    /// <para>밀리초 단위 연결 제한 시간을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the connection timeout in milliseconds.</para>
    /// \endif
    /// </summary>
    public int ConnectTimeoutMs { get; set; } = 3000;

    /// <summary>
    /// \if KO
    /// <para>밀리초 단위 수신 제한 시간을 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the receive timeout in milliseconds.</para>
    /// \endif
    /// </summary>
    public int ReceiveTimeoutMs { get; set; } = 3000;

    /// <summary>
    /// \if KO
    /// <para>송수신 재시도 횟수를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the send-and-receive retry count.</para>
    /// \endif
    /// </summary>
    public int RetryCount { get; set; } = 1;

    /// <summary>
    /// \if KO
    /// <para>목적지 네트워크 주소를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the destination network address.</para>
    /// \endif
    /// </summary>
    public byte DestinationNetwork { get; set; } = 0x00;

    /// <summary>
    /// \if KO
    /// <para>목적지 노드 주소를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the destination node address.</para>
    /// \endif
    /// </summary>
    public byte DestinationNode { get; set; } = 0x00;

    /// <summary>
    /// \if KO
    /// <para>목적지 유닛 주소를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the destination unit address.</para>
    /// \endif
    /// </summary>
    public byte DestinationUnit { get; set; } = 0x00;

    /// <summary>
    /// \if KO
    /// <para>송신자 네트워크 주소를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the source network address.</para>
    /// \endif
    /// </summary>
    public byte SourceNetwork { get; set; } = 0x00;

    /// <summary>
    /// \if KO
    /// <para>송신자 노드 주소를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the source node address.</para>
    /// \endif
    /// </summary>
    public byte SourceNode { get; set; } = 0x01;

    /// <summary>
    /// \if KO
    /// <para>송신자 유닛 주소를 가져오거나 설정합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Gets or sets the source unit address.</para>
    /// \endif
    /// </summary>
    public byte SourceUnit { get; set; } = 0x00;
}
