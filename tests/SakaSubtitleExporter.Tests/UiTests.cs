using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
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
            Layout(root, 1024);
            Check(Descendants<ComboBox>(root).All(c => c.ActualWidth > 100), "Dropdown remains measurable at minimum width");
            Check(window.MascotImage.Source != null && window.Icon != null, "Embedded branding assets load");
            CheckMascotTransparency((BitmapSource)window.MascotImage.Source);
            CheckMascotRendering(window);
            Check(!window.MascotImage.IsHitTestVisible && !window.PatternBackdrop.IsHitTestVisible, "Decorations do not intercept input");
            Check(window.PatternBackdrop.Fill is DrawingBrush, "Repeating vector pattern loads");
            CheckMascotBounds(window, root);
            if (renderFolder != null) Render(window, root, renderFolder, "saka-compact.png");
            Layout(root, 1024, 640);
            CheckMascotBounds(window, root);
            Check(window.ExtractButton.TranslatePoint(new Point(0, window.ExtractButton.ActualHeight), root).Y <= root.ActualHeight, "Extraction stays visible at minimum height");
            if (renderFolder != null) Render(window, root, renderFolder, "saka-small.png");
            for (int i = 2; i <= 30; i++)
            {
                var batchItem = new EpisodeItem(System.IO.Path.Combine(System.IO.Path.GetTempPath(), string.Format("[Saka] Frieren - {0:00} (WEB 1080p HEVC Dual Audio).mkv", i)));
                batchItem.Load(Enumerable.Range(0, 24).Select(track => new SubtitleStream
                {
                    index = track + 2, codec_name = "ass",
                    tags = new StreamTags { language = "eng", title = track == 0 ? "Full dialogue" : "Subtitle track " + track },
                    disposition = new StreamDisposition { @default = track == 0 ? 1 : 0 }
                }));
                if (i == 15) { batchItem.AddSlot(); batchItem.Selections[1].Selected = batchItem.Tracks[1]; }
                window.Enqueue(batchItem);
            }
            Layout(root, 1240);
            var mascotPosition = window.MascotImage.TranslatePoint(new Point(0, 0), root);
            var queueScroll = Descendants<ScrollViewer>(window.EpisodeList).First();
            CheckScrolling(window, root, queueScroll);
            window.EpisodeList.ScrollIntoView(window.EpisodeList.Items[14]);
            Layout(root, 1240);
            Check(window.EpisodeList.Items.Count == 30 && queueScroll.ScrollableHeight > 0, "Long episode queue scrolls");
            Check(window.MascotImage.TranslatePoint(new Point(0, 0), root) == mascotPosition, "Mascot stays fixed when the queue scrolls");
            CheckMascotBounds(window, root);
            Check(window.ExtractButton.IsEnabled, "Thirty-file batch remains extractable");
            if (renderFolder != null) Render(window, root, renderFolder, "saka-batch.png");
            window.ThemeButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Layout(root, 1240);
            if (renderFolder != null) Render(window, root, renderFolder, "saka-batch-light.png");
            window.ThemeButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            window.SetDropFeedback(true);
            Layout(root, 1240);
            Check(window.DropArea.BorderBrush == window.Resources["Accent"] && window.DropArea.Background == window.Resources["Field"], "Valid drop highlights the card");
            Check(window.DropTitle.Text.StartsWith("Release"), "Drop instruction updates");
            if (renderFolder != null) Render(window, root, renderFolder, "saka-drop-active.png");
            window.SetDropFeedback(false);
            Check(window.DropTitle.Text == "Drop your MKV files", "Drop instruction resets");
            Check(MainWindow.ContainsMkv(new[] { "EPISODE.MKV", "notes.txt" }) && !MainWindow.ContainsMkv(new[] { "notes.txt" }), "Drop feedback accepts MKV files only");
            window.ClearButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Layout(root, 1240);
            Check(window.EmptyState.Visibility == Visibility.Visible, "Empty state survives visual redesign");
            if (renderFolder != null) Render(window, root, renderFolder, "saka-empty.png");
            Layout(root, 1878, 1035);
            CheckMascotBounds(window, root);
            if (renderFolder != null) Render(window, root, renderFolder, "saka-wide.png");
            window.Close();
            Check(!window.Scrolling.IsAnimating, "No animation timer remains after closing");
        }

        private static void CheckScrolling(MainWindow window, FrameworkElement root, ScrollViewer scroll)
        {
            Check(VirtualizingPanel.GetScrollUnit(window.EpisodeList) == ScrollUnit.Pixel && VirtualizingPanel.GetIsVirtualizing(window.EpisodeList), "Pixel scrolling retains virtualization");
            scroll.ScrollToVerticalOffset(42.5);
            Layout(root, 1240);
            Check(Math.Abs(scroll.VerticalOffset - 42.5) < 1, "Offsets are pixels, not whole episode jumps");
            var bar = Descendants<ScrollBar>(scroll).Single(b => b.Orientation == Orientation.Vertical);
            var thumb = (Thumb)bar.Template.FindName("BlueThumb", bar);
            Check(thumb != null && bar.ActualWidth == 20, "Wider custom scrollbar replaces the native bar");
            var fill = (Border)thumb.Template.FindName("ThumbFill", thumb);
            Check(fill.Background == window.Resources["ScrollThumbBody"], "Scrollbar uses the darker jelly gradient");
            Check(fill.ActualWidth >= 16 && ((ScaleTransform)fill.RenderTransform).ScaleY == 0.7, "Thumb visual is wider and thirty percent shorter without shrinking its hit target");

            double start = scroll.VerticalOffset;
            Check(window.Scrolling.ScrollWheel(-120, 3, true), "Wheel input is handled");
            double target = window.Scrolling.Target;
            window.Scrolling.Advance(TimeSpan.FromMilliseconds(55));
            Layout(root, 1240);
            Check(scroll.VerticalOffset > start && scroll.VerticalOffset < target, "Wheel animation produces intermediate pixel offsets");
            window.Scrolling.ScrollWheel(-120, 3, true);
            Check(window.Scrolling.Target > target, "Repeated wheel input accumulates smoothly");
            window.Scrolling.ScrollWheel(120, 3, true);
            Check(window.Scrolling.Target < scroll.VerticalOffset, "Reversing the wheel immediately reverses direction");
            window.Scrolling.Advance(TimeSpan.FromMilliseconds(200));
            Layout(root, 1240);
            Check(!window.Scrolling.IsAnimating && Math.Abs(scroll.VerticalOffset - window.Scrolling.Target) < 1, "Animation reaches its target and stops");

            window.Scrolling.ScrollWheel(-120, 3, true);
            thumb.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left) { RoutedEvent = Mouse.PreviewMouseDownEvent });
            Check(!window.Scrolling.IsAnimating, "Thumb input cancels wheel easing");
            double beforeDrag = scroll.VerticalOffset;
            thumb.RaiseEvent(new DragDeltaEventArgs(0, 10));
            Layout(root, 1240);
            Check(scroll.VerticalOffset > beforeDrag, "Dragging the custom thumb updates the viewport immediately");
            window.Scrolling.ScrollWheel(int.MaxValue, 3, false);
            Layout(root, 1240);
            Check(scroll.VerticalOffset == 0, "Wheel clamps at the start");
            window.Scrolling.ScrollWheel(int.MinValue, 3, false);
            Layout(root, 1240);
            Check(Math.Abs(scroll.VerticalOffset - scroll.ScrollableHeight) < 1, "Wheel clamps at the end");
            window.Scrolling.ScrollWheel(120, 3, false);
            Layout(root, 1240);
            Check(!window.Scrolling.IsAnimating, "Reduced-motion preference uses immediate scrolling");
            Check(!window.Scrolling.ScrollWheel(120, 0, true), "Disabled wheel scrolling is respected");
            scroll.ScrollToTop();
            Layout(root, 1240);
            window.Scrolling.ScrollWheel(-120, 3, true);
            double liveTarget = window.Scrolling.Target;
            int intermediateFrames = 0;
            var frame = new DispatcherFrame();
            var watch = System.Diagnostics.Stopwatch.StartNew();
            var sampler = new DispatcherTimer(DispatcherPriority.Background) { Interval = TimeSpan.FromMilliseconds(12) };
            sampler.Tick += (sender, args) =>
            {
                root.UpdateLayout();
                if (scroll.VerticalOffset > 0 && scroll.VerticalOffset < liveTarget - 0.1) intermediateFrames++;
                if (watch.ElapsedMilliseconds > 450) { sampler.Stop(); frame.Continue = false; }
            };
            sampler.Start();
            Dispatcher.PushFrame(frame);
            Check(intermediateFrames > 1 && !window.Scrolling.IsAnimating, "Live dispatcher produces multiple smooth frames and stops its timer");
            Check(Math.Abs(scroll.VerticalOffset - liveTarget) < 1, "Live animation reaches its final offset");
        }

        private static void CheckMascotBounds(MainWindow window, FrameworkElement root)
        {
            Point mascot = window.MascotImage.TranslatePoint(new Point(0, 0), root);
            Point workspace = window.WorkspacePanel.TranslatePoint(new Point(window.WorkspacePanel.ActualWidth, 0), root);
            Check(mascot.X >= workspace.X && window.MascotImage.ActualWidth >= 180, "Mascot stays to the right without overlapping controls");
            Check(mascot.Y + window.MascotImage.ActualHeight <= root.ActualHeight, "Full mascot fits in the viewport");
        }

        private static void CheckMascotRendering(MainWindow window)
        {
            var crop = window.MascotImage.Source as CroppedBitmap;
            Check(crop != null, "Mascot uses the original artwork with transparent padding cropped");
            var original = new FormatConvertedBitmap(crop.Source, PixelFormats.Bgra32, null, 0);
            byte[] pixels = new byte[original.PixelWidth * original.PixelHeight * 4];
            original.CopyPixels(pixels, original.PixelWidth * 4, 0);
            var bounds = crop.SourceRect;
            for (int y = 0; y < original.PixelHeight; y++)
                for (int x = 0; x < original.PixelWidth; x++)
                    if (x < bounds.X || x >= bounds.X + bounds.Width || y < bounds.Y || y >= bounds.Y + bounds.Height)
                        Check(pixels[(y * original.PixelWidth + x) * 4 + 3] == 0, "Cropping never removes visible artwork");
            Check(window.MascotImage.StretchDirection == StretchDirection.DownOnly, "Mascot is not enlarged beyond its natural size");
            Check(RenderOptions.GetBitmapScalingMode(window.MascotImage) == BitmapScalingMode.HighQuality, "Mascot keeps high-quality bitmap resampling");
            Check(window.UseLayoutRounding && window.SnapsToDevicePixels, "Window layout uses device-pixel alignment");
        }

        private static void CheckMascotTransparency(BitmapSource source)
        {
            var bitmap = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
            byte[] pixels = new byte[bitmap.PixelWidth * bitmap.PixelHeight * 4];
            bitmap.CopyPixels(pixels, bitmap.PixelWidth * 4, 0);
            int transparent = 0, opaqueWhite = 0;
            for (int i = 0; i < pixels.Length; i += 4)
            {
                if (pixels[i + 3] == 0) transparent++;
                if (pixels[i + 3] == 255 && pixels[i] > 235 && pixels[i + 1] > 235 && pixels[i + 2] > 235) opaqueWhite++;
            }
            Check(transparent > bitmap.PixelWidth * bitmap.PixelHeight / 2, "Mascot background has actual alpha, not a baked-in checkerboard");
            Check(opaqueWhite > 1000, "White details remain opaque");
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

        private static void Layout(FrameworkElement root, double width, double height = 740)
        {
            root.Measure(new Size(width, height));
            root.Arrange(new Rect(0, 0, width, height));
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
