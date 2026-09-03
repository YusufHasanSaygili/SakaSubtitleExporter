using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;

namespace SakaSubtitleExporter
{
    internal static class SubtitleExporter
    {
        public const string ExportDirectory = @"A:\Anime";

        public static void ExportAll(string mkvPath, bool openFolder)
        {
            string resolvedPath = Path.GetFullPath(mkvPath);
            if (!File.Exists(resolvedPath)) throw new FileNotFoundException("MKV file not found.", resolvedPath);
            if (!string.Equals(Path.GetExtension(resolvedPath), ".mkv", StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("This tool only supports MKV files.");

            Directory.CreateDirectory(ExportDirectory);
            string baseName = FileNames.Safe(Path.GetFileNameWithoutExtension(resolvedPath), "mkv", 110);
            string reportPath = Path.Combine(ExportDirectory, baseName + "._Saka-report.txt");
            var report = new List<string>
            {
                "Source: " + resolvedPath,
                "Date: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                string.Empty
            };

            try
            {
                string ffprobe = ToolLocator.Find("ffprobe.exe");
                string ffmpeg = ToolLocator.Find("ffmpeg.exe");
                List<SubtitleStream> streams = Probe(ffprobe, resolvedPath);

                report.Add("Subtitle tracks: " + streams.Count);
                report.Add(string.Empty);

                for (int ordinal = 0; ordinal < streams.Count; ordinal++)
                    ExtractStream(ffmpeg, resolvedPath, baseName, streams[ordinal], ordinal + 1, report);

                if (streams.Count == 0) report.Add("No subtitle tracks were found in this MKV file.");
            }
            catch (Exception exception)
            {
                report.Add("ERROR: " + exception.Message);
                throw;
            }
            finally
            {
                File.WriteAllLines(reportPath, report.ToArray(), new UTF8Encoding(false));
                if (openFolder) OpenExportDirectory();
            }
        }

        private static List<SubtitleStream> Probe(string ffprobe, string mkvPath)
        {
            string entries = "stream=index,codec_type,codec_name:stream_tags=language,title:stream_disposition=default,forced";
            string arguments = "-v error -show_entries " + CommandLine.Quote(entries) + " -of json " + CommandLine.Quote(mkvPath);
            ProcessResult result = ProcessRunner.Run(ffprobe, arguments);
            if (result.ExitCode != 0)
                throw new InvalidOperationException("ffprobe error: " + result.StandardError.Trim());

            var serializer = new DataContractJsonSerializer(typeof(ProbeResult));
            ProbeResult probe;
            using (var input = new MemoryStream(Encoding.UTF8.GetBytes(result.StandardOutput)))
                probe = (ProbeResult)serializer.ReadObject(input);

            return (probe.streams ?? new List<SubtitleStream>())
                .Where(stream => string.Equals(stream.codec_type, "subtitle", StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        private static void ExtractStream(
            string ffmpeg,
            string mkvPath,
            string baseName,
            SubtitleStream stream,
            int ordinal,
            ICollection<string> report)
        {
            string codec = stream.codec_name ?? "unknown";
            string language = FileNames.Safe(stream.tags == null ? null : stream.tags.language, "und", 25);
            string title = FileNames.Safe(stream.tags == null ? null : stream.tags.title, "untitled", 70);
            SubtitleOutputFormat format = SubtitleFormats.Resolve(codec);
            string stem = string.Format("{0}.{1:D2}.{2}.{3}", baseName, ordinal, language, title);
            string outputPath = Path.Combine(ExportDirectory, stem + format.Extension);

            if (File.Exists(outputPath))
            {
                report.Add("SKIPPED (already exists): " + Path.GetFileName(outputPath));
                return;
            }

            var arguments = new List<string>
            {
                "-nostdin", "-n", "-v", "error", "-i", mkvPath,
                "-map", "0:" + stream.index,
                "-c:s", format.Codec
            };
            if (format.Container != null)
            {
                arguments.Add("-f");
                arguments.Add(format.Container);
            }
            arguments.Add(outputPath);

            ProcessResult result = ProcessRunner.Run(ffmpeg, CommandLine.Join(arguments));
            if (result.ExitCode != 0 || !File.Exists(outputPath))
            {
                if (File.Exists(outputPath)) File.Delete(outputPath);
                report.Add(string.Format("ERROR: track={0}, stream={1}, codec={2}: {3}", ordinal, stream.index, codec, result.StandardError.Trim()));
                return;
            }

            var flags = new List<string>();
            if (stream.disposition != null && stream.disposition.@default == 1) flags.Add("default");
            if (stream.disposition != null && stream.disposition.forced == 1) flags.Add("forced");
            string flagText = flags.Count == 0 ? string.Empty : " [" + string.Join(", ", flags.ToArray()) + "]";
            report.Add(string.Format("OK: {0} (stream={1}, codec={2}){3}", Path.GetFileName(outputPath), stream.index, codec, flagText));
        }

        private static void OpenExportDirectory()
        {
            try
            {
                Process.Start(new ProcessStartInfo("explorer.exe", CommandLine.Quote(ExportDirectory)) { UseShellExecute = true });
            }
            catch
            {
            }
        }
    }
}
