using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace EyeRest
{
    internal static class QuoteCollection
    {
        public static string[] Clean(string[] quotes)
        {
            List<string> result = new List<string>();
            if (quotes != null) foreach (string quote in quotes)
            {
                if (String.IsNullOrWhiteSpace(quote)) continue;
                string value = quote.Trim(); result.Add(value.Length > 500 ? value.Substring(0, 500) : value);
                if (result.Count == 200) break;
            }
            return result.ToArray();
        }
        public static string[] Parse(string text)
        {
            List<string> values = new List<string>(); StringBuilder current = new StringBuilder();
            foreach (string line in text.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n'))
            {
                if (line.Trim() == "---") { Add(values, current.ToString()); current.Length = 0; }
                else { if (current.Length > 0) current.Append('\n'); current.Append(line); }
            }
            Add(values, current.ToString());
            if (values.Count > 200) throw new ArgumentException("语录集最多保存 200 条。");
            return values.ToArray();
        }
        private static void Add(List<string> values, string text)
        {
            text = text.Trim(); if (text.Length == 0) return;
            if (text.Length > 500) throw new ArgumentException("第 " + (values.Count + 1) + " 条语录超过了 500 字，请缩短后保存。");
            values.Add(text);
        }
        public static string Format(string[] quotes) { return String.Join("\r\n---\r\n", quotes).Replace("\r\n", "\n").Replace("\n", "\r\n"); }
    }
    internal sealed class QuotePlayer
    {
        private readonly Random random;
        private string signature;
        private int cursor;
        private int last = -1;
        public QuotePlayer() : this(new Random()) { }
        public QuotePlayer(Random random) { this.random = random; }
        public string Select(Preferences p, bool advance)
        {
            string[] quotes = p.CustomQuotes;
            if (p.QuoteMode == "fixed" || quotes == null || quotes.Length == 0) return p.DisplayMessage;
            // Length prefixes avoid collisions even when user text contains separators.
            StringBuilder key = new StringBuilder(p.QuoteMode);
            foreach (string quote in quotes) key.Append('|').Append(quote.Length).Append(':').Append(quote);
            bool same = signature == key.ToString();
            int next = same ? cursor : 0, previous = same ? last : -1;
            int index;
            if (p.QuoteMode == "random")
            {
                Random source = advance ? random : new Random();
                index = source.Next(quotes.Length > 1 && previous >= 0 ? quotes.Length - 1 : quotes.Length);
                if (quotes.Length > 1 && previous >= 0 && index >= previous) index++;
            }
            else index = next % quotes.Length;
            if (advance) { signature = key.ToString(); cursor = (index + 1) % quotes.Length; last = index; }
            return quotes[index];
        }
    }
    internal sealed class QuoteEditor : ThemedForm
    {
        public string[] Quotes { get; private set; }
        private readonly Font ownedFont;
        public QuoteEditor(Preferences p)
        {
            SuspendLayout(); AutoScaleMode = AutoScaleMode.None;
            Text = "编辑自定义语录集"; ClientSize = new Size(660, 480); MinimumSize = new Size(560, 420);
            StartPosition = FormStartPosition.CenterParent; ownedFont = Styles.FontFor(p, p.FontSize, FontStyle.Regular); Font = ownedFont;
            TableLayoutPanel layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(22), ColumnCount = 1, RowCount = 4 };
            layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 62)); layout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 35)); layout.RowStyles.Add(new RowStyle(SizeType.Absolute, 45)); Controls.Add(layout);
            layout.Controls.Add(new Label { Text = "每条语录之间用单独一行 --- 分隔。\n同一条内可以换行；最多 200 条，每条最多 500 字。", Dock = DockStyle.Fill });
            TextBox editor = new TextBox { Name = "QuoteCollectionText", Multiline = true, AcceptsReturn = true, ScrollBars = ScrollBars.Vertical, Dock = DockStyle.Fill, MaxLength = 102000, Text = QuoteCollection.Format(p.CustomQuotes) };
            layout.Controls.Add(editor);
            Shown += delegate { editor.SelectionStart = editor.TextLength; editor.SelectionLength = 0; };
            Label feedback = new Label { Dock = DockStyle.Fill, TextAlign = ContentAlignment.MiddleLeft, Text = "保存此编辑后，还需在主窗口点击「保存设置」。" }; layout.Controls.Add(feedback);
            FlowLayoutPanel buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, WrapContents = false };
            Button save = Styles.Button("使用此语录集", delegate {
                try { Quotes = QuoteCollection.Parse(editor.Text); DialogResult = DialogResult.OK; Close(); }
                catch (ArgumentException ex) { feedback.Text = ex.Message; }
            }); save.Tag = "primary"; buttons.Controls.Add(save);
            Button cancel = Styles.Button("取消", delegate { DialogResult = DialogResult.Cancel; Close(); }); buttons.Controls.Add(cancel);
            layout.Controls.Add(buttons); CancelButton = cancel;
            Styles.Apply(this, p, Preferences.ColorOf(p.Background), false); ApplyChrome(p, false);
            Styles.PrepareLayout(this); AutoScaleMode = AutoScaleMode.Dpi; AutoScaleDimensions = new SizeF(96, 96); ResumeLayout(true);
        }
        protected override void Dispose(bool disposing) { base.Dispose(disposing); if (disposing) ownedFont.Dispose(); }
    }
}
