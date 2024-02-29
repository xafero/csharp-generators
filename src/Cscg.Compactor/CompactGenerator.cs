using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cscg.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cscg.Compactor
{
    [Generator(LanguageNames.CSharp)]
    public sealed class CompactGenerator : IIncrementalGenerator
    {
        private const string LibSpace = "Cscg.Compactor.Lib";
        private const string Space = Coding.AutoNamespace;
        private const string BinObjName = "Compacted";
        private const string ExtObjName = "CompactedExt";
        private const string IntObjName = "ICompacted";
        private const string ItfObjName = "ICompactor";
        private const string RflObjName = "Reflections";

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
            var cds = syntax.Class;
            var space = cds.GetParentName() ?? Coding.AutoNamespace;
            var name = cds.GetClassName();
            var fileName = $"{name}.g.cs";
            var code = new CodeWriter();
            code.AppendLine($"using {LibSpace};");
            code.AppendLine("using System;");
            code.AppendLine("using System.Collections.Generic;");
            code.AppendLine("using System.Text;");
            code.AppendLine("using System.IO;");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");
            code.AppendLine($"partial class {name} : {IntObjName}");
            code.AppendLine("{");
            ExecBody(code, cds, syntax);
            code.AppendLine("}");
            code.AppendLine("}");
            ctx.AddSource(fileName, code.ToString());
        }

        private static void ExecBody(CodeWriter code, ClassDeclarationSyntax cds, SyntaxWrap s)
        {
            var construct = new CodeWriter();
            var reader = new CodeWriter();
            var writer = new CodeWriter();
            construct.AppendLine($"static {cds.GetClassName()}()");
            construct.AppendLine("{");
            reader.AppendLine("public void ReadBy(ICompactor c)");
            reader.AppendLine("{");
            writer.AppendLine("public void WriteBy(ICompactor c)");
            writer.AppendLine("{");
            var registry = new List<string>();
            foreach (var member in cds.Members)
                if (member is PropertyDeclarationSyntax pds)
                {
                    var pp = s.GetInfo(s.GetSymbol(pds));
                    var pName = pp.Name;

                    var (pFull, pRt, leafs) = Simplify(pp.ReturnType);
                    GenerateTypeReg(registry, pFull, leafs);

                    var readMeth = $"Read{pRt}";
                    reader.AppendLine($"this.{pName} = c.{readMeth}();");

                    var writeMeth = $"Write{pRt}";
                    writer.AppendLine($"c.{writeMeth}(this.{pName});");
                }
            construct.AppendLines(registry.OrderBy(e => e).Distinct());
            construct.AppendLine("}");
            reader.AppendLine("}");
            writer.AppendLine("}");

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