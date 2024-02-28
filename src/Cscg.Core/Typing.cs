﻿using System.Linq;
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

        public static bool IsArray(this ITypeSymbol type, out ITypeSymbol underlying)
        {
            if (Is(type, TypeKind.Array))
            {
                underlying = (type as IArrayTypeSymbol)?.ElementType;
                return true;
            }
            underlying = null;
            return false;
        }

        public static bool IsTyped(this ITypeSymbol type, out ITypeSymbol orig,
            out ITypeSymbol[] args, out bool isList, out bool isDict)
        {
            if (type is INamedTypeSymbol { IsGenericType: true, TypeArguments: { Length: >= 1 } nta } nts)
            {
                args = nta.ToArray();
                orig = nts.OriginalDefinition;
                IsColl(orig, out isList, out isDict);
                return true;
            }
            args = null;
            orig = type.OriginalDefinition;
            isList = false;
            isDict = false;
            return false;
        }

        public static bool CanBeParent(this ITypeSymbol type, out ITypeSymbol parent)
        {
            if (Is(type, TypeKind.Interface) ||
                (Is(type, TypeKind.Class) && (type.IsAbstract || !type.IsSealed)))
            {
                parent = type.OriginalDefinition;
                return true;
            }
            parent = null;
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

        private static void IsColl(this ITypeSymbol type, out bool isList, out bool isDict)
        {
            isList = false;
            isDict = false;
            var text = type.ToTrimDisplay();
            switch (text)
            {
                case "System.Collections.Generic.List<T>":
                case "System.Collections.Generic.IList<T>":
                case "System.Collections.Generic.ICollection<T>":
                case "System.Collections.Generic.IEnumerable<T>":
                    isList = true;
                    break;
                case "System.Collections.Generic.Dictionary<TKey, TValue>":
                case "System.Collections.Generic.IDictionary<TKey, TValue>":
                case "System.Collections.Generic.ICollection<System.Collections.Generic.KeyValuePair<TKey, TValue>>":
                case "System.Collections.Generic.IEnumerable<System.Collections.Generic.KeyValuePair<TKey, TValue>>":
                    isDict = true;
                    break;
            }
        }

        public static void ExtractBase(this ISymbol symbol, out INamedTypeSymbol baseType,
            out bool isAbstract, out bool isSealed)
        {
            isAbstract = symbol.IsAbstract;
            isSealed = symbol.IsSealed;
            baseType = null;
            if (symbol is ITypeSymbol ts)
            {
                var bt = ts.BaseType;
                var noBase = bt == null || bt.ToTrimDisplay() is "object";
                if (!noBase) baseType = bt;
            }
        }
    }
}