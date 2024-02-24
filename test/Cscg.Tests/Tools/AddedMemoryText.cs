using System.Threading;
using Cscg.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Cscg.Tests.Tools
{
    public sealed class AddedMemoryText : AdditionalText
    {
        private readonly (string name, string content) _file;

        public AddedMemoryText((string name, string content) file)
        {
            _file = file;
        }

        public override string Path => _file.name;

        public override SourceText GetText(CancellationToken cancellationToken = new())
        {
            var text = Sources.From(_file.content);
            return text;
        }

        public static implicit operator AddedMemoryText((string n, string c) f)
            => new(f);
    }
}