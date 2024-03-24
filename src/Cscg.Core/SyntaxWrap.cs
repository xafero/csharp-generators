using System.Linq;
using Cscg.Core.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cscg.Core
{
    public class SyntaxWrap(GeneratorAttributeSyntaxContext context) : ISymbolFetch
    {
        public ClassDeclarationSyntax Class => context.TargetNode as ClassDeclarationSyntax ??
                                               context.TargetNode.Parent as ClassDeclarationSyntax;

        public INamedTypeSymbol Symbol => context.TargetSymbol as INamedTypeSymbol ??
                                          context.TargetSymbol.ContainingSymbol as INamedTypeSymbol;

        public (MethodDeclarationSyntax, IMethodSymbol)? Method
            => context.TargetNode is not MethodDeclarationSyntax md ||
               context.TargetSymbol is not IMethodSymbol ms
                ? null
                : (md, ms);

        public AttributeData Attribute => context.Attributes.First();

        public Particle GetInfo(ISymbol symbol)
        {
            ITypeSymbol retType;
            string name;
            switch (symbol)
            {
                case IFieldSymbol fs:
                    retType = fs.Type;
                    name = fs.Name;
                    break;
                case IParameterSymbol rs:
                    retType = rs.Type;
                    name = rs.Name;
                    break;
                case IMethodSymbol ms:
                    retType = ms.ReturnType;
                    name = ms.Name;
                    break;
                case IPropertySymbol ps:
                    retType = ps.Type;
                    name = ps.Name;
                    break;
                default:
                    return null;
            }
            return new Particle { Name = name, ReturnType = retType };
        }

        public ISymbol GetSymbol(SyntaxNode node)
        {
            var model = context.SemanticModel;
            var symbol = model.GetDeclaredSymbol(node)
                         ?? model.GetSymbolInfo(node).Symbol;
            return symbol;
        }
    }
}