using System.Threading;
using Cscg.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Cscg.AdoNet.AdoSource;

namespace Cscg.AdoNet
{
    [Generator(LanguageNames.CSharp)]
    public sealed class AdoGenerator : IIncrementalGenerator
    {
        private const string ItfName = $"{BinObjName}Attribute";
        private const string ItfFqn = $"{LibSpace}.{ItfName}";

        public void Initialize(IncrementalGeneratorInitializationContext igi)
        {
            igi.RegisterPostInitializationOutput(PostInitial);

            var sp = igi.SyntaxProvider;
            igi.RegisterSourceOutput(sp.ForAttributeWithMetadataName(ItfFqn, Check, Wrap), Exec);
        }

        private static void PostInitial(IncrementalGeneratorPostInitializationContext ctx)
        {
            var itfCode = CodeTool.CreateAttribute(ItfName);
            ctx.AddSource($"{ItfName}.cs", itfCode);
        }

        private static bool Check(SyntaxNode node, CancellationToken _)
            => node is ClassDeclarationSyntax;

        private static SyntaxWrap Wrap(GeneratorAttributeSyntaxContext ctx, CancellationToken _)
            => ctx.Wrap();

        private static void Exec(SourceProductionContext ctx, SyntaxWrap syntax)
        {
            var cds = syntax.Class;
            var space = cds.GetParentName() ?? Coding.AutoNamespace;
            var name = cds.GetClassName();
            var fileName = $"{name}.g.cs";
            var code = new CodeWriter();
            code.AddUsings(LibSpace, "System");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");
            code.WriteClassLine(name, interfaces: "IDisposable");
            code.AppendLine("{");
            // ExecBody(code, cds, syntax, fmt);
            code.AppendLine("}");
            code.AppendLine("}");
            ctx.AddSource(fileName, code.ToString());
        }
    }
}