using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cscg.Core
{
    public static class Typing
    {
        public static string Parse(TypeSyntax type)
        {
            var name = type.GetText().ToString().TrimNull();
            switch (name)
            {
                case "float": return nameof(System.Single);
                case "string": return nameof(System.String);
                case "int": return nameof(System.Int32);
                case "bool": return nameof(System.Boolean);
                default: return $"_{name}";
            }
        }
    }
}