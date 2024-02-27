using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cscg.Core
{
    public static class Deep
    {
        public static IEnumerable<ClassDeclarationSyntax> SearchType(this ITypeSymbol type,
            IModuleSymbol[] modules = null)
        {
            var typeName = type.ToTrimDisplay();
            var module = new[] { type.ContainingModule }.Concat(modules ?? []);
            foreach (var loc in module.SelectMany(m => m.Locations))
                if (loc.SourceTree is var tree && tree.TryGetRoot(out var root))
                    if (root is CompilationUnitSyntax cs)
                        foreach (var member in cs.Members)
                            if (member is NamespaceDeclarationSyntax nds)
                                foreach (var child in nds.Members)
                                    if (child is ClassDeclarationSyntax cds)
                                        if (cds.BaseList != null)
                                            foreach (var cn in cds.BaseList.ChildNodes())
                                            {
                                                var cnt = cn.ToFullString().TrimNull();
                                                var fqn = $"{cds.GetParentName()}.{cnt}";
                                                if (cnt == typeName || fqn == typeName)
                                                    yield return cds;
                                            }
        }
    }
}