using System;
using SakaSubtitleExporter;

namespace SakaSubtitleExporter.Tests
{
    internal static class Program
    {
        private static int Main()
        {
            try
            {
                MapsSubtitleFormats();
                SanitizesFileNames();
                QuotesWindowsArguments();
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

        private static void AssertEqual(string expected, string actual, string name)
        {
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
                throw new InvalidOperationException(name + ": expected=" + expected + ", actual=" + actual);
        }
    }
}
