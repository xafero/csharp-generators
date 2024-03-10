using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cscg.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Cscg.AdoNet.AdoSource;
using static Cscg.Core.CodeTool;

namespace Cscg.AdoNet
{
    [Generator(LanguageNames.CSharp)]
    public sealed class AdoGenerator : IIncrementalGenerator
    {
        private static readonly string TableAn = GetAttributeName(TableAttrName);
        private static readonly string MappingAn = GetAttributeName(MappingAttrName);
        private static readonly string ColAn = GetAttributeName(ColAttrName);
        private static readonly string KeyAn = GetAttributeName(KeyAttrName);
        private static readonly string ForeignAn = GetAttributeName(ForeignAttrName);

        public void Initialize(IncrementalGeneratorInitializationContext igi)
        {
            igi.RegisterPostInitializationOutput(PostInitial);

            var sp = igi.SyntaxProvider;

            var tableAf = GetFullName(LibSpace, TableAn);
            igi.RegisterSourceOutput(sp.ForAttributeWithMetadataName(tableAf, Check, Wrap), Exec);

            var mapAf = GetFullName(LibSpace, MappingAn);
            igi.RegisterSourceOutput(sp.ForAttributeWithMetadataName(mapAf, Check, Wrap), Exec);
        }

        private static void PostInitial(IncrementalGeneratorPostInitializationContext ctx)
        {
            /*
            var tableAc = CreateAttribute(TableAn, LibSpace, new CodeWriter
            {
                Lines = { "public string Name { get; set; }" }
            }, default, default, AttributeTargets.Class);
            ctx.AddSource($"{TableAn}.cs", tableAc);

            var mapAc = CreateAttribute(MappingAn, LibSpace, new CodeWriter
            {
                Lines =
                {
                    "public string First { get; set; }",
                    "public string Second { get; set; }"
                }
            }, default, default, AttributeTargets.Class);
            ctx.AddSource($"{MappingAn}.cs", mapAc);

            var colAc = CreateAttribute(ColAn, LibSpace, new CodeWriter
            {
                Lines = { "public string Name { get; set; }" }
            }, default, default, AttributeTargets.Property, AttributeTargets.Field);
            ctx.AddSource($"{ColAn}.cs", colAc);

            var keyAc = CreateAttribute(KeyAn, LibSpace, new CodeWriter
            {
                Lines = { }
            }, default, default, AttributeTargets.Property, AttributeTargets.Field);
            ctx.AddSource($"{KeyAn}.cs", keyAc);

            var forAc = CreateAttribute(ForeignAn, LibSpace, new CodeWriter
            {
                Lines =
                {
                    "public string Table { get; set; }",
                    "public string Column { get; set; }",
                    "public bool Unique { get; set; }"
                }
            }, default, default, AttributeTargets.Property, AttributeTargets.Field);
            ctx.AddSource($"{ForeignAn}.cs", forAc);
            */

            // var dbsBody = new CodeWriter();
            // var dbsCode = CodeTool.CreateClass($"{DbsName}<T>", LibSpace, dbsBody);
            // ctx.AddSource($"{DbsName}.cs", dbsCode);
        }

        private static bool Check(SyntaxNode node, CancellationToken _)
            => node is ClassDeclarationSyntax;

        private static SyntaxWrap Wrap(GeneratorAttributeSyntaxContext ctx, CancellationToken _)
            => ctx.Wrap();

        private static void Exec(SourceProductionContext ctx, SyntaxWrap syntax)
        {
            var ccs = syntax.Symbol.FindArgs(simple: true);
            var cds = syntax.Class;
            var space = cds.GetParentName() ?? Coding.AutoNamespace;
            var name = cds.GetClassName();
            var fileName = $"{name}.g.cs";
            var code = new CodeWriter();
            code.AddUsings(LibSpace, "System");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");
            code.WriteClassLine(name);
            code.AppendLine("{");

            var body = new CodeWriter();
            body.AppendLine("public static string CreateTable()");
            body.AppendLine("{");
            var isMap = ccs.TryGetValue(MappingAn, out _);
            var tableName = isMap ? name : BuildPlural(name);
            if (ccs.TryGetValue($"{TableAn}_Name", out var tbn)) tableName = tbn;
            var table = SqliteSource.Quote(tableName);
            body.AppendLine("var sql = string.Join(Environment.NewLine, [");
            body.AppendLine($"@\"CREATE TABLE IF NOT EXISTS \"{table}\" (\",");

            var after = new List<string>();
            var inner = new List<string>();
            var mapPk = new List<string>();
            foreach (var member in cds.Members)
                if (member is PropertyDeclarationSyntax pds)
                {
                    var pps = syntax.GetSymbol(pds);
                    var ppa = pps.FindArgs(simple: true);
                    if (!ppa.ContainsKey(ColAn))
                        continue;
                    var pp = syntax.GetInfo(pps);
                    var ppName = ppa.TryGetValue($"{ColAn}_Name", out var tpn) ? tpn : pp.Name;
                    var pName = SqliteSource.Quote(ppName);
                    var pk = !ppa.TryGetValue(KeyAn, out _)
                        ? null
                        : SqliteSource.Quote($"PK_{tableName.Trim('"')}");
                    var (pType, pCond) = SqliteSource.GetType(pp.ReturnType, pk);
                    body.AppendLine($"@\"    \"{pName}\" {pType} {pCond},\",");

                    if (ppa.TryGetValue(ForeignAn, out _))
                    {
                        ppa.TryGetValue($"{ForeignAn}_Table", out var ft);
                        ppa.TryGetValue($"{ForeignAn}_Column", out var fc);
                        ppa.TryGetValue($"{ForeignAn}_Unique", out var fu);
                        var u = fu == "true";
                        var (fi, fo) = SqliteSource.GetForeign(tableName, ppName, ft, fc, u);
                        inner.AddRange(fi);
                        if (isMap)
                            mapPk.Add(ppName);
                        after.AddRange(fo);
                    }
                }

            if (isMap)
            {
                inner.Insert(0, SqliteSource.GetMapKey(tableName, mapPk));
            }

            foreach (var item in inner)
            {
                body.AppendLine(item);
            }

            body.ModifyLast(l => l.Replace(",\",", "\","));
            body.AppendLine("\");\"");
            if (after.Any())
            {
                body.ModifyLast(l => l + ",");
                body.AppendLines(["\"\",", "\"\","]);
                body.AppendLines(after);
                body.ModifyLast(l => l.Replace(";\",", ";\""));
            }
            body.AppendLine("]);");
            body.AppendLine("return sql;");

            body.AppendLine("}");
            code.AppendLines(body);

            code.AppendLine("}");
            code.AppendLine("}");
            ctx.AddSource(fileName, code.ToString());
        }
    }
}