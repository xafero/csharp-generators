using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cscg.Core
{
    public class SyntaxWrap(GeneratorSyntaxContext context)
    {
        public ClassDeclarationSyntax Class => context.Node as ClassDeclarationSyntax;

        public Particle GetInfo(ISymbol symbol)
        {
            ITypeSymbol retType;
            switch (symbol)
            {
                case IFieldSymbol fs:
                    retType = fs.Type;
                    break;
                case IParameterSymbol rs:
                    retType = rs.Type;
                    break;
                case IMethodSymbol ms:
                    retType = ms.ReturnType;
                    break;
                case IPropertySymbol ps:
                    retType = ps.Type;
                    break;
                default:
                    return null;
            }
            return new Particle { ReturnType = retType };
        }

        public ISymbol GetSymbol(SyntaxNode node)
        {
            var model = context.SemanticModel;
            var symbol = model.GetDeclaredSymbol(node);
            return symbol;
        }
    }
}