using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading;

namespace SakaSubtitleExporter
{
    internal sealed class ExportResult
    {
        public int Exported;
        public int Skipped;
        public int Failed;
        public string ReportPath;
        public readonly List<string> Errors = new List<string>();
    }

    internal static class SubtitleExporter
    {
        public static List<SubtitleStream> Scan(string mkvPath, CancellationToken token = default(CancellationToken))
        {
            string path = ValidatePath(mkvPath);
            string entries = "stream=index,codec_type,codec_name:stream_tags=language,title:stream_disposition=default,forced";
            string arguments = CommandLine.Join(new[] { "-v", "error", "-select_streams", "s", "-show_entries", entries, "-of", "json", path });
            ProcessResult result = ProcessRunner.Run(ToolLocator.Find("ffprobe.exe"), arguments, token);
            if (result.ExitCode != 0) throw new InvalidOperationException("Could not read subtitle tracks. " + result.StandardError.Trim());
            var serializer = new DataContractJsonSerializer(typeof(ProbeResult));
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes(result.StandardOutput)))
            {
                var probe = (ProbeResult)serializer.ReadObject(input);
                return (probe.streams ?? new List<SubtitleStream>()).Where(s => s.codec_type == "subtitle").ToList();
            }
        }

        public static void ExportAll(string mkvPath)
        {
            var tracks = Scan(mkvPath);
            var result = ExportSelected(mkvPath, tracks, tracks.Select(t => t.index), CancellationToken.None);
            if (result.Failed > 0) throw new InvalidOperationException(string.Join(Environment.NewLine, result.Errors));
        }

        public static string ValidatePath(string mkvPath)
        {
            string path = Path.GetFullPath(mkvPath);
            if (!string.Equals(Path.GetExtension(path), ".mkv", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Only MKV files are supported.");
            if (!File.Exists(path)) throw new FileNotFoundException("MKV file not found.", path);
            return path;
        }

        public static ExportResult ExportSelected(string mkvPath, IList<SubtitleStream> tracks,
            IEnumerable<int> streamIds, CancellationToken token, IProgress<int> progress = null)
        {
            string path = ValidatePath(mkvPath);
            int[] selected = streamIds.Distinct().ToArray();
            if (selected.Any(id => !tracks.Any(t => t.index == id))) throw new ArgumentException("A selected subtitle track no longer exists.");
            string folder = Path.GetDirectoryName(path);
            string baseName = FileNames.Safe(Path.GetFileNameWithoutExtension(path), "mkv", 110);
            var result = new ExportResult { ReportPath = Path.Combine(folder, baseName + "._Saka-report.txt") };
            var report = new List<string> { "Source: " + path, "Date: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                "Available tracks: " + tracks.Count, "Selected tracks: " + selected.Length, string.Empty };
            try
            {
                token.ThrowIfCancellationRequested();
                string ffmpeg = ToolLocator.Find("ffmpeg.exe");
                for (int i = 0; i < selected.Length; i++)
                {
                    token.ThrowIfCancellationRequested();
                    var track = tracks.First(t => t.index == selected[i]);
                    int ordinal = tracks.IndexOf(track) + 1;
                    string outputPath = OutputPath(path, track, ordinal);
                    try
                    {
                        if (Extract(ffmpeg, path, outputPath, track, token))
                        {
                            result.Exported++;
                            report.Add("OK: " + Path.GetFileName(outputPath) + " (stream=" + track.index + ")");
                        }
                        else
                        {
                            result.Skipped++;
                            report.Add("SKIPPED (already exists): " + Path.GetFileName(outputPath));
                        }
                    }
                    catch (OperationCanceledException) { throw; }
                    catch (Exception ex)
                    {
                        result.Failed++;
                        string error = "Track " + ordinal + ": " + ex.Message;
                        result.Errors.Add(error);
                        report.Add("ERROR: " + error);
                    }
                    if (progress != null) progress.Report(i + 1);
                }
                if (selected.Length == 0) report.Add("No subtitle tracks selected.");
                return result;
            }
            catch (OperationCanceledException) { report.Add("CANCELLED: Completed files were kept."); throw; }
            catch (Exception ex) { report.Add("ERROR: " + ex.Message); throw; }
            finally { File.WriteAllLines(result.ReportPath, report, new UTF8Encoding(false)); }
        }

        internal static string OutputPath(string mkvPath, SubtitleStream track, int ordinal)
        {
            string baseName = FileNames.Safe(Path.GetFileNameWithoutExtension(mkvPath), "mkv", 110);
            string language = FileNames.Safe(track.tags == null ? null : track.tags.language, "und", 25);
            string title = FileNames.Safe(track.tags == null ? null : track.tags.title, "untitled", 70);
            return Path.Combine(Path.GetDirectoryName(Path.GetFullPath(mkvPath)),
                string.Format("{0}.{1:D2}.{2}.{3}{4}", baseName, ordinal, language, title, SubtitleFormats.Resolve(track.codec_name).Extension));
        }

        private static bool Extract(string ffmpeg, string source, string output, SubtitleStream track, CancellationToken token)
        {
            if (File.Exists(output)) return false;
            var format = SubtitleFormats.Resolve(track.codec_name);
            string temporary = Path.Combine(Path.GetDirectoryName(output), ".saka-" + Guid.NewGuid().ToString("N") + format.Extension);
            try
            {
                var arguments = new List<string> { "-nostdin", "-n", "-v", "error", "-i", source,
                    "-map", "0:" + track.index, "-c:s", format.Codec };
                if (format.Container != null) { arguments.Add("-f"); arguments.Add(format.Container); }
                arguments.Add(temporary);
                var extraction = ProcessRunner.Run(ffmpeg, CommandLine.Join(arguments), token);
                if (extraction.ExitCode != 0 || !File.Exists(temporary))
                    throw new InvalidOperationException("FFmpeg could not extract this track. " + extraction.StandardError.Trim());
                token.ThrowIfCancellationRequested();
                try { File.Move(temporary, output); }
                catch (IOException) { if (File.Exists(output)) return false; throw; }
                return true;
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
        }
    }
}
