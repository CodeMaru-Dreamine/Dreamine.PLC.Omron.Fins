using Dreamine.PLC.Abstractions.Devices;
using Dreamine.PLC.Omron.Fins.Devices;
using Dreamine.PLC.Omron.Fins.Options;

namespace Dreamine.PLC.Omron.Fins.Protocol;

/// <summary>
/// \if KO
/// <para>메모리 영역 읽기 및 쓰기 작업을 위한 Omron FINS 명령 프레임을 생성합니다.</para>
/// \endif
/// \if EN
/// <para>Builds Omron FINS command frames for memory-area read and write operations.</para>
/// \endif
/// </summary>
public sealed class OmronFinsFrameBuilder
{
    /// <summary>
    /// \if KO
    /// <para>sid 값을 보관합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Stores the sid value.</para>
    /// \endif
    /// </summary>
    private int _sid;

    /// <summary>
    /// \if KO
    /// <para>FINS 메모리 영역 읽기 프레임을 생성합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds a FINS memory-area read frame.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>프레임 헤더에 사용할 FINS 연결 옵션입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The FINS connection options used for the frame header.</para>
    /// \endif
    /// </param>
    /// <param name="address">
    /// \if KO
    /// <para>읽기를 시작할 PLC 주소입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The PLC address at which to begin reading.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>읽을 요소 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The number of elements to read.</para>
    /// \endif
    /// </param>
    /// <param name="bitAccess">
    /// \if KO
    /// <para>요청이 비트 영역 접근인지 여부입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Whether the request targets bit-area access.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>생성된 원시 FINS 요청 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The generated raw FINS request frame.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// \if KO
    /// <para>주소 오프셋 또는 요소 수가 FINS 허용 범위를 벗어날 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the address offset or element count is outside the FINS-supported range.</para>
    /// \endif
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// \if KO
    /// <para>PLC 장치 형식을 FINS가 지원하지 않을 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the PLC device type is unsupported by FINS.</para>
    /// \endif
    /// </exception>
    public byte[] BuildMemoryAreaRead(
        OmronFinsConnectionOptions options,
        PlcAddress address,
        int count,
        bool bitAccess)
    {
        var command = BuildMemoryCommandBody(OmronFinsCommand.MemoryAreaRead, address, count, bitAccess, null);
        return BuildFrame(options, command);
    }

    /// <summary>
    /// \if KO
    /// <para>워드 값용 FINS 메모리 영역 쓰기 프레임을 생성합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds a FINS memory-area write frame for word values.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>프레임 헤더에 사용할 FINS 연결 옵션입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The FINS connection options used for the frame header.</para>
    /// \endif
    /// </param>
    /// <param name="address">
    /// \if KO
    /// <para>쓰기를 시작할 PLC 주소입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The PLC address at which to begin writing.</para>
    /// \endif
    /// </param>
    /// <param name="values">
    /// \if KO
    /// <para>빅 엔디언 페이로드로 기록할 워드 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The word values to encode in the big-endian payload.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>생성된 원시 FINS 요청 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The generated raw FINS request frame.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="values"/>가 <see langword="null"/>일 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when <paramref name="values"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// \if KO
    /// <para>주소 오프셋 또는 값 개수가 FINS 허용 범위를 벗어날 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the address offset or value count is outside the FINS-supported range.</para>
    /// \endif
    /// </exception>
    /// <exception cref="OverflowException">
    /// \if KO
    /// <para>값 개수에 2를 곱한 페이로드 길이가 정수 범위를 벗어날 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when doubling the value count for the payload length exceeds the integer range.</para>
    /// \endif
    /// </exception>
    public byte[] BuildMemoryAreaWriteWords(
        OmronFinsConnectionOptions options,
        PlcAddress address,
        IReadOnlyList<short> values)
    {
        var payload = new byte[values.Count * 2];
        for (var i = 0; i < values.Count; i++)
        {
            OmronFinsEndian.WriteInt16(payload, i * 2, values[i]);
        }

        var command = BuildMemoryCommandBody(OmronFinsCommand.MemoryAreaWrite, address, values.Count, false, payload);
        return BuildFrame(options, command);
    }

    /// <summary>
    /// \if KO
    /// <para>비트 값용 FINS 메모리 영역 쓰기 프레임을 생성합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Builds a FINS memory-area write frame for bit values.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>프레임 헤더에 사용할 FINS 연결 옵션입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The FINS connection options used for the frame header.</para>
    /// \endif
    /// </param>
    /// <param name="address">
    /// \if KO
    /// <para>쓰기를 시작할 PLC 주소입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The PLC address at which to begin writing.</para>
    /// \endif
    /// </param>
    /// <param name="values">
    /// \if KO
    /// <para>0 또는 1 바이트로 기록할 비트 값입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The bit values to encode as zero or one bytes.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>생성된 원시 FINS 요청 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The generated raw FINS request frame.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="values"/>가 <see langword="null"/>일 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="values"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    /// <exception cref="ArgumentOutOfRangeException">
    /// \if KO
    /// <para>주소 오프셋 또는 값 개수가 FINS 허용 범위를 벗어날 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the address offset or value count is outside the FINS-supported range.</para>
    /// \endif
    /// </exception>
    public byte[] BuildMemoryAreaWriteBits(
        OmronFinsConnectionOptions options,
        PlcAddress address,
        IReadOnlyList<bool> values)
    {
        var payload = values.Select(value => value ? (byte)1 : (byte)0).ToArray();
        var command = BuildMemoryCommandBody(OmronFinsCommand.MemoryAreaWrite, address, values.Count, true, payload);
        return BuildFrame(options, command);
    }

