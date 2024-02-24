using System.Linq;
using Cscg.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Cscg.Core.Coding;

namespace SqlPreparer
{
    [Generator(LanguageNames.CSharp)]
    public sealed class SqlGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx =>
            {
                var attrCode = GenerateAttr("Funny");
                ctx.AddSource("FunnyAttribute.g.cs", Sources.From(attrCode));



                /*
                IncrementalValuesProvider<ClassDeclarationSyntax> enumDeclarations = context.SyntaxProvider
                    .CreateSyntaxProvider(
                        predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                        transform: static (ctx, _) => GetTargetForGeneration(ctx));
                IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndEnums
                    = context.CompilationProvider.Combine(enumDeclarations.Collect());
                context.RegisterSourceOutput(compilationAndEnums,
                    (spc, source) =>
                        Execute(source.Item1, source.Item2, spc));
                */
            });
        }

        public void Initialize2(IncrementalGeneratorInitializationContext context)
        {
            var classProvider = context.SyntaxProvider
                .CreateSyntaxProvider((node, _) => { return node is ClassDeclarationSyntax; },
                    (ctx, _) => { return (ClassDeclarationSyntax)ctx.Node; });

            context.RegisterSourceOutput(classProvider, Generate);
        }

        private static void Generate(SourceProductionContext ctx, ClassDeclarationSyntax cds)
        {
            Generate(ctx, cds.Identifier.Text);
        }

        private static void Generate(SourceProductionContext ctx, string name)
        {
            var ns = "DemoConsoleApplication";

            ctx.AddSource($"{ns}.{name}.perf.cs", $@"//

namespace {ns}
{{
   partial class {name}
   {{
   }}
}}
");
        }

        public static bool IsSyntaxTargetForGeneration(SyntaxNode syntaxNode)
        {
            return syntaxNode is ClassDeclarationSyntax { AttributeLists.Count: > 0 } classDeclarationSyntax &&
                   classDeclarationSyntax.AttributeLists
                       .Any(al => al.Attributes.Any(a => a.Name.ToString() == "GenerateService"));
        }

        public static ClassDeclarationSyntax GetTargetForGeneration(GeneratorSyntaxContext context)
        {
            var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
            return classDeclarationSyntax;
        }
    }
}