using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Xml.Linq;
using Cscg.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Simplification;
using static Cscg.Core.Sources;

namespace Cscg.Compactor
{
    [Generator(LanguageNames.CSharp)]
    public sealed class CompactGenerator : IIncrementalGenerator
    {
        private const string Space = Coding.AutoNamespace;
        private const string BinObjName = "Compacted";
        private const string ExtObjName = "CompactedExt";
        private const string IntObjName = "ICompacted";

        public void Initialize(IncrementalGeneratorInitializationContext igi)
        {
            igi.RegisterPostInitializationOutput(ctx =>
            {
                var attrCode = Coding.GenerateAttr(BinObjName, Space);
                ctx.AddSource($"{BinObjName}Attribute.g.cs", From(attrCode));

                var intCode = Coding.GenerateIntf(IntObjName, Space, new List<string>
                {
                }, "System.IO");
                ctx.AddSource($"{IntObjName}.g.cs", From(intCode));

                var extCode = Coding.GenerateExt(ExtObjName, Space, new List<string[]>
                {
                }, "System.IO");
                ctx.AddSource($"{ExtObjName}.g.cs", From(extCode));
            });

            const string fqn = $"{Space}.{BinObjName}Attribute";
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
            ExecBody(code, cds, syntax);
            code.AppendLine("}");
            code.AppendLine("}");
            ctx.AddSource(fileName, code.ToString());
        }

        private static void ExecBody(CodeWriter code, ClassDeclarationSyntax cds, SyntaxWrap s)
        {
            var reader = new CodeWriter();
            var writer = new CodeWriter();
            foreach (var member in cds.Members)
                if (member is PropertyDeclarationSyntax pds)
                {
                    var pp = s.GetInfo(s.GetSymbol(pds));
                    var pName = pp.Name;
                    var (pFull, pRt) = Simplify(pp.ReturnType);

                    var readMeth = $"Read{pRt}";
                    reader.AppendLine($" // TODO ?! {pName} | {readMeth} | {pFull}");

                    var writeMeth = $"Write{pRt}";
                    writer.AppendLine($" // TODO ?! {pName} | {writeMeth} | {pFull}");
                }
            code.AppendLines(reader);
            code.AppendLine();
            code.AppendLines(writer);
        }

        private static (string fqn, string name) Simplify(ITypeSymbol type)
        {
            string full;
            var name = full = type.ToTrimDisplay();
            name = name.Replace("System.", string.Empty);
            name = name.ToTitle();
            if (type.IsEnum(out var eut))
            {
                name = eut.ToTrimDisplay().ToTitle();
                name += "Enum";
            }
            if (name.EndsWith("?"))
                name = nameof(Nullable) + name.Substring(0, name.Length - 1);
            while (name.EndsWith("[]"))
                name = name.Split(['[']).First().ToTitle() + nameof(Array);
            if (name.Contains("."))
            {
                if (!type.CanBeParent(out _))
                {
                    name = "Exact";
                }
                else if (type.IsTyped(out _, out var ta, out var isList, out var isDict))
                {
                    if (isList)
                    {
                        var lt = ta.First();
                        var lts = Simplify(lt);
                        name = "List";
                        full = lts.fqn;
                    }
                    else if (isDict)
                    {
                        var dt = ta.Last();
                        var dts = Simplify(dt);
                        name = "Dict";
                        full = dts.fqn;
                    }
                }
                else
                {
                    var sub = type.SearchType().ToArray();
                    name = "OneOf";
                    full = $"{full} # {string.Join(" ; ", sub.Select(s => s.GetFqn()))}";
                }
            }
            return (full, name);
        }
    }
}