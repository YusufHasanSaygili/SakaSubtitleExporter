# Usage

## Extracting subtitles

Open Saka from the Start menu, or right-click an MKV and select **Export Subtitles With Saka**. Add more MKVs with **Browse files** or drag and drop. Duplicate paths and non-MKV files are ignored.

Each readable file starts with one selected subtitle. Default tracks take priority; otherwise Saka chooses the first subtitle. This is a starting choice, not a guarantee that the track contains full dialogue.

**Extract additional subtitle** adds an empty selector to that file only. Choose another track or remove the extra selector before extracting. The same track cannot be selected twice for a file.

**Use first selection for all** matches the first ready file's primary selection by language, title and codec. It does not use stream position, change additional selections, or guess between multiple matches. Check the footer for files left unchanged.

Files with unreadable tracks or no subtitles show a message in their row. Other ready files can still be extracted. Remove and add a file again to retry scanning.

## Command line

Open the selection window with multiple files:

```powershell
SakaSubtitleExporter.exe --ui "A:\Anime\Episode 01.mkv" "A:\Anime\Episode 02.mkv"
```

The legacy all-tracks command remains available without a main window:

```powershell
SakaSubtitleExporter.exe --extract "A:\Anime\example.mkv"
```

It writes every track and a report beside the MKV. Failures display an error dialog and return a nonzero exit code.

## Filenames

Output files follow this pattern:

```text
VideoName.Track.Language.Title.extension
```

If the MKV does not contain language or title metadata, `und` and `untitled` are used. Characters that Windows does not allow in filenames are replaced with underscores.

Long names are shortened. If two source names become identical after shortening or sanitizing, rename the MKVs before extracting to avoid an existing-file skip. Track numbers remain the same regardless of selection order.

## Job report

Each run creates a report in the output directory:

```text
VideoName._Saka-report.txt
```

The report lists every exported, skipped, or failed selected track. Each run replaces the previous report; subtitle files are never overwritten.

## Cancel and retry

**Cancel** stops the current FFmpeg process, removes its temporary output, and keeps completed files. Closing during extraction asks whether to cancel first. Run extraction again to skip finished files and process the remaining selections.

Saka does not open Explorer when a job finishes. Read the per-file status and footer for results.

## Uninstalling

Open **Settings > Apps > Installed apps** in Windows and uninstall **Saka Subtitle Exporter**. The app, Start menu shortcut and context-menu entry are removed. Videos and extracted subtitles are kept.
