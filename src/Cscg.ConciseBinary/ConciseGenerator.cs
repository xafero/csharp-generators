using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Text;
using Cscg.Core;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cscg.ConciseBinary
{
    [Generator(LanguageNames.CSharp)]
    public sealed class ConciseGenerator : IIncrementalGenerator
    {
        private const string Space = Coding.AutoNamespace;
        private const string BinObjName = "ConciseObj";
        private const string ExtObjName = "ConciseObjExt";
        private const string IntObjName = "IConciseObj";

        public void Initialize(IncrementalGeneratorInitializationContext igi)
        {
            igi.RegisterPostInitializationOutput(ctx =>
            {
                var attrCode = Coding.GenerateAttr(BinObjName, Space);
                ctx.AddSource($"{BinObjName}Attribute.g.cs", Sources.From(attrCode));

                var intCode = Coding.GenerateIntf(IntObjName, Space, new List<string>
                {
                    "void ReadCBOR(System.IO.Stream stream)", "void WriteCBOR(System.IO.Stream stream)"
                });
                ctx.AddSource($"{IntObjName}.g.cs", Sources.From(intCode));

                var extCode = Coding.GenerateExt(ExtObjName, Space, new List<string>());
                ctx.AddSource($"{ExtObjName}.g.cs", Sources.From(extCode));
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
            code.AppendLine("using autogen;");
            code.AppendLine("using System;");
            code.AppendLine("using System.Formats.Cbor;");
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
                    var propType = Typing.Parse(pds.Type, out var rank, out var nil).ToTitle();

                    reader.AppendLine(" // " + propName + " / " + propType);
                    writer.AppendLine(" // " + propName + " / " + propType);
                }

            code.AppendLine("\tpublic void ReadCBOR(Stream stream)");
            code.AppendLine("\t{");
            code.AppendLine("\t\tbyte[] array;");
            code.AppendLine("\t\tif (stream is MemoryStream mem)");
            code.AppendLine("\t\t{");
            code.AppendLine("\t\t\tarray = mem.ToArray();");
            code.AppendLine("\t\t}");
            code.AppendLine("\t\telse");
            code.AppendLine("\t\t{");
            code.AppendLine("\t\t\tusing var copy = new MemoryStream();");
            code.AppendLine("\t\t\tstream.CopyTo(copy);");
            code.AppendLine("\t\t\tarray = copy.ToArray();");
            code.AppendLine("\t\t}");
            code.AppendLine("\t\tReadCBOR(array);");
            code.AppendLine("\t}");
            code.AppendLine();
            code.AppendLine("\tpublic void ReadCBOR(byte[] array)");
            code.AppendLine("\t{");
            code.AppendLine("\t\tvar reader = new CborReader(array, CborConformanceMode.Canonical);");
            code.Append(reader);
            code.AppendLine("\t}");
            code.AppendLine();
            code.AppendLine("\tpublic void WriteCBOR(Stream stream)");
            code.AppendLine("\t{");
            code.AppendLine("\t\tvar array = WriteCBOR();");
            code.AppendLine("\t\tstream.Write(array, 0, array.Length);");
            code.AppendLine("\t\tstream.Flush();");
            code.AppendLine("\t}");
            code.AppendLine();
            code.AppendLine("\tpublic byte[] WriteCBOR()");
            code.AppendLine("\t{");
            code.AppendLine("\t\tvar writer = new CborWriter(CborConformanceMode.Canonical, true);");
            code.Append(writer);
            code.AppendLine("\t\treturn writer.Encode();");
            code.AppendLine("\t}");

            code.AppendLine("}");
            ctx.AddSource(fileName, code.ToString());
        }
    }
}