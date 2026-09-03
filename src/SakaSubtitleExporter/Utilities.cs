using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace SakaSubtitleExporter
{
    internal sealed class SubtitleOutputFormat
    {
        public SubtitleOutputFormat(string extension, string codec, string container)
        {
            Extension = extension;
            Codec = codec;
            Container = container;
        }

        public string Extension { get; private set; }
        public string Codec { get; private set; }
        public string Container { get; private set; }
    }

    internal static class SubtitleFormats
    {
        private static readonly HashSet<string> TextCodecs = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "mov_text", "tx3g", "text", "subviewer", "microdvd", "jacosub", "sami", "realtext"
        };

        public static SubtitleOutputFormat Resolve(string codec)
        {
            if (string.Equals(codec, "ass", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(codec, "ssa", StringComparison.OrdinalIgnoreCase))
                return new SubtitleOutputFormat(".ass", "copy", null);

            if (string.Equals(codec, "subrip", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(codec, "srt", StringComparison.OrdinalIgnoreCase))
                return new SubtitleOutputFormat(".srt", "copy", null);

            if (string.Equals(codec, "webvtt", StringComparison.OrdinalIgnoreCase))
                return new SubtitleOutputFormat(".vtt", "copy", null);

            if (string.Equals(codec, "hdmv_pgs_subtitle", StringComparison.OrdinalIgnoreCase))
                return new SubtitleOutputFormat(".sup", "copy", "sup");

            if (TextCodecs.Contains(codec ?? string.Empty))
                return new SubtitleOutputFormat(".srt", "srt", null);

            return new SubtitleOutputFormat(".mkv", "copy", "matroska");
        }
    }

    internal static class FileNames
    {
        public static string Safe(string value, string fallback, int maximumLength)
        {
            string safe = string.IsNullOrWhiteSpace(value) ? fallback : value;
            foreach (char invalid in Path.GetInvalidFileNameChars())
                safe = safe.Replace(invalid, '_');

            safe = safe.Trim().TrimEnd('.');
            if (safe.Length == 0) safe = fallback;
            if (safe.Length > maximumLength) safe = safe.Substring(0, maximumLength).TrimEnd();
            return safe;
        }
    }

    internal static class CommandLine
    {
        public static string Join(IEnumerable<string> arguments)
        {
            return string.Join(" ", arguments.Select(Quote).ToArray());
        }

        public static string Quote(string value)
        {
            if (value == null) return "\"\"";
            if (value.Length > 0 && value.IndexOfAny(new[] { ' ', '\t', '\n', '\v', '\"' }) < 0) return value;

            var result = new StringBuilder("\"");
            int backslashes = 0;
            foreach (char character in value)
            {
                if (character == '\\')
                {
                    backslashes++;
                    continue;
                }

                if (character == '\"')
                {
                    result.Append('\\', backslashes * 2 + 1).Append('\"');
                    backslashes = 0;
                    continue;
                }

                result.Append('\\', backslashes).Append(character);
                backslashes = 0;
            }

            result.Append('\\', backslashes * 2).Append('\"');
            return result.ToString();
        }
    }
}
