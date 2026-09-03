using System;
using System.IO;
using System.Linq;

namespace SakaSubtitleExporter
{
    internal static class ToolLocator
    {
        public static string Find(string fileName)
        {
            string path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
            foreach (string entry in path.Split(Path.PathSeparator))
            {
                string directory = entry.Trim().Trim('"');
                if (directory.Length == 0) continue;

                string candidate = Path.Combine(directory, fileName);
                if (File.Exists(candidate)) return candidate;
            }

            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string packageRoot = Path.Combine(localAppData, "Microsoft", "WinGet", "Packages");
            if (Directory.Exists(packageRoot))
            {
                foreach (string package in Directory.GetDirectories(packageRoot, "Gyan.FFmpeg*"))
                {
                    try
                    {
                        string candidate = Directory.GetFiles(package, fileName, SearchOption.AllDirectories).FirstOrDefault();
                        if (candidate != null) return candidate;
                    }
                    catch (UnauthorizedAccessException)
                    {
                    }
                }
            }

            throw new FileNotFoundException(fileName + " was not found. FFmpeg must be installed.");
        }
    }
}
