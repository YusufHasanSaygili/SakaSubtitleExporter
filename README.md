# Saka Subtitle Exporter

<img src="assets/fish.png" alt="Saka fish icon" width="160">

A small Windows desktop app for extracting subtitles from a batch of MKV files. Drop in a season, choose the tracks you want, and extract. Everything runs locally.

## Installation

1. Download `SakaSubtitleExporterSetup.exe` from the **Releases** page.
2. Complete the setup.
3. Open Saka from the Start menu, or right-click an MKV and select **Export Subtitles With Saka**.

On Windows 11, the command may appear under **Show more options**.

The application and its context-menu entry are installed for the current Windows user only. Administrator access is not required.

Setup defaults to `A:\Anime\Saka Subtitle Exporter`; choose another folder if you prefer. Upgrades keep the existing installation location.

## Choose your tracks

1. Add MKVs with **Browse files** or drag and drop.
2. Each file starts with one subtitle selected: the track marked as default, or the first track if none is marked.
3. Choose the track you want. **Use first selection for all** matches the first episode's primary choice to other files by language, title and codec. Missing or ambiguous matches are left unchanged.
4. Need two subtitles from just one episode? Click **Extract additional subtitle** on that episode and choose another track in the new dropdown.
5. Click **Extract selected**.

Track names and language labels come from the MKV. Saka does not guess whether an unnamed track contains full dialogue or only songs and signs. The window includes light and dark themes.

## Output

Selected subtitle tracks are written beside their source MKV, even when files in the queue come from different folders. Each filename includes the source video name, original subtitle-track number, language, and track title.

Example:

```text
Episode 01.01.eng.Full Subtitles.ass
Episode 01.02.eng.Signs & Songs.ass
```

Existing subtitle files are never overwritten. A `_Saka-report.txt` file records the result. Extraction runs without opening another File Explorer window.

Cancelling keeps completed subtitles and removes the unfinished temporary output. Saka extracts subtitles; it does not translate them or edit their text.

## Supported subtitle formats

- ASS / SSA
- SRT / SubRip
- WebVTT
- PGS (`.sup`)
- Other text-based subtitles (converted to SRT)
- Unsupported bitmap or container-specific subtitles (saved as a single-track MKV)

## Requirements

Windows 10 or 11, .NET Framework 4.8, and FFmpeg (including ffprobe) are required. FFmpeg is not bundled. If it is not installed, run:

```powershell
winget install Gyan.FFmpeg
```

## Building from source

Visual Studio 2022 or the .NET 8 SDK and Inno Setup 6 are required.

```powershell
.\installer\Build-Setup.ps1
```

The setup file is created in `artifacts\setup`. See the [development notes](docs/DEVELOPMENT.md) for details.

The upload area was visually inspired by [Ravi Katiyar's File Upload Card on 21st.dev](https://21st.dev/@ravikatiyar162/components/file-upload-card). Saka's UI is independently implemented in WPF; it does not include that component's React source.
