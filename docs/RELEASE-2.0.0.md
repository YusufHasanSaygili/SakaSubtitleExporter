# Saka 2.0.0

Saka now has a desktop window for batch subtitle extraction.

- Add multiple MKVs with drag and drop or the file picker.
- Start with one selected subtitle per episode.
- Add another selector to individual episodes with **Extract additional subtitle**.
- Apply a primary selection across a season by matching language, title and codec.
- Switch between light and dark themes.
- Cancel scanning or extraction without losing completed subtitles.
- Open the window from Start or **Export Subtitles With Saka** in Explorer.

Outputs still go beside each source MKV. Existing subtitles are skipped, and Explorer no longer opens after extraction. The `--extract` command remains available for all-track extraction without the main window.

The installer upgrades the existing per-user installation. FFmpeg and ffprobe are still required and are not bundled.

Validation includes a 30-file, 24-track-per-file integration batch producing exactly 31 selected subtitles, repeat-extraction protection, FFmpeg cancellation, and in-process WPF tests for selectors, bindings and themes.
