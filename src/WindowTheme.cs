using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace EyeRest
{
    internal class ThemedForm : Form
    {
        private Panel caption;
        private Button closeButton;
        private Button minimizeButton;
        private Button maximizeButton;
        private Icon ownedIcon;
        private Color frameColor;
        private bool reminder;
        [DllImport("user32.dll")] private static extern bool ReleaseCapture();
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr handle, int message, IntPtr wParam, IntPtr lParam);

        protected ThemedForm() { FormBorderStyle = FormBorderStyle.None; DoubleBuffered = true; }
        protected void ApplyChrome(Preferences p, bool isReminder)
        {
            reminder = isReminder;
            if (caption == null)
            {
                Padding = new Padding(1, 37, 1, 1);
                caption = new Panel { Name = "ThemeTitleBar", Height = 35, Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right };
                caption.Paint += delegate(object sender, PaintEventArgs e) {
                    int iconSize = Math.Max(16, caption.Height * 3 / 5);
                    if (Icon != null) e.Graphics.DrawIcon(Icon, new Rectangle(10, (caption.Height - iconSize) / 2, iconSize, iconSize));
                    TextRenderer.DrawText(e.Graphics, Text, Font, new Rectangle(iconSize + 20, 0, Math.Max(1, caption.Width - iconSize - 20 - closeButton.Width * (reminder ? 1 : 3)), caption.Height), caption.ForeColor,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
                };
                caption.MouseDown += delegate(object sender, MouseEventArgs e) {
                    if (e.Button == MouseButtons.Left) { ReleaseCapture(); SendMessage(Handle, 0xA1, new IntPtr(2), IntPtr.Zero); }
                };
                closeButton = CaptionButton("×", "关闭窗口", delegate { Close(); });
                minimizeButton = CaptionButton("−", "最小化到托盘", delegate { WindowState = FormWindowState.Minimized; });
                maximizeButton = CaptionButton("□", "最大化或还原", delegate { ToggleMaximize(); });
                caption.MouseDoubleClick += delegate { if (!reminder) ToggleMaximize(); };
                caption.Controls.Add(closeButton); caption.Controls.Add(minimizeButton); caption.Controls.Add(maximizeButton);
                Controls.Add(caption); caption.BringToFront();
            }
            Color background = Preferences.ColorOf(isReminder ? p.ReminderBackground : p.Background);
            Color foreground = Preferences.ColorOf(isReminder ? p.ReminderForeground : p.Foreground);
            frameColor = Preferences.ColorOf(p.Accent);
            caption.BackColor = background; caption.ForeColor = foreground;
            foreach (Button button in new [] { closeButton, minimizeButton, maximizeButton })
            {
                button.BackColor = background; button.ForeColor = foreground;
                button.FlatAppearance.MouseOverBackColor = Preferences.ColorOf(p.Surface);
            }
            minimizeButton.Visible = !isReminder;
            maximizeButton.Visible = !isReminder;
            Icon next = Program.CreateIcon(p); Icon = next;
            if (ownedIcon != null) ownedIcon.Dispose(); ownedIcon = next;
            LayoutCaption(); caption.Invalidate(); Invalidate();
        }
        private static Button CaptionButton(string text, string name, EventHandler action)
        {
            Button button = new Button { Text = text, Name = name, AccessibleName = name, Width = 42, FlatStyle = FlatStyle.Flat, TabStop = true };
            button.FlatAppearance.BorderSize = 0; button.Click += action; return button;
        }
        private void ToggleMaximize()
        {
            MaximizedBounds = Screen.FromControl(this).WorkingArea;
            WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
        }
        private void LayoutCaption()
        {
            if (caption == null) return;
            int border = Math.Max(1, Padding.Left), height = Math.Max(24, Padding.Top - border * 2);
            caption.SetBounds(border, border, Math.Max(1, ClientSize.Width - border * 2), height);
            int width = height * 6 / 5;
            closeButton.SetBounds(Math.Max(0, caption.Width - width), 0, width, height);
            maximizeButton.SetBounds(Math.Max(0, caption.Width - width * 2), 0, width, height);
            minimizeButton.SetBounds(Math.Max(0, caption.Width - width * 3), 0, width, height);
        }
        protected override void OnLayout(LayoutEventArgs e) { base.OnLayout(e); LayoutCaption(); }
        protected override void OnTextChanged(EventArgs e) { base.OnTextChanged(e); if (caption != null) caption.Invalidate(); }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            using (Pen border = new Pen(frameColor)) e.Graphics.DrawRectangle(border, 0, 0, Math.Max(1, Width - 1), Math.Max(1, Height - 1));
        }
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);
            if (m.Msg != 0x84 || reminder || WindowState != FormWindowState.Normal) return;
            long value = m.LParam.ToInt64();
            Point point = PointToClient(new Point((short)(value & 0xffff), (short)((value >> 16) & 0xffff)));
            int edge = Math.Max(5, Padding.Left * 4);
            bool left = point.X < edge, right = point.X >= Width - edge, top = point.Y < edge, bottom = point.Y >= Height - edge;
            if (left || right || top || bottom)
                m.Result = new IntPtr(top ? (left ? 13 : right ? 14 : 12) : bottom ? (left ? 16 : right ? 17 : 15) : left ? 10 : 11);
        }
        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing && ownedIcon != null) { ownedIcon.Dispose(); ownedIcon = null; }
        }
    }

    internal sealed class ThemedNumericUpDown : NumericUpDown
    {
        public bool RequireClickForWheel;
        private bool wheelActivated;
        public ThemedNumericUpDown()
        {
            foreach (Control child in Controls) child.MouseDown += delegate { wheelActivated = true; };
        }
        protected override void OnMouseDown(MouseEventArgs e) { wheelActivated = true; base.OnMouseDown(e); }
        protected override void OnLeave(EventArgs e) { wheelActivated = false; base.OnLeave(e); }
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            if (RequireClickForWheel && (!wheelActivated || !ContainsFocus))
            { HandledMouseEventArgs handled = e as HandledMouseEventArgs; if (handled != null) handled.Handled = true; return; }
            base.OnMouseWheel(e);
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x20A && RequireClickForWheel && (!wheelActivated || !ContainsFocus)) { WheelRouting.Forward(this, ref m); return; }
            base.WndProc(ref m);
        }
        private ArrowPainter arrows;
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            if (Controls.Count > 0)
            {
                if (arrows != null) arrows.ReleaseHandle();
                arrows = new ArrowPainter(Controls[0], this);
            }
        }
        protected override void OnBackColorChanged(EventArgs e) { base.OnBackColorChanged(e); if (Controls.Count > 0) Controls[0].Invalidate(); }
        protected override void OnForeColorChanged(EventArgs e) { base.OnForeColorChanged(e); if (Controls.Count > 0) Controls[0].Invalidate(); }
        protected override void Dispose(bool disposing)
        { if (disposing && arrows != null) arrows.ReleaseHandle(); base.Dispose(disposing); }
        private sealed class ArrowPainter : NativeWindow
        {
            private readonly Control control;
            private readonly NumericUpDown owner;
            public ArrowPainter(Control control, NumericUpDown owner) { this.control = control; this.owner = owner; AssignHandle(control.Handle); }
            protected override void WndProc(ref Message m)
            {
                base.WndProc(ref m);
                if (m.Msg != 0xF && m.Msg != 0x318) return;
                using (Graphics graphics = m.Msg == 0x318 ? Graphics.FromHdc(m.WParam) : Graphics.FromHwnd(Handle))
                using (Brush background = new SolidBrush(owner.BackColor))
                using (Pen line = new Pen(owner.ForeColor, 1.5f))
                {
                    graphics.FillRectangle(background, control.ClientRectangle);
                    int x = control.Width / 2, up = control.Height / 4, down = control.Height * 3 / 4;
                    graphics.DrawLine(line, x - 3, up + 2, x, up - 1); graphics.DrawLine(line, x, up - 1, x + 3, up + 2);
                    graphics.DrawLine(line, x - 3, down - 2, x, down + 1); graphics.DrawLine(line, x, down + 1, x + 3, down - 2);
                }
            }
        }
    }

    internal sealed class ThemedComboBox : ComboBox
    {
        private bool wheelActivated;
        protected override void OnLeave(EventArgs e) { wheelActivated = false; base.OnLeave(e); }
        public Color Accent { get; set; }
        public ThemedComboBox()
        {
            DropDownStyle = ComboBoxStyle.DropDownList; DrawMode = DrawMode.OwnerDrawFixed; FlatStyle = FlatStyle.Flat;
        }
        protected override void OnFontChanged(EventArgs e) { base.OnFontChanged(e); ItemHeight = Font.Height + 6; }
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            bool selected = (e.State & DrawItemState.Selected) != 0 && (e.State & DrawItemState.ComboBoxEdit) == 0;
            Color background = selected ? Accent : BackColor;
            Color foreground = selected ? Styles.Contrast(Accent) : ForeColor;
            using (Brush brush = new SolidBrush(background)) e.Graphics.FillRectangle(brush, e.Bounds);
            string text = e.Index >= 0 ? GetItemText(Items[e.Index]) : Text;
            TextRenderer.DrawText(e.Graphics, text, Font, Rectangle.Inflate(e.Bounds, -4, 0), foreground, TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }
        protected override void WndProc(ref Message m)
        {
            if (m.Msg == 0x201) wheelActivated = true;
            if (m.Msg == 0x20A && (!wheelActivated || !ContainsFocus)) { WheelRouting.Forward(this, ref m); return; }
            base.WndProc(ref m);
            if (m.Msg != 0xF && m.Msg != 0x317 && m.Msg != 0x318) return;
            using (Graphics graphics = m.Msg == 0xF ? Graphics.FromHwnd(Handle) : Graphics.FromHdc(m.WParam))
            {
                int width = Math.Max(SystemInformation.VerticalScrollBarWidth, Height);
                Rectangle rect = new Rectangle(Width - width, 1, width - 1, Height - 2);
                using (Brush brush = new SolidBrush(BackColor)) graphics.FillRectangle(brush, rect);
                int x = rect.Left + rect.Width / 2, y = Height / 2;
                using (Pen pen = new Pen(ForeColor, 1.5f)) { graphics.DrawLine(pen, x - 4, y - 2, x, y + 2); graphics.DrawLine(pen, x, y + 2, x + 4, y - 2); }
                using (Pen border = new Pen(Accent)) graphics.DrawRectangle(border, 0, 0, Width - 1, Height - 1);
            }
        }
    }

    internal static class WheelRouting
    {
        [DllImport("user32.dll")] private static extern IntPtr SendMessage(IntPtr handle, int message, IntPtr wParam, IntPtr lParam);
        public static void Forward(Control control, ref Message message)
        {
            for (Control parent = control.Parent; parent != null; parent = parent.Parent)
            {
                ScrollableControl scroll = parent as ScrollableControl;
                if (scroll != null && scroll.AutoScroll)
                { SendMessage(scroll.Handle, message.Msg, message.WParam, message.LParam); break; }
            }
            message.Result = IntPtr.Zero;
        }
    }

    internal sealed class ThemeMenuRenderer : ToolStripProfessionalRenderer
    {
        private readonly Color surface, foreground, accent;
        public ThemeMenuRenderer(Preferences p)
        {
            surface = Preferences.ColorOf(p.Surface); foreground = Preferences.ColorOf(p.Foreground); accent = Preferences.ColorOf(p.Accent);
        }
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
        { using (Brush brush = new SolidBrush(surface)) e.Graphics.FillRectangle(brush, e.AffectedBounds); }
        protected override void OnRenderImageMargin(ToolStripRenderEventArgs e) { }
        protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
        { using (Pen pen = new Pen(accent)) e.Graphics.DrawRectangle(pen, 0, 0, e.ToolStrip.Width - 1, e.ToolStrip.Height - 1); }
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
        {
            using (Brush brush = new SolidBrush(e.Item.Selected && e.Item.Enabled ? accent : surface))
                e.Graphics.FillRectangle(brush, new Rectangle(Point.Empty, e.Item.Size));
        }
        protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
        {
            e.TextColor = e.Item.Selected && e.Item.Enabled ? Styles.Contrast(accent) : foreground;
            TextRenderer.DrawText(e.Graphics, e.Text, e.TextFont, e.TextRectangle, e.TextColor, e.TextFormat);
        }
        protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
        { using (Pen pen = new Pen(Color.FromArgb(90, accent))) e.Graphics.DrawLine(pen, 10, e.Item.Height / 2, e.Item.Width - 10, e.Item.Height / 2); }
        protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e) { e.ArrowColor = foreground; base.OnRenderArrow(e); }
        public static void Apply(ContextMenuStrip menu, Preferences p)
        {
            menu.Renderer = new ThemeMenuRenderer(p); menu.BackColor = Preferences.ColorOf(p.Surface); menu.ForeColor = Preferences.ColorOf(p.Foreground);
            menu.ShowImageMargin = false; menu.DropShadowEnabled = false;
            foreach (ToolStripItem item in menu.Items) item.ForeColor = menu.ForeColor;
        }
    }
}
