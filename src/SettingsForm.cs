using System;
using System.Drawing;
using System.Windows.Forms;

namespace EyeRest
{
    internal sealed class SettingsForm : ThemedForm
    {
        private readonly Func<Preferences, bool> save;
        private readonly Action<Preferences> preview;
        private Preferences draft;
        private readonly NumericUpDown interval = Number(1, 240);
        private readonly NumericUpDown rest = Number(5, 600);
        private readonly NumericUpDown snooze = Number(1, 60);
        private readonly TextBox message = new TextBox { Multiline = true, MaxLength = 500, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Top, Height = 76, Margin = new Padding(0, 4, 0, 12) };
        private readonly CheckBox topmost = new CheckBox { Text = "提醒窗口保持置顶", AutoSize = true };
        private readonly CheckBox sound = new CheckBox { Text = "提醒时播放提示音", AutoSize = true };
        private readonly CheckBox startup = new CheckBox { Text = "开机自启（登录后进入托盘）", AutoSize = true };
        private readonly NumericUpDown fontSize = Number(9, 14);
        private readonly NumericUpDown reminderScale = Number(70, 150);
        private readonly ComboBox fontFamily = new ThemedComboBox { Width = 225, AccessibleName = "界面字体" };
        private readonly ColorChoice[] colors;
        private readonly Label status = Styles.Label("专注中 · 下次休息 20:00");
        private readonly Label feedback = Styles.Label("设置仅保存在这台电脑上。");
        private readonly Button pause;
        private readonly ArtHeader header;
        private readonly RestMark mark;
        private readonly ArtPortrait avatar;
        private readonly Label heading;
        private readonly Label subtitle;
        private readonly Label messageTitle;
        private readonly Button nextQuote;
        private readonly ComboBox portraitPicker = new ThemedComboBox { Width = 260, AccessibleName = "人物形象" };
        private readonly ComboBox quoteMode = new ThemedComboBox { Width = 200, AccessibleName = "语录播放方式" };
        private readonly FlowLayoutPanel portraitRow;
        private readonly Button editQuotes;
        private bool loading;
        public bool AllowClose;
        private readonly Font titleFont;

