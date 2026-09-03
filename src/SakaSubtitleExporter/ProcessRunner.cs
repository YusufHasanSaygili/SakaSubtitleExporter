using System.Diagnostics;
using System.Text;
using System.Threading;

namespace SakaSubtitleExporter
{
    internal static class ProcessRunner
    {
        public static ProcessResult Run(string executable, string arguments)
        {
            var start = new ProcessStartInfo
            {
                FileName = executable,
                Arguments = arguments,
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                StandardOutputEncoding = Encoding.UTF8,
                StandardErrorEncoding = Encoding.UTF8
            };

            using (var process = new Process { StartInfo = start })
            {
                process.Start();
                string standardOutput = null;
                string standardError = null;
                var outputThread = new Thread(() => standardOutput = process.StandardOutput.ReadToEnd());
                var errorThread = new Thread(() => standardError = process.StandardError.ReadToEnd());
                outputThread.Start();
                errorThread.Start();
                process.WaitForExit();
                outputThread.Join();
                errorThread.Join();

                return new ProcessResult
                {
                    ExitCode = process.ExitCode,
                    StandardOutput = standardOutput ?? string.Empty,
                    StandardError = standardError ?? string.Empty
                };
            }
        }
    }
}
