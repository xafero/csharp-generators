using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Cscg.Core
{
    public static class Sources
    {
        public static readonly CultureInfo Invariant = CultureInfo.InvariantCulture;

        public static SourceText From(string source)
        {
            var text = SourceText.From(source, Encoding.UTF8);
            return text;
        }

        public static (string name, string content) ToNamed(AdditionalText text, CancellationToken token)
            => (name: Path.GetFileNameWithoutExtension(text.Path), content: ToContent(text, token));

        private static string ToContent(this AdditionalText file, CancellationToken token)
            => file.GetText(token)!.ToString();

        public static bool HasEnding(this AdditionalText file, string ending)
            => file.Path.EndsWith($".{ending}");

        public static bool HasThisAttribute(this SyntaxNode syntax, string name)
            => syntax is ClassDeclarationSyntax { AttributeLists.Count: >= 1 } clazz
               && clazz.AttributeLists.Any(al =>
                   al.Attributes.Any(a => a.Name.ToString() == name));

        public static ClassDeclarationSyntax GetTarget(this GeneratorSyntaxContext context)
        {
            var classDeclaration = (ClassDeclarationSyntax)context.Node;
            return classDeclaration;
        }
    }
}