using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace SakaSubtitleExporter
{
    // Pixel virtualization keeps large queues cheap. Only wheel input is eased;
    // dragging the thumb and keyboard navigation retain immediate native behavior.
    internal sealed class SmoothScrollController : IDisposable
    {
        private readonly ListBox list;
        private readonly DispatcherTimer timer;
        private readonly Stopwatch clock = new Stopwatch();
        private ScrollViewer viewer;
        private double start, target;
        private int direction;
        internal bool IsAnimating => timer.IsEnabled;
        internal double Target => target;

        internal SmoothScrollController(ListBox list)
        {
            this.list = list;
            timer = new DispatcherTimer(DispatcherPriority.Render, list.Dispatcher) { Interval = TimeSpan.FromMilliseconds(16) };
            timer.Tick += OnTick;
            list.PreviewMouseWheel += OnWheel;
            list.PreviewMouseDown += OnDirectInput;
            list.PreviewKeyDown += OnKeyDown;
            list.Unloaded += OnUnloaded;
        }

        private void OnWheel(object sender, MouseWheelEventArgs e)
        {
            if (ScrollWheel(e.Delta, SystemParameters.WheelScrollLines, SystemParameters.ClientAreaAnimation)) e.Handled = true;
        }

        internal bool ScrollWheel(int delta, int lines, bool animate)
        {
            if (delta == 0 || lines == 0) return false;
            viewer = viewer ?? FindViewer(list);
            if (viewer == null || viewer.ScrollableHeight <= 0) return false;
            int nextDirection = Math.Sign(delta);
            double origin = IsAnimating && nextDirection == direction ? target : viewer.VerticalOffset;
            double distance = lines < 0 ? viewer.ViewportHeight : lines * 24.0;
            target = Math.Max(0, Math.Min(viewer.ScrollableHeight, origin - delta / 120.0 * distance));
            start = viewer.VerticalOffset;
            direction = nextDirection;
            if (!animate || Math.Abs(target - start) < 0.1)
            {
                Stop();
                viewer.ScrollToVerticalOffset(target);
                return true;
            }
            clock.Restart();
            timer.Start();
            return true;
        }

        private void OnTick(object sender, EventArgs e) { Advance(clock.Elapsed); }

        // Deterministic stepping also lets tests validate intermediate frames.
        internal void Advance(TimeSpan elapsed)
        {
            if (!IsAnimating || viewer == null) return;
            target = Math.Max(0, Math.Min(viewer.ScrollableHeight, target));
            double progress = Math.Max(0, Math.Min(1, elapsed.TotalMilliseconds / 170.0));
            double eased = 1 - Math.Pow(1 - progress, 3);
            viewer.ScrollToVerticalOffset(start + (target - start) * eased);
            if (progress >= 1) Stop();
        }

        internal void Stop() { timer.Stop(); clock.Stop(); }
        private void OnDirectInput(object sender, MouseButtonEventArgs e) { Stop(); }
        private void OnKeyDown(object sender, KeyEventArgs e) { Stop(); }
        private void OnUnloaded(object sender, RoutedEventArgs e) { Stop(); viewer = null; }

        private static ScrollViewer FindViewer(DependencyObject parent)
        {
            for (int i = 0; i < VisualTreeHelper.GetChildrenCount(parent); i++)
            {
                var child = VisualTreeHelper.GetChild(parent, i);
                if (child is ScrollViewer scroll) return scroll;
                var nested = FindViewer(child);
                if (nested != null) return nested;
            }
            return null;
        }

        public void Dispose()
        {
            Stop();
            timer.Tick -= OnTick;
            list.PreviewMouseWheel -= OnWheel;
            list.PreviewMouseDown -= OnDirectInput;
            list.PreviewKeyDown -= OnKeyDown;
            list.Unloaded -= OnUnloaded;
            viewer = null;
        }
    }
}
