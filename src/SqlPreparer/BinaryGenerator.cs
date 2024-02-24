﻿using System.Text;
using Cscg.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

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

                var classes = context.SyntaxProvider.CreateSyntaxProvider(
                    predicate: static (sn, _) => sn.HasThisAttribute(binObjName),
                    transform: static (ctx, _) => ctx.GetTarget());
                context.RegisterSourceOutput(classes, Generate);
            });
        }

        private static void Generate(SourceProductionContext ctx, ClassDeclarationSyntax cds)
        {
            var space = cds.GetParentName() ?? Coding.AutoNamespace;
            var name = cds.GetClassName();
            var fileName = $"{name}.g.cs";
            var code = new StringBuilder();
            code.AppendLine($"namespace {space};");
            code.AppendLine();
            code.AppendLine($"partial class {name}");
            code.AppendLine("{");
            code.AppendLine($" // ");
            code.AppendLine("}");
            ctx.AddSource(fileName, code.ToString());
        }
    }
}