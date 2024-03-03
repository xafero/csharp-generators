using System.Globalization;
using System.IO;
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

        public static string GetParentName(this ClassDeclarationSyntax cds)
        {
            if (cds.Parent is NamespaceDeclarationSyntax nds)
                return nds.Name.GetText().ToString().TrimNull();
            return null;
        }

        public static string GetClassName(this ClassDeclarationSyntax cds)
        {
            var text = cds.Identifier.Text.TrimNull();
            return text;
        }

        public static SyntaxWrap Wrap(this GeneratorAttributeSyntaxContext ctx)
        {
            var wrap = new SyntaxWrap(ctx);
            return wrap;
        }

        public static string GetFqn(this ClassDeclarationSyntax clazz)
        {
            return $"{clazz.GetParentName()}.{clazz.GetClassName()}";
        }
    }
}