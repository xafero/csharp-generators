using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Cscg.Core
{
    public static class Typing
    {
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

        public static bool IsNull(this ITypeSymbol type, out INamedTypeSymbol underlying)
        {
            if (Is(type, TypeKind.Struct) && type.Name == "Nullable" &&
                type is INamedTypeSymbol { TypeArguments.Length: 1 } nts)
            {
                underlying = (INamedTypeSymbol)nts.TypeArguments[0];
                return true;
            }
            underlying = null;
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

        public static IEnumerable<(string k, TypedConstant v)> FindArgs(this AttributeData ad)
            => ad.ConstructorArguments
                .Select((tc, id) => (k: id.ToString(), v: tc))
                .Concat(ad.NamedArguments
                    .Select(n => (k: n.Key, v: n.Value)));

        public static (string n, Dictionary<string, TypedConstant> c) GetArgDict(this AttributeData ad)
        {
            var name = ad.AttributeClass.ToTrimDisplay();
            return (name, ad.FindArgs().ToDictionary(a => a.k, a => a.v));
        }

        public static Dictionary<string, string> FindArgs(this ISymbol symbol, bool simple)
        {
            var dict = new Dictionary<string, string>();
            foreach (var data in symbol.GetAttributes())
            {
                var (name, constants) = data.GetArgDict();
                if (simple) name = name.Split('.').Last();
                dict[name] = default;
                foreach (var item in constants)
                    dict[$"{name}_{item.Key}"] = item.Value.ToCSharpString();
            }
            return dict;
        }

        public static string ToStr(object arg)
        {
            switch (arg)
            {
                case null:
                    return "null";
                case bool v:
                    return v ? "true" : "false";
                case string s:
                    return s.StartsWith("\"") ? s : $"\"{s}\"";
                default:
                    return $"[ {arg} ({arg.GetType()}) ]";
            }
        }

        public static IEnumerable<string> SplitTypeOf(string text)
        {
            foreach (var item in text.Trim('{', '}')
                         .Split([", "], StringSplitOptions.None)
                         .Select(t => t.Split(['('], 2)[1].TrimEnd(')')))
                yield return item;
        }

        public static (string nsp, string name) SplitType(string text)
        {
            var last = text.LastIndexOf('.');
            var nsp = text.Substring(0, last);
            var name = text.Substring(last).TrimStart('.');
            return (nsp, name);
        }
    }
}