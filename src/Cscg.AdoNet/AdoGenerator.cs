using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Cscg.Core;
using Cscg.Core.Model;
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
            // TODO igi.RegisterPostInitializationOutput(PostInitial);

            var sp = igi.SyntaxProvider;

            var tableAf = GetFullName(LibSpace, TableAn);
            igi.RegisterSourceOutput(sp.ForAttributeWithMetadataName(tableAf, Check, Wrap), Exec);

            var mapAf = GetFullName(LibSpace, MappingAn);
            igi.RegisterSourceOutput(sp.ForAttributeWithMetadataName(mapAf, Check, Wrap), Exec);
        }

        // TODO
        //private static void PostInitial(IncrementalGeneratorPostInitializationContext ctx)
        //{
            // var dbsBody = new CodeWriter();
            // var dbsCode = CodeTool.CreateClass($"{DbsName}<T>", LibSpace, dbsBody);
            // ctx.AddSource($"{DbsName}.cs", dbsCode);
        //}

        private static bool Check(SyntaxNode node, CancellationToken _)
            => node is ClassDeclarationSyntax;

        private static SyntaxWrap Wrap(GeneratorAttributeSyntaxContext ctx, CancellationToken _)
            => ctx.Wrap();

        private static void Exec(SourceProductionContext ctx, SyntaxWrap syntax)
        {
            Memory.Store(syntax.Class);

            var ccs = syntax.Symbol.FindArgs(simple: true);
            var cds = syntax.Class;
            var space = cds.GetParentName() ?? Coding.AutoNamespace;
            var name = cds.GetClassName();
            var fileName = $"{name}.g.cs";
            var code = new CodeWriter();
            code.AddUsings(LibSpace, "System", "Microsoft.Data.Sqlite");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");
            code.WriteClassLine(name);
            code.AppendLine("{");

            const string connType = "SqliteConnection";

            var crea = new CodeWriter();
            crea.AppendLine("public static string CreateTable()");
            crea.AppendLine("{");
            var isMap = ccs.TryGetValue(MappingAn, out _);
            var tableName = isMap ? name : BuildPlural(name);
            if (ccs.TryGetValue($"{TableAn}_Name", out var tbn)) tableName = tbn;
            var table = SqliteSource.Quote(tableName);
            crea.AppendLine("var sql = string.Join(Environment.NewLine, [");
            crea.AppendLine($"@\"CREATE TABLE IF NOT EXISTS \"{table}\" (\",");

            var after = new List<string>();
            var inner = new List<string>();
            var mapPk = new List<string>();
            var lastPk = default(string);
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
                    if (pk != null) lastPk = ppName;
                    var (pType, pCond) = SqliteSource.GetType(pp.ReturnType, pk);
                    crea.AppendLine($"@\"    \"{pName}\" {pType} {pCond},\",");

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
                crea.AppendLine(item);
            }

            crea.ModifyLast(l => l.Replace(",\",", "\","));
            crea.AppendLine("\");\"");
            if (after.Any())
            {
                crea.ModifyLast(l => l + ",");
                crea.AppendLines(["\"\",", "\"\","]);
                crea.AppendLines(after);
                crea.ModifyLast(l => l.Replace(";\",", ";\""));
            }
            crea.AppendLine("]);");
            crea.AppendLine("return sql;");
            crea.AppendLine("}");

            var del = new CodeWriter();
            if (!string.IsNullOrWhiteSpace(lastPk))
            {
                del.AppendLine($"public bool Delete({connType} conn)");
                del.AppendLine("{");
                del.AppendLine("using var cmd = conn.CreateCommand();");
                del.AppendLine($@"cmd.CommandText = @""DELETE FROM ""{table}"" WHERE {lastPk} = @p0;"";");
                del.AppendLine($@"cmd.Parameters.AddWithValue(""@p0"", this.{lastPk});");
                del.AppendLine("var delCount = cmd.ExecuteNonQuery();");
                del.AppendLine("return delCount == 1;");
                del.AppendLine("}");
            }

            var body = new CodeWriter();
            body.AppendLines(crea);
            body.AppendLine();
            body.AppendLines(del);

            code.AppendLines(body);
            code.AppendLine("}");
            code.AppendLine("}");
            ctx.AddSource(fileName, code.ToString());
        }
    }
}