        private static NumericUpDown Number(int min, int max)
        { return new ThemedNumericUpDown { AutoSize = false, Minimum = min, Maximum = max, Width = 88, Height = 30, Margin = new Padding(0, 5, 10, 5) }; }
        public SettingsForm(Preferences current, Func<Preferences, bool> onSave, Action<Preferences> onPreview, Action onPause)
        {
            SuspendLayout();
            draft = current.Copy(); save = onSave; preview = onPreview;
            ((ThemedNumericUpDown)fontSize).RequireClickForWheel = true; fontSize.AccessibleName = "界面字号";
            ((ThemedNumericUpDown)reminderScale).RequireClickForWheel = true; reminderScale.AccessibleName = "提醒窗口大小"; reminderScale.Increment = 5;
            Text = "目间 · 休息提醒 · v" + typeof(Program).Assembly.GetName().Version.ToString(3); Icon = Program.AppIcon;
            AutoScaleMode = AutoScaleMode.None;
            ClientSize = new Size(850, 760); MinimumSize = new Size(780, 660);
            StartPosition = FormStartPosition.CenterScreen;
            titleFont = Styles.FontFor(draft, 25, FontStyle.Bold);
            Font = Styles.FontFor(draft, draft.FontSize, FontStyle.Regular);
            TableLayoutPanel outer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3, Padding = new Padding(30, 22, 30, 20) };
            outer.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 112));
            outer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            outer.RowStyles.Add(new RowStyle(SizeType.Absolute, 90));
            Controls.Add(outer);

            header = new ArtHeader { Dock = DockStyle.Fill };
            mark = new RestMark { Location = new Point(0, 4), Size = new Size(58, 58) };
            header.Controls.Add(mark);
            avatar = new ArtPortrait { Location = new Point(0, 0), Size = new Size(58, 78), Visible = false };
            header.Controls.Add(avatar);
            heading = new Label { Text = "目间", Font = titleFont, AutoSize = true, Location = new Point(76, 0) };
            subtitle = new Label { Text = "留一点时间，让眼睛歇一歇。", AutoSize = true, Location = new Point(80, 49) };
            header.Controls.Add(heading); header.Controls.Add(subtitle);
            status.Location = new Point(0, 83); header.Controls.Add(status);
            outer.Controls.Add(header, 0, 0);

            Panel scroll = new Panel { Dock = DockStyle.Fill, AutoScroll = true };
            outer.Controls.Add(scroll, 0, 1);
            TableLayoutPanel content = new TableLayoutPanel { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 1, Padding = new Padding(0, 0, 18, 0) };
            content.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100)); scroll.Controls.Add(content);
            content.Controls.Add(Section("01   提醒节奏"));
            FlowLayoutPanel timing = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top, WrapContents = true, Margin = new Padding(0, 0, 0, 8) };
            timing.Controls.Add(Styles.Label("每隔")); timing.Controls.Add(interval); timing.Controls.Add(Styles.Label("分钟提醒  /  休息")); timing.Controls.Add(rest); timing.Controls.Add(Styles.Label("秒  /  稍后")); timing.Controls.Add(snooze); timing.Controls.Add(Styles.Label("分钟"));
            content.Controls.Add(timing);
            FlowLayoutPanel messageHeading = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top };
            messageTitle = Styles.Label("提醒内容"); messageHeading.Controls.Add(messageTitle);
            nextQuote = Styles.Button("换一句 Netzach 台词", delegate {
                int index = Array.IndexOf(Preferences.ArtQuotes, message.Text.Replace("\r\n", "\n"));
                message.Text = Preferences.ArtQuotes[(index + 1) % Preferences.ArtQuotes.Length].Replace("\n", Environment.NewLine);
            });
            nextQuote.Visible = false; messageHeading.Controls.Add(nextQuote);
            content.Controls.Add(messageHeading); content.Controls.Add(message);
            FlowLayoutPanel playback = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 12) };
            quoteMode.Items.AddRange(new object[] { "单条提醒", "语录集 · 按顺序", "语录集 · 随机" });
            playback.Controls.Add(Styles.Label("播放方式")); playback.Controls.Add(quoteMode);
            editQuotes = Styles.Button("编辑语录集", delegate {
                using (QuoteEditor editor = new QuoteEditor(draft))
                {
                    if (editor.ShowDialog(this) != DialogResult.OK) return;
                    draft.CustomQuotes = editor.Quotes; UpdateQuoteControls(); Dirty();
                }
            }); playback.Controls.Add(editQuotes); content.Controls.Add(playback);
            FlowLayoutPanel switches = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 14) };
            topmost.Margin = new Padding(0, 0, 24, 0); switches.Controls.Add(topmost); switches.Controls.Add(sound); content.Controls.Add(switches);
            startup.Margin = new Padding(0, 0, 0, 12); content.Controls.Add(startup);
            content.Controls.Add(Section("02   界面与提醒样式"));
            FlowLayoutPanel presets = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 3, 0, 14) };
            string[] names = { "松林 · 浅色", "夜航 · 深色", "暖纸 · 柔和", "艺术层 · Netzach" };
            for (int i = 0; i < names.Length; i++)
            {
                int index = i;
                presets.Controls.Add(Styles.Button(names[i], delegate {
                    StoreMessage(); draft.SetPreset(index);
                    message.Text = draft.DisplayMessage.Replace("\r\n", "\n").Replace("\n", Environment.NewLine);
                    LoadColors(); ApplyDraft(); Dirty();
                }));
            }
            content.Controls.Add(presets);
            portraitRow = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 0, 0, 12) };
            portraitPicker.Items.AddRange(new object[] { "Q 版 · 图 1 去背景", "立绘 · 图 2 原图", "Q 版 · 倦怠插兜" });
            portraitRow.Controls.Add(Styles.Label("人物形象")); portraitRow.Controls.Add(portraitPicker); content.Controls.Add(portraitRow);
            string[] colorNames = { "界面背景", "输入框 / 按钮", "界面文字", "强调颜色", "提醒背景", "提醒文字" };
            colors = new ColorChoice[6];
            TableLayoutPanel palette = new TableLayoutPanel { AutoSize = true, Dock = DockStyle.Top, ColumnCount = 3, RowCount = 2, Margin = new Padding(0) };
            for (int i = 0; i < 3; i++) palette.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 33.333f));
            for (int i = 0; i < 6; i++)
            {
                colors[i] = new ColorChoice(colorNames[i], "#FFFFFF");
                colors[i].ColorChanged += delegate { ReadColors(); ApplyDraft(); Dirty(); };
                palette.Controls.Add(colors[i], i % 3, i / 3);
            }
            content.Controls.Add(palette);
            FlowLayoutPanel fonts = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 4, 0, 8) };
            foreach (FontFamily family in System.Drawing.FontFamily.Families) { fontFamily.Items.Add(family.Name); family.Dispose(); }
            fonts.Controls.Add(Styles.Label("字体")); fonts.Controls.Add(fontFamily); fonts.Controls.Add(Styles.Label("字号")); fonts.Controls.Add(fontSize); content.Controls.Add(fonts);
            FlowLayoutPanel sizing = new FlowLayoutPanel { AutoSize = true, Dock = DockStyle.Top, Margin = new Padding(0, 4, 0, 8) };
            sizing.Controls.Add(Styles.Label("提醒窗口大小")); sizing.Controls.Add(reminderScale); sizing.Controls.Add(Styles.Label("%（固定宽高比，70–150）")); content.Controls.Add(sizing);
            content.Controls.Add(new Label { Text = "人物、文字和按钮同步缩放；超出屏幕时自动等比例缩小。", AutoSize = true, MaximumSize = new Size(680, 0), Margin = new Padding(0, 0, 0, 8) });
            content.Controls.Add(new Label { Text = "样式即时预览；点击保存后应用于后续提醒。关闭窗口后仍在托盘运行。", AutoSize = true, MaximumSize = new Size(680, 0), Margin = new Padding(0, 6, 0, 14) });

            TableLayoutPanel footer = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2 };
            footer.RowStyles.Add(new RowStyle(SizeType.Absolute, 34)); footer.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            feedback.Dock = DockStyle.Fill; feedback.Margin = new Padding(0, 4, 0, 4); footer.Controls.Add(feedback);
            FlowLayoutPanel buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            Button saveButton = Styles.Button("保存设置", delegate {
                Preferences value = ReadDraft();
                if (value != null && save(value)) feedback.Text = "已保存 · 提醒设置已生效";
            }); saveButton.Tag = "primary";
            buttons.Controls.Add(saveButton);
            buttons.Controls.Add(Styles.Button("预览提醒", delegate { Preferences value = ReadDraft(); if (value != null) preview(value); }));
            pause = Styles.Button("暂停提醒", delegate { onPause(); }); buttons.Controls.Add(pause);
            buttons.Controls.Add(Styles.Button("收起到托盘", delegate { HideToTray(); }));
            footer.Controls.Add(buttons); outer.Controls.Add(footer, 0, 2);

            loading = true;
            interval.Value = draft.IntervalMinutes; rest.Value = draft.RestSeconds; snooze.Value = draft.SnoozeMinutes;
            message.Text = draft.DisplayMessage.Replace("\r\n", "\n").Replace("\n", Environment.NewLine); topmost.Checked = draft.AlwaysOnTop; sound.Checked = draft.Sound;
            startup.Checked = draft.StartWithWindows;
            if (!fontFamily.Items.Contains(draft.FontFamily)) fontFamily.Items.Add(draft.FontFamily);
            fontFamily.SelectedItem = draft.FontFamily; fontSize.Value = draft.FontSize;
            reminderScale.Value = draft.ReminderScalePercent;
            portraitPicker.SelectedIndex = draft.PortraitId == "weary" ? 2 : draft.PortraitId == "standing" ? 1 : 0;
            quoteMode.SelectedIndex = draft.QuoteMode == "random" ? 2 : draft.QuoteMode == "sequential" ? 1 : 0;
            LoadColors(); ApplyDraft(); loading = false;
            interval.ValueChanged += delegate { Dirty(); }; rest.ValueChanged += delegate { Dirty(); }; snooze.ValueChanged += delegate { Dirty(); };
            message.TextChanged += delegate { Dirty(); }; topmost.CheckedChanged += delegate { Dirty(); }; sound.CheckedChanged += delegate { Dirty(); };
            startup.CheckedChanged += delegate { Dirty(); };
            reminderScale.ValueChanged += delegate { Dirty(); };
            fontSize.ValueChanged += FontChangedByUser; fontFamily.SelectedIndexChanged += FontChangedByUser;
            portraitPicker.SelectedIndexChanged += delegate {
                if (loading) return; draft.PortraitId = portraitPicker.SelectedIndex == 2 ? "weary" : portraitPicker.SelectedIndex == 1 ? "standing" : "chibi";
                avatar.PortraitId = draft.PortraitId; Dirty();
            };
            quoteMode.SelectedIndexChanged += delegate {
                if (loading) return; draft.QuoteMode = quoteMode.SelectedIndex == 2 ? "random" : quoteMode.SelectedIndex == 1 ? "sequential" : "fixed";
                UpdateQuoteControls(); Dirty();
            };
            FormClosing += delegate(object sender, FormClosingEventArgs e) {
                if (!AllowClose && e.CloseReason == CloseReason.UserClosing) { e.Cancel = true; HideToTray(); }
            };
            Resize += delegate { if (WindowState == FormWindowState.Minimized) HideToTray(); };
            Styles.PrepareLayout(this);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96, 96);
            ResumeLayout(true);
        }
        private Control Section(string text)
        { return new Label { Text = text, AutoSize = true, Margin = new Padding(0, 12, 0, 10) }; }
        private void Dirty() { if (!loading) feedback.Text = "有未保存的更改 · 预览满意后请保存"; }
        private void FontChangedByUser(object sender, EventArgs e)
        {
            if (loading) return;
            draft.FontFamily = (string)fontFamily.SelectedItem; draft.FontSize = (int)fontSize.Value;
            ApplyDraft(); Dirty();
        }
        private void LoadColors()
        {
            string[] values = { draft.Background, draft.Surface, draft.Foreground, draft.Accent, draft.ReminderBackground, draft.ReminderForeground };
            for (int i = 0; i < colors.Length; i++) colors[i].SetColor(values[i]);
        }
        private void ReadColors()
        {
            draft.Background = colors[0].Value; draft.Surface = colors[1].Value; draft.Foreground = colors[2].Value;
            draft.Accent = colors[3].Value; draft.ReminderBackground = colors[4].Value; draft.ReminderForeground = colors[5].Value;
        }
        private void ApplyDraft()
        {
            Font old = Font;
            Font next = Styles.FontFor(draft, draft.FontSize, FontStyle.Regular);
            // WinForms retains the previous object when the new font is equal.
            if (old.Equals(next)) next.Dispose();
            else { Font = next; old.Dispose(); }
            Styles.Apply(this, draft, Preferences.ColorOf(draft.Background), false);
            header.ArtEnabled = draft.IsArtTheme; header.Accent = Preferences.ColorOf(draft.Accent); header.Invalidate();
            mark.Visible = !draft.IsArtTheme; avatar.Visible = draft.IsArtTheme; avatar.Accent = header.Accent;
            heading.Text = draft.IsArtTheme ? "艺术层" : "目间";
            subtitle.Text = draft.IsArtTheme ? "Netzach · 慢一点也没关系。" : "留一点时间，让眼睛歇一歇。";
            messageTitle.Text = draft.IsArtTheme ? "Netzach 对你说" : "提醒内容";
            nextQuote.Visible = draft.IsArtTheme;
            portraitRow.Visible = draft.IsArtTheme; avatar.PortraitId = draft.PortraitId;
            UpdateQuoteControls();
            ApplyChrome(draft, false);
        }
        private void UpdateQuoteControls()
        {
            editQuotes.Text = "编辑语录集（" + draft.CustomQuotes.Length + " 条）";
            bool single = draft.QuoteMode == "fixed";
            message.ReadOnly = !single; nextQuote.Enabled = single;
            messageTitle.Text = single ? (draft.IsArtTheme ? "Netzach 对你说" : "提醒内容") : "单条备用文本（当前使用语录集）";
        }
        private void StoreMessage()
        {
            if (draft.IsArtTheme) draft.ArtMessage = message.Text;
            else draft.Message = message.Text;
        }
        private Preferences ReadDraft()
        {
            draft.IntervalMinutes = (int)interval.Value; draft.RestSeconds = (int)rest.Value; draft.SnoozeMinutes = (int)snooze.Value;
            StoreMessage(); draft.AlwaysOnTop = topmost.Checked; draft.Sound = sound.Checked;
            draft.StartWithWindows = startup.Checked;
            draft.ReminderScalePercent = (int)reminderScale.Value;
            draft.Normalize();
            if (draft.QuoteMode != "fixed" && draft.CustomQuotes.Length == 0)
            { feedback.Text = "语录集为空，请先编辑语录集，或选择「单条提醒」。"; return null; }
            return draft.Copy();
        }
        private void HideToTray() { Hide(); }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            using (Graphics graphics = CreateGraphics()) message.Height = (int)(76 * graphics.DpiY / 96);
            Rectangle area = Screen.FromControl(this).WorkingArea;
            Size limit = new Size(Math.Max(400, area.Width - 40), Math.Max(400, area.Height - 40));
            MinimumSize = new Size(Math.Min(MinimumSize.Width, limit.Width), Math.Min(MinimumSize.Height, limit.Height));
            Size = new Size(Math.Min(Width, limit.Width), Math.Min(Height, limit.Height));
            Location = new Point(area.Left + (area.Width - Width) / 2, area.Top + (area.Height - Height) / 2);
        }
        public void UpdateStatus(Schedule schedule)
        {
            status.Text = schedule.State == Phase.Paused ? "已暂停 · 剩余 " + Schedule.Format(schedule.Remaining) :
                schedule.State == Phase.Resting ? "休息时间 · 给眼睛一点留白" : "专注中 · 下次休息 " + Schedule.Format(schedule.Remaining);
            pause.Text = schedule.State == Phase.Paused ? "继续提醒" : "暂停提醒";
            pause.Enabled = schedule.State != Phase.Resting;
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) { titleFont.Dispose(); Font.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
