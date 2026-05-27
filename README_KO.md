# Dreamine.PLC.Omron.Fins

Dreamine PLC 통신 스택을 위한 Omron FINS 프로토콜 어댑터입니다.

## 목적

`Dreamine.PLC.Omron.Fins`는 Dreamine PLC 패키지군의 일부입니다.

이 패키지는 OMRON CX-Compolet 또는 SYSMAC Gateway 런타임 파일에 의존하지 않는 FINS 프로토콜 어댑터 경계를 제공합니다.

책임은 다음과 같이 분리합니다.

- `Dreamine.PLC.Abstractions`는 벤더 중립 PLC 계약을 정의합니다.
- `Dreamine.PLC.Core`는 공통 Client 생명주기와 검증 인프라를 제공합니다.
- `Dreamine.PLC.Omron.Fins`는 Omron FINS 어댑터 경계를 구현합니다.
- `Dreamine.PLC.Wpf`는 `IPlcClient`를 통해 이 패키지를 사용할 수 있습니다.

## 기능 범위

- Omron FINS 어댑터 프로젝트 구조
- FINS/UDP 전송 경계
- FINS/TCP 전송 경계
- Memory Area Read 명령 경계 (`0101`)
- Memory Area Write 명령 경계 (`0102`)
- DM/CIO/WR/HR 영역 매핑 경계
- Dreamine `IPlcClient` 통합
- `IOmronFinsTransport` 기반 테스트 가능한 전송 추상화

## 현재 범위

이 저장소는 Omron FINS 구현 경계를 준비하기 위한 프로젝트입니다.

첫 번째 안정화 목표는 다음과 같습니다.

- FINS/UDP Memory Area Read
- FINS/UDP Memory Area Write
- FINS/TCP 연결 협상
- FINS/TCP Memory Area Read
- FINS/TCP Memory Area Write
- 1PC 및 2PC 테스트용 로컬 시뮬레이터 서버

## 벤더 런타임 안내

이 패키지는 OMRON CX-Compolet 또는 SYSMAC Gateway 런타임 파일을 포함하지 않습니다.

벤더 런타임 기반 어댑터를 사용하는 경우 사용자는 필요한 벤더 소프트웨어를 별도로 설치하고 정식 라이선스를 보유해야 합니다.

`Dreamine.PLC.Omron.Fins`는 FINS 프로토콜 통신을 목표로 하며 OMRON DLL을 재배포하지 않습니다.

OMRON, CX-Compolet, SYSMAC Gateway는 각 소유사의 상표 또는 제품입니다.

## 기본 사용 예

```csharp
using Dreamine.PLC.Abstractions.Devices;
using Dreamine.PLC.Omron.Fins.Clients;
using Dreamine.PLC.Omron.Fins.Options;

var client = new OmronFinsPlcClient(new OmronFinsConnectionOptions
{
    Host = "192.168.0.10",
    Port = 9600,
    TransportType = OmronFinsTransportType.Udp,
    SourceNode = 10,
    DestinationNode = 1,
    TimeoutMs = 3000,
    RetryCount = 3
});

await client.ConnectAsync();

var result = await client.ReadWordsAsync(
    new PlcAddress(PlcDeviceType.D, 100),
    count: 4);
```

## 디바이스 매핑 정책

초기 매핑은 보수적으로 유지합니다.

| Dreamine Device | FINS 의미 | Word Area | Bit Area |
|---|---|---:|---:|
| `D` | Data Memory | `DM Word` | `DM Bit` |
| `M` | Internal/CIO-compatible relay boundary | `CIO Word` | `CIO Bit` |
| `W` | Work area | `WR Word` | `WR Bit` |
| `R` | Holding area boundary | `HR Word` | `HR Bit` |

지원하지 않는 디바이스 타입은 추측하지 않고 명확히 실패해야 합니다.

## 프로젝트 참조

- `Dreamine.PLC.Abstractions`
- `Dreamine.PLC.Core`

## 대상 프레임워크

```xml
<TargetFramework>net8.0</TargetFramework>
```

## 패키지 메타데이터

| 항목 | 값 |
|---|---|
| PackageId | `Dreamine.PLC.Omron.Fins` |
| Version | `1.0.0` |
| License | `MIT` |
| Repository | `https://github.com/CodeMaru-Dreamine/Dreamine.PLC.Omron.Fins` |
| Project URL | `https://github.com/CodeMaru-Dreamine/Dreamine.PLC.FullKit` |

## 아키텍처 규칙

이 저장소는 애플리케이션 레벨 프로젝트를 참조하면 안 됩니다.

의존성 방향은 단방향으로 유지합니다.

```text
Abstractions
    ▲
    │
Core
    ▲
    │
Vendor Adapter
```

`Dreamine.PLC.Omron.Fins`는 `Dreamine.PLC.Wpf`, 샘플, 애플리케이션 프로젝트를 참조하지 않습니다.

## License

이 프로젝트는 MIT License를 따릅니다.
