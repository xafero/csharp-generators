using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cscg.Core
{
    public delegate void Exec(ClassDeclarationSyntax cds, string name, CodeWriter code, SyntaxWrap syntax);
}