using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Windows.Forms;

namespace EyeRest
{
    internal static class SelfTests
    {
        [System.Runtime.InteropServices.DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr handle, int message, IntPtr wParam, IntPtr lParam);
        private static void Check(bool condition, string message)
        { if (!condition) throw new InvalidOperationException(message); }
        private sealed class TestStartup : IStartupRegistration
        {
            public string Command;
            public bool RejectWrites;
            public string Read() { return Command; }
            public void Write(string value) { if (RejectWrites) throw new UnauthorizedAccessException("Test denied startup write"); Command = value; }
        }
        public static int Run(string report)
        {
            List<string> results = new List<string>();
            string temp = Path.Combine(Path.GetTempPath(), "EyeRest-test-" + Guid.NewGuid().ToString("N"));
            try
            {
                double now = 0;
                Schedule schedule = new Schedule(delegate { return now; }, 20);
                now = 1199; Check(!schedule.Tick(), "Reminder fired early");
                now = 1200; Check(schedule.Tick(), "Reminder did not fire");
                Check(!schedule.Tick(), "Duplicate reminder");
                results.Add("PASS: deadline and duplicate-reminder prevention");
                schedule.Start(10); now += 120; schedule.Pause(); double remaining = schedule.Remaining;
                now += 50000; Check(schedule.Remaining == remaining && !schedule.Tick(), "Pause drifted");
                schedule.Resume(); now += remaining - 1; Check(!schedule.Tick(), "Resume fired early");
                now++; Check(schedule.Tick(), "Resume deadline incorrect");
                results.Add("PASS: pause freezes remaining time; resume preserves it");
                schedule.Start(5); now += 300; Check(schedule.Tick(), "Snooze deadline incorrect");
                schedule.Start(20); now += 500000; Check(schedule.Tick() && !schedule.Tick(), "Long clock jump caused repeated reminders");
                schedule.Start(20); schedule.Pause(); schedule.UpdateInterval(30);
                Check(schedule.State == Phase.Paused && schedule.Remaining == 1800, "Saving interval resumed paused timer");
                results.Add("PASS: snooze, long clock jump, and interval changes while paused");

                SettingsStore store = new SettingsStore(Path.Combine(temp, "settings.json"));
                Preferences preferences = store.Load(); preferences.Message = "你好，眼睛。\nUnicode ✓ & 多行";
                preferences.SetPreset(1); store.Save(preferences);
                Preferences loaded = store.Load();
                Check(loaded.Message == preferences.Message && loaded.Background == preferences.Background, "Persistence round trip failed");
                preferences.IntervalMinutes = 42; store.Save(preferences);
                Check(store.Load().IntervalMinutes == 42 && File.Exists(store.Path + ".bak"), "Atomic replacement failed");
                File.WriteAllText(store.Path, "corrupted");
                Check(store.Load().Message == preferences.Message && store.Warning != null, "Backup not recovered");
                File.WriteAllText(store.Path + ".bak", "also corrupted");
                Check(store.Load().IntervalMinutes == 20, "Defaults not recovered");
                results.Add("PASS: UTF-8 settings, atomic save, backup and corrupt-file recovery");

                preferences = new Preferences { IntervalMinutes = -1, RestSeconds = 99999, FontSize = 99, Accent = "invalid", Message = "" };
                preferences.Normalize();
                Check(preferences.IntervalMinutes == 1 && preferences.RestSeconds == 600 && preferences.FontSize == 14 && Preferences.ValidColor(preferences.Accent) && preferences.Message.Length > 0, "Invalid input not normalized");
                results.Add("PASS: invalid configuration is normalized safely");
                Preferences art = new Preferences { Message = "保留我的原提醒", ArtMessage = "……自定义的休息台词。" };
                art.SetPreset(3); store.Save(art);
                loaded = store.Load();
                Check(loaded.IsArtTheme && loaded.DisplayMessage == art.ArtMessage && loaded.Message == art.Message, "Art theme did not round-trip independently");
                loaded.SetPreset(0); Check(loaded.DisplayMessage == art.Message, "Switching theme destroyed original reminder");
                File.WriteAllText(store.Path, "{\"IntervalMinutes\":20,\"RestSeconds\":20,\"SnoozeMinutes\":5,\"Message\":\"legacy\",\"FontSize\":10}");
                loaded = store.Load();
                Check(!loaded.IsArtTheme && loaded.DisplayMessage == "legacy" && loaded.ArtMessage == Preferences.ArtQuotes[0], "Legacy settings migration failed");
                results.Add("PASS: art theme persistence, independent dialogue, and old settings compatibility");
                Check(loaded.ReminderScalePercent == 100, "Old settings did not default to 100% reminder size");
                Preferences sized = art.Copy(); sized.ReminderScalePercent = 85; store.Save(sized);
                Check(store.Load().ReminderScalePercent == 85, "Reminder size not persisted");
                sized.ReminderScalePercent = -1; sized.Normalize(); Check(sized.ReminderScalePercent == 70, "Reminder minimum scale not enforced");
                sized.ReminderScalePercent = 999; sized.Normalize(); Check(sized.ReminderScalePercent == 150, "Reminder maximum scale not enforced");
                VerifyReminderSizes();
                results.Add("PASS: reminder size persistence, fixed proportions, screen fit, font scaling and reachable actions");
                Preferences collection = art.Copy(); collection.PortraitId = "standing"; collection.QuoteMode = "sequential";
                collection.CustomQuotes = QuoteCollection.Parse("第一条\n可以换行\n---\n第二条\n---\n第三条");
                store.Save(collection); loaded = store.Load();
                Check(loaded.PortraitId == "standing" && loaded.CustomQuotes.Length == 3 && loaded.CustomQuotes[0].Contains("\n"), "Portrait/quote collection persistence failed");
                Preferences weary = collection.Copy(); weary.PortraitId = "weary"; store.Save(weary);
                Check(store.Load().PortraitId == "weary", "Weary portrait selection did not persist");
                Preferences independent = collection.Copy(); independent.CustomQuotes[0] = "edited";
                Check(collection.CustomQuotes[0] != "edited", "Draft quote array mutated saved preferences");
                QuotePlayer player = new QuotePlayer(new Random(41));
                Check(player.Select(collection, false) == collection.CustomQuotes[0] && player.Select(collection, false) == collection.CustomQuotes[0], "Preview changed sequential cursor");
                for (int i = 0; i < 7; i++) Check(player.Select(collection, true) == collection.CustomQuotes[i % 3], "Sequential quotes did not wrap in order");
                collection.Accent = "#ABCDEF";
                Check(player.Select(collection, true) == collection.CustomQuotes[1], "Cosmetic changes reset quote cursor");
                collection.QuoteMode = "random"; string previousQuote = null;
                for (int i = 0; i < 50; i++)
                { string value = player.Select(collection, true); Check(Array.IndexOf(collection.CustomQuotes, value) >= 0 && value != previousQuote, "Random quote was invalid or immediately repeated"); previousQuote = value; }
                collection.CustomQuotes = new [] { "only one" }; Check(player.Select(collection, true) == "only one", "Single random quote failed");
                collection.CustomQuotes = new string[0]; Check(player.Select(collection, true) == collection.DisplayMessage, "Empty collection did not fall back safely");
                bool oversized = false; try { QuoteCollection.Parse(new String('字', 501)); } catch (ArgumentException) { oversized = true; }
                Check(oversized, "Oversized quote was silently truncated by editor");
                using (QuoteEditor editor = new QuoteEditor(art))
                {
                    editor.Show(); Application.DoEvents();
                    TextBox text = (TextBox)editor.Controls.Find("QuoteCollectionText", true)[0];
                    text.Text = "自定义一\r\n---\r\n自定义二";
                    FindButton(editor, "使用此语录集").PerformClick();
                    Check(editor.DialogResult == DialogResult.OK && editor.Quotes.Length == 2, "Quote editor did not return edited collection");
                }
                results.Add("PASS: quote collection parsing/persistence, deep copy, sequential wrap, preview isolation and random playback");
                Check(!loaded.StartWithWindows, "Old configuration unexpectedly enabled startup");
                TestStartup registration = new TestStartup();
                string executable = @"C:\Apps With Spaces\目间\EyeRest.exe";
                Preferences automatic = new Preferences { StartWithWindows = true };
                SettingsCommit.Save(automatic, store, registration, executable);
                Check(registration.Command == "\"" + executable + "\" --startup" && store.Load().StartWithWindows, "Startup command quoting or persistence failed");
                automatic.StartWithWindows = false;
                SettingsCommit.Save(automatic, store, registration, executable);
                Check(registration.Command == null && !store.Load().StartWithWindows, "Disabling startup failed");
                string blocked = Path.Combine(temp, "blocked"); File.WriteAllText(blocked, "file, not a directory");
                registration.Command = "previous registration"; bool failed = false;
                try { SettingsCommit.Save(automatic, new SettingsStore(Path.Combine(blocked, "settings.json")), registration, executable); }
                catch (IOException) { failed = true; }
                Check(failed && registration.Command == "previous registration", "Failed settings save did not roll back startup");
                registration.RejectWrites = true; failed = false;
                try { SettingsCommit.Save(automatic, store, registration, executable); }
                catch (UnauthorizedAccessException) { failed = true; }
                Check(failed && !store.Load().StartWithWindows, "Registry failure overwrote saved preferences");
                results.Add("PASS: startup enable/disable, quoted paths, save rollback and registry denial (isolated adapter)");

                Preferences savedDraft = null;
                using (SettingsForm settings = new SettingsForm(art, delegate(Preferences value) { savedDraft = value; return true; }, delegate { }, delegate { }))
                {
                    settings.AllowClose = true; settings.Show(); Application.DoEvents();
                    Check(settings.FormBorderStyle == FormBorderStyle.None, "Settings still uses an unthemed native title bar");
                    FindButton(settings, "松林 · 浅色").PerformClick();
                    FindButton(settings, "艺术层 · Netzach").PerformClick();
                    FindButton(settings, "保存设置").PerformClick();
                    Check(savedDraft != null && savedDraft.IsArtTheme && savedDraft.ArtMessage == art.ArtMessage && savedDraft.Message == art.Message, "GUI theme switch lost custom text");
                    FindButton(settings, "换一句 Netzach 台词").PerformClick();
                    FindButton(settings, "保存设置").PerformClick();
                    Check(savedDraft.ArtMessage.Replace("\r\n", "\n") == Preferences.ArtQuotes[0], "Quote selection did not save");
                    CheckLayout(settings);
                    ComboBox fontControl = FindCombo(settings);
                    Check(fontControl is ThemedComboBox && fontControl.BackColor == Preferences.ColorOf(art.Surface) && fontControl.ForeColor == Preferences.ColorOf(art.Foreground), "Font picker did not adopt theme");
                    using (Bitmap picker = new Bitmap(fontControl.Width, fontControl.Height))
                    {
                        fontControl.DrawToBitmap(picker, new Rectangle(Point.Empty, picker.Size));
                        Check(picker.GetPixel(picker.Width - fontControl.Height / 2, 3).ToArgb() == Preferences.ColorOf(art.Surface).ToArgb(), "Font picker arrow retained the white system background");
                    }
                    CheckBox startupControl = FindCheck(settings, "开机自启（登录后进入托盘）");
                    startupControl.Checked = true; FindButton(settings, "保存设置").PerformClick();
                    Check(savedDraft.StartWithWindows, "Startup checkbox did not reach settings callback");
                    FindCombo(settings, "人物形象").SelectedIndex = 1; FindButton(settings, "保存设置").PerformClick();
                    Check(savedDraft.PortraitId == "standing", "Portrait selection was not saved");
                    FindCombo(settings, "人物形象").SelectedIndex = 2; FindButton(settings, "保存设置").PerformClick();
                    Check(savedDraft.PortraitId == "weary", "Weary portrait GUI option was not saved");
                    foreach (NumericUpDown number in Numbers(settings)) if (number.AccessibleName == "提醒窗口大小") number.Value = 85;
                    FindButton(settings, "保存设置").PerformClick(); Check(savedDraft.ReminderScalePercent == 85, "Reminder size setting did not reach save callback");
                    Preferences beforeEmptyCollection = savedDraft;
                    FindCombo(settings, "语录播放方式").SelectedIndex = 1; FindButton(settings, "保存设置").PerformClick();
                    Check(Object.ReferenceEquals(savedDraft, beforeEmptyCollection), "Empty quote collection was allowed to save as sequential");
                    FindCombo(settings, "语录播放方式").SelectedIndex = 0;
                }
                using (ReminderForm reminder = new ReminderForm(art, false))
                {
                    reminder.Show(); Application.DoEvents();
                    Check(HasText(reminder, "Netzach") && HasText(reminder, art.ArtMessage), "Character dialogue not displayed");
                    Check(reminder.FormBorderStyle == FormBorderStyle.None, "Reminder still has a native white title bar");
                    FindButton(reminder, "5 分钟后提醒").PerformClick(); Check(reminder.Snoozed, "Themed snooze failed");
                }
                results.Add("PASS: themed GUI, custom dialogue preservation, quote selection and snooze");

                using (SettingsForm settings = new SettingsForm(new Preferences(), delegate { return true; }, delegate { }, delegate { }))
                {
                    settings.Show();
                    Application.DoEvents();
                    CheckLayout(settings);
                    FindButton(settings, "夜航 · 深色").PerformClick();
                    FindButton(settings, "松林 · 浅色").PerformClick();
                    foreach (NumericUpDown number in Numbers(settings))
                    {
                        if (number.Maximum == 14) { number.Value = 14; Application.DoEvents(); CheckLayout(settings); number.Value = 10; }
                    }
                    settings.Close();
                    Check(!settings.IsDisposed && !settings.Visible, "Closing settings did not hide to tray");
                    settings.AllowClose = true;
                }
                using (ReminderForm reminder = new ReminderForm(new Preferences(), false))
                {
                    reminder.Show(); Application.DoEvents(); FindButton(reminder, "×").PerformClick();
                    Check(reminder.Snoozed, "Title bar close must snooze");
                }
                using (ReminderForm reminder = new ReminderForm(new Preferences(), false))
                {
                    reminder.Show(); Application.DoEvents(); FindButton(reminder, "完成休息").PerformClick();
                    Check(!reminder.Snoozed, "Complete button must start normal interval");
                }
                using (ReminderForm reminder = new ReminderForm(new Preferences(), false))
                {
                    reminder.Show(); Application.DoEvents(); FindButton(reminder, "5 分钟后提醒").PerformClick();
                    Check(reminder.Snoozed, "Snooze button failed");
                }
                results.Add("PASS: settings close-to-tray and real reminder close/complete/snooze buttons");
                using (ReminderForm reminder = new ReminderForm(new Preferences { RestSeconds = 5 }, false))
                {
                    bool closed = false; reminder.FormClosed += delegate { closed = true; };
                    reminder.Show();
                    System.Diagnostics.Stopwatch timeout = System.Diagnostics.Stopwatch.StartNew();
                    while (!closed && timeout.Elapsed.TotalSeconds < 8) { Application.DoEvents(); System.Threading.Thread.Sleep(25); }
                    Check(closed && !reminder.Snoozed, "Countdown did not automatically finish");
                }
                results.Add("PASS: theme/font changes, scaled input bounds, and real 5-second automatic close");
                CheckResourceHash("chibi", "1292BCD58CFA30195D8F5A73BC13E2A732D588B4EDE632F8E86E263582F942E3");
                CheckResourceHash("standing", "9241ECE53FAA22EF0D7870451CCC1680E498361BD2D30C833E551A93C910DC14");
                CheckResourceHash("weary", "2700B8939D014E92A8C345CAB18ABE189A01A3483808979870778ADC72724D7F");
                Bitmap wearySprite = (Bitmap)ArtResources.GetPortrait("weary");
                foreach (Point gap in new [] { new Point(0, 0), new Point(414, 667), new Point(735, 489), new Point(520, 1320) })
                    Check(wearySprite.GetPixel(gap.X, gap.Y).A == 0, "Weary portrait has opaque background: " + gap);
                Check(ArtResources.GetPortrait("chibi").Width > 0 && ArtResources.GetPortrait("standing").Width > 0, "Original portraits cannot be decoded");
                VerifyPortraitTransparency();
                VerifyWheelGates();
                SettingsStore startupStore = new SettingsStore(Path.Combine(temp, "startup-settings.json"));
                startupStore.Save(art);
                using (System.Threading.EventWaitHandle signal = new System.Threading.EventWaitHandle(false, System.Threading.EventResetMode.AutoReset))
                using (RestApplication application = new RestApplication(signal, true, startupStore, new TestStartup()))
                {
                    Application.DoEvents();
                    foreach (Form window in Application.OpenForms) Check(!window.Visible, "Startup launch displayed a settings window");
                    application.ExitThread();
                }
                results.Add("PASS: transparent chibi and original standing resource integrity, click-gated font/size wheels, themed font picker and silent startup");
                results.Add("All checks passed.");
                File.WriteAllLines(report, results, Encoding.UTF8); return 0;
            }
            catch (Exception error) { results.Add("FAIL: " + error); File.WriteAllLines(report, results, Encoding.UTF8); return 1; }
            finally { if (Directory.Exists(temp)) Directory.Delete(temp, true); }
        }
        private static Button FindButton(Control root, string text)
        {
            foreach (Control control in root.Controls)
            {
                Button button = control as Button;
                if (button != null && button.Text == text) return button;
                Button child = FindButton(control, text); if (child != null) return child;
            }
            return null;
        }
        private static bool HasText(Control root, string text)
        {
            if (root.Text == text) return true;
            foreach (Control child in root.Controls) if (HasText(child, text)) return true;
            return false;
        }
        private static ComboBox FindCombo(Control root, string name = "界面字体")
        {
            foreach (Control child in root.Controls)
            { ComboBox combo = child as ComboBox; if (combo != null && combo.AccessibleName == name) return combo; combo = FindCombo(child, name); if (combo != null) return combo; }
            return null;
        }
        private static void CheckResourceHash(string id, string expected)
        {
            using (Stream stream = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceStream(ArtResources.ResourceName(id)))
            using (System.Security.Cryptography.SHA256 sha = System.Security.Cryptography.SHA256.Create())
                Check(BitConverter.ToString(sha.ComputeHash(stream)).Replace("-", "") == expected, "Image resource hash mismatch: " + id);
        }
        private static void VerifyPortraitTransparency()
        {
            Bitmap sprite = (Bitmap)ArtResources.GetPortrait("chibi");
            Point[] gaps = { new Point(10, 10), new Point(651, 93), new Point(443, 612), new Point(337, 602), new Point(394, 710), new Point(531, 1200) };
            foreach (Point point in gaps) Check(sprite.GetPixel(point.X, point.Y).A == 0, "Background gap remains opaque: " + point);
            Check(sprite.GetPixel(500, 520).ToArgb() == Color.FromArgb(255, 207, 197, 192).ToArgb(), "Original face color changed");
            using (ArtPortrait portrait = new ArtPortrait { Size = sprite.Size, BackColor = Color.FromArgb(31, 37, 23) })
            using (Bitmap rendered = new Bitmap(sprite.Width, sprite.Height))
            {
                portrait.CreateControl(); portrait.DrawToBitmap(rendered, new Rectangle(Point.Empty, rendered.Size));
                foreach (Point point in gaps) Check(rendered.GetPixel(point.X, point.Y).ToArgb() == portrait.BackColor.ToArgb(), "Background gap failed to composite: " + point);
            }
        }
        private static void VerifyReminderSizes()
        {
            foreach (bool art in new [] { false, true })
            {
                Size basis = art ? new Size(810, 565) : new Size(600, 530);
                foreach (int percent in new [] { 70, 100, 150 })
                {
                    foreach (Size screen in new [] { new Size(1920, 1080), new Size(800, 600), new Size(360, 300) })
                    {
                        Size fitted = ReminderSizing.Fit(basis, percent, screen);
                        Check(fitted.Width <= screen.Width && fitted.Height <= screen.Height, "Reminder exceeds screen");
                        Check(Math.Abs(fitted.Width * basis.Height - fitted.Height * basis.Width) <= basis.Width, "Screen fit distorted reminder proportions");
                    }
                    Preferences p = new Preferences { ReminderScalePercent = percent }; if (art) p.SetPreset(3);
                    using (ReminderForm form = new ReminderForm(p, false))
                    {
                        form.Show(); Application.DoEvents();
                        Check(Math.Abs(form.Width * basis.Height - form.Height * basis.Width) <= basis.Width, "Shown reminder ratio incorrect");
                        Check(form.MinimumSize == form.Size && form.MaximumSize == form.Size, "Reminder size can be stretched freely");
                        Button done = FindButton(form, "完成休息"), snooze = FindButton(form, "5 分钟后提醒");
                        Rectangle doneBounds = form.RectangleToClient(done.RectangleToScreen(done.ClientRectangle));
                        Rectangle snoozeBounds = form.RectangleToClient(snooze.RectangleToScreen(snooze.ClientRectangle));
                        Check(form.ClientRectangle.Contains(doneBounds) && form.ClientRectangle.Contains(snoozeBounds) && !doneBounds.IntersectsWith(snoozeBounds), "Scaled buttons clipped or overlap");
                        if (percent == 70) Check(form.Font.Size < 10, "Window contents did not shrink with window");
                        done.PerformClick(); Check(form.IsDisposed && !form.Snoozed, "Scaled reminder actions failed");
                    }
                }
            }
        }
        private static void VerifyWheelGates()
        {
            using (Form form = new Form { ClientSize = new Size(400, 220) })
            {
                ThemedComboBox combo = new ThemedComboBox { Location = new Point(15, 15), Width = 260 };
                combo.Items.AddRange(new object[] { "A", "B", "C" }); combo.SelectedIndex = 1;
                ThemedNumericUpDown number = new ThemedNumericUpDown { Location = new Point(15, 90), Minimum = 9, Maximum = 14, Value = 10, RequireClickForWheel = true };
                Button other = new Button { Text = "other", Location = new Point(15, 150) };
                form.Controls.Add(combo); form.Controls.Add(number); form.Controls.Add(other); form.Show(); Application.DoEvents();
                combo.Focus(); SendMessage(combo.Handle, 0x20A, new IntPtr(120 << 16), IntPtr.Zero);
                Check(combo.SelectedIndex == 1, "Font changed before a mouse click");
                SendMessage(combo.Handle, 0x201, new IntPtr(1), new IntPtr(5 | (5 << 16))); SendMessage(combo.Handle, 0x202, IntPtr.Zero, new IntPtr(5 | (5 << 16)));
                combo.DroppedDown = false; combo.Focus(); int before = combo.SelectedIndex;
                SendMessage(combo.Handle, 0x20A, new IntPtr(120 << 16), IntPtr.Zero); Check(combo.SelectedIndex != before, "Font wheel remained locked after click");
                number.Focus(); SendMessage(number.Handle, 0x20A, new IntPtr(120 << 16), IntPtr.Zero); Check(number.Value == 10, "Font size changed before a mouse click");
                Control edit = number.Controls[1];
                SendMessage(edit.Handle, 0x201, new IntPtr(1), new IntPtr(5 | (5 << 16))); SendMessage(edit.Handle, 0x202, IntPtr.Zero, new IntPtr(5 | (5 << 16)));
                number.Focus(); SendMessage(number.Handle, 0x20A, new IntPtr(120 << 16), IntPtr.Zero); Check(number.Value > 10, "Size wheel remained locked after editor click");
                form.ActiveControl = other; other.Focus(); Application.DoEvents();
                Check(!number.ContainsFocus, "Test could not transfer focus away from size field");
                decimal saved = number.Value; SendMessage(number.Handle, 0x20A, new IntPtr(120 << 16), IntPtr.Zero);
                Check(number.Value == saved, "Size wheel stayed enabled after leaving field");
                form.Close();
            }
        }
        private static CheckBox FindCheck(Control root, string text)
        {
            foreach (Control child in root.Controls)
            { CheckBox check = child as CheckBox; if (check != null && check.Text == text) return check; check = FindCheck(child, text); if (check != null) return check; }
            return null;
        }
        private static IEnumerable<NumericUpDown> Numbers(Control root)
        {
            foreach (Control control in root.Controls)
            {
                NumericUpDown number = control as NumericUpDown;
                if (number != null) yield return number;
                else foreach (NumericUpDown child in Numbers(control)) yield return child;
            }
        }
        private static void CheckLayout(Control root)
        {
            foreach (NumericUpDown number in Numbers(root))
                Check(number.Width < 500 && number.Top >= 0 && number.Bottom <= number.Parent.ClientSize.Height && number.Right <= number.Parent.ClientSize.Width, "Numeric input was clipped after DPI/font scaling");
        }
        public static int Render(string directory)
        {
            Directory.CreateDirectory(directory);
            for (int index = 0; index < 4; index++)
            {
                Preferences p = new Preferences(); p.SetPreset(index);
                using (SettingsForm settings = new SettingsForm(p, delegate { return true; }, delegate { }, delegate { }))
                {
                    settings.AllowClose = true;
                    Capture(settings, Path.Combine(directory, "settings-" + index + ".png"));
                }
                using (ReminderForm reminder = new ReminderForm(p, true)) Capture(reminder, Path.Combine(directory, "reminder-" + index + ".png"));
                if (index == 3)
                {
                    using (SettingsForm settings = new SettingsForm(p, delegate { return true; }, delegate { }, delegate { }))
                    { settings.AllowClose = true; Capture(settings, Path.Combine(directory, "settings-fonts.png"), true); }
                    using (SettingsForm settings = new SettingsForm(p, delegate { return true; }, delegate { }, delegate { }))
                    { settings.AllowClose = true; Capture(settings, Path.Combine(directory, "settings-size.png"), true, true); }
                    using (ContextMenuStrip menu = new ContextMenuStrip())
                    using (Font menuFont = Styles.FontFor(p, p.FontSize, FontStyle.Regular))
                    {
                        menu.Font = menuFont; menu.Items.Add(new ToolStripMenuItem("下次休息 · 20:00") { Enabled = false });
                        menu.Items.Add(new ToolStripSeparator()); menu.Items.Add("打开设置"); menu.Items.Add("现在休息"); menu.Items.Add("暂停提醒");
                        menu.Items.Add(new ToolStripSeparator()); menu.Items.Add("退出目间");
                        ThemeMenuRenderer.Apply(menu, p); menu.Show(new Point(100, 100)); menu.Items[2].Select(); Application.DoEvents();
                        using (Bitmap bitmap = new Bitmap(menu.Width, menu.Height))
                        { menu.DrawToBitmap(bitmap, new Rectangle(Point.Empty, menu.Size)); bitmap.Save(Path.Combine(directory, "tray-menu.png"), ImageFormat.Png); }
                        menu.Close();
                    }
                }
            }
            Preferences large = new Preferences { FontSize = 14, Message = new String('休', 500) };
            using (SettingsForm settings = new SettingsForm(large, delegate { return true; }, delegate { }, delegate { }))
            {
                settings.AllowClose = true; settings.Size = settings.MinimumSize;
                Capture(settings, Path.Combine(directory, "settings-large.png"));
            }
            using (ReminderForm reminder = new ReminderForm(large, true)) Capture(reminder, Path.Combine(directory, "reminder-long.png"));
            large.SetPreset(3); large.ArtMessage = new String('休', 500);
            using (SettingsForm settings = new SettingsForm(large, delegate { return true; }, delegate { }, delegate { }))
            {
                settings.AllowClose = true; settings.Size = settings.MinimumSize;
                Capture(settings, Path.Combine(directory, "settings-art-large.png"));
            }
            using (ReminderForm reminder = new ReminderForm(large, false))
            {
                reminder.Size = reminder.MinimumSize;
                Capture(reminder, Path.Combine(directory, "reminder-art-long.png"));
            }
            Preferences alternate = new Preferences(); alternate.SetPreset(3); alternate.PortraitId = "standing";
            foreach (int scale in new [] { 70, 150 })
            {
                Preferences sized = new Preferences { ReminderScalePercent = scale }; sized.SetPreset(3);
                using (ReminderForm reminder = new ReminderForm(sized, false)) Capture(reminder, Path.Combine(directory, "reminder-size-" + scale + ".png"));
            }
            using (ReminderForm reminder = new ReminderForm(alternate, true)) Capture(reminder, Path.Combine(directory, "reminder-standing.png"));
            alternate.PortraitId = "weary";
            using (ReminderForm reminder = new ReminderForm(alternate, true)) Capture(reminder, Path.Combine(directory, "reminder-weary.png"));
            alternate.CustomQuotes = new [] { "先看看远处吧。\n忙完这段，也别忘了休息。", "慢慢来，给自己留一点时间。", "放松肩膀，喝口水吧。" }; alternate.QuoteMode = "sequential";
            using (QuoteEditor editor = new QuoteEditor(alternate)) Capture(editor, Path.Combine(directory, "quote-editor.png"));
            return 0;
        }
        private static void Capture(Form form, string path, bool showFonts = false, bool showSize = false)
        {
            form.Show(); Application.DoEvents();
            if (showFonts)
            {
                Control combo = FindCombo(form);
                if (showSize) foreach (NumericUpDown number in Numbers(form)) if (number.AccessibleName == "提醒窗口大小") combo = number;
                Control parent = combo.Parent;
                while (parent != null)
                {
                    ScrollableControl scroll = parent as ScrollableControl;
                    if (scroll != null && scroll.AutoScroll) { scroll.ScrollControlIntoView(combo); break; }
                    parent = parent.Parent;
                }
                Application.DoEvents();
            }
            StringBuilder tree = new StringBuilder(); Describe(form, tree, 0);
            File.WriteAllText(path + ".layout.txt", tree.ToString());
            using (Bitmap bitmap = new Bitmap(form.Width, form.Height))
            { form.DrawToBitmap(bitmap, new Rectangle(Point.Empty, form.Size)); bitmap.Save(path, ImageFormat.Png); }
            form.Close();
        }
        private static void Describe(Control control, StringBuilder text, int depth)
        {
            text.AppendLine(new String(' ', depth * 2) + control.GetType().Name + " " + control.Text.Replace("\r", "").Replace("\n", " ").Substring(0, Math.Min(30, control.Text.Replace("\r", "").Replace("\n", " ").Length)) + " " + control.Bounds + " Font=" + control.Font.Size + " Margin=" + control.Margin + " Minimum=" + control.MinimumSize);
            foreach (Control child in control.Controls) Describe(child, text, depth + 1);
        }
    }
}
