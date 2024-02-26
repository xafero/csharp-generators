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
                    "void ReadCBOR(System.IO.Stream stream)",
                    "void ReadCBOR(ref System.Formats.Cbor.CborReader reader)",
                    "void WriteCBOR(System.IO.Stream stream)",
                    "void WriteCBOR(ref System.Formats.Cbor.CborWriter writer)"
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

            writer.AppendLine("\t\twriter.WriteStartMap(null);");

            foreach (var member in cds.Members)
                if (member is PropertyDeclarationSyntax pds)
                {
                    var propName = pds.GetName();
                    var propType = Typing.Parse(pds.Type, out var rank, out var nil);

                    var pnW = $"writer.WriteTextString(\"{propName}\");";
                    writer.AppendLine($"\t\t{pnW}");

                    var pvW = propType switch
                    {
                        "DateTimeOffset" => $"writer.WriteDateTimeOffset(this.{propName});",
                        "Decimal" => $"writer.WriteDecimal(this.{propName});",
                        "Double" => $"writer.WriteDouble(this.{propName});",
                        "Single" => $"writer.WriteSingle(this.{propName});",
                        "Int32" => $"writer.WriteInt32(this.{propName});",
                        "Int64" => $"writer.WriteInt64(this.{propName});",
                        "UInt32" => $"writer.WriteUInt32(this.{propName});",
                        "UInt64" => $"writer.WriteUInt64(this.{propName});",
                        "Half" => $"writer.WriteHalf(this.{propName});",
                        "Boolean" => $"writer.WriteBoolean(this.{propName});",
                        "String" => $"writer.WriteTextString(this.{propName});",
                        _ => BuildSubWrite(propType.Substring(1), $"this.{propName}")
                    };
                    writer.AppendLine($"\t\t{pvW}");

                    var pvR = propType switch
                    {
                        "DateTimeOffset" => $"this.{propName} = reader.ReadDateTimeOffset();",
                        "Decimal" => $"this.{propName} = reader.ReadDecimal();",
                        "Double" => $"this.{propName} = reader.ReadDouble();",
                        "Single" => $"this.{propName} = reader.ReadSingle();",
                        "Int32" => $"this.{propName} = reader.ReadInt32();",
                        "Int64" => $"this.{propName} = reader.ReadInt64();",
                        "UInt32" => $"this.{propName} = reader.ReadUInt32();",
                        "UInt64" => $"this.{propName} = reader.ReadUInt64();",
                        "Half" => $"this.{propName} = reader.ReadHalf();",
                        "Boolean" => $"this.{propName} = reader.ReadBoolean();",
                        "String" => $"this.{propName} = reader.ReadTextString();",
                        _ => BuildSubRead(propType.Substring(1), $"this.{propName}")
                    };
                    reader.AppendLine($"\t\t{pvR}");
                }

            writer.AppendLine("\t\twriter.WriteEndMap();");

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
            code.AppendLine("\t\tvar reader = new CborReader(array, CborConformanceMode.Canonical);");
            code.AppendLine("\t\tReadCBOR(ref reader);");
            code.AppendLine("\t}");
            code.AppendLine();
            code.AppendLine("\tpublic void ReadCBOR(ref CborReader reader)");
            code.AppendLine("\t{");
            code.Append(reader);
            code.AppendLine("\t}");
            code.AppendLine();
            code.AppendLine("\tpublic void WriteCBOR(Stream stream)");
            code.AppendLine("\t{");
            code.AppendLine("\t\tvar writer = new CborWriter(CborConformanceMode.Canonical, true);");
            code.AppendLine("\t\tWriteCBOR(ref writer);");
            code.AppendLine("\t\tvar array = writer.Encode();");
            code.AppendLine("\t\tstream.Write(array, 0, array.Length);");
            code.AppendLine("\t\tstream.Flush();");
            code.AppendLine("\t}");
            code.AppendLine();
            code.AppendLine("\tpublic void WriteCBOR(ref CborWriter writer)");
            code.AppendLine("\t{");
            code.Append(writer);
            code.AppendLine("\t}");

            code.AppendLine("}");
            ctx.AddSource(fileName, code.ToString());
        }

        private static string BuildSubRead(string type, string prop)
        {
            var code = new StringBuilder();
            code.AppendLine($"if (typeof({type}).IsEnum) {{ {prop} = ({type})(object)reader.ReadInt32(); }}");
            code.AppendLine($"\t\t else {{ if (reader.PeekState() == CborReaderState.Null) {{ reader.ReadNull(); {prop} = default; }}");
            code.Append($"\t\t else {{ var v = new {type}(); (({IntObjName})(object)v)!.ReadCBOR(ref reader); {prop} = v; }} }}");
            return code.ToString();
        }

        private static string BuildSubWrite(string type, string prop)
        {
            var code = new StringBuilder();
            code.AppendLine($"if (typeof({type}).IsEnum) {{ writer.WriteInt32((int)(object){prop}); }}");
            code.Append($"\t\t else {{ if ({prop} == default) writer.WriteNull(); else");
            code.Append($" (({IntObjName})(object){prop})!.WriteCBOR(ref writer); }}");
            return code.ToString();
        }
    }
}