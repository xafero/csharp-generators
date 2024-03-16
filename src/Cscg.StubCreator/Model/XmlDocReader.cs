using System.IO;
using System.Text.RegularExpressions;

namespace Cscg.StubCreator.Model
{
    public sealed class XmlDocReader : TextReader
    {
        private readonly TextReader _input;

        public XmlDocReader(TextReader input)
        {
            _input = input;
        }

        public override int Read(char[] buffer, int index, int count)
        {
            var result = _input.Read(buffer, index, count);
            PatchInnerTags(buffer);
            return result;
        }

        private static void PatchInnerTags(char[] buffer)
        {
            var text = new string(buffer);
            var pattern = "<see\\scref=\\\"(.*?)\\\" />";
            text = Regex.Replace(text, pattern, "@see_cref=_($1)__");
            pattern = "<see\\scref=\\\"(.*?)\\\"/>";
            text = Regex.Replace(text, pattern, "@see_cref=_($1)_");
            pattern = "<c>(.*?)</c>";
            text = Regex.Replace(text, pattern, "@c_($1)__");
            text.ToCharArray().CopyTo(buffer, 0);
        }
    }
}