using System;
using System.Threading;
using Cscg.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Cscg.Core.Sources;

namespace Cscg.Compactor
{
    [Generator(LanguageNames.CSharp)]
    public sealed class CompactGenerator : IIncrementalGenerator
    {
        private const string Space = Coding.AutoNamespace;
        private const string BinObjName = "Compacted";
        private const string ExtObjName = "CompactedExt";
        private const string IntObjName = "ICompacted";

        public void Initialize(IncrementalGeneratorInitializationContext igi)
        {
            igi.RegisterPostInitializationOutput(ctx =>
            {
                var attrCode = Coding.GenerateAttr(BinObjName, Space);
                ctx.AddSource($"{BinObjName}Attribute.g.cs", From(attrCode));
            });

            const string fqnA = $"{Space}.{BinObjName}Attribute";
            var sp = igi.SyntaxProvider;
            igi.RegisterSourceOutput(sp.ForAttributeWithMetadataName(fqnA, Check, Fuck2), Fuck3);
        }

        private static bool Check(SyntaxNode node, CancellationToken _)
        {
            return node is ClassDeclarationSyntax;
        }

        private static object Fuck2(GeneratorAttributeSyntaxContext ctx, CancellationToken _)
        {
            return ctx.TargetNode as ClassDeclarationSyntax;
        }

        private void Fuck3(SourceProductionContext ctx, object arg2)
        {

        }
    }
}