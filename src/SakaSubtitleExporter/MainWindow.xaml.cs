using Microsoft.Win32;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace SakaSubtitleExporter
{
    public partial class MainWindow : Window
    {
        private readonly ObservableCollection<EpisodeItem> episodes = new ObservableCollection<EpisodeItem>();
        private CancellationTokenSource cancellation;
        private bool busy;
        private bool light;
        private bool closeAfterWork;

        public MainWindow(string[] paths)
        {
            InitializeComponent();
            EpisodeList.ItemsSource = episodes;
            RefreshSummary();
            Loaded += async (s, e) => await AddFiles(paths);
        }

        private async void BrowseFiles(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFileDialog { Title = "Add MKV files", Filter = "Matroska video (*.mkv)|*.mkv", Multiselect = true, CheckFileExists = true };
            if (dialog.ShowDialog(this) == true) await AddFiles(dialog.FileNames);
        }

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.Effects = !busy && e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
            e.Handled = true;
        }

        private async void OnDrop(object sender, DragEventArgs e)
        {
            e.Handled = true;
            if (!busy && e.Data.GetDataPresent(DataFormats.FileDrop)) await AddFiles((string[])e.Data.GetData(DataFormats.FileDrop));
        }

        private async Task AddFiles(string[] paths)
        {
            if (busy || paths.Length == 0) return;
            var added = new System.Collections.Generic.List<EpisodeItem>();
            int ignored = 0;
            foreach (string path in paths)
            {
                if (!File.Exists(path) || !string.Equals(Path.GetExtension(path), ".mkv", StringComparison.OrdinalIgnoreCase)) { ignored++; continue; }
                string fullPath = Path.GetFullPath(path);
                if (episodes.Any(existing => string.Equals(existing.Path, fullPath, StringComparison.OrdinalIgnoreCase))) { ignored++; continue; }
                var item = new EpisodeItem(fullPath);
                Enqueue(item); added.Add(item);
            }
            if (added.Count == 0) { StatusText.Text = "No new MKV files to add. Duplicates and other file types are ignored."; return; }
            SetBusy(true, true);
            int scanned = 0;
            try
            {
                foreach (var item in added)
                {
                    cancellation.Token.ThrowIfCancellationRequested();
                    item.Status = "Reading subtitle tracks...";
                    StatusText.Text = string.Format("Reading file {0} of {1}...", scanned + 1, added.Count);
                    try
                    {
                        var tracks = await Task.Run(() => SubtitleExporter.Scan(item.Path, cancellation.Token));
                        item.Load(tracks);
                    }
                    catch (OperationCanceledException) { item.Status = "Scan cancelled. Remove and add this file to retry."; throw; }
                    catch (Exception ex) { item.Status = "Could not read tracks: " + ex.Message; }
                    scanned++;
                }
                StatusText.Text = string.Format("Read {0} files. {1}Saved beside each MKV; existing subtitles are kept.", scanned,
                    ignored > 0 ? ignored + " duplicates or non-MKV files ignored. " : string.Empty);
            }
            catch (OperationCanceledException)
            {
                foreach (var item in added.Where(item => item.Status == "Waiting to scan...")) item.Status = "Scan cancelled. Remove and add this file to retry.";
                StatusText.Text = "Scan cancelled. Files already read are ready to use.";
            }
            finally { SetBusy(false); }
        }

        private async void ExtractSelected(object sender, RoutedEventArgs e)
        {
            var ready = episodes.Where(item => item.IsReady).ToArray();
            if (busy || ready.Length == 0 || ready.Any(item => !item.IsValid)) return;
            int total = ready.Sum(item => item.Selections.Count), done = 0, exported = 0, skipped = 0, failed = 0;
            SetBusy(true);
            Progress.Maximum = total; Progress.Value = 0;
            try
            {
                foreach (var item in ready)
                {
                    cancellation.Token.ThrowIfCancellationRequested();
                    item.Status = "Extracting...";
                    var ids = item.Selections.Select(slot => slot.Selected.Id).ToArray();
                    var tracks = item.Tracks.Select(option => option.Stream).ToList();
                    int offset = done;
                    var progress = new Progress<int>(value =>
                    {
                        Progress.Value = offset + value;
                        StatusText.Text = string.Format("Processed {0} of {1} selected subtitles...", offset + value, total);
                    });
                    try
                    {
                        var result = await Task.Run(() => SubtitleExporter.ExportSelected(item.Path, tracks, ids, cancellation.Token, progress));
                        exported += result.Exported; skipped += result.Skipped; failed += result.Failed;
                        item.Status = string.Format("{0} extracted / {1} already exist / {2} failed", result.Exported, result.Skipped, result.Failed);
                        if (result.Errors.Count > 0) item.Status += ". " + string.Join(" ", result.Errors);
                    }
                    catch (OperationCanceledException) { item.Status = "Cancelled. Completed subtitles were kept."; throw; }
                    catch (Exception ex) { failed += ids.Length; item.Status = "Extraction failed: " + ex.Message; }
                    done += ids.Length;
                    Progress.Value = done;
                }
                StatusText.Text = string.Format("Done: {0} extracted, {1} already exist, {2} failed. Saved beside each MKV.", exported, skipped, failed);
            }
            catch (OperationCanceledException) { StatusText.Text = "Cancelled. Completed subtitles were kept beside their MKVs."; }
            finally { SetBusy(false); }
        }

        private void SetBusy(bool value, bool indeterminate = false)
        {
            busy = value;
            DropArea.IsEnabled = QueueToolbar.IsEnabled = EpisodeList.IsEnabled = !value;
            CancelButton.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            CancelButton.IsEnabled = value;
            Progress.Visibility = value ? Visibility.Visible : Visibility.Collapsed;
            Progress.IsIndeterminate = indeterminate;
            if (value) cancellation = new CancellationTokenSource();
            else { cancellation.Dispose(); cancellation = null; }
            RefreshSummary();
            if (!value && closeAfterWork) Close();
        }

        internal void Enqueue(EpisodeItem item)
        {
            item.Changed += RefreshSummary;
            episodes.Add(item);
            RefreshSummary();
        }

        private void RefreshSummary()
        {
            int selected = episodes.Sum(item => item.Selections.Count(slot => slot.Selected != null));
            int ready = episodes.Count(item => item.IsReady);
            FileCount.Text = episodes.Count + (episodes.Count == 1 ? " file" : " files");
            SummaryText.Text = selected == 0 ? "No subtitles selected" : string.Format("{0} subtitle{1} selected across {2} file{3}", selected, selected == 1 ? "" : "s", ready, ready == 1 ? "" : "s");
            EmptyState.Visibility = episodes.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
            ExtractButton.IsEnabled = !busy && ready > 0 && episodes.Where(item => item.IsReady).All(item => item.IsValid);
            MatchButton.IsEnabled = !busy && ready > 1;
            ClearButton.IsEnabled = !busy && episodes.Count > 0;
        }

        private void AddSlot(object sender, RoutedEventArgs e) { ((EpisodeItem)((FrameworkElement)sender).DataContext).AddSlot(); }
        private void RemoveSlot(object sender, RoutedEventArgs e) { var slot = (TrackSlot)((FrameworkElement)sender).DataContext; slot.Owner.RemoveSlot(slot); }
        private void RemoveFile(object sender, RoutedEventArgs e)
        {
            var item = (EpisodeItem)((FrameworkElement)sender).DataContext;
            item.Changed -= RefreshSummary; episodes.Remove(item); RefreshSummary();
        }
        private void ClearFiles(object sender, RoutedEventArgs e)
        {
            foreach (var item in episodes) item.Changed -= RefreshSummary;
            episodes.Clear(); RefreshSummary(); StatusText.Text = "Saved beside each MKV. Existing subtitles are kept.";
        }
        private void MatchAll(object sender, RoutedEventArgs e)
        {
            var ready = episodes.Where(item => item.IsReady).ToArray();
            if (ready.Length < 2) return;
            int count = ready.Skip(1).Count(item => item.MatchPrimary(ready[0].Selections[0].Selected));
            StatusText.Text = string.Format("Matched {0} other files by language, title and codec. {1} left unchanged.", count, ready.Length - 1 - count);
        }
        private void TrackSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            var combo = (ComboBox)sender;
            Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(() => combo.GetBindingExpression(ComboBox.SelectedItemProperty)?.UpdateTarget()));
        }
        private void CancelWork(object sender, RoutedEventArgs e) { cancellation?.Cancel(); CancelButton.IsEnabled = false; StatusText.Text = "Cancelling..."; }
        private void OnClosing(object sender, CancelEventArgs e)
        {
            if (!busy) return;
            e.Cancel = true;
            if (MessageBox.Show(this, "Cancel the current operation and close Saka? Completed subtitles will be kept.", "Close Saka", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            { closeAfterWork = true; cancellation.Cancel(); }
        }
        private void ToggleTheme(object sender, RoutedEventArgs e)
        {
            light = !light;
            string[] keys = { "Canvas", "Surface", "Field", "Line", "Ink", "Muted", "Accent", "AccentInk", "PatternInk" };
            string[] colors = light ? new[] { "#EDF3FA", "#FAFCFF", "#E7EFF8", "#B9CBDD", "#172B42", "#4B627C", "#2167A7", "#FFFFFF", "#D5E2F0" }
                : new[] { "#0D1725", "#152235", "#1C2D43", "#344A64", "#F0F5FC", "#A9BCD3", "#79BBF3", "#102236", "#20344D" };
            for (int i = 0; i < keys.Length; i++) Resources[keys[i]] = new SolidColorBrush((Color)ColorConverter.ConvertFromString(colors[i]));
            ThemeButton.Content = light ? "Dark theme" : "Light theme";
        }
    }
}
