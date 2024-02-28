using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cscg.Core;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;

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
                transform: static (ctx, _) => ctx.Wrap());
            igi.RegisterSourceOutput(classes, Generate);
        }

        private static void Generate(SourceProductionContext ctx, SyntaxWrap sw)
        {
            var cds = sw.Class;
            var space = cds.GetParentName() ?? Coding.AutoNamespace;
            var name = cds.GetClassName();
            var fileName = $"{name}.g.cs";
            var code = new CodeWriter();
            code.AppendLine("using autogen;");
            code.AppendLine("using System;");
            code.AppendLine("using System.Collections.Generic;");
            code.AppendLine("using System.Formats.Cbor;");
            code.AppendLine("using System.Text;");
            code.AppendLine("using System.IO;");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");
            code.AppendLine($"partial class {name} : {Space}.{IntObjName}");
            code.AppendLine("{");

            var reader = new CodeWriter();
            var writer = new CodeWriter();

            writer.AppendLine("w.WriteStartMap(null);");
            reader.AppendLine("var count = (int)r.ReadStartMap();");
            reader.AppendLine("string key;");
            reader.AppendLine("for (var i = 0; i < count; i++)");
            reader.AppendLine("{");
            reader.AppendLine("key = r.ReadTextString();");

            foreach (var member in cds.Members)
                if (member is PropertyDeclarationSyntax pds)
                {
                    var propSym = sw.GetSymbol(pds);
                    var propInfo = sw.GetInfo(propSym);
                    var propName = pds.GetName();
                    var propType = propInfo.ReturnType;

                    var instr = TryCreate(propName, propType);

                    reader.AppendLine($"if (key == nameof(this.{propName}))");
                    reader.AppendLine("{");
                    if (instr is { } isr)
                        reader.AppendLines(isr.r);
                    else
                        reader.AppendLine($"// TODO {propType}");
                    reader.AppendLine("continue;");
                    reader.AppendLine("}");

                    writer.AppendLine($"w.WriteTextString(nameof(this.{propName}));");
                    if (instr is { } isw)
                        writer.AppendLines(isw.w);
                    else
                        writer.AppendLine($"// TODO {propType}");
                }

            reader.AppendLine("}");
            reader.AppendLine("r.ReadEndMap();");
            writer.AppendLine("w.WriteEndMap();");

            code.AppendLine("public void ReadCBOR(Stream stream)");
            code.AppendLine("{");
            code.AppendLine("byte[] array;");
            code.AppendLine("if (stream is MemoryStream mem)");
            code.AppendLine("{");
            code.AppendLine("array = mem.ToArray();");
            code.AppendLine("}");
            code.AppendLine("else");
            code.AppendLine("{");
            code.AppendLine("using var copy = new MemoryStream();");
            code.AppendLine("stream.CopyTo(copy);");
            code.AppendLine("array = copy.ToArray();");
            code.AppendLine("}");
            code.AppendLine("var reader = new CborReader(array, CborConformanceMode.Canonical);");
            code.AppendLine("ReadCBOR(ref reader);");
            code.AppendLine("}");
            code.AppendLine();
            code.AppendLine("public void ReadCBOR(ref CborReader r)");
            code.AppendLine("{");
            code.AppendLines(reader);
            code.AppendLine("}");
            code.AppendLine();
            code.AppendLine("public void WriteCBOR(Stream stream)");
            code.AppendLine("{");
            code.AppendLine("var writer = new CborWriter(CborConformanceMode.Canonical, true);");
            code.AppendLine("WriteCBOR(ref writer);");
            code.AppendLine("var array = writer.Encode();");
            code.AppendLine("stream.Write(array, 0, array.Length);");
            code.AppendLine("stream.Flush();");
            code.AppendLine("}");
            code.AppendLine();
            code.AppendLine("public void WriteCBOR(ref CborWriter w)");
            code.AppendLine("{");
            code.AppendLines(writer);
            code.AppendLine("}");

            code.AppendLine("}");
            code.AppendLine("}");
            ctx.AddSource(fileName, code.ToString());
        }

        private static (string[] r, string[] w)? TryCreate(string name, ITypeSymbol type)
        {
            var isNullable = false;
            var isArray = false;
            var tn = $"this.{name}";
            var txt = type.ToTrimDisplay();
            var rank = txt.Count(l => l == '[');
            if (txt != "byte[]" && rank >= 1)
            {
                isArray = true;
                txt = txt.Split(['['], 2).First();
            }
            if (txt is "byte[]" or "string")
            {
                txt += "?";
            }
            if (txt.EndsWith("?"))
            {
                isNullable = true;
                txt = txt.Substring(0, txt.Length - 1);
            }
            (string[] r, string[] w) x;
            switch (txt)
            {
                case "ulong":
                case "uint":
                case "decimal":
                case "double":
                case "float":
                case "long":
                case "int":
                case "short":
                case "bool":
                case "System.Half":
                case "System.DateTimeOffset":
                    x = (r: [$"{tn} = {GetOneRead(txt)};"], w:
                        [$"{GetOneWrite(txt, $"{tn}{(isNullable ? ".Value" : "")}")};"]);
                    break;
                case "byte[]":
                case "string":
                    x = (r: [$"{tn} = {GetOneRead(txt)};"], w: [$"{GetOneWrite(txt, tn)};"]);
                    break;
                case "System.TimeSpan":
                    x = (r: [$"{tn} = TimeSpan.FromTicks({GetOneRead("long")});"],
                        w: [$"{GetOneWrite("long", $"{tn}.Ticks")};"]);
                    break;
                default:
                    if (type.IsEnum(out var eut))
                    {
                        x = (r: GetEnumRead(name, type, eut), w: GetEnumWrite(name, type, eut));
                        break;
                    }
                    if (!type.CanBeParent(out _))
                    {
                        isNullable = true;
                        x = (r: [$"var v = new {type}();", "v.ReadCBOR(ref r);", $"{tn} = v;"],
                            w: [$"{tn}.WriteCBOR(ref w);"]);
                        break;
                    }
                    return null;
            }
            if (isArray)
            {
                isNullable = true;
                x = (r:
                    [
                        "var size = (int)r.ReadStartArray();", $"var v = new {txt}[size];",
                        "for (var j = 0; j < size; j++)", "{", $"v[j] = {GetOneRead(txt)};",
                        "}", "r.ReadEndArray();", $"{tn} = v;"
                    ],
                    w:
                    [
                        "w.WriteStartArray(null);", $"for (var j = 0; j < {tn}.Length; j++)",
                        "{", $"{GetOneWrite(txt, $"{tn}[j]")};", "}", "w.WriteEndArray();"
                    ]);
            }
            if (isNullable)
            {
                x = GetNullRead(x, tn);
            }
            return x;
        }

        private static (string[] r, string[] w) GetNullRead((string[] r, string[] w) x, string tn)
            => (r: GetIfStat("r.PeekState() == CborReaderState.Null",
                        ["r.ReadNull();", $"{tn} = default;"], x.r),
                    w: GetIfStat($"{tn} == default",
                        ["w.WriteNull();"], x.w)
                );

        private static string[] GetIfStat(string cond, string[] then, string[] @else)
        {
            var lines = new List<string> { $"if ({cond})", "{" };
            lines.AddRange(then);
            lines.AddRange(new[] { "}", "else", "{" });
            lines.AddRange(@else);
            lines.Add("}");
            return lines.ToArray();
        }

        private static string GetOneRead(string typeTxt)
        {
            switch (typeTxt)
            {
                case "ulong": return "r.ReadUInt64()";
                case "uint": return "r.ReadUInt32()";
                case "decimal": return "r.ReadDecimal()";
                case "double": return "r.ReadDouble()";
                case "float": return "r.ReadSingle()";
                case "long": return "r.ReadInt64()";
                case "int": return "r.ReadInt32()";
                case "short": return "(short)r.ReadInt32()";
                case "bool": return "r.ReadBoolean()";
                case "System.Half": return "r.ReadHalf()";
                case "System.DateTimeOffset": return "r.ReadDateTimeOffset()";
                case "byte[]": return "r.ReadByteString()";
                case "string": return "r.ReadTextString()";
                default: return null;
            }
        }

        private static string GetOneWrite(string typeTxt, string val)
        {
            switch (typeTxt)
            {
                case "ulong": return $"w.WriteUInt64({val})";
                case "uint": return $"w.WriteUInt32({val})";
                case "decimal": return $"w.WriteDecimal({val})";
                case "double": return $"w.WriteDouble({val})";
                case "float": return $"w.WriteSingle({val})";
                case "long": return $"w.WriteInt64({val})";
                case "int":
                case "short": return $"w.WriteInt32({val})";
                case "bool": return $"w.WriteBoolean({val})";
                case "System.Half": return $"w.WriteHalf({val})";
                case "System.DateTimeOffset": return $"w.WriteDateTimeOffset({val})";
                case "byte[]": return $"w.WriteByteString({val})";
                case "string": return $"w.WriteTextString({val})";
                default: return null;
            }
        }

        private static string[] GetEnumRead(string name, ITypeSymbol type, INamedTypeSymbol ut)
        {
            var utt = ut.ToTrimDisplay();
            return [$"this.{name} = ({type}){GetOneRead(utt)};"];
        }

        private static string[] GetEnumWrite(string name, ITypeSymbol type, INamedTypeSymbol ut)
        {
            var utt = ut.ToTrimDisplay();
            return [$"{GetOneWrite(utt, $"({ut})this.{name}")};"];
        }

        private static string BuildBytesRead(string prop)
        {
            var code = new StringBuilder();
            code.AppendLine($"if (reader.PeekState() == CborReaderState.Null) {{ reader.ReadNull(); {prop} = default; }}");
            code.Append($" else {{ {prop} = reader.ReadByteString(); }}");
            return code.ToString();
        }

        private static string BuildBytesWrite(string prop)
        {
            var code = new StringBuilder();
            code.Append($"if ({prop} == default) writer.WriteNull(); else");
            code.Append($" writer.WriteByteString({prop});");
            return code.ToString();
        }

        private static string BuildStringRead(string prop)
        {
            var code = new StringBuilder();
            code.AppendLine($"if (reader.PeekState() == CborReaderState.Null) {{ reader.ReadNull(); {prop} = default; }}");
            code.Append($" else {{ {prop} = reader.ReadTextString(); }}");
            return code.ToString();
        }

        private static string BuildStringWrite(string prop)
        {
            var code = new StringBuilder();
            code.Append($"if ({prop} == default) writer.WriteNull(); else");
            code.Append($" writer.WriteTextString({prop});");
            return code.ToString();
        }

        private static string BuildSubRead(Particle info, string type, string prop)
        {
            var code = new StringBuilder();
            if (info.ReturnType.IsEnum(out _))
            {
                code.Append($"{prop} = ({type})reader.ReadInt32();");
            }
            else
            {
                code.AppendLine($"if (reader.PeekState() == CborReaderState.Null) {{ reader.ReadNull(); {prop} = default; }}");
                code.Append($" else {{ var v = new {type}(); v.ReadCBOR(ref reader); {prop} = v; }}");
            }
            return code.ToString();
        }

        private static string BuildSubWrite(Particle info, string type, string prop)
        {
            var code = new StringBuilder();
            if (info.ReturnType.IsEnum(out var eut))
            {
                code.Append($"writer.WriteInt32(({eut}){prop});");
            }
            else
            {
                code.Append($"if ({prop} == default) writer.WriteNull(); else");
                code.Append($" {prop}.WriteCBOR(ref writer);");
            }
            return code.ToString();
        }
    }
}