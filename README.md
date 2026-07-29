# Dreamine.PLC.Omron.Fins

[![CI](https://github.com/CodeMaru-Dreamine/Dreamine.PLC.Omron.Fins/actions/workflows/ci.yml/badge.svg?branch=main)](https://github.com/CodeMaru-Dreamine/Dreamine.PLC.Omron.Fins/actions/workflows/ci.yml)
[![Quality Gate](https://sonarcloud.io/api/project_badges/measure?project=CodeMaru-Dreamine_Dreamine.PLC.Omron.Fins&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=CodeMaru-Dreamine_Dreamine.PLC.Omron.Fins)
[![Security Rating](https://sonarcloud.io/api/project_badges/measure?project=CodeMaru-Dreamine_Dreamine.PLC.Omron.Fins&metric=security_rating)](https://sonarcloud.io/summary/new_code?id=CodeMaru-Dreamine_Dreamine.PLC.Omron.Fins)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=CodeMaru-Dreamine_Dreamine.PLC.Omron.Fins&metric=coverage)](https://sonarcloud.io/summary/new_code?id=CodeMaru-Dreamine_Dreamine.PLC.Omron.Fins)

[![License](https://img.shields.io/badge/license-MIT-2496ED.svg)](LICENSE)
[![.NET](https://img.shields.io/badge/.NET-8-512BD4.svg)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![NuGet](https://img.shields.io/nuget/v/Dreamine.PLC.Omron.Fins.svg)](https://www.nuget.org/packages/Dreamine.PLC.Omron.Fins)
[![Downloads](https://img.shields.io/nuget/dt/Dreamine.PLC.Omron.Fins.svg)](https://www.nuget.org/packages/Dreamine.PLC.Omron.Fins)

[![Docs](https://img.shields.io/badge/%F0%9F%93%98%20Docs-dreamine.kr-2496ED)](https://dreamine.kr/libraries?lang=en)
[![Guide](https://img.shields.io/badge/%F0%9F%93%98%20Guide-dreamine.kr-2496ED)](https://dreamine.kr/guide?lang=en)
[![Playground](https://img.shields.io/badge/%F0%9F%8E%AE%20Playground-dreamine.kr-7B2CBF)](https://dreamine.kr/playground?lang=en)
[![Book](https://img.shields.io/badge/%F0%9F%93%96%20Book-Practical%20MVVM%20Architecture-black)](https://bookk.co.kr/bookStore/69c0f1b41461ec1ae849a0f6)

[Korean documentation](./README_KO.md)

Omron FINS TCP/UDP PLC adapter for the Dreamine PLC package family.

This package provides FINS TCP/UDP client support and built-in FINS simulator servers for local and PC-to-PC validation.

## Features

- Omron FINS TCP client
- Omron FINS UDP client
- FINS TCP simulator server
- FINS UDP simulator server
- Memory area read/write support boundary
- Word read/write diagnostics
- Repeated handshake validation flow
- Timeout and retry support for UDP
- Integration with `IPlcClient`

## Supported simulator test modes

The SampleSmart PLC Protocol page supports:

```text
FinsTcp ↔ FinsTcp
FinsUdp ↔ FinsUdp
```

The server and client modes must match. A `SimulatorTcp`, `McTcp`, or `McUdp` server cannot be used with a `FinsTcp` or `FinsUdp` client.

## 1PC test

Use this flow for local validation.

```text
Mode: FinsTcp or FinsUdp
Host: 127.0.0.1
Port: 55000
Start Server
Use Client
Connect
Write Words
Read Words
Run Handshake
```

## 2PC test

Server PC:

```text
Mode: FinsTcp or FinsUdp
Host: 0.0.0.0
Port: 55000
Start Server
```

Client PC:

```text
Mode: same as server
Host: server PC IP
Port: 55000
Use Client
Connect
Read/Write or Handshake
```

## Firewall requirement for PC-to-PC tests

Open the inbound port on the server PC.

For TCP:

```powershell
New-NetFirewallRule -DisplayName "Dreamine PLC FINS TCP 55000" -Direction Inbound -Protocol TCP -LocalPort 55000 -Action Allow
```

For UDP:

```powershell
New-NetFirewallRule -DisplayName "Dreamine PLC FINS UDP 55000" -Direction Inbound -Protocol UDP -LocalPort 55000 -Action Allow
```

Run PowerShell as Administrator. Without these rules, local 1PC tests may pass while 2PC tests fail.

## Physical PLC test notice

FINS support is currently validated with the built-in simulator. Physical Omron PLC integration must still be tested.

Before connecting to a real Omron PLC, verify:

- PLC model and Ethernet module support
- FINS TCP/UDP setting
- Port number, commonly configured as 9600 in many FINS environments
- Source and destination node settings
- Network number
- Unit address
- Memory area mapping
- PLC Ethernet module routing settings
- Safe polling interval

FINS/TCP may require device-specific handshake or node configuration. Simulator success does not guarantee physical PLC compatibility without field testing.

## Polling and write safety

Do not use 1ms polling against a physical PLC.

Recommended physical PLC values:

- Monitoring: 100ms to 500ms
- UI display refresh: 250ms to 1000ms
- Write: event-driven only
- Handshake stress test: simulator only unless explicitly approved for a real machine

## Vendor runtime policy

This package does not include Omron CX-Compolet, SYSMAC Gateway, or any Omron runtime DLL.

This package implements FINS TCP/UDP communication directly. CX-Compolet integration, if needed, must remain in a separate adapter package without redistributing vendor DLLs.

## Validation status

Validated:

- 1PC FINS TCP read/write and handshake
- 1PC FINS UDP read/write and handshake
- 2PC FINS TCP read/write and handshake
- 2PC FINS UDP read/write and handshake
- WPF monitor integration

Pending:

- Physical Omron PLC validation

## License

MIT License.