    /// <summary>
    /// \if KO
    /// <para>FINS 헤더와 명령 본문을 결합하고 다음 서비스 ID를 할당합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Combines a FINS header with a command body and assigns the next service ID.</para>
    /// \endif
    /// </summary>
    /// <param name="options">
    /// \if KO
    /// <para>네트워크·노드·유닛 주소를 제공하는 연결 옵션입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The connection options supplying network, node, and unit addresses.</para>
    /// \endif
    /// </param>
    /// <param name="command">
    /// \if KO
    /// <para>헤더 뒤에 복사할 명령 본문입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The command body to copy after the header.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>완성된 원시 FINS 프레임입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The completed raw FINS frame.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// \if KO
    /// <para><paramref name="options"/> 또는 <paramref name="command"/>가 <see langword="null"/>일 때 발생할 수 있습니다.</para>
    /// \endif
    /// \if EN
    /// <para>May be thrown when <paramref name="options"/> or <paramref name="command"/> is <see langword="null"/>.</para>
    /// \endif
    /// </exception>
    private byte[] BuildFrame(OmronFinsConnectionOptions options, byte[] command)
    {
        var frame = new byte[10 + command.Length];
        frame[0] = 0x80;
        frame[1] = 0x00;
        frame[2] = 0x02;
        frame[3] = options.DestinationNetwork;
        frame[4] = options.DestinationNode;
        frame[5] = options.DestinationUnit;
        frame[6] = options.SourceNetwork;
        frame[7] = options.SourceNode;
        frame[8] = options.SourceUnit;
        frame[9] = unchecked((byte)Interlocked.Increment(ref _sid));
        Buffer.BlockCopy(command, 0, frame, 10, command.Length);
        return frame;
    }

    /// <summary>
    /// \if KO
    /// <para>메모리 영역 명령, 주소, 개수 및 선택적 페이로드를 FINS 명령 본문으로 인코딩합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Encodes a memory-area command, address, count, and optional payload as a FINS command body.</para>
    /// \endif
    /// </summary>
    /// <param name="command">
    /// \if KO
    /// <para>인코딩할 FINS 명령 코드입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The FINS command code to encode.</para>
    /// \endif
    /// </param>
    /// <param name="address">
    /// \if KO
    /// <para>명령의 시작 PLC 주소입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The starting PLC address for the command.</para>
    /// \endif
    /// </param>
    /// <param name="count">
    /// \if KO
    /// <para>처리할 포인트 수입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The number of points to process.</para>
    /// \endif
    /// </param>
    /// <param name="bitAccess">
    /// \if KO
    /// <para>비트 영역 코드를 사용할지 여부입니다.</para>
    /// \endif
    /// \if EN
    /// <para>Whether to use a bit-area code.</para>
    /// \endif
    /// </param>
    /// <param name="payload">
    /// \if KO
    /// <para>본문 뒤에 추가할 선택적 쓰기 페이로드입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The optional write payload to append to the body.</para>
    /// \endif
    /// </param>
    /// <returns>
    /// \if KO
    /// <para>인코딩된 FINS 명령 본문입니다.</para>
    /// \endif
    /// \if EN
    /// <para>The encoded FINS command body.</para>
    /// \endif
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// \if KO
    /// <para><paramref name="count"/>가 1~65535가 아니거나 주소 오프셋이 0~65535가 아닐 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when <paramref name="count"/> is not between 1 and 65535 or the address offset is not between 0 and 65535.</para>
    /// \endif
    /// </exception>
    /// <exception cref="NotSupportedException">
    /// \if KO
    /// <para>PLC 장치 형식을 FINS가 지원하지 않을 때 발생합니다.</para>
    /// \endif
    /// \if EN
    /// <para>Thrown when the PLC device type is unsupported by FINS.</para>
    /// \endif
    /// </exception>
    private static byte[] BuildMemoryCommandBody(
        OmronFinsCommand command,
        PlcAddress address,
        int count,
        bool bitAccess,
        byte[]? payload)
    {
        if (count <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(count), count, "The FINS point count must be greater than zero.");
        }

        if (address.Offset is < 0 or > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(address),
                address.Offset,
                "The FINS address offset must be between 0 and 65535.");
        }

        if (count > ushort.MaxValue)
        {
            throw new ArgumentOutOfRangeException(
                nameof(count),
                count,
                "The FINS point count must be between 1 and 65535.");
        }

        var areaCode = OmronFinsMemoryAreaMapper.Map(address, bitAccess);
        var bodyLength = 8 + (payload?.Length ?? 0);
        var body = new byte[bodyLength];
        var commandValue = (ushort)command;

        OmronFinsEndian.WriteUInt16(body, 0, commandValue);
        body[2] = areaCode;
        OmronFinsEndian.WriteUInt16(body, 3, (ushort)address.Offset);
        body[5] = (byte)(address.BitOffset ?? 0);
        OmronFinsEndian.WriteUInt16(body, 6, (ushort)count);

        if (payload is not null)
        {
            Buffer.BlockCopy(payload, 0, body, 8, payload.Length);
        }

        return body;
    }
}
