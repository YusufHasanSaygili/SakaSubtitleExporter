using System;
using System.Windows.Forms;

namespace SakaSubtitleExporter
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            if (args.Length != 2 || !string.Equals(args[0], "--extract", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show(
                    "Bir MKV dosyasına sağ tıklayıp\n\"Export Subtitles With Saka\" seçeneğini kullan.",
                    "Saka Subtitle Exporter",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
                return 0;
            }

            try
            {
                SubtitleExporter.ExportAll(args[1], true);
                return 0;
            }
            catch (Exception exception)
            {
                MessageBox.Show(exception.Message, "Saka Subtitle Exporter", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return 1;
            }
        }
    }
}
