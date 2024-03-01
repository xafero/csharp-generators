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

            var construct = new CodeWriter();
            var readerH = new CodeWriter();
            var readerC = new CodeWriter();
            var reader = new CodeWriter();
            var writerH = new CodeWriter();
            var writerC = new CodeWriter();
            var writer = new CodeWriter();

            construct.AppendLine($"static {cds.GetClassName()}()");
            construct.AppendLine("{");

            var registry = new List<string>();
            foreach (var member in cds.Members)
                if (member is PropertyDeclarationSyntax pds)
                {
                    var pp = s.GetInfo(s.GetSymbol(pds));
                    var pName = pp.Name;

                    var (pFull, pRt, leafs) = Simplify(pp.ReturnType);
                    GenerateTypeReg(registry, pFull, leafs);

                    var readMeth = $"Read{pRt}";
                    readerC.AppendLine($"if (key == nameof(this.{pName}))");
                    readerC.AppendLine("{");
                    readerC.AppendLine($"this.{pName} = this.{readMeth}(ref r);");
                    readerC.AppendLine(isAlone ? "continue;" : "return;");
                    readerC.AppendLine("}");

                    var writeMeth = $"Write{pRt}";
                    writerC.AppendLine($"this.WriteProperty(ref w, \"{pName}\");");
                    writerC.AppendLine($"this.{writeMeth}(ref w, this.{pName});");
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

            reader.AppendLine($"public {callMode}void ReadCBOR(ref CborReader r)");
            reader.AppendLine("{");
            reader.AppendLines(readerH);
            reader.AppendLine("}");

            if (!isAlone)
            {
                reader.AppendLine();
                reader.AppendLine($"public {callMode}void ReadCBORCore(ref CborReader r, string key)");
                reader.AppendLine("{");
                if (callBase) reader.AppendLine("base.ReadCBORCore(ref r, key);");
                reader.AppendLines(readerC);
                reader.AppendLine("}");
            }

            writer.AppendLine($"public {callMode}void WriteCBOR(ref CborWriter w)");
            writer.AppendLine("{");
            writer.AppendLines(writerH);
            writer.AppendLine("}");

            if (!isAlone)
            {
                writer.AppendLine();
                writer.AppendLine($"public {callMode}void WriteCBORCore(ref CborWriter w)");
                writer.AppendLine("{");
                if (callBase) writer.AppendLine("base.WriteCBORCore(ref w);");
                writer.AppendLines(writerC);
                writer.AppendLine("}");
            }

            construct.AppendLines(registry.OrderBy(e => e).Distinct());
            construct.AppendLine("}");

            code.AppendLines(construct);
            code.AppendLine();
            code.AppendLines(reader);
            code.AppendLine();
            code.AppendLines(writer);
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
                    name = $"Exact<{type}>";
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