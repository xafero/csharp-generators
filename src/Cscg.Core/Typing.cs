using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cscg.Core
{
    public static class Typing
    {
        public static string Parse(TypeSyntax type, out int arrayRank)
        {
            var name = type.GetText().ToString().TrimNull();
            arrayRank = name.Count(n => n == '[');
            if (arrayRank >= 1)
                name = name.Split(['['], 2).First();
            switch (name)
            {
                case "float": return nameof(System.Single);
                case "string": return nameof(System.String);
                case "long": return nameof(System.Int64);
                case "int": return nameof(System.Int32);
                case "short": return nameof(System.Int16);
                case "ulong": return nameof(System.UInt64);
                case "uint": return nameof(System.UInt32);
                case "ushort": return nameof(System.UInt16);
                case "bool": return nameof(System.Boolean);
                case "sbyte": return nameof(System.SByte);
                case "char": return nameof(System.Char);
                case "double": return nameof(System.Double);
                case "decimal": return nameof(System.Decimal);
                case "byte": return nameof(System.Byte);
                case "DateTime": return nameof(System.DateTime);
                case "Guid": return nameof(System.Guid);
                default: return $"_{name}";
            }
        }
    }
}