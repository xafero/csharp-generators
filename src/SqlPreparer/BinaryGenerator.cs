using System.IO;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
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
            code.AppendLine("using System.Text;");
            code.AppendLine("using System.IO;");
            code.AppendLine();
            code.AppendLine($"namespace {space};");
            code.AppendLine();
            code.AppendLine($"partial class {name}");
            code.AppendLine("{");

            var reader = new StringBuilder();
            var writer = new StringBuilder();
            foreach (var member in cds.Members)
                if (member is PropertyDeclarationSyntax pds)
                {
                    var propName = pds.GetName();
                    var propType = Typing.Parse(pds.Type).ToTitle();

                    reader.AppendLine($"\t\tthis.{propName} = reader.Read{propType}();");
                    writer.AppendLine($"\t\twriter.Write(this.{propName});");
                }

            code.AppendLine("\tpublic void Read(dynamic reader)");
            code.AppendLine("\t{");
            code.Append(reader);
            code.AppendLine("\t}");
            code.AppendLine();
            code.AppendLine("\tpublic void Write(Stream stream)");
            code.AppendLine("\t{");
            code.AppendLine("\t\tusing var writer = new BinaryWriter(stream, Encoding.UTF8, true);");
            code.Append(writer);
            code.AppendLine("\t}");

            code.AppendLine("}");
            ctx.AddSource(fileName, code.ToString());
        }
    }
}