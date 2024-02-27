using System.Linq;
using System.Text;

namespace Cscg.Core
{
    public sealed class CodeWriter
    {
        private readonly StringBuilder _code;

        public CodeWriter()
        {
            _code = new StringBuilder();
        }

        public string Space { get; set; } = '\t' + "";
        public int Level { get; set; }

        public void AppendLine(string text = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                _code.AppendLine();
                return;
            }
            if (text == "}")
                Level--;
            var prefix = string.Join("", Enumerable.Repeat(Space, Level));
            _code.AppendLine(prefix + text);
            if (text == "{")
                Level++;
        }

        public override string ToString()
        {
            return _code.ToString();
        }
    }
}