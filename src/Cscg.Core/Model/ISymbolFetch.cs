using Microsoft.CodeAnalysis;

namespace Cscg.Core.Model
{
    public interface ISymbolFetch
    {
        ISymbol GetSymbol(SyntaxNode node);
    }
}