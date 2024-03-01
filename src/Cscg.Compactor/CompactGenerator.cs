using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cscg.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Cscg.Compactor.Lib;
using static Cscg.Compactor.CompactSource;

namespace Cscg.Compactor
{
    [Generator(LanguageNames.CSharp)]
    public sealed class CompactGenerator : IIncrementalGenerator
    {
        public void Initialize(IncrementalGeneratorInitializationContext igi)
        {
            const string fqn = $"{LibSpace}.{BinObjName}Attribute";
            var sp = igi.SyntaxProvider;
            igi.RegisterSourceOutput(sp.ForAttributeWithMetadataName(fqn, Check, Wrap), Exec);
        }

        private static bool Check(SyntaxNode node, CancellationToken _)
            => node is ClassDeclarationSyntax;

        private static SyntaxWrap Wrap(GeneratorAttributeSyntaxContext ctx, CancellationToken _)
            => ctx.Wrap();

        private static void Exec(SourceProductionContext ctx, SyntaxWrap syntax)
        {
            syntax.Attribute.ExtractArg(out var fmt);
            var cds = syntax.Class;
            var space = cds.GetParentName() ?? Coding.AutoNamespace;
            var name = cds.GetClassName();
            var fileName = $"{name}.g.cs";
            var code = new CodeWriter();
            code.AppendLine($"using {LibSpace};");
            code.AppendLine("using System;");
            code.AppendLine("using System.Collections.Generic;");
            code.AppendLine("using System.Formats.Cbor;");
            code.AppendLine("using System.Text;");
            code.AppendLine("using System.Text.Json;");
            code.AppendLine("using System.Xml;");
            code.AppendLine("using System.IO;");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");
            code.AppendLine($"partial class {name} : {IntObjName}");
            code.AppendLine("{");
            ExecBody(code, cds, syntax, fmt);
            code.AppendLine("}");
            code.AppendLine("}");
            ctx.AddSource(fileName, code.ToString());
        }

        private static void ExecBody(CodeWriter code, ClassDeclarationSyntax cds, SyntaxWrap s, DataFormat f)
        {
            s.Symbol.ExtractBase(out var cBase, out _, out var cSealed);
            var callBase = cBase != null;
            var isAlone = cSealed && cBase == null;
            var callMode = isAlone ? string.Empty : callBase ? "override " : "virtual ";

            var readerC = new CodeWriter();
            var writerC = new CodeWriter();

            var registry = new List<string>();
            foreach (var member in cds.Members)
                if (member is PropertyDeclarationSyntax pds)
                {
                    var pp = s.GetInfo(s.GetSymbol(pds));
                    var pName = pp.Name;

                    var (pFull, pRt, leafs) = Simplify(pp.ReturnType);
                    GenerateTypeReg(registry, pFull, leafs);

                    var readMeth = $"Read{pRt}";
                    var readLine = $"this.{pName} = this.{readMeth}(ref r);";
                    readerC.AppendLine($"if (key == nameof(this.{pName}))");
                    readerC.AppendLine("{");
                    readerC.AppendLine(readLine.Replace(")(", ", "));
                    readerC.AppendLine(isAlone ? "continue;" : "return;");
                    readerC.AppendLine("}");

                    var writeMeth = $"Write{pRt}";
                    var writeLine = $"this.{writeMeth}(ref w, this.{pName});";
                    writerC.AppendLine($"this.WriteProperty(ref w, nameof(this.{pName}));");
                    writerC.AppendLine(writeLine.Replace(")(", ", "));
                }

            var rr = new CodeWriter();
            var ww = new CodeWriter();

            if (f.HasFlag(DataFormat.Cbor))
            {
                var readerH = GetCborReadHead(isAlone, readerC);
                var writerH = GetCborWriteHead(isAlone, writerC);
                rr.AppendLines(GetReadCode(isAlone, callBase, callMode, readerH, readerC, "Cbor", "CborReader"));
                rr.AppendLine();
                ww.AppendLines(GetWriteCode(isAlone, callBase, callMode, writerH, writerC, "Cbor", "CborWriter"));
                ww.AppendLine();
            }
            if (f.HasFlag(DataFormat.Json))
            {
                var readerH = GetJsonReadHead(isAlone, readerC);
                var writerH = GetJsonWriteHead(isAlone, writerC);
                rr.AppendLines(GetReadCode(isAlone, callBase, callMode, readerH, readerC, "Json", "Utf8JsonReader"));
                rr.AppendLine();
                ww.AppendLines(GetWriteCode(isAlone, callBase, callMode, writerH, writerC, "Json", "Utf8JsonWriter"));
                ww.AppendLine();
            }
            if (f.HasFlag(DataFormat.Xml))
            {
                var readerH = GetXmlReadHead(isAlone, readerC);
                var writerH = GetXmlWriteHead(isAlone, writerC);
                rr.AppendLines(GetReadCode(isAlone, callBase, callMode, readerH, readerC, "Xml", "XmlReader"));
                rr.AppendLine();
                ww.AppendLines(GetWriteCode(isAlone, callBase, callMode, writerH, writerC, "Xml", "XmlWriter"));
                ww.AppendLine();
            }
            if (f.HasFlag(DataFormat.Binary))
            {
                var readerH = GetBinReadHead(isAlone, readerC);
                var writerH = GetBinWriteHead(isAlone, writerC);
                rr.AppendLines(GetReadCode(isAlone, callBase, callMode, readerH, readerC, "Binary", "BinaryReader"));
                rr.AppendLine();
                ww.AppendLines(GetWriteCode(isAlone, callBase, callMode, writerH, writerC, "Binary", "BinaryWriter"));
                ww.AppendLine();
            }

            var construct = GetConstruct(cds, registry);
            code.AppendLines(construct);
            code.AppendLine();
            code.AppendLines(rr);
            code.AppendLine();
            code.AppendLines(ww);
        }

        private static (string fqn, string name, string[] leafs) Simplify(ITypeSymbol type)
        {
            var ignoreDot = false;
            string[] leafs = null;
            string full;
            var name = full = type.ToTrimDisplay();
            name = name.Replace("System.", string.Empty);
            name = name.ToTitle();
            if (type.IsEnum(out var eut))
            {
                name = eut.ToTrimDisplay().ToTitle();
                name += $"Enum<{type}>";
                ignoreDot = true;
            }
            if (name.EndsWith("?"))
                name = nameof(Nullable) + name.Substring(0, name.Length - 1);
            while (name.EndsWith("[]"))
                name = name.Split(['[']).First().ToTitle() + nameof(Array);
            if (!ignoreDot && name.Contains("."))
            {
                if (!type.CanBeParent(out _))
                {
                    name = $"Exact<{type}>(\"{type}\")";
                    leafs = [type.ToString()];
                }
                else if (type.IsTyped(out _, out var ta, out var isList, out var isDict))
                {
                    if (isList)
                    {
                        var lt = ta.First();
                        var lts = Simplify(lt);
                        name = $"List<{lt}>";
                        full = lts.fqn;
                        leafs = lts.leafs;
                    }
                    else if (isDict)
                    {
                        var dt = ta.Last();
                        var dts = Simplify(dt);
                        name = $"Dict<{dt}>";
                        full = dts.fqn;
                        leafs = dts.leafs;
                    }
                }
                else
                {
                    name = $"OneOf<{type}>";
                    leafs = type.SearchType().Select(s => s.GetFqn()).ToArray();
                }
            }
            return (full, name, leafs);
        }

        private static void GenerateTypeReg(ICollection<string> code, string baseType, string[] leafs)
        {
            if (leafs == null)
                return;
            foreach (var leaf in leafs)
            {
                code.Add($"{RflObjName}.CanBe(\"{baseType}\", \"{leaf}\");");
                code.Add($"{RflObjName}.Learn(\"{leaf}\", () => new {leaf}());");
            }
        }
    }
}