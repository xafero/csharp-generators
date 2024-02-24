using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cscg.Core
{
    public static class Typing
    {
        public static string Parse(TypeSyntax type)
        {
            var name = type.GetText().ToString().TrimNull();
            return name;
        }
    }
}