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
                Members = Extract(type.Members).ToList()
            };
            return item;
        }

        public static IEnumerable<CMember> Extract(this IEnumerable<MemberDeclarationSyntax> members)
        {
            foreach (var member in members)
                if (member is PropertyDeclarationSyntax pds)
                    yield return new CMember
                    {
                        Kind = MemberKind.Property,
                        Name = pds.Identifier.Text,
                        Type = pds.Type.ToString()
                    };
        }
    }
}