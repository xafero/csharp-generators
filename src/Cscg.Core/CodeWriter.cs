using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Cscg.Core
{
    public sealed class CodeWriter
    {
        public string Space { get; set; } = "\t";
        public int Level { get; set; }

        public void AppendLine(string text = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                Lines.Add(string.Empty);
                return;
            }
            if (text == "}")
                Level--;
            var prefix = string.Join("", Enumerable.Repeat(Space, Level));
            Lines.Add(prefix + text);
            if (text == "{")
                Level++;
        }

        public IList<string> Lines { get; } = new List<string>();

        public void AppendLines(CodeWriter writer) => AppendLines(writer.Lines);

        public void AppendLines(IEnumerable<string> lines, bool trim = true)
        {
            foreach (var line in lines)
                AppendLine(trim ? line.Trim() : line);
        }

        public override string ToString()
        {
            var code = new StringBuilder();
            foreach (var line in Lines)
                code.AppendLine(line);
            return code.ToString();
        }

        public void ModifyLast(Func<string, string> func)
        {
            Lines.ModifyLast(func);
        }
    }
}