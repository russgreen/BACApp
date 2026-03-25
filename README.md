# BACApp

Cross-platform desktop client to the cloudbasega.com app built with `Avalonia UI` and a shared `Core` library.  The app only allows logging into the Bristol Aeroclub and Cloudbase Demo Club platforms.

## Solution layout

- `source/BACApp.Core` – Shared models/services (`net10.0`)
- `source/BACApp.UI` – Avalonia UI library (`net10.0`)
- `source/BACApp.Desktop` – Windows/macOS/Linux desktop entry point (`net10.0`, `WinExe`)
- `source/BACApp.iOS` – iPad entry point (`net10.0-ios`, experimental)
- `build` – NUKE build project (`_build.csproj`, `Build.cs`)

## Tech stack

- .NET `10.0`
- `Avalonia 11` (cross-platform UI)
- `CommunityToolkit.Mvvm` (MVVM helpers)
- `Microsoft.Extensions.Hosting` (DI/hosting)
- `ProDataGrid` (data grid control)
- `LiveChartsCore.SkiaSharpView.Avalonia` (charts)
- `Serilog` (logging)

## Prerequisites

- .NET SDK `10.x`
- (Optional) Visual Studio / Rider / VS Code

## Application behavior

### Single-instance activation

`BACApp.Desktop` enforces a single running instance using a mutex + named pipe.

Launch arguments:

- `--maximize` – requests the primary instance to maximize
- (default) – requests activation (bring to foreground)

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

- API client abstraction (`IApiClient`) and implementation (`ApiClient2`)
- Services: auth, aircraft, calendar, flight logs, tech logs, CSV export, members
- Models/DTOs used by the client
- Messaging types (via `CommunityToolkit.Mvvm` messenger)

### `BACApp.UI`

Avalonia UI library shared by all platform entry points:

- MVVM `ViewModels` and `Views` with compiled bindings
  - `LoginPageViewModel` – authentication and company selection
  - `CalendarPageViewModel` – resource/booking calendar
  - `LogsPageViewModel` – flight logs by aircraft
  - `LogsAirframePageViewModel` – airframe-specific log history
  - `ReportsPageViewModel` / `ReportsPage2ViewModel` – reports and analytics
  - `MainWindowViewModel` – top-level coordinator
- DI/hosting setup in `Host.cs`
- HTTP client configured with an `AuthHeaderHandler`
- Token persistence via `TokenStore`
- Custom `ResourceScheduleControl`

### `BACApp.Desktop`

Windows, macOS, and Linux desktop entry point:

- Output type `WinExe`; references `Avalonia.Desktop`
- Applies the application manifest and icon from `BACApp.UI`
- Delegates all UI and logic to `BACApp.UI`

### `BACApp.iOS`

Experimental iPad entry point (`net10.0-ios`, iOS 16.0+):

- References `Avalonia.iOS` and `BACApp.UI`
- Shares all UI and business logic with the desktop build

## Build automation (NUKE)

The repository includes a NUKE build project in `build/`.

## Development notes

- Target framework is `net10.0` (desktop) / `net10.0-ios` (iOS) across projects.
- Package versions use floating ranges (e.g. `11.*`, `10.*`, `8.*`). Pin versions if you need fully reproducible restores.
- Avalonia compiled bindings are enabled by default (`AvaloniaUseCompiledBindingsByDefault`).

## License

Add licensing information here (if applicable).
