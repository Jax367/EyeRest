using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

[assembly: System.Reflection.AssemblyTitle("目间 EyeRest")]
[assembly: System.Reflection.AssemblyDescription("轻量桌面休息提醒")]
[assembly: System.Reflection.AssemblyVersion("1.5.0.0")]
[assembly: System.Reflection.AssemblyFileVersion("1.5.0.0")]
[assembly: System.Reflection.AssemblyCompany("Lar")]

namespace EyeRest
{
    internal static class Program
    {
        public static Icon AppIcon;
        [DllImport("user32.dll", SetLastError = true)] private static extern bool DestroyIcon(IntPtr handle);
        [STAThread]
        private static int Main(string[] args)
        {
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            Application.EnableVisualStyles(); Application.SetCompatibleTextRenderingDefault(false);
            AppIcon = CreateIcon(new Preferences());
            try
            {
                if (args.Length >= 2 && args[0] == "--self-test") return SelfTests.Run(args[1]);
                if (args.Length >= 2 && args[0] == "--render-preview") return SelfTests.Render(args[1]);
                bool created;
                using (Mutex mutex = new Mutex(true, "Local\\EyeRest.Desktop.Singleton", out created))
                using (EventWaitHandle activate = new EventWaitHandle(false, EventResetMode.AutoReset, "Local\\EyeRest.Desktop.Activate"))
                {
                    bool background = Array.IndexOf(args, "--startup") >= 0;
                    if (!created) { if (!background) activate.Set(); return 0; }
                    try { using (RestApplication app = new RestApplication(activate, background)) Application.Run(app); }
                    finally { mutex.ReleaseMutex(); }
                }
                return 0;
            }
            catch (Exception error)
            {
                if (args.Length >= 2 && args[0].StartsWith("--"))
                {
                    string report = args[0] == "--self-test" ? args[1] : Path.Combine(args[1], "error.txt");
                    File.WriteAllText(report, error.ToString()); return 1;
                }
                MessageBox.Show("程序遇到问题：\n" + error.Message, "目间", MessageBoxButtons.OK, MessageBoxIcon.Error); return 1;
            }
            finally { ArtResources.Dispose(); AppIcon.Dispose(); }
        }
        internal static Icon CreateIcon(Preferences p)
        {
            using (Bitmap bitmap = new Bitmap(32, 32))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (Brush fill = new SolidBrush(Preferences.ColorOf(p.Accent)))
            using (Pen line = new Pen(Styles.Contrast(Preferences.ColorOf(p.Accent)), 2))
            {
                graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                graphics.FillEllipse(fill, 1, 1, 30, 30);
                if (p.IsArtTheme)
                {
                    graphics.DrawRectangle(line, 7, 8, 18, 16); graphics.DrawLine(line, 16, 8, 16, 24);
                    graphics.DrawLine(line, 10, 12, 13, 12); graphics.DrawLine(line, 19, 12, 22, 12);
                }
                else
                {
                    graphics.DrawArc(line, 7, 9, 18, 12, 0, 180);
                    graphics.DrawLine(line, 10, 20, 8, 23); graphics.DrawLine(line, 16, 21, 16, 25); graphics.DrawLine(line, 22, 20, 24, 23);
                }
                IntPtr handle = bitmap.GetHicon();
                try { using (Icon temporary = Icon.FromHandle(handle)) return (Icon)temporary.Clone(); }
                finally { DestroyIcon(handle); }
            }
        }
    }

    internal sealed class RestApplication : ApplicationContext
    {
        private readonly SettingsStore store;
        private Preferences settings;
        private readonly Stopwatch clock = Stopwatch.StartNew();
        private readonly Schedule schedule;
        private readonly SettingsForm form;
        private readonly NotifyIcon tray;
        private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer { Interval = 1000 };
        private readonly EventWaitHandle activate;
        private readonly ContextMenuStrip menu;
        private readonly ToolStripMenuItem pauseItem;
        private readonly ToolStripMenuItem statusItem;
        private readonly IStartupRegistration startup;
        private Icon trayIcon;
        private Font menuFont;
        private ReminderForm reminder;
        private ReminderForm preview;
        private bool exiting;
        private readonly QuotePlayer quotes = new QuotePlayer();

