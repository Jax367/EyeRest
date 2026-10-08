using System;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace EyeRest
{
    internal sealed class ReminderForm : ThemedForm
    {
        private readonly Timer timer = new Timer { Interval = 250 };
        private readonly Stopwatch clock = new Stopwatch();
        private readonly Label countdown;
        private readonly int duration;
        private readonly Font titleFont;
        private readonly Font countFont;
        private readonly Font messageFont;
        private readonly Font bodyFont;
        private readonly Label copy;
        private readonly TableLayoutPanel layout;
        private readonly int scalePercent;
        private readonly Size logicalSize;
        private readonly System.Collections.Generic.List<Font> scaledFonts = new System.Collections.Generic.List<Font>();
        private bool completedByButton;
        public bool Snoozed { get; private set; }
        public bool IsPreview { get; private set; }
        public ReminderForm(Preferences p, bool isPreview, string selectedQuote = null)
        {
            SuspendLayout();
            IsPreview = isPreview; duration = p.RestSeconds;
            scalePercent = p.ReminderScalePercent == 0 ? 100 : Math.Max(70, Math.Min(150, p.ReminderScalePercent));
            Text = isPreview ? "提醒样式预览 · 目间" : "休息一下 · 目间";
            Icon = Program.AppIcon; AutoScaleMode = AutoScaleMode.None;
            bodyFont = Styles.FontFor(p, p.FontSize, FontStyle.Regular); Font = bodyFont;
            titleFont = Styles.FontFor(p, 25, FontStyle.Bold);
            countFont = Styles.FontFor(p, 42, FontStyle.Regular);
            messageFont = Styles.FontFor(p, p.FontSize + 4, FontStyle.Regular);
            ClientSize = new Size(600, 530); MinimumSize = new Size(500, 460);
            if (p.IsArtTheme) { ClientSize = new Size(810, 565); MinimumSize = new Size(780, 535); }
            logicalSize = ClientSize;
            StartPosition = FormStartPosition.CenterScreen; TopMost = p.AlwaysOnTop;
            MaximizeBox = false; MinimizeBox = false;
            layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(40, 25, 40, 24), ColumnCount = 1, RowCount = p.IsArtTheme ? 6 : 7 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 27));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 66));
            if (!p.IsArtTheme) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 54));
            layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 81));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 31));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45));
            if (p.IsArtTheme)
            {
                TableLayoutPanel scene = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(18), ColumnCount = 2, RowCount = 1 };
                scene.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40)); scene.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
                scene.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
                scene.Controls.Add(new ArtPortrait { Dock = DockStyle.Fill, Margin = new Padding(0), Accent = Preferences.ColorOf(p.Accent), PortraitId = p.PortraitId }, 0, 0);
                layout.Padding = new Padding(24, 12, 14, 12);
                layout.RowStyles[1].Height = 61;
                scene.Controls.Add(layout, 1, 0); Controls.Add(scene);
            }
            else Controls.Add(layout);
            layout.Controls.Add(new Label { Text = p.IsArtTheme ? "FLOOR OF ART  /  艺术层" : isPreview ? "目间  /  样式预览" : "目间  /  休息时间", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter });
            if (p.IsArtTheme)
                layout.Controls.Add(new Label { Text = "Netzach", Font = titleFont, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter });
            else layout.Controls.Add(new RestMark { Anchor = AnchorStyles.None, Size = new Size(56, 56) });
            if (!p.IsArtTheme) layout.Controls.Add(new Label { Text = "暂时放下屏幕", Font = titleFont, Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter });
            Panel textArea = new Panel { Dock = DockStyle.Fill, AutoScroll = true, Margin = new Padding(0, 8, 0, 8) };
            copy = new Label { Text = selectedQuote ?? p.DisplayMessage, Font = messageFont, AutoSize = true, TextAlign = ContentAlignment.MiddleCenter, Location = new Point(0, 0), UseMnemonic = false };
            textArea.Controls.Add(copy);
            textArea.Resize += delegate { LayoutMessage(textArea); };
            layout.Controls.Add(textArea);
            countdown = new Label { Text = Schedule.Format(duration), Dock = DockStyle.Fill, Font = countFont, TextAlign = ContentAlignment.MiddleCenter };
            layout.Controls.Add(countdown);
            layout.Controls.Add(new Label { Text = isPreview ? "预览不会改变下一次提醒时间" : p.IsArtTheme ? "这段时间，不用勉强自己。" : "休息结束后自动关闭，并开始下一轮计时", Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleCenter });
            FlowLayoutPanel actions = new FlowLayoutPanel { AutoSize = true, Anchor = AnchorStyles.None, WrapContents = false, Margin = new Padding(0, 5, 0, 0) };
            Button done = Styles.Button(isPreview ? "关闭预览" : "完成休息", delegate { completedByButton = true; Close(); }); done.Tag = "primary"; actions.Controls.Add(done);
            if (!isPreview) actions.Controls.Add(Styles.Button(p.SnoozeMinutes + " 分钟后提醒", delegate { Snoozed = true; Close(); }));
            layout.Controls.Add(actions);
            Styles.Apply(this, p, Preferences.ColorOf(p.ReminderBackground), true);
            ApplyChrome(p, true);
            Shown += delegate { LayoutMessage(textArea); if (!isPreview) { clock.Start(); timer.Start(); } };
            timer.Tick += delegate {
                double remaining = duration - clock.Elapsed.TotalSeconds;
                countdown.Text = Schedule.Format(remaining);
                if (remaining <= 0) Close();
            };
            FormClosing += delegate(object sender, FormClosingEventArgs e) {
                // The title-bar close button postpones a real reminder, so it never traps the user.
                if (!isPreview && e.CloseReason == CloseReason.UserClosing && clock.Elapsed.TotalSeconds < duration && !Snoozed && !completedByButton)
                    Snoozed = true;
                timer.Stop();
            };
            Styles.PrepareLayout(this);
            AutoScaleMode = AutoScaleMode.Dpi;
            AutoScaleDimensions = new SizeF(96, 96);
            ResumeLayout(true);
        }
        private void LayoutMessage(Panel area)
        {
            int width = Math.Max(80, area.ClientSize.Width - SystemInformation.VerticalScrollBarWidth - 4);
            copy.MaximumSize = new Size(width, 0); copy.MinimumSize = new Size(width, 0);
            copy.Top = Math.Max(0, (area.ClientSize.Height - copy.Height) / 2);
        }
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            Rectangle area = Screen.FromControl(this).WorkingArea;
            Size baseline;
            using (Graphics graphics = CreateGraphics()) baseline = new Size((int)Math.Round(logicalSize.Width * graphics.DpiX / 96f), (int)Math.Round(logicalSize.Height * graphics.DpiY / 96f));
            Size target = ReminderSizing.Fit(baseline, scalePercent, new Size(Math.Max(1, area.Width - 40), Math.Max(1, area.Height - 40)));
            MinimumSize = Size.Empty; MaximumSize = Size.Empty; Size = baseline;
            ScaleContents((float)target.Width / baseline.Width);
            Size = target;
            // Size is controlled by the saved percentage; dragging/snap cannot distort it.
            MinimumSize = target; MaximumSize = target;
            Location = new Point(area.Left + (area.Width - Width) / 2, area.Top + (area.Height - Height) / 2);
        }
        private void ScaleContents(float factor)
        {
            if (Math.Abs(factor - 1) < 0.0001f) return;
            var fonts = new System.Collections.Generic.Dictionary<Control, Font>();
            CaptureFonts(this, fonts);
            var replacements = new System.Collections.Generic.Dictionary<Font, Font>();
            SuspendLayout();
            Scale(new SizeF(factor, factor));
            foreach (var item in fonts)
            {
                Font replacement;
                if (!replacements.TryGetValue(item.Value, out replacement))
                {
                    replacement = new Font(item.Value.FontFamily, item.Value.Size * factor, item.Value.Style, item.Value.Unit);
                    replacements.Add(item.Value, replacement); scaledFonts.Add(replacement);
                }
                item.Key.Font = replacement;
            }
            ResumeLayout(true);
        }
        private static void CaptureFonts(Control control, System.Collections.Generic.Dictionary<Control, Font> fonts)
        {
            fonts.Add(control, control.Font);
            foreach (Control child in control.Controls) CaptureFonts(child, fonts);
        }
        protected override void Dispose(bool disposing)
        {
            if (disposing) { timer.Dispose(); titleFont.Dispose(); countFont.Dispose(); messageFont.Dispose(); bodyFont.Dispose(); foreach (Font font in scaledFonts) font.Dispose(); }
            base.Dispose(disposing);
        }
    }
}
