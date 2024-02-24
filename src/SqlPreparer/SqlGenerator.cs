using Cscg.Core;
using Microsoft.CodeAnalysis;
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
                var attrCode = GenerateAttr("Funny", "test");
                ctx.AddSource("FunnyAttribute.g.cs", Sources.From(attrCode));



                /*
                IncrementalValueProvider<(Compilation, ImmutableArray<ClassDeclarationSyntax>)> compilationAndEnums
                    = context.CompilationProvider.Combine(enumDeclarations.Collect());
                context.RegisterSourceOutput(compilationAndEnums,
                    (spc, source) =>
                        Execute(source.Item1, source.Item2, spc));
                */
            });
        }
    }
}