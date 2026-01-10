# BACApp

Cross-platform desktop client built with `Avalonia UI` and a shared `Core` library.

## Solution layout

- `source/BACApp.UI` — Desktop UI app (`net10.0`, Avalonia)
- `source/BACApp.Core` — Shared models/services (`net10.0`)
- `build` — NUKE build project (`_build.csproj`, `Build.cs`)

## Tech stack

- .NET `10.0`
- `Avalonia 11` (desktop UI)
- `CommunityToolkit.Mvvm` (MVVM helpers)
- `Microsoft.Extensions.Hosting` (DI/hosting)
- `Serilog` (logging)

## Prerequisites

- .NET SDK `10.x`
- (Optional) Visual Studio / Rider / VS Code

## Getting started

### Restore

From the repo root:

```bash
dotnet restore
```

### Build

```bash
dotnet build -c Debug
```

### Run the UI

```bash
dotnet run --project source/BACApp.UI/BACApp.UI.csproj
```

## Application behavior

### Single-instance activation

`BACApp.UI` enforces a single running instance using a mutex + named pipe.

Launch arguments:

- `--maximize` — requests the primary instance to maximize
- (default) — requests activation (bring to foreground)

### API configuration

The app currently targets the API base URL:

- `https://v1.cbga-api.com`

This is configured in `source/BACApp.UI/Host.cs`.

### Logging

Serilog is configured in `source/BACApp.UI/Host.cs`.

- Debug output: verbose (Debug level)
- File output: JSON, warnings and above, rolling daily, retained 7 files
- Current path: `log.json` (TODO in code: cross-platform storage location)

## Projects

### `BACApp.Core`

Contains shared code used by the UI:

- API client abstraction (`IApiClient`) and implementation (`ApiClient`)
- Services (auth, calendar, tech logs, CSV export, etc.)
- Models/DTOs used by the client

### `BACApp.UI`

Avalonia desktop application:

- MVVM `ViewModels` and `Views`
- DI/hosting setup in `Host.cs`
- HTTP client configured with an auth header handler
- Single-instance coordination in `SingleInstanceCoordinator.cs`

## Publishing for MacOS and Linux

Avalonia apps can be published as self-contained single-file executables for various platforms. To publish for Linux and MacOS, run the following commands from the repo root:

- dotnet publish .\BACApp.UI.Avalonia\BACApp.UI.Avalonia.csproj -c Release -r linux-x64 --self-contained true /p:PublishSingleFile=true
- dotnet publish .\BACApp.UI.Avalonia\BACApp.UI.Avalonia.csproj -c Release -r linux-arm64 --self-contained true /p:PublishSingleFile=true
- dotnet publish .\BACApp.UI.Avalonia\BACApp.UI.Avalonia.csproj -c Release -r osx-x64 --self-contained true /p:PublishSingleFile=true
- dotnet publish .\BACApp.UI.Avalonia\BACApp.UI.Avalonia.csproj -c Release -r osx-arm64 --self-contained true /p:PublishSingleFile=true

## Build automation (NUKE)

The repository includes a NUKE build project in `build/`.

Notes:

- Targets are defined in `build/Build.cs` (`Clean`, `Restore`, `Compile`)
- Some targets are placeholders (no operations implemented)

## Development notes

- Target framework is `net10.0` across projects.
- Package versions use floating ranges (e.g. `11.*`, `10.*`, `8.*`). Pin versions if you need fully reproducible restores.

## License

Add licensing information here (if applicable).