        public RestApplication(EventWaitHandle activation, bool background, SettingsStore configuredStore = null, IStartupRegistration startupRegistration = null)
        {
            activate = activation;
            startup = startupRegistration ?? new StartupRegistration();
            store = configuredStore ?? new SettingsStore(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "EyeRest", "settings.json"));
            settings = store.Load();
            try { settings.StartWithWindows = !String.IsNullOrEmpty(startup.Read()); }
            catch (Exception ex)
            {
                if (!(ex is System.Security.SecurityException) && !(ex is UnauthorizedAccessException) && !(ex is IOException)) throw;
                store.Warning = "无法读取开机自启状态：" + ex.Message;
            }
            schedule = new Schedule(delegate { return clock.Elapsed.TotalSeconds; }, settings.IntervalMinutes);
            form = new SettingsForm(settings, Save, Preview, TogglePause);
            menu = new ContextMenuStrip();
            statusItem = new ToolStripMenuItem { Enabled = false }; menu.Items.Add(statusItem); menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add("打开设置", null, delegate { ShowSettings(); });
            menu.Items.Add("现在休息", null, delegate { ShowReminder(); });
            pauseItem = new ToolStripMenuItem("暂停提醒", null, delegate { TogglePause(); }); menu.Items.Add(pauseItem);
            menu.Items.Add(new ToolStripSeparator()); menu.Items.Add("退出目间", null, delegate { ExitThread(); });
            tray = new NotifyIcon { Icon = Program.AppIcon, Text = "目间 · 休息提醒", ContextMenuStrip = menu, Visible = true };
            tray.DoubleClick += delegate { ShowSettings(); };
            tray.MouseClick += delegate(object sender, MouseEventArgs e) { if (e.Button == MouseButtons.Left) menu.Show(Cursor.Position); };
            ApplyTrayTheme();
            timer.Tick += delegate {
                if (activate.WaitOne(0)) ShowSettings();
                if (schedule.Tick()) ShowReminder();
                UpdateStatus();
            };
            UpdateStatus(); timer.Start(); if (!background) form.Show();
            if (store.Warning != null) MessageBox.Show(form, store.Warning, "设置恢复", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        private bool Save(Preferences value)
        {
            try { SettingsCommit.Save(value, store, startup, Application.ExecutablePath); }
            catch (Exception ex)
            {
                if (!(ex is IOException) && !(ex is UnauthorizedAccessException) && !(ex is System.Security.SecurityException) && !(ex is AggregateException)) throw;
                MessageBox.Show(form, "设置未能保存。\n" + ex.Message, "无法保存", MessageBoxButtons.OK, MessageBoxIcon.Warning); return false;
            }
            bool changedInterval = settings.IntervalMinutes != value.IntervalMinutes;
            settings = value.Copy(); if (changedInterval) schedule.UpdateInterval(settings.IntervalMinutes);
            ApplyTrayTheme();
            UpdateStatus(); return true;
        }
        private void ApplyTrayTheme()
        {
            ThemeMenuRenderer.Apply(menu, settings);
            Font nextFont = Styles.FontFor(settings, settings.FontSize, FontStyle.Regular);
            if (menuFont != null && menuFont.Equals(nextFont)) nextFont.Dispose();
            else { menu.Font = nextFont; if (menuFont != null) menuFont.Dispose(); menuFont = nextFont; }
            Icon nextIcon = Program.CreateIcon(settings); tray.Icon = nextIcon;
            if (trayIcon != null) trayIcon.Dispose(); trayIcon = nextIcon;
        }
        private void Preview(Preferences value)
        {
            if (preview != null) { preview.Close(); }
            ReminderForm window = new ReminderForm(value, true, quotes.Select(value, false)); preview = window;
            window.FormClosed += delegate { if (preview == window) preview = null; window.Dispose(); };
            window.Show();
        }
        private void ShowSettings() { form.Show(); form.WindowState = FormWindowState.Normal; form.Activate(); }
        private void TogglePause()
        {
            if (schedule.State == Phase.Paused) schedule.Resume(); else schedule.Pause(); UpdateStatus();
        }
        private void ShowReminder()
        {
            if (reminder != null) { reminder.Activate(); return; }
            if (preview != null) preview.Close();
            schedule.RestNow();
            ReminderForm window = new ReminderForm(settings.Copy(), false, quotes.Select(settings, true)); reminder = window;
            window.FormClosed += delegate {
                reminder = null;
                if (!exiting) { schedule.Start(window.Snoozed ? settings.SnoozeMinutes : settings.IntervalMinutes); UpdateStatus(); }
                window.Dispose();
            };
            window.Show();
            if (settings.Sound) System.Media.SystemSounds.Asterisk.Play();
            UpdateStatus();
        }
        private void UpdateStatus()
        {
            form.UpdateStatus(schedule);
            string status = schedule.State == Phase.Resting ? "正在休息" :
                (schedule.State == Phase.Paused ? "已暂停 · " : "下次休息 · ") + Schedule.Format(schedule.Remaining);
            tray.Text = "目间 · " + status; statusItem.Text = status;
            pauseItem.Text = schedule.State == Phase.Paused ? "继续提醒" : "暂停提醒";
            pauseItem.Enabled = schedule.State != Phase.Resting;
        }
        protected override void ExitThreadCore()
        {
            exiting = true; timer.Stop(); tray.Visible = false;
            if (preview != null) preview.Close(); if (reminder != null) reminder.Close();
            form.AllowClose = true; form.Close(); base.ExitThreadCore();
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) { timer.Dispose(); tray.Dispose(); menu.Dispose(); form.Dispose(); if (trayIcon != null) trayIcon.Dispose(); if (menuFont != null) menuFont.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
