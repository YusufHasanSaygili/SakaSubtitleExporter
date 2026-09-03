using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;

namespace SakaSubtitleExporter
{
    internal abstract class Bindable : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void Notify([CallerMemberName] string name = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(name));
        }
    }

    internal sealed class SubtitleOption
    {
        public SubtitleOption(SubtitleStream stream, int ordinal) { Stream = stream; Ordinal = ordinal; }
        public SubtitleStream Stream { get; private set; }
        public int Ordinal { get; private set; }
        public int Id { get { return Stream.index; } }
        public string Label
        {
            get
            {
                string language = Stream.tags == null ? "und" : Stream.tags.language ?? "und";
                string name = language;
                var culture = CultureInfo.GetCultures(CultureTypes.NeutralCultures).FirstOrDefault(c =>
                    c.ThreeLetterISOLanguageName == language || c.TwoLetterISOLanguageName == language);
                if (culture != null) name = culture.EnglishName;
                if (language == "und") name = "Unknown language";
                string title = Stream.tags == null ? null : Stream.tags.title;
                string flag = Stream.disposition != null && Stream.disposition.@default == 1 ? ", default" :
                    Stream.disposition != null && Stream.disposition.forced == 1 ? ", forced" : string.Empty;
                return string.Format("{0:00}  {1} / {2} [{3}{4}]", Ordinal, name, string.IsNullOrWhiteSpace(title) ? "Untitled" : title,
                    (Stream.codec_name ?? "unknown").ToUpperInvariant(), flag);
            }
        }
    }

    internal sealed class TrackSlot : Bindable
    {
        private SubtitleOption selected;
        public TrackSlot(EpisodeItem owner, bool additional) { Owner = owner; IsAdditional = additional; }
        public EpisodeItem Owner { get; private set; }
        public bool IsAdditional { get; private set; }
        public string Caption { get { return IsAdditional ? "Additional subtitle" : "Subtitle"; } }
        public SubtitleOption Selected
        {
            get { return selected; }
            set
            {
                if (ReferenceEquals(selected, value)) return;
                if (value != null && Owner.Selections.Any(s => s != this && s.Selected != null && s.Selected.Id == value.Id))
                {
                    Owner.Status = "That track is already selected for this episode.";
                    Notify();
                    return;
                }
                selected = value;
                Notify();
                Owner.SelectionChanged();
            }
        }
    }

    internal sealed class EpisodeItem : Bindable
    {
        private string status = "Waiting to scan...";
        public EpisodeItem(string path) { Path = System.IO.Path.GetFullPath(path); }
        public event Action Changed;
        public string Path { get; private set; }
        public string FileName { get { return System.IO.Path.GetFileName(Path); } }
        public ObservableCollection<SubtitleOption> Tracks { get; } = new ObservableCollection<SubtitleOption>();
        public ObservableCollection<TrackSlot> Selections { get; } = new ObservableCollection<TrackSlot>();
        public bool IsReady { get { return Tracks.Count > 0; } }
        public bool CanAdd { get { return IsReady && Selections.Count < Tracks.Count; } }
        public bool IsValid { get { return IsReady && Selections.Count > 0 && Selections.All(s => s.Selected != null)
                    && Selections.Select(s => s.Selected.Id).Distinct().Count() == Selections.Count; } }
        public string Details { get { return Tracks.Count + " subtitle tracks / " + System.IO.Path.GetDirectoryName(Path); } }
        public string Status { get { return status; } set { status = value; Notify(); } }
        public void Load(IEnumerable<SubtitleStream> streams)
        {
            Selections.Clear(); Tracks.Clear();
            int ordinal = 0;
            foreach (var stream in streams) Tracks.Add(new SubtitleOption(stream, ++ordinal));
            if (Tracks.Count > 0)
            {
                var slot = new TrackSlot(this, false);
                Selections.Add(slot);
                slot.Selected = Tracks.FirstOrDefault(t => t.Stream.disposition != null && t.Stream.disposition.@default == 1) ?? Tracks[0];
            }
            Status = IsReady ? "Ready" : "No embedded subtitles found.";
            Notify("IsReady"); Notify("Details"); SelectionChanged();
        }
        public void AddSlot()
        {
            if (!CanAdd) return;
            Selections.Add(new TrackSlot(this, true));
            Status = "Choose an additional subtitle.";
            SelectionChanged(false);
        }
        public void RemoveSlot(TrackSlot slot)
        {
            if (!slot.IsAdditional) return;
            Selections.Remove(slot); SelectionChanged();
        }
        public void SelectionChanged(bool updateStatus = true)
        {
            if (updateStatus && IsReady) Status = IsValid ? "Ready" : "Choose an additional subtitle.";
            Notify("IsValid"); Notify("CanAdd");
            if (Changed != null) Changed();
        }
        public bool MatchPrimary(SubtitleOption source)
        {
            if (!IsReady || source == null) return false;
            var candidates = Tracks.Where(t => string.Equals(t.Stream.codec_name, source.Stream.codec_name, StringComparison.OrdinalIgnoreCase)
                && string.Equals(t.Stream.tags == null ? null : t.Stream.tags.language, source.Stream.tags == null ? null : source.Stream.tags.language, StringComparison.OrdinalIgnoreCase)
                && string.Equals(t.Stream.tags == null ? null : t.Stream.tags.title, source.Stream.tags == null ? null : source.Stream.tags.title, StringComparison.OrdinalIgnoreCase)).ToArray();
            if (candidates.Length != 1 || Selections.Skip(1).Any(s => s.Selected == candidates[0])) return false;
            Selections[0].Selected = candidates[0];
            return true;
        }
    }
}
