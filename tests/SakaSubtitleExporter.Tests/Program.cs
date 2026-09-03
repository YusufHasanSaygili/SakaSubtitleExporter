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
                Console.WriteLine("Bütün testler geçti.");
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
            AssertEqual(".ass", SubtitleFormats.Resolve("ass").Extension, "ASS uzantısı");
            AssertEqual(".srt", SubtitleFormats.Resolve("subrip").Extension, "SRT uzantısı");
            AssertEqual("srt", SubtitleFormats.Resolve("mov_text").Codec, "Metin dönüştürme");
            AssertEqual(".sup", SubtitleFormats.Resolve("hdmv_pgs_subtitle").Extension, "PGS uzantısı");
            AssertEqual(".mkv", SubtitleFormats.Resolve("dvd_subtitle").Extension, "Bilinmeyen resim altyazısı");
        }

        private static void SanitizesFileNames()
        {
            AssertEqual("und", FileNames.Safe(null, "und", 25), "Boş dil");
            AssertEqual("abc", FileNames.Safe("abc.", "x", 25), "Sondaki nokta");
            AssertEqual("1234", FileNames.Safe("123456", "x", 4), "Uzunluk sınırı");
        }

        private static void QuotesWindowsArguments()
        {
            AssertEqual("plain", CommandLine.Quote("plain"), "Basit argüman");
            AssertEqual("\"iki kelime\"", CommandLine.Quote("iki kelime"), "Boşluklu argüman");
            AssertEqual("\"\"", CommandLine.Quote(string.Empty), "Boş argüman");
        }

        private static void AssertEqual(string expected, string actual, string name)
        {
            if (!string.Equals(expected, actual, StringComparison.Ordinal))
                throw new InvalidOperationException(name + ": beklenen=" + expected + ", gelen=" + actual);
        }
    }
}
