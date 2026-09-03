using System;
using System.Linq;
using System.Windows;

namespace SakaSubtitleExporter
{
    internal static class Program
    {
        [STAThread]
        private static int Main(string[] args)
        {
            try
            {
                if (args.Length == 2 && string.Equals(args[0], "--extract", StringComparison.OrdinalIgnoreCase))
                {
                    SubtitleExporter.ExportAll(args[1]);
                    return 0;
                }
                var application = new Application();
                application.Run(new MainWindow(args.Where(a => a != "--ui").ToArray()));
                return 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Saka Subtitle Exporter", MessageBoxButton.OK, MessageBoxImage.Error);
                return 1;
            }
        }
    }
}
