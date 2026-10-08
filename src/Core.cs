using System;
using System.Drawing;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace EyeRest
{
    [DataContract]
    internal sealed class Preferences
    {
        [DataMember] public int IntervalMinutes = 20;
        [DataMember] public int RestSeconds = 20;
        [DataMember] public int SnoozeMinutes = 5;
        [DataMember] public string Message = "让眼睛休息一下\n看向远处，放松肩颈，慢慢呼吸。";
        [DataMember] public bool AlwaysOnTop = true;
        [DataMember] public bool Sound = false;
        [DataMember] public bool StartWithWindows = false;
        [DataMember] public string Background = "#F2F5F3";
        [DataMember] public string Surface = "#FFFFFF";
        [DataMember] public string Foreground = "#233D33";
        [DataMember] public string Accent = "#28765B";
        [DataMember] public string ReminderBackground = "#163E32";
        [DataMember] public string ReminderForeground = "#FFFFFF";
        [DataMember] public string FontFamily = "Microsoft YaHei UI";
        [DataMember] public int FontSize = 10;
        [DataMember] public int ReminderScalePercent = 100;
        [DataMember] public string ThemeId = "plain";
        [DataMember] public string ArtMessage = ArtQuotes[0];
        [DataMember] public string PortraitId = "chibi";
        [DataMember] public string QuoteMode = "fixed";
        [DataMember] public string[] CustomQuotes = new string[0];

        public static readonly string[] ArtQuotes = {
            "……已经忙了好一会儿了吧。\n先把视线从屏幕上挪开，看看远处。\n别急，我陪你歇一会儿。",
            "嗯……肩膀是不是又绷紧了？\n放松一点，慢慢呼吸。\n这会儿就先别勉强自己了。",
            "手头的事，等一小会儿也没关系。\n眨眨眼，看看窗外吧。\n休息好了，再慢慢来。",
            "……过来歇会儿吧。\n不用一直盯着屏幕，也不用赶着做什么。\n这点休息时间，留给自己就好。"
        };
        public bool IsArtTheme { get { return ThemeId == "art-netzach"; } }
        public string DisplayMessage { get { return IsArtTheme ? ArtMessage : Message; } }

        public Preferences Copy()
        {
            Preferences copy = (Preferences)MemberwiseClone();
            copy.CustomQuotes = CustomQuotes == null ? new string[0] : (string[])CustomQuotes.Clone(); return copy;
        }
        public static bool ValidColor(string value)
        {
            int number;
            return value != null && value.Length == 7 && value[0] == '#' &&
                Int32.TryParse(value.Substring(1), System.Globalization.NumberStyles.HexNumber,
                    System.Globalization.CultureInfo.InvariantCulture, out number);
        }
        public static Color ColorOf(string value) { return ColorTranslator.FromHtml(value); }
        public void Normalize()
        {
            IntervalMinutes = Math.Max(1, Math.Min(240, IntervalMinutes));
            RestSeconds = Math.Max(5, Math.Min(600, RestSeconds));
            SnoozeMinutes = Math.Max(1, Math.Min(60, SnoozeMinutes));
            FontSize = Math.Max(9, Math.Min(14, FontSize));
            ReminderScalePercent = ReminderScalePercent == 0 ? 100 : Math.Max(70, Math.Min(150, ReminderScalePercent));
            Preferences defaults = new Preferences();
            if (PortraitId != "standing" && PortraitId != "weary") PortraitId = "chibi";
            if (QuoteMode != "sequential" && QuoteMode != "random") QuoteMode = "fixed";
            CustomQuotes = QuoteCollection.Clean(CustomQuotes);
            if (!IsArtTheme) ThemeId = "plain";
            if (String.IsNullOrWhiteSpace(ArtMessage)) ArtMessage = ArtQuotes[0];
            if (ArtMessage.Length > 500) ArtMessage = ArtMessage.Substring(0, 500);
            if (String.IsNullOrWhiteSpace(Message)) Message = defaults.Message;
            if (Message.Length > 500) Message = Message.Substring(0, 500);
            if (String.IsNullOrWhiteSpace(FontFamily) || FontFamily.Length > 100) FontFamily = defaults.FontFamily;
            if (!ValidColor(Background)) Background = defaults.Background;
            if (!ValidColor(Surface)) Surface = defaults.Surface;
            if (!ValidColor(Foreground)) Foreground = defaults.Foreground;
            if (!ValidColor(Accent)) Accent = defaults.Accent;
            if (!ValidColor(ReminderBackground)) ReminderBackground = defaults.ReminderBackground;
            if (!ValidColor(ReminderForeground)) ReminderForeground = defaults.ReminderForeground;
        }
        public void SetPreset(int index)
        {
            string[][] colors = {
                new [] { "#F2F5F3", "#FFFFFF", "#233D33", "#28765B", "#163E32", "#FFFFFF" },
                new [] { "#171C25", "#232B38", "#E7EDF5", "#87BEAA", "#232B38", "#E7EDF5" },
                new [] { "#F6F0E5", "#FFFBF4", "#574735", "#986B33", "#514333", "#FFF5E3" },
                new [] { "#1B2115", "#2B3320", "#EEEBD2", "#C1D66E", "#202719", "#EEEBD2" }
            };
            index = Math.Max(0, Math.Min(3, index));
            ThemeId = index == 3 ? "art-netzach" : "plain";
            string[] palette = colors[index];
            Background = palette[0]; Surface = palette[1]; Foreground = palette[2];
            Accent = palette[3]; ReminderBackground = palette[4]; ReminderForeground = palette[5];
        }
    }

    internal static class ReminderSizing
    {
        public static Size Fit(Size baseline, int percent, Size available)
        {
            double scale = Math.Min(percent / 100.0, Math.Min((double)available.Width / baseline.Width, (double)available.Height / baseline.Height));
            return new Size(Math.Max(1, (int)Math.Floor(baseline.Width * scale)), Math.Max(1, (int)Math.Floor(baseline.Height * scale)));
        }
    }

    internal sealed class SettingsStore
    {
        public readonly string Path;
        public string Warning;
        public SettingsStore(string path) { Path = path; }
        private Preferences Read(string path)
        {
            using (FileStream stream = File.OpenRead(path))
            {
                Preferences value = (Preferences)new DataContractJsonSerializer(typeof(Preferences)).ReadObject(stream);
                if (value == null) throw new SerializationException("Empty preferences");
                value.Normalize();
                return value;
            }
        }
        public Preferences Load()
        {
            if (!File.Exists(Path)) return new Preferences();
            try { return Read(Path); }
            catch (Exception ex)
            {
                if (!Recoverable(ex)) throw;
                Warning = "设置文件无法读取，已恢复默认值。保存设置可修复文件。";
                try {
                    Preferences backup = Read(Path + ".bak");
                    Warning = "设置文件无法读取，已恢复上一份备份。";
                    return backup;
                }
                catch (Exception backupError) { if (!Recoverable(backupError)) throw; return new Preferences(); }
            }
        }
        private static bool Recoverable(Exception ex)
        {
            return ex is IOException || ex is UnauthorizedAccessException || ex is SerializationException || ex is System.Xml.XmlException || ex is FormatException;
        }
        public void Save(Preferences settings)
        {
            settings.Normalize();
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            string temp = Path + ".tmp";
            try
            {
                using (FileStream stream = new FileStream(temp, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    new DataContractJsonSerializer(typeof(Preferences)).WriteObject(stream, settings);
                    stream.Flush(true);
                }
                if (File.Exists(Path)) File.Replace(temp, Path, Path + ".bak");
                else File.Move(temp, Path);
            }
            finally { if (File.Exists(temp)) File.Delete(temp); }
        }
    }

    internal enum Phase { Working, Paused, Resting }
    internal sealed class Schedule
    {
        private readonly Func<double> now;
        private double deadline;
        private double pausedRemaining;
        public Phase State { get; private set; }
        public Schedule(Func<double> clock, int minutes) { now = clock; Start(minutes); }
        public double Remaining { get { return Math.Max(0, State == Phase.Paused ? pausedRemaining : deadline - now()); } }
        public void Start(int minutes) { State = Phase.Working; deadline = now() + minutes * 60; }
        public void Pause()
        {
            if (State != Phase.Working) return;
            pausedRemaining = Remaining; State = Phase.Paused;
        }
        public void Resume()
        {
            if (State != Phase.Paused) return;
            deadline = now() + pausedRemaining; State = Phase.Working;
        }
        public bool Tick()
        {
            if (State != Phase.Working || Remaining > 0) return false;
            State = Phase.Resting; return true;
        }
        public void RestNow() { State = Phase.Resting; }
        public void UpdateInterval(int minutes)
        {
            if (State == Phase.Paused) pausedRemaining = minutes * 60;
            else if (State == Phase.Working) Start(minutes);
        }
        public static string Format(double seconds)
        {
            int value = (int)Math.Ceiling(Math.Max(0, seconds));
            return String.Format("{0:00}:{1:00}", value / 60, value % 60);
        }
    }
}
