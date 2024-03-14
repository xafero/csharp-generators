using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cscg.Core.Model
{
    public static class MetaTool
    {
        public static CType Extract(this TypeDeclarationSyntax type)
        {
            var kind = type is ClassDeclarationSyntax ? TypeKind.Class : default;
            var name = type.Identifier.Text;
            var parent = type.Parent is NamespaceDeclarationSyntax nds
                ? nds.Name.ToString()
                : default;
            var item = new CType
            {
                Parent = parent, Kind = kind, Name = name,
                Attributes = Extract(type.AttributeLists).ToList(),
                Members = Extract(type.Members).ToList()
            };
            return item;
        }

        public static IEnumerable<CAttribute> Extract(this IEnumerable<AttributeListSyntax> attributes)
        {
            foreach (var attribute in attributes)
            {
                var prm = new List<CParam>();
                var text = attribute.ToString();
                var tmp2 = text.Split(["("], 2, StringSplitOptions.None);
                var type = tmp2.FirstOrDefault()?.Trim('[', ']');
                if (tmp2.Length == 2)
                {
                    var mul = tmp2.Last().Split([", "], StringSplitOptions.None);
                    foreach (var mm in mul)
                    {
                        var sub = mm.Split([" = "], StringSplitOptions.None);
                        if (sub.Length == 2)
                        {
                            var key = sub.First().Trim();
                            var val = sub.Last().Trim(']', ')', '"').Trim();
                            if (val.StartsWith("nameof("))
                                val = val.Split(['.'], 2).Last();
                            prm.Add(new CParam { Name = key, Value = val });
                        }
                    }
                }
                yield return new CAttribute
                {
                    Type = type, Params = prm
                };
            }
        }

        public static IEnumerable<CMember> Extract(this IEnumerable<MemberDeclarationSyntax> members)
        {
            foreach (var member in members)
                if (member is PropertyDeclarationSyntax pds)
                    yield return new CMember
                    {
                        Kind = MemberKind.Property,
                        Name = pds.Identifier.Text,
                        Type = pds.Type.ToString(),
                        Attributes = Extract(pds.AttributeLists).ToList()
                    };
        }
    }
}