using System.Collections.Generic;
using System.Text;
using Cscg.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace SqlPreparer
{
    [Generator(LanguageNames.CSharp)]
    public sealed class BinaryGenerator : IIncrementalGenerator
    {
        private const string Space = Coding.AutoNamespace;
        private const string BinObjName = "BinaryObj";
        private const string IntObjName = "IBinaryObj";

        public void Initialize(IncrementalGeneratorInitializationContext igi)
        {
            igi.RegisterPostInitializationOutput(ctx =>
            {
                var attrCode = Coding.GenerateAttr(BinObjName, Space);
                ctx.AddSource($"{BinObjName}Attribute.g.cs", Sources.From(attrCode));

                var intCode = Coding.GenerateIntf(IntObjName, Space, new List<string>
                {
                    "void Read(System.IO.Stream stream)", "void Write(System.IO.Stream stream)"
                });
                ctx.AddSource($"{IntObjName}.g.cs", Sources.From(intCode));
            });

            var classes = igi.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (sn, _) => sn.HasThisAttribute(BinObjName),
                transform: static (ctx, _) => ctx.GetTarget());
            igi.RegisterSourceOutput(classes, Generate);
        }

        private static void Generate(SourceProductionContext ctx, ClassDeclarationSyntax cds)
        {
            var space = cds.GetParentName() ?? Coding.AutoNamespace;
            var name = cds.GetClassName();
            var fileName = $"{name}.g.cs";
            var code = new StringBuilder();
            code.AppendLine("using System;");
            code.AppendLine("using System.Text;");
            code.AppendLine("using System.IO;");
            code.AppendLine();
            code.AppendLine($"namespace {space};");
            code.AppendLine();
            code.AppendLine($"partial class {name} : {Space}.{IntObjName}");
            code.AppendLine("{");

            var reader = new StringBuilder();
            var writer = new StringBuilder();
            foreach (var member in cds.Members)
                if (member is PropertyDeclarationSyntax pds)
                {
                    var propName = pds.GetName();
                    var propType = Typing.Parse(pds.Type, out var rank).ToTitle();
                    if (rank >= 1)
                    {
                        if (propType is "Byte" or "Char")
                            reader.AppendLine($"\t\tthis.{propName} = reader.Read{propType}s(reader.ReadInt32());");
                    }
                    else
                    {
                        var readFunc = $"reader.Read{propType}()";
                        if (propType == "DateTime") readFunc = "DateTime.FromBinary(reader.ReadInt64())";
                        reader.AppendLine($"\t\tthis.{propName} = {readFunc};");
                    }
                    if (rank >= 1)
                    {
                        writer.AppendLine($"\t\twriter.Write(this.{propName}?.Length ?? 0);");
                    }
                    var writeArg = $"this.{propName}";
                    if (propType == "DateTime") writeArg += ".ToBinary()";
                    else if (propType == "String") writeArg += " ?? string.Empty";
                    if (rank >= 1) writeArg += " ?? []";
                    writer.AppendLine($"\t\twriter.Write({writeArg});");
                }

            code.AppendLine("\tpublic void Read(Stream stream)");
            code.AppendLine("\t{");
            code.AppendLine("\t\tusing var reader = new BinaryReader(stream, Encoding.UTF8, true);");
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