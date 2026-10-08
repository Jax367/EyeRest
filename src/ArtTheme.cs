using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Reflection;
using System.Windows.Forms;

namespace EyeRest
{
    internal static class ArtResources
    {
        private static readonly System.Collections.Generic.Dictionary<string, Image> portraits = new System.Collections.Generic.Dictionary<string, Image>();
        public static Image Portrait { get { return GetPortrait("chibi"); } }
        public static string ResourceName(string id) { return id == "weary" ? "EyeRest.weary.png" : id == "standing" ? "EyeRest.user-standing.png" : "EyeRest.user-chibi.png"; }
        public static Image GetPortrait(string id)
        {
                string resource = ResourceName(id);
                Image portrait;
                if (!portraits.TryGetValue(resource, out portrait))
                {
                    using (Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(resource))
                    {
                        if (stream == null) throw new InvalidOperationException("缺少艺术层插画资源，请重新构建程序。");
                        using (Image decoded = Image.FromStream(stream)) portrait = new Bitmap(decoded);
                    }
                    portraits.Add(resource, portrait);
                }
                return portrait;
        }
        public static void Dispose() { foreach (Image image in portraits.Values) image.Dispose(); portraits.Clear(); }
    }

    internal sealed class ArtPortrait : Control
    {
        public Color Accent = Color.FromArgb(193, 214, 110);
        private string portraitId = "chibi";
        public string PortraitId { get { return portraitId; } set { portraitId = value; Invalidate(); } }
        public ArtPortrait()
        {
            DoubleBuffered = true; ResizeRedraw = true;
            AccessibleName = "Netzach 人物形象，保持完整比例显示";
        }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            Image image = ArtResources.GetPortrait(PortraitId);
            float ratio = Math.Min((float)Width / image.Width, (float)Height / image.Height);
            RectangleF rect = new RectangleF((Width - image.Width * ratio) / 2, (Height - image.Height * ratio) / 2, image.Width * ratio, image.Height * ratio);
            e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
            e.Graphics.DrawImage(image, rect);
        }
    }

    // Static decoration: no animation timer or additional background work.
    internal sealed class ArtHeader : Panel
    {
        public bool ArtEnabled;
        public Color Accent;
        public ArtHeader() { DoubleBuffered = true; ResizeRedraw = true; }
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            if (!ArtEnabled) return;
            Graphics g = e.Graphics;
            GraphicsState state = g.Save();
            g.SmoothingMode = SmoothingMode.AntiAlias;
            float scale = Height / 106f;
            g.TranslateTransform(Width - 225 * scale, 0); g.ScaleTransform(scale, scale);
            using (Pen line = new Pen(Color.FromArgb(62, Accent), 1))
            using (Pen root = new Pen(Color.FromArgb(95, Accent), 2))
            using (Brush glow = new SolidBrush(Color.FromArgb(150, Accent)))
            {
                for (int row = 0; row < 3; row++)
                {
                    int y = 12 + row * 26;
                    g.DrawLine(line, 5, y + 22, 219, y + 22);
                    for (int book = 0; book < 16; book++)
                        g.DrawRectangle(line, 8 + book * 13, y + book % 3 * 2, 8, 19 - book % 3 * 2);
                }
                g.DrawBezier(root, 167, 99, 114, 66, 207, 25, 179, 0);
                g.DrawBezier(line, 176, 53, 119, 34, 130, 23, 115, 2);
                for (int i = 0; i < 10; i++) g.FillEllipse(glow, (i * 59 + 17) % 215, (i * 37 + 9) % 90, 2, 2);
            }
            g.Restore(state);
        }
    }
}
