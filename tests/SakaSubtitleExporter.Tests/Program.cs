using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using SakaSubtitleExporter;

namespace SakaSubtitleExporter.Tests
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                MapsSubtitleFormats();
                SanitizesFileNames();
                QuotesWindowsArguments();
                BatchSelections();
                SafeOutputPaths();
                UiTests.Run(args.Length == 2 && args[0] == "--render-ui" ? args[1] : null);
                if (args.Length == 2 && args[0] == "--integration") Integration(args[1]);
                Console.WriteLine("All tests passed.");
                return 0;
            }
            catch (Exception exception)
            {
                Console.Error.WriteLine(exception.Message);
                return 1;
            }
        }

        private static void MapsSubtitleFormats()
        {
            AssertEqual(".ass", SubtitleFormats.Resolve("ass").Extension, "ASS extension");
            AssertEqual(".srt", SubtitleFormats.Resolve("subrip").Extension, "SRT extension");
            AssertEqual("srt", SubtitleFormats.Resolve("mov_text").Codec, "Text conversion");
            AssertEqual(".sup", SubtitleFormats.Resolve("hdmv_pgs_subtitle").Extension, "PGS extension");
            AssertEqual(".mkv", SubtitleFormats.Resolve("dvd_subtitle").Extension, "Unknown bitmap subtitle");
        }

        private static void SanitizesFileNames()
        {
            AssertEqual("und", FileNames.Safe(null, "und", 25), "Missing language");
            AssertEqual("abc", FileNames.Safe("abc.", "x", 25), "Trailing period");
            AssertEqual("1234", FileNames.Safe("123456", "x", 4), "Length limit");
        }

        private static void QuotesWindowsArguments()
        {
            AssertEqual("plain", CommandLine.Quote("plain"), "Simple argument");
            AssertEqual("\"two words\"", CommandLine.Quote("two words"), "Argument with spaces");
            AssertEqual("\"\"", CommandLine.Quote(string.Empty), "Empty argument");
        }

        private static List<SubtitleStream> Tracks(int count)
        {
            return Enumerable.Range(0, count).Select(i => new SubtitleStream
            {
                index = i + 2, codec_name = "subrip", codec_type = "subtitle",
                tags = new StreamTags { language = i % 2 == 0 ? "eng" : "tur", title = "Track " + (i + 1) },
                disposition = new StreamDisposition { @default = i == 4 ? 1 : 0 }
            }).ToList();
        }

        private static void BatchSelections()
        {
            var batch = Enumerable.Range(1, 30).Select(i => new EpisodeItem(Path.Combine(Path.GetTempPath(), "Episode " + i + ".mkv"))).ToArray();
            foreach (var item in batch) item.Load(Tracks(24));
            Check(batch.All(item => item.Selections.Count == 1 && item.IsValid), "One valid selection per episode");
            Check(batch.All(item => item.Selections[0].Selected.Id == 6), "Prefer default subtitle");
            var episode15 = batch[14];
            episode15.AddSlot();
            Check(!episode15.IsValid && episode15.Selections[1].Selected == null, "Additional subtitle requires a choice");
            episode15.Selections[1].Selected = episode15.Tracks[0];
            Check(episode15.IsValid && batch.Sum(item => item.Selections.Count) == 31, "30 episodes with 31 selections");
            episode15.Selections[1].Selected = episode15.Selections[0].Selected;
            Check(episode15.Selections[1].Selected.Id == 2 && episode15.IsValid, "Duplicate choices rejected");
            episode15.RemoveSlot(episode15.Selections[0]);
            Check(episode15.Selections.Count == 2, "Primary slot cannot be removed");
            episode15.RemoveSlot(episode15.Selections[1]);
            Check(episode15.Selections.Count == 1 && episode15.IsValid, "Additional slot can be removed");
            var reversed = Tracks(24); reversed.Reverse(); batch[1].Load(reversed);
            batch[0].Selections[0].Selected = batch[0].Tracks[7];
            Check(batch[1].MatchPrimary(batch[0].Selections[0].Selected), "Match reordered streams by metadata");
            Check(batch[1].Selections[0].Selected.Id == 9, "Correct reordered stream selected");
            var ambiguous = Tracks(2); ambiguous[1].tags = ambiguous[0].tags;
            batch[2].Load(ambiguous);
            Check(!batch[2].MatchPrimary(batch[2].Tracks[0]), "Do not guess ambiguous matches");
            batch[3].Load(Tracks(0));
            Check(!batch[3].IsReady && !batch[3].IsValid && !batch[3].CanAdd, "No-subtitle file is not extractable");
            batch[4].Load(Tracks(1)); batch[4].AddSlot();
            Check(batch[4].Selections.Count == 1 && !batch[4].CanAdd, "No extra slot beyond track count");
            Check(batch[4].Selections[0].Selected.Id == 2, "Fallback to first track");
        }

        private static void SafeOutputPaths()
        {
            string path = Path.Combine(Path.GetTempPath(), "Saka season", "Episode 15.mkv");
            string output = SubtitleExporter.OutputPath(path, Tracks(2)[1], 2);
            AssertEqual(Path.GetDirectoryName(path), Path.GetDirectoryName(output), "Output beside source MKV");
            Check(Path.GetFileName(output).StartsWith("Episode 15.02.tur."), "Stable original track ordinal");
        }

        private static void Integration(string parent)
        {
            string folder = Path.Combine(Path.GetFullPath(parent), "saka-integration-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(folder);
            string fixture = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Fixtures", "sample.srt");
            string ffmpeg = ToolLocator.Find("ffmpeg.exe");
            string master = Path.Combine(folder, "Episode 01.mkv");
            var args = new List<string> { "-nostdin", "-n", "-v", "error", "-f", "lavfi", "-i", "color=size=32x32:rate=1:duration=2", "-i", fixture, "-map", "0:v" };
            for (int i = 0; i < 24; i++) { args.Add("-map"); args.Add("1:s:0"); }
            args.AddRange(new[] { "-c:v", "ffv1", "-c:s", "copy" });
            for (int i = 0; i < 24; i++) args.AddRange(new[] { "-metadata:s:s:" + i, "language=" + (i % 2 == 0 ? "eng" : "tur"), "-metadata:s:s:" + i, "title=Track " + (i + 1), "-disposition:s:" + i, i == 4 ? "default" : "0" });
            args.Add(master);
            var made = ProcessRunner.Run(ffmpeg, CommandLine.Join(args));
            Check(made.ExitCode == 0, "Create 24-track fixture: " + made.StandardError);
            int extracted = 0;
            for (int i = 1; i <= 30; i++)
            {
                string path = Path.Combine(folder, "Episode " + i.ToString("00") + ".mkv");
                if (i != 1) File.Copy(master, path);
                var streams = SubtitleExporter.Scan(path);
                Check(streams.Count == 24, "Probe all 24 tracks");
                var item = new EpisodeItem(path); item.Load(streams);
                if (i == 15) { item.AddSlot(); item.Selections[1].Selected = item.Tracks[1]; }
                var ids = item.Selections.Select(slot => slot.Selected.Id).ToArray();
                var result = SubtitleExporter.ExportSelected(path, streams, ids, CancellationToken.None);
                Check(result.Exported == ids.Length && result.Failed == 0, "Extract selected tracks for episode " + i);
                extracted += result.Exported;
                foreach (var id in ids)
                {
                    var track = streams.First(s => s.index == id);
                    string output = SubtitleExporter.OutputPath(path, track, streams.IndexOf(track) + 1);
                    string before = File.ReadAllText(output);
                    Check(before.Contains("İş bulmamız gerekecek.") && before.Contains("Hello, world."), "Subtitle content intact");
                    var repeat = SubtitleExporter.ExportSelected(path, streams, new[] { id }, CancellationToken.None);
                    Check(repeat.Skipped == 1 && repeat.Exported == 0 && File.ReadAllText(output) == before, "No overwrite on repeat extraction");
                }
            }
            Check(extracted == 31 && Directory.GetFiles(folder, "*.srt").Length == 31, "Exactly 31 outputs for 30 files");
            Check(Directory.GetFiles(folder, ".saka-*").Length == 0, "No temporary files left");
            using (var token = new CancellationTokenSource(300))
            {
                bool cancelled = false;
                try { ProcessRunner.Run(ffmpeg, "-nostdin -v error -re -f lavfi -i color=size=32x32:rate=1 -f null -", token.Token); }
                catch (OperationCanceledException) { cancelled = true; }
                Check(cancelled, "Cancel a running FFmpeg process");
            }
            Console.WriteLine("Integration fixtures: " + folder);
        }

        private static void Check(bool condition, string name)
        {
            if (!condition) throw new InvalidOperationException(name);
        }

        private static void AssertEqual(string expected, string actual, string name)
        {
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
                throw new InvalidOperationException(name + ": expected=" + expected + ", actual=" + actual);
        }
    }
}
