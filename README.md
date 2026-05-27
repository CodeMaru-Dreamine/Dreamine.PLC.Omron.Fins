# Dreamine.PLC.Omron.Fins

Omron FINS protocol adapter for the Dreamine PLC communication stack.

## Purpose

`Dreamine.PLC.Omron.Fins` is part of the Dreamine PLC package family.

This package provides the adapter boundary for Omron FINS communication without depending on OMRON CX-Compolet or SYSMAC Gateway runtime files.

The package keeps PLC communication responsibilities separated:

- `Dreamine.PLC.Abstractions` defines vendor-neutral PLC contracts.
- `Dreamine.PLC.Core` provides shared client lifecycle and validation infrastructure.
- `Dreamine.PLC.Omron.Fins` implements the Omron FINS adapter boundary.
- `Dreamine.PLC.Wpf` can consume this package through `IPlcClient`.

## Features

- Omron FINS adapter project structure
- FINS/UDP transport boundary
- FINS/TCP transport boundary
- Memory Area Read command boundary (`0101`)
- Memory Area Write command boundary (`0102`)
- DM/CIO/WR/HR area mapping boundary
- Dreamine `IPlcClient` integration
- Testable transport abstraction through `IOmronFinsTransport`

## Current Scope

This repository is prepared as the Omron FINS implementation boundary.

The first stable target should be:

- FINS/UDP Memory Area Read
- FINS/UDP Memory Area Write
- FINS/TCP connection negotiation
- FINS/TCP Memory Area Read
- FINS/TCP Memory Area Write
- Local simulator server for 1PC and 2PC tests

## Vendor Runtime Notice

This package does not include OMRON CX-Compolet or SYSMAC Gateway runtime files.

Users must install and license vendor runtime software separately when using vendor-runtime-based adapters.

`Dreamine.PLC.Omron.Fins` is intended to communicate through the FINS protocol and does not redistribute OMRON DLLs.

OMRON, CX-Compolet, and SYSMAC Gateway are trademarks or products of their respective owners.

## Basic Usage

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

## Device Mapping Policy

The initial mapping is intentionally conservative.

| Dreamine Device | FINS Meaning | Word Area | Bit Area |
|---|---|---:|---:|
| `D` | Data Memory | `DM Word` | `DM Bit` |
| `M` | Internal/CIO-compatible relay boundary | `CIO Word` | `CIO Bit` |
| `W` | Work area | `WR Word` | `WR Bit` |
| `R` | Holding area boundary | `HR Word` | `HR Bit` |

Unsupported device types must fail explicitly instead of being guessed.

## Project References

- `Dreamine.PLC.Abstractions`
- `Dreamine.PLC.Core`

## Target Framework

```xml
<TargetFramework>net8.0</TargetFramework>
```

## Package Metadata

| Item | Value |
|---|---|
| PackageId | `Dreamine.PLC.Omron.Fins` |
| Version | `1.0.0` |
| License | `MIT` |
| Repository | `https://github.com/CodeMaru-Dreamine/Dreamine.PLC.Omron.Fins` |
| Project URL | `https://github.com/CodeMaru-Dreamine/Dreamine.PLC.FullKit` |

## Architecture Rule

This repository must not reference application-level projects.

Dependency direction must remain one-way:

```text
Abstractions
    ▲
    │
Core
    ▲
    │
Vendor Adapter
```

`Dreamine.PLC.Omron.Fins` must not reference `Dreamine.PLC.Wpf`, samples, or application projects.

## License

This project is licensed under the MIT License.
