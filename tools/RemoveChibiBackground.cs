using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;

// Offline asset operation. Preserve every RGB channel; only clear alpha on
// neutral checkerboard pixels connected to explicitly identified background.
internal static class RemoveChibiBackground
{
    public static void Main(string[] args)
    {
        using (Bitmap source = new Bitmap(args[0]))
        using (Bitmap output = new Bitmap(source.Width, source.Height, PixelFormat.Format32bppArgb))
        {
            int width = source.Width, height = source.Height;
            bool[] removed = new bool[width * height];
            Queue<Point> queue = new Queue<Point>();
            for (int x = 0; x < width; x++) { queue.Enqueue(new Point(x, 0)); queue.Enqueue(new Point(x, height - 1)); }
            for (int y = 0; y < height; y++) { queue.Enqueue(new Point(0, y)); queue.Enqueue(new Point(width - 1, y)); }
            bool weary = args.Length > 2 && args[2] == "weary";
            if (weary)
            {
                queue.Enqueue(new Point(414, 667));
                queue.Enqueue(new Point(735, 489));
            }
            else
            {
            // Enclosed checkerboard: hair-bun hole, fringe/neck gap, coat/legs gaps.
            queue.Enqueue(new Point(651, 93));
            queue.Enqueue(new Point(443, 612));
            queue.Enqueue(new Point(337, 602));
            queue.Enqueue(new Point(390, 670));
            // Separate closed triangle between the lower fringe and right shoulder.
            queue.Enqueue(new Point(394, 710));
            queue.Enqueue(new Point(531, 1200));
            queue.Enqueue(new Point(399, 1200));
            queue.Enqueue(new Point(648, 1180));
            }
            while (queue.Count > 0)
            {
                Point p = queue.Dequeue();
                if (p.X < 0 || p.Y < 0 || p.X >= width || p.Y >= height) continue;
                int index = p.Y * width + p.X;
                if (removed[index]) continue;
                Color c = source.GetPixel(p.X, p.Y);
                int min = Math.Min(c.R, Math.Min(c.G, c.B));
                int max = Math.Max(c.R, Math.Max(c.G, c.B));
                if (min < 200 || max - min > 16) continue;
                removed[index] = true;
                queue.Enqueue(new Point(p.X - 1, p.Y)); queue.Enqueue(new Point(p.X + 1, p.Y));
                queue.Enqueue(new Point(p.X, p.Y - 1)); queue.Enqueue(new Point(p.X, p.Y + 1));
            }
            int transparent = 0;
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                Color c = source.GetPixel(x, y);
                bool clear = removed[y * width + x];
                if (clear) transparent++;
                output.SetPixel(x, y, Color.FromArgb(clear ? 0 : c.A, c.R, c.G, c.B));
            }
            output.Save(args[1], ImageFormat.Png);
            using (Bitmap check = new Bitmap(args[1]))
            {
                for (int y = 0; y < height; y++)
                for (int x = 0; x < width; x++)
                {
                    Color a = source.GetPixel(x, y), b = check.GetPixel(x, y);
                    if (a.R != b.R || a.G != b.G || a.B != b.B) throw new Exception("RGB changed");
                    // Face interior must remain fully opaque and pixel-identical.
                    if (!weary && x >= 415 && x <= 605 && y >= 460 && y <= 545 && a != b) throw new Exception("Face changed");
                    if (weary && x >= 420 && x <= 560 && y >= 580 && y <= 625 && a != b) throw new Exception("Face changed");
                }
            }
            Console.WriteLine("Verified: all RGB pixels unchanged; face RGBA unchanged; transparent pixels: " + transparent);
        }
    }
}
