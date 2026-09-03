using System.Diagnostics;
using System.Text;
using System.Threading;

namespace SakaSubtitleExporter
{
    internal static class ProcessRunner
    {
        public static ProcessResult Run(string executable, string arguments, CancellationToken cancellationToken = default(CancellationToken))
        {
            cancellationToken.ThrowIfCancellationRequested();
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
                try
                {
                    while (!process.WaitForExit(100))
                    {
                        if (!cancellationToken.IsCancellationRequested) continue;
                        try { process.Kill(); } catch (System.InvalidOperationException) { }
                        process.WaitForExit();
                        cancellationToken.ThrowIfCancellationRequested();
                    }
                }
                finally
                {
                    outputThread.Join();
                    errorThread.Join();
                }
                cancellationToken.ThrowIfCancellationRequested();

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
