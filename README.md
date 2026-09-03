# Saka Subtitle Exporter

<img src="assets/fish.png" alt="Saka fish icon" width="160">

A small Windows tool that extracts every subtitle track from an MKV file in one go. There is no main window; it runs from the File Explorer context menu.

## Installation

1. Download `SakaSubtitleExporterSetup.exe` from the **Releases** page.
2. Complete the setup.
3. Right-click an MKV file and select **Export Subtitles With Saka**.

On Windows 11, the command may appear under **Show more options**.

The application and its context-menu entry are installed for the current Windows user only. Administrator access is not required.

## Output

All subtitle tracks are written to the same folder as the source MKV. Each filename includes the source video name, track number, language, and track title.

Example:

```text
Episode 01.01.eng.Full Subtitles.ass
Episode 01.02.eng.Signs & Songs.ass
```

Existing subtitle files are never overwritten. A `_Saka-report.txt` file records the result. Extraction runs without opening another File Explorer window.

## Supported subtitle formats

- ASS / SSA
- SRT / SubRip
- WebVTT
- PGS (`.sup`)
- Other text-based subtitles (converted to SRT)
- Unsupported bitmap or container-specific subtitles (saved as a single-track MKV)

## Requirements

Windows 10 or 11 and FFmpeg are required. If FFmpeg is not installed, run:

```powershell
winget install Gyan.FFmpeg
```

## Building from source

Visual Studio 2022 or the .NET 8 SDK and Inno Setup 6 are required.

```powershell
.\installer\Build-Setup.ps1
```

The setup file is created in `artifacts\setup`. See the [development notes](docs/DEVELOPMENT.md) for details.
