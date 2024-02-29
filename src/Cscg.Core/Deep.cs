using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cscg.Core
{
    public static class Deep
    {
        public static IEnumerable<ClassDeclarationSyntax> SearchType(this ITypeSymbol type,
            IModuleSymbol[] modules = null, string forceName = null)
        {
            var typeName = forceName ?? type.ToTrimDisplay();
            var module = new[] { type.ContainingModule }.Concat(modules ?? []);
            foreach (var loc in module.SelectMany(m => m.Locations))
                if (loc.SourceTree is var tree && tree.TryGetRoot(out var root))
                    if (root is CompilationUnitSyntax cs)
                        foreach (var member in cs.Members)
                            if (member is NamespaceDeclarationSyntax nds)
                                foreach (var child in nds.Members)
                                    if (child is ClassDeclarationSyntax cds)
                                    {
                                        var cdsFqn = cds.GetFqn();
                                        if (cdsFqn == typeName && !cds.IsAbstract())
                                            yield return cds;
                                        if (cds.BaseList != null)
                                            foreach (var cn in cds.BaseList.ChildNodes())
                                            {
                                                var cnt = cn.ToFullString().TrimNull();
                                                var fqn = $"{cds.GetParentName()}.{cnt}";
                                                if (cnt == typeName || fqn == typeName)
                                                    foreach (var s in SearchType(type, forceName: cdsFqn))
                                                        yield return s;
                                            }
                                    }
        }

        private static bool IsAbstract(this ClassDeclarationSyntax cds)
            => HasModifier(cds, SyntaxKind.AbstractKeyword);

        private static bool HasModifier(this ClassDeclarationSyntax cds, SyntaxKind kind)
            => cds.Modifiers.Any(m => m.IsKind(kind));
    }
}