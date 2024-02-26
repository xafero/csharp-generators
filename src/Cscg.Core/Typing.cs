using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cscg.Core
{
    public static class Typing
    {
        public static string Parse(TypeSyntax type, out int arrayRank, out bool canNull)
        {
            var name = type.GetText().ToString().TrimNull();
            arrayRank = name.Count(n => n == '[');
            if (arrayRank >= 1)
                name = name.Split(['['], 2).First();
            canNull = name.EndsWith("?");
            if (canNull)
                name = name.Substring(0, name.Length - 1);
            switch (name)
            {
                case "Half": return "Half";
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
                case "DateTimeOffset": return nameof(System.DateTimeOffset);
                case "TimeSpan": return nameof(System.TimeSpan);
                case "Guid": return nameof(System.Guid);
                default: return $"_{name}";
            }
        }

        public static bool UnwrapNullable(this INamedTypeSymbol symbol, out ITypeSymbol found)
        {
            if (symbol.OriginalDefinition.SpecialType == SpecialType.System_Nullable_T
                && symbol.TypeArguments[0] is var nullType)
            {
                found = nullType;
                return true;
            }
            found = symbol;
            return false;
        }

        public static bool IsEnum(this ITypeSymbol type, out INamedTypeSymbol underlying)
        {
            if (Is(type, TypeKind.Enum))
            {
                underlying = (type as INamedTypeSymbol)?.EnumUnderlyingType;
                return true;
            }
            underlying = null;
            return false;
        }

        public static bool Is(this ITypeSymbol type, TypeKind kind) => type.TypeKind == kind;

        public static string ToTrimDisplay(this ITypeSymbol type) => type.ToDisplayString().TrimNull();
    }
}