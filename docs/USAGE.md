# Usage

## Extracting subtitles

Right-click an `.mkv` file and select **Export Subtitles With Saka**. Every subtitle track in the file is extracted to `A:\Anime`.

The application can also be run from a command prompt:

```powershell
SakaSubtitleExporter.exe --extract "A:\Anime\example.mkv"
```

## Filenames

Output files follow this pattern:

```text
VideoName.Track.Language.Title.extension
```

If the MKV does not contain language or title metadata, `und` and `untitled` are used. Characters that Windows does not allow in filenames are replaced with underscores.

## Job report

Each run creates a report in the output directory:

```text
VideoName._Saka-report.txt
```

The report lists every exported, skipped, or failed subtitle track.

## Uninstalling

Open **Settings > Apps > Installed apps** in Windows and uninstall **Saka Subtitle Exporter**. The context-menu entry is removed with the application.
