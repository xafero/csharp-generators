using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SqlPreparer
{
    [Generator(LanguageNames.CSharp)]
    public sealed class SqlGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx =>
            {
                var attrCode = Sources.GenerateAttr("Funny");
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

        public void Initialize3(IncrementalGeneratorInitializationContext initContext)
        {
            // define the execution pipeline here via a series of transformations:

            // find all additional files that end with .txt
            var textFiles =
                initContext.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".txt"));

            // read their contents and save their name
            var namesAndContents =
                textFiles.Select((text, cancellationToken) => (name: Path.GetFileNameWithoutExtension(text.Path),
                    content: text.GetText(cancellationToken)!.ToString()));

            // generate a class that contains their values as const strings
            initContext.RegisterSourceOutput(namesAndContents, (spc, nameAndContent) =>
            {
                spc.AddSource($"ConstStrings.{nameAndContent.name}", $@"
    public static partial class ConstStrings
    {{
        public const string {nameAndContent.name} = ""{nameAndContent.content}"";
    }}");
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
            return syntaxNode is ClassDeclarationSyntax classDeclarationSyntax &&
                   classDeclarationSyntax.AttributeLists.Count > 0 &&
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