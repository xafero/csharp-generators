using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cscg.Core;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cscg.Binary
{
    [Generator(LanguageNames.CSharp)]
    public sealed class BinaryGenerator : IIncrementalGenerator
    {
        private const string Space = Coding.AutoNamespace;
        private const string BinObjName = "BinaryObj";
        private const string ExtObjName = "BinaryObjExt";
        private const string IntObjName = "IBinaryObj";

        public void Initialize(IncrementalGeneratorInitializationContext igi)
        {
            igi.RegisterPostInitializationOutput(ctx =>
            {
                var attrCode = Coding.GenerateAttr(BinObjName, Space);
                ctx.AddSource($"{BinObjName}Attribute.g.cs", Sources.From(attrCode));

                var intCode = Coding.GenerateIntf(IntObjName, Space, new List<string>
                {
                    "void ReadBinary(System.IO.Stream stream)",
                    "void ReadBinary(System.IO.BinaryReader reader)",
                    "void WriteBinary(System.IO.Stream stream)",
                    "void WriteBinary(System.IO.BinaryWriter writer)"
                });
                ctx.AddSource($"{IntObjName}.g.cs", Sources.From(intCode));

                var extCode = Coding.GenerateExt(ExtObjName, Space, new List<string>());
                ctx.AddSource($"{ExtObjName}.g.cs", Sources.From(extCode));
            });

            var classes = igi.SyntaxProvider.CreateSyntaxProvider(
                predicate: static (sn, _) => sn.HasThisAttribute(BinObjName),
                transform: static (ctx, _) => ctx.Wrap());
            igi.RegisterSourceOutput(classes, Generate);
        }

        private static void Generate(SourceProductionContext ctx, SyntaxWrap sw)
        {
            var cds = sw.Class;
            var space = cds.GetParentName() ?? Coding.AutoNamespace;
            var name = cds.GetClassName();
            var fileName = $"{name}.g.cs";
            var code = new StringBuilder();
            code.AppendLine("using autogen;");
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
                    var propSym = sw.GetSymbol(pds);
                    var propInfo = sw.GetInfo(propSym);
                    var propName = pds.GetName();
                    var propType = propInfo.ReturnType.ToTrimDisplay();

                    var readLeft = $"this.{propName}";
                    string readRight = null;
                    var writeLeft = "w.Write";
                    var writeRight = $"this.{propName}";

                    if (propType == "string") propType = "string?";

                    var isNullable = false;
                    if (propType.EndsWith("?"))
                    {
                        isNullable = true;
                        propType = propType.Substring(0, propType.Length - 1);
                    }

                    switch (propType)
                    {
                        case "ulong":
                            readRight = "r.ReadUInt64()";
                            break;
                        case "uint":
                            readRight = "r.ReadUInt32()";
                            break;
                        case "ushort":
                            readRight = "r.ReadUInt16()";
                            break;
                        case "decimal":
                            readRight = "r.ReadDecimal()";
                            break;
                        case "double":
                            readRight = "r.ReadDouble()";
                            break;
                        case "float":
                            readRight = "r.ReadSingle()";
                            break;
                        case "long":
                            readRight = "r.ReadInt64()";
                            break;
                        case "int":
                            readRight = "r.ReadInt32()";
                            if (isNullable) writeRight += ".Value";
                            break;
                        case "short":
                            readRight = "r.ReadInt16()";
                            break;
                        case "byte":
                            readRight = "r.ReadByte()";
                            break;
                        case "sbyte":
                            readRight = "r.ReadSByte()";
                            break;
                        case "bool":
                            readRight = "r.ReadBoolean()";
                            if (isNullable) writeRight += ".Value";
                            break;
                        case "char":
                            readRight = "r.ReadChar()";
                            break;
                        case "byte[]":
                            readRight = $"r.ReadBytes({propName}_i);";
                            writeRight = $"w.Write({writeRight}.Length); w.Write({writeRight});";
                            break;
                        case "char[]":
                            readRight = $"r.ReadChars({propName}_i);";
                            writeRight = $"w.Write({writeRight}.Length); w.Write({writeRight});";
                            break;
                        case "string":
                            readRight = "r.ReadString()";
                            break;
                        case "System.TimeSpan":
                            readRight = "TimeSpan.FromTicks(r.ReadInt64())";
                            writeRight = $"{writeRight}{(isNullable ? ".Value" : "")}.Ticks";
                            break;
                        case "System.DateTime":
                            readRight = "DateTime.FromBinary(r.ReadInt64())";
                            writeRight = $"{writeRight}{(isNullable ? ".Value" : "")}.ToBinary()";
                            break;
                        case "System.Guid":
                            readRight = "new Guid(r.ReadBytes(16))";
                            writeRight = $"{writeRight}{(isNullable ? ".Value" : "")}.ToByteArray()";
                            break;
                    }
                    if (propInfo.ReturnType.IsEnum(out var eut))
                        switch (eut.ToTrimDisplay())
                        {
                            case "int":
                                readRight = $"({propType})r.ReadInt32()";
                                writeRight = $"(int){writeRight}";
                                break;
                        }

                    var readLine = $"\t\t{readLeft} = {readRight};";
                    var writeLine = $"\t\t{writeLeft}({writeRight});";
                    if (propType is "byte[]" or "char[]")
                    {
                        readLine = $"\t\t{BuildArrayRead(readLeft, readRight)}";
                        writeLine = $"\t\t{BuildArrayWrite(readLeft, writeRight)}";
                    }
                    else if (readRight == null)
                    {
                        readLine = $"\t\t{BuildSubRead(propType, readLeft)}";
                        writeLine = $"\t\t{BuildSubWrite(propType, writeRight)}";
                    }
                    else if (isNullable)
                    {
                        readLine = $"\t\t{BuildNullableRead(readLeft, readLine)}";
                        writeLine = $"\t\t{BuildNullableWrite(readLeft, writeLine)}";
                    }
                    reader.AppendLine(readLine);
                    writer.AppendLine(writeLine);
                }

            code.AppendLine("\tpublic void ReadBinary(Stream stream)");
            code.AppendLine("\t{");
            code.AppendLine("\t\tusing var reader = new BinaryReader(stream, Encoding.UTF8, true);");
            code.AppendLine("\t\tReadBinary(reader);");
            code.AppendLine("\t}");
            code.AppendLine();
            code.AppendLine("\tpublic void ReadBinary(BinaryReader r)");
            code.AppendLine("\t{");
            code.Append(reader);
            code.AppendLine("\t}");
            code.AppendLine();
            code.AppendLine("\tpublic void WriteBinary(Stream stream)");
            code.AppendLine("\t{");
            code.AppendLine("\t\tusing var writer = new BinaryWriter(stream, Encoding.UTF8, true);");
            code.AppendLine("\t\tWriteBinary(writer);");
            code.AppendLine("\t}");
            code.AppendLine();
            code.AppendLine("\tpublic void WriteBinary(BinaryWriter w)");
            code.AppendLine("\t{");
            code.Append(writer);
            code.AppendLine("\t}");

            code.AppendLine("}");
            ctx.AddSource(fileName, code.ToString());
        }

        private static string BuildArrayRead(string prop, string func)
        {
            var code = new StringBuilder();
            var idx = $"{prop.Split(['.'], 2).Last()}_i";
            code.Append($"{prop} = r.ReadInt32() is var {idx} && {idx} == -1 ? default");
            code.Append($" : {func}");
            return code.ToString();
        }

        private static string BuildArrayWrite(string prop, string func)
        {
            var code = new StringBuilder();
            code.Append($"if ({prop} == default) {{ w.Write(-1); }}");
            code.Append($" else {{ {func.Trim()} }}");
            return code.ToString();
        }

        private static string BuildSubRead(string type, string prop)
        {
            var code = new StringBuilder();
            code.AppendLine($"if (r.ReadByte() == 0) {{ {prop} = default; }}");
            code.Append($"\t\t else {{ var v = new {type}(); v.ReadBinary(r); {prop} = v; }}");
            return code.ToString();
        }

        private static string BuildSubWrite(string _, string prop)
        {
            var code = new StringBuilder();
            code.Append($"w.Write((byte)({prop} == default ? 0 : 1));");
            code.Append($" {prop}?.WriteBinary(w);");
            return code.ToString();
        }

        private static string BuildNullableWrite(string prop, string func)
        {
            var code = new StringBuilder();
            code.Append($"if ({prop} == default) {{ w.Write((byte)0); }}");
            code.Append($" else {{ w.Write((byte)1); {func.Trim()} }}");
            return code.ToString();
        }

        private static string BuildNullableRead(string prop, string func)
        {
            var code = new StringBuilder();
            code.Append($"if (r.ReadByte() == 0) {{ {prop} = default; }}");
            code.Append($" else {{ {func.Trim()} }}");
            return code.ToString();
        }
    }
}