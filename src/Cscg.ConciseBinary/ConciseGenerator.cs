using Microsoft.CodeAnalysis;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Cscg.Core;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Xml.Linq;

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
                    "void ReadCBOR(ref CborReader reader)", 
                    "void WriteCBOR(ref CborWriter writer)"
                }, "System.Formats.Cbor");
                ctx.AddSource($"{IntObjName}.g.cs", Sources.From(intCode));

                var extCode = Coding.GenerateExt(ExtObjName, Space, new List<string[]>
                {
                    new[]
                    {
                        $"public static void ReadCBOR(this {IntObjName} obj, Stream stream)", "{",
                        "byte[] array;", "if (stream is MemoryStream mem)", "{",
                        "array = mem.ToArray();", "}", "else", "{", "using var copy = new MemoryStream();",
                        "stream.CopyTo(copy);", "array = copy.ToArray();", "}",
                        "var reader = new CborReader(array, CborConformanceMode.Canonical);",
                        "obj.ReadCBOR(ref reader);", "}", ""
                    },
                    new[]
                    {
                        $"public static void WriteCBOR(this {IntObjName} obj, Stream stream)", "{",
                        "var writer = new CborWriter(CborConformanceMode.Canonical, true);",
                        "obj.WriteCBOR(ref writer);", "var array = writer.Encode();",
                        "stream.Write(array, 0, array.Length);", "stream.Flush();", "}"
                    }
                }, "System.Formats.Cbor", "System.IO");
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
            code.AppendLine($"using {Coding.AutoNamespace};");
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

            var cdSym = sw.GetSymbol(cds);
            cdSym.ExtractBase(out var cdBase, out _, out var cdSealed);

            var callBase = cdBase != null;
            var isAlone = cdSealed && cdBase == null;
            var callMode = isAlone ? string.Empty : callBase ? "override " : "virtual ";

            var readerH = new CodeWriter();
            var readerC = new CodeWriter();
            var writerH = new CodeWriter();
            var writerC = new CodeWriter();

            foreach (var member in cds.Members)
                if (member is PropertyDeclarationSyntax pds)
                {
                    var propSym = sw.GetSymbol(pds);
                    var propInfo = sw.GetInfo(propSym);
                    var propName = pds.GetName();
                    var propType = propInfo.ReturnType;

                    var instr = TryCreate(propName, propType);

                    readerC.AppendLine($"if (key == nameof(this.{propName}))");
                    readerC.AppendLine("{");
                    if (instr is { } isr)
                        readerC.AppendLines(isr.r);
                    else
                        readerC.AppendLine($"// TODO {propType}");
                    readerC.AppendLine(isAlone ? "continue;" : "return;");
                    readerC.AppendLine("}");

                    writerC.AppendLine($"w.WriteTextString(nameof(this.{propName}));");
                    if (instr is { } isw)
                        writerC.AppendLines(isw.w);
                    else
                        writerC.AppendLine($"// TODO {propType}");
                }

            readerH.AppendLine("var count = (int)r.ReadStartMap();");
            readerH.AppendLine("string key;");
            readerH.AppendLine("for (var i = 0; i < count; i++)");
            readerH.AppendLine("{");
            readerH.AppendLine("key = r.ReadTextString();");
            if (isAlone)
                readerH.AppendLines(readerC);
            else
                readerH.AppendLine("ReadCBORCore(ref r, key);");
            readerH.AppendLine("}");
            readerH.AppendLine("r.ReadEndMap();");

            writerH.AppendLine("w.WriteStartMap(null);");
            if (isAlone)
                writerH.AppendLines(writerC);
            else
                writerH.AppendLine("WriteCBORCore(ref w);");
            writerH.AppendLine("w.WriteEndMap();");

            code.AppendLine($"public {callMode}void ReadCBOR(ref CborReader r)");
            code.AppendLine("{");
            code.AppendLines(readerH);
            code.AppendLine("}");

            if (!isAlone)
            {
                code.AppendLine();
                code.AppendLine($"public {callMode}void ReadCBORCore(ref CborReader r, string key)");
                code.AppendLine("{");
                if (callBase) code.AppendLine("base.ReadCBORCore(ref r, key);");
                code.AppendLines(readerC);
                code.AppendLine("}");
            }

            code.AppendLine();
            code.AppendLine($"public {callMode}void WriteCBOR(ref CborWriter w)");
            code.AppendLine("{");
            code.AppendLines(writerH);
            code.AppendLine("}");

            if (!isAlone)
            {
                code.AppendLine();
                code.AppendLine($"public {callMode}void WriteCBORCore(ref CborWriter w)");
                code.AppendLine("{");
                if (callBase) code.AppendLine("base.WriteCBORCore(ref w);");
                code.AppendLines(writerC);
                code.AppendLine("}");
            }

            code.AppendLine("}");
            code.AppendLine("}");
            ctx.AddSource(fileName, code.ToString());
        }

        private static (string[] r, string[] w)? TryCreate(string name, ITypeSymbol type, string tnf = null)
        {
            var isNullable = false;
            var isArray = false;
            var tn = tnf ?? $"this.{name}";
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
                    if (type.IsTyped(out _, out var tArgs, out var isList, out var isDict))
                    {
                        var atSym = tArgs.LastOrDefault();
                        if (isList)
                            x = GetListRead(name, atSym, tn);
                        else if (isDict)
                            x = GetDictRead(name, atSym, tn);
                        else
                            return null;
                        break;
                    }
                    if (!type.CanBeParent(out _))
                    {
                        isNullable = true;
                        x = GetSubSingle(tn, txt);
                        break;
                    }
                    if (type.SearchType().ToArray() is { Length: >= 1 } sub)
                    {
                        isNullable = true;
                        x = GetSubPoly(tn, sub);
                        break;
                    }
                    return null;
            }
            if (isArray)
            {
                isNullable = true;
                x = GetArrayRead(tn, txt);
            }
            if (isNullable)
            {
                x = GetNullRead(x, tn);
            }
            return x;
        }

        private static (string[] r, string[] w) GetDictRead(string name, ITypeSymbol sym, string tn)
        {
            var f = TryCreate(name, sym, "dv");
            var txt = sym.ToTrimDisplay();

            var rr = new List<string>
            {
                "var size = (int)r.ReadStartMap();",
                $"var d = new Dictionary<string, {txt}>(size);", "for (var j = 0; j < size; j++)",
                "{", $"var dk = {GetOneRead("string")};", $"{sym} dv = default;"
            };
            rr.AddRange(f?.r ?? []);
            rr.Add("d[dk] = dv;");
            rr.Add("}");
            rr.Add("r.ReadEndMap();");
            rr.Add($"{tn} = d;");

            var ww = new List<string> { "w.WriteStartMap(null);", $"foreach (var dv in {tn})", "{" };
            ww.AddRange(f?.w ?? []);
            ww.Add("}");
            ww.Add("w.WriteEndMap();");

            return (r: rr.ToArray(), w: ww.ToArray());
        }

        private static (string[] r, string[] w) GetListRead(string name, ITypeSymbol sym, string tn)
        {
            var f = TryCreate(name, sym);
            var txt = sym.ToTrimDisplay();

            var rr = new List<string>();
            rr.Add("var size = (int)r.ReadStartArray();");
            rr.Add($"var d = new List<{txt}>(size);");
            rr.Add("for (var j = 0; j < size; j++)");
            rr.Add("{");
            rr.AddRange(f.Value.r);
            rr.Add("}");
            rr.Add("r.ReadEndArray();");
            rr.Add($"{tn} = d;");

            var ww = new List<string>();
            ww.Add("w.WriteStartArray(null);");
            ww.Add($"foreach (var item in {tn})");
            ww.Add("{");
            ww.Add($"{GetOneWrite(txt, "item")};");
            ww.AddRange(f.Value.w);
            ww.Add("}");
            ww.Add("w.WriteEndArray();");

            return (r: rr.ToArray(), w: ww.ToArray());
        }

        private static (string[] r, string[] w) GetArrayRead(string tn, string txt)
        {
            return (r:
                [
                    "var size = (int)r.ReadStartArray();", $"var v = new {txt}[size];",
                    "for (var j = 0; j < size; j++)", "{", $"v[j] = {GetOneRead(txt)};",
                    "}", "r.ReadEndArray();", $"{tn} = v;"
                ],
                w:
                [
                    "w.WriteStartArray(null);", $"for (var j = 0; j < {tn}.Length; j++)",
                    "{", $"{GetOneWrite(txt, $"{tn}[j]")};",
                    "}", "w.WriteEndArray();"
                ]);
        }

        private static (string[] r, string[] w) GetSubSingle(string tn, string type)
        {
            return (r: [$"var v = new {type}();", "v.ReadCBOR(ref r);", $"{tn} = v;"],
                w: [$"{tn}.WriteCBOR(ref w);"]);
        }

        private static (string[] r, string[] w) GetSubPoly(string tn, ClassDeclarationSyntax[] sub)
        {
            var read = new List<string>
            {
                "r.ReadStartArray();", "var st = r.ReadTextString();"
            };
            var write = new List<string>
            {
                "w.WriteStartArray(2);", $"w.WriteTextString({tn}.GetType().FullName);"
            };
            var idx = 0;
            foreach (var item in sub)
            {
                var isFirst = idx == 0;
                idx++;
                var itemFqn = item.GetFqn();
                var gss = GetSubSingle(tn, itemFqn);
                read.Add($"{(isFirst ? "" : "else ")}if (st == \"{itemFqn}\")");
                read.Add("{");
                read.AddRange(gss.r);
                read.Add("}");
                var vr = $"sv{idx}";
                write.Add($"{(isFirst ? "" : "else ")}if ({tn} is {itemFqn} {vr})");
                write.Add("{");
                write.Add(gss.w.Single().Replace(tn, vr));
                write.Add("}");
            }
            write.Add("w.WriteEndArray();");
            read.Add("r.ReadEndArray();");
            return (r: read.ToArray(), w: write.ToArray());
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