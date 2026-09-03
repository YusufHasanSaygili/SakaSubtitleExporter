using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using SakaSubtitleExporter;

namespace SakaSubtitleExporter.Tests
{
    // In-process component tests: no desktop input, UI Automation or user files.
    internal static class UiTests
    {
        internal static void Run(string renderFolder = null)
        {
            var window = new MainWindow(new string[0]);
            var root = (FrameworkElement)window.Content;
            var episode = new EpisodeItem(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "Saka UI test.mkv"));
            episode.Load(new[]
            {
                new SubtitleStream { index = 2, codec_name = "ass", tags = new StreamTags { language = "eng", title = "Full dialogue" }, disposition = new StreamDisposition { @default = 1 } },
                new SubtitleStream { index = 3, codec_name = "ass", tags = new StreamTags { language = "eng", title = "Signs and songs" } }
            });
            window.Enqueue(episode);
            Layout(root, 1100);
            var add = Descendants<Button>(root).Single(b => ReferenceEquals(b.DataContext, episode) && b.Content.ToString().Contains("additional subtitle"));
            add.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Layout(root, 1100);
            Check(episode.Selections.Count == 2, "Additional button creates one slot");
            var combos = Descendants<ComboBox>(root).ToArray();
            Check(combos.Length == 2, "Additional dropdown rendered");
            Check(!window.ExtractButton.IsEnabled, "Blank additional selection blocks extraction");
            combos[1].SelectedItem = episode.Tracks[1];
            Pump();
            Check(episode.Selections[1].Selected == episode.Tracks[1], "Dropdown writes through to selected track");
            Check(window.ExtractButton.IsEnabled, "Valid selections enable extraction");
            Layout(root, 1100);
            if (renderFolder != null) Render(window, root, renderFolder, "saka-dark.png");
            combos[1].SelectedItem = episode.Tracks[0];
            Pump();
            Check(episode.Selections[1].Selected == episode.Tracks[1] && combos[1].SelectedItem == episode.Tracks[1], "Duplicate choice restored in model and dropdown");
            var remove = Descendants<Button>(root).Single(b => ReferenceEquals(b.DataContext, episode.Selections[1]) && b.Content.ToString() == "Remove");
            remove.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Layout(root, 1100);
            Check(episode.Selections.Count == 1 && Descendants<ComboBox>(root).Count() == 1, "Remove button removes additional selector");
            window.ThemeButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Check(window.ThemeButton.Content.ToString() == "Dark theme", "Light theme switch");
            Layout(root, 1100);
            if (renderFolder != null) Render(window, root, renderFolder, "saka-light.png");
            window.ThemeButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Check(window.ThemeButton.Content.ToString() == "Light theme", "Dark theme switch");
            Layout(root, 780);
            Check(Descendants<ComboBox>(root).All(c => c.ActualWidth > 100), "Dropdown remains measurable at minimum width");
            window.Close();
        }

        private static void Render(Window window, FrameworkElement root, string folder, string name)
        {
            System.IO.Directory.CreateDirectory(folder);
            int width = (int)(root.ActualWidth + root.Margin.Left + root.Margin.Right);
            int height = (int)(root.ActualHeight + root.Margin.Top + root.Margin.Bottom);
            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            var background = new DrawingVisual();
            using (var drawing = background.RenderOpen()) drawing.DrawRectangle(window.Background, null, new Rect(0, 0, width, height));
            bitmap.Render(background);
            bitmap.Render(root);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));
            using (var output = System.IO.File.Create(System.IO.Path.Combine(folder, name))) encoder.Save(output);
        }

        private static void Layout(FrameworkElement root, double width)
        {
            root.Measure(new Size(width, 740));
            root.Arrange(new Rect(0, 0, width, 740));
            root.UpdateLayout();
            Pump();
        }

        private static void Pump()
        {
            var frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() => frame.Continue = false));
            Dispatcher.PushFrame(frame);
        }

        private static IEnumerable<T> Descendants<T>(DependencyObject parent) where T : DependencyObject
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is T) yield return (T)child;
                foreach (var descendant in Descendants<T>(child)) yield return descendant;
            }
        }

        private static void Check(bool condition, string message)
        {
            if (!condition) throw new InvalidOperationException("UI: " + message);
        }
    }
}
