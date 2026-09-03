# Development

## Repository layout

- `src/SakaSubtitleExporter`: Windows application
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
