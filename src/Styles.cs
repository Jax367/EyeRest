using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace EyeRest
{
    internal static class Styles
    {
        public static void PrepareLayout(Control root)
        {
            Panel panel = root as Panel;
            if (panel != null && panel.AutoSize) panel.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            foreach (Control child in root.Controls) PrepareLayout(child);
        }
        public static Font FontFor(Preferences p, float size, FontStyle style)
        {
            try { return new Font(p.FontFamily, size, style, GraphicsUnit.Point); }
            catch (ArgumentException) { return new Font("Microsoft YaHei UI", size, style); }
        }
        public static Color Contrast(Color color)
        {
            return color.R * 0.299 + color.G * 0.587 + color.B * 0.114 > 155 ? Color.FromArgb(24, 37, 31) : Color.White;
        }
        public static Label Label(string text)
        {
            return new Label { Text = text, AutoSize = true, Margin = new Padding(0, 8, 12, 8), Anchor = AnchorStyles.Left };
        }
        public static Button Button(string text, EventHandler handler)
        {
            Button button = new Button { Text = text, AutoSize = true, AutoSizeMode = AutoSizeMode.GrowAndShrink, MinimumSize = new Size(104, 38), Padding = new Padding(12, 3, 12, 3), FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand, Margin = new Padding(0, 0, 10, 0) };
            button.FlatAppearance.BorderSize = 0;
            button.Click += handler;
            return button;
        }
        public static void Apply(Control root, Preferences p, Color background, bool reminder)
        {
            Color foreground = Preferences.ColorOf(reminder ? p.ReminderForeground : p.Foreground);
            Color surface = Preferences.ColorOf(reminder ? p.ReminderBackground : p.Surface);
            root.BackColor = background; root.ForeColor = foreground;
            foreach (Control control in root.Controls)
            {
                Apply(control, p, background, reminder);
                if (control is TextBoxBase || control is NumericUpDown || control is ComboBox)
                { control.BackColor = surface; control.ForeColor = foreground; }
                ThemedComboBox combo = control as ThemedComboBox;
                if (combo != null) { combo.Accent = Preferences.ColorOf(p.Accent); combo.Invalidate(); }
                CheckBox check = control as CheckBox;
                if (check != null)
                {
                    check.FlatStyle = FlatStyle.Flat; check.FlatAppearance.BorderColor = foreground;
                    check.FlatAppearance.CheckedBackColor = surface; check.FlatAppearance.MouseOverBackColor = surface;
                }
                Button button = control as Button;
                if (button != null)
                {
                    Color accent = Preferences.ColorOf(p.Accent);
                    button.BackColor = button.Tag as string == "primary" ? accent : surface;
                    button.ForeColor = button.Tag as string == "primary" ? Contrast(accent) : foreground;
                    button.FlatAppearance.BorderColor = foreground;
                }
            }
        }
    }

    internal sealed class RestMark : Control
    {
        public RestMark() { DoubleBuffered = true; Size = new Size(64, 64); }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (Pen pen = new Pen(ForeColor, 2.4f))
            {
                float scale = Math.Min(Width, Height) / 64f;
                e.Graphics.ScaleTransform(scale, scale);
                e.Graphics.DrawEllipse(pen, 2, 2, 59, 59);
                e.Graphics.DrawArc(pen, 14, 23, 36, 19, 0, 180);
                e.Graphics.DrawLine(pen, 18, 38, 15, 43);
                e.Graphics.DrawLine(pen, 32, 42, 32, 48);
                e.Graphics.DrawLine(pen, 46, 38, 49, 43);
            }
        }
    }

    internal sealed class ColorChoice : Button
    {
        public string Value { get; private set; }
        public event EventHandler ColorChanged;
        public ColorChoice(string label, string color)
        {
            Text = label; AccessibleName = label; Value = color;
            Dock = DockStyle.Fill; Height = 37; FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 1; TextAlign = ContentAlignment.MiddleLeft;
            Padding = new Padding(35, 0, 0, 0); Margin = new Padding(0, 0, 12, 10);
            Click += delegate {
                using (ColorDialog dialog = new ColorDialog { Color = Preferences.ColorOf(Value), FullOpen = true })
                {
                    if (dialog.ShowDialog(this) != DialogResult.OK) return;
                    SetColor(String.Format("#{0:X2}{1:X2}{2:X2}", dialog.Color.R, dialog.Color.G, dialog.Color.B));
                    if (ColorChanged != null) ColorChanged(this, EventArgs.Empty);
                }
            };
        }
        public void SetColor(string color) { Value = color; Invalidate(); }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Brush brush = new SolidBrush(Preferences.ColorOf(Value))) e.Graphics.FillEllipse(brush, 12, (Height - 16) / 2, 16, 16);
            using (Pen pen = new Pen(ForeColor)) e.Graphics.DrawEllipse(pen, 12, (Height - 16) / 2, 16, 16);
        }
    }
}
