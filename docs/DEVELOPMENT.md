# Development

## Repository layout

- `src/SakaSubtitleExporter`: .NET Framework 4.8 WPF application, batch models and extraction engine
- `tests/SakaSubtitleExporter.Tests`: dependency-free unit tests
- `installer`: Inno Setup package and build script
- `.github/workflows`: Windows build and artifact generation

## Build and test

```powershell
dotnet build .\SakaSubtitleExporter.sln -c Release
dotnet run --project .\tests\SakaSubtitleExporter.Tests -c Release --no-build
```

Inno Setup 6 is required to create the installer:

```powershell
winget install JRSoftware.InnoSetup
.\installer\Build-Setup.ps1
```

The build script compiles the solution, runs the tests, and creates `artifacts\setup\SakaSubtitleExporterSetup.exe`.

## Integration tests

With FFmpeg and ffprobe installed, run:

```powershell
dotnet run --project .\tests\SakaSubtitleExporter.Tests -c Release -- --integration "$env:TEMP"
```

This creates a uniquely named test folder containing 30 small MKVs, each with 24 subtitle tracks. It extracts one track per file and an extra track for episode 15, verifies all 31 outputs and their text, checks that repeat extraction does not overwrite files, and tests cancellation of a running FFmpeg process. The folder is printed and retained for manual UI testing.

Unit tests cover default selections, additional selectors, duplicate rejection, metadata matching with reordered tracks, ambiguous matches, no-subtitle files, filename handling and output locations.

WPF component tests also exercise the actual additional/remove buttons, dropdown bindings, duplicate-choice restoration, extract-button state, theme switching and narrow layouts. They run in-process without desktop input. To render the component-test layouts:

```powershell
dotnet run --project .\tests\SakaSubtitleExporter.Tests -c Release -- --render-ui "$env:TEMP\saka-ui-qa"
```

## Interface

The native WPF UI uses semantic color resources for light and dark themes, keyboard-operable dropdowns, a virtualized file list and asynchronous scanning/extraction. No browser runtime, server or account is involved. The context menu opens `--ui`; `--extract` retains the original all-tracks behavior.

For manual checks: add the generated batch, add and remove an extra selector, reject a duplicate selection, match tracks, extract, repeat, cancel a job, switch themes and resize the window. Check that output stays beside each MKV and Explorer does not open.
