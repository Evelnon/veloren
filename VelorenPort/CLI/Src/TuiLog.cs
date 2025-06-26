using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace VelorenPort.CLI {
    /// <summary>
    /// Captures log lines for the text user interface. This mirrors the
    /// behaviour of <c>tuilog.rs</c> by storing the parsed lines that should
    /// be rendered to the screen. ANSI colour codes are stripped but the text
    /// content is preserved.
    /// </summary>
    public class TuiLog : TextWriter {
        private readonly List<string> _lines = new();
        public IReadOnlyList<string> Lines => _lines;

        public override Encoding Encoding => Encoding.UTF8;

        public override void Write(char value) => Write(value.ToString());

        public override void Write(string? value) {
            if (string.IsNullOrEmpty(value)) return;
            AppendLines(value);
        }

        private void AppendLines(string value) {
            value = value.Replace("\r", string.Empty);
            foreach (var line in value.Split('\n')) {
                if (line.Length > 0) _lines.Add(RemoveAnsi(line));
            }
        }

        /// <summary>
        /// Keeps only the last <paramref name="height"/> lines in the log.
        /// </summary>
        public void Resize(int height) {
            if (height <= 0) {
                _lines.Clear();
                return;
            }
            if (_lines.Count > height) {
                _lines.RemoveRange(0, _lines.Count - height);
            }
        }

        private static string RemoveAnsi(string text) {
            // Very small subset of ANSI escape removal suitable for log storage
            int idx;
            while ((idx = text.IndexOf("\x1b[", StringComparison.Ordinal)) >= 0) {
                int end = text.IndexOf('m', idx);
                if (end < 0) break;
                text = text.Remove(idx, end - idx + 1);
            }
            return text;
        }
    }
}
