using Cscg.Core;
using Microsoft.CodeAnalysis;

namespace SqlPreparer
{
    [Generator(LanguageNames.CSharp)]
    public sealed class BinaryGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            context.RegisterPostInitializationOutput(ctx =>
            {
                const string space = Coding.AutoNamespace;
                const string binObjName = "BinaryObj";
                var attrCode = Coding.GenerateAttr(binObjName, space);
                ctx.AddSource($"{binObjName}Attribute.g.cs", Sources.From(attrCode));
            });
        }
    }
}