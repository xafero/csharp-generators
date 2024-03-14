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
            var sp = igi.SyntaxProvider;

            var tableAf = GetFullName(LibSpace, TableAn);
            igi.RegisterSourceOutput(sp.ForAttributeWithMetadataName(tableAf, Check, Wrap), Exec);

            var mapAf = GetFullName(LibSpace, MappingAn);
            igi.RegisterSourceOutput(sp.ForAttributeWithMetadataName(mapAf, Check, Wrap), Exec);
        }

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
            code.AddUsings(LibSpace, "System", "System.Linq", "Microsoft.Data.Sqlite");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");

            const string connType = "SqliteConnection";
            const string writType = "SqliteCommand";
            const string readType = "SqliteDataReader";
            var adiType = $"IActiveData<{readType}, {writType}>";
            code.WriteClassLine(name, interfaces: [adiType]);
            code.AppendLine("{");

            var crea = new CodeWriter();
            crea.AppendLine("public static string CreateTable()");
            crea.AppendLine("{");
            var isMap = ccs.TryGetValue(MappingAn, out _);
            var tableName = isMap ? name : BuildPlural(name);
            if (ccs.TryGetValue($"{TableAn}_Name", out var tbn)) tableName = tbn;
            var table = SqliteSource.Quote(tableName);
            crea.AppendLine("var sql = string.Join(Environment.NewLine, [");
            crea.AppendLine($"@\"CREATE TABLE IF NOT EXISTS \"{table}\" (\",");

            var deser = new CodeWriter();
            var sqser = new CodeWriter();

            var after = new List<string>();
            var inner = new List<string>();
            var mapPk = new List<string>();
            var lastPk = default(string);
            var lastPkT = default(ITypeSymbol);
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
                    if (pk != null)
                    {
                        lastPk = ppName;
                        lastPkT = pp.ReturnType;
                    }
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

                    deser.AppendLine($"if (key == {pName})");
                    deser.AppendLine("{");
                    deser.AppendLine($"this.{pp.Name} = {SqliteSource.GetRead(pp.ReturnType)};");
                    deser.AppendLine("return;");
                    deser.AppendLine("}");

                    var pNamePm = $"\"@p{pName.TrimStart('"')}";
                    sqser.AppendLine($"if (this.{pp.Name} != default)");
                    sqser.AppendLine("{");
                    sqser.AppendLine($"w.Parameters.AddWithValue({pNamePm}, this.{pp.Name});");
                    sqser.AppendLine("}");
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

            var sel = new CodeWriter();
            sel.AppendLine($"public void ReadSql({readType} r, string key, int i)");
            sel.AppendLine("{");
            sel.AppendLines(deser);
            sel.AppendLine("}");
            sel.AppendLine();
            sel.AppendLine($"public void WriteSql({writType} w)");
            sel.AppendLine("{");
            sel.AppendLines(sqser);
            sel.AppendLine("}");
            sel.AppendLine();

            sel.AppendLine($"public static {name}[] FindSame({connType} conn, params Action<{name}>[] func)");
            sel.AppendLine("{");
            sel.AppendLine("using var cmd = conn.CreateCommand();");
            sel.AppendLine($"var sample = new {name}();");
            sel.AppendLine("Array.ForEach(func, f => f(sample));");
            sel.AppendLine("sample.WriteSql(cmd);");
            sel.AppendLine($@"cmd.CommandText = cmd.GetColumns().CreateSelect({table});");
            sel.AppendLine("using var reader = cmd.ExecuteReader();");
            sel.AppendLine($"return reader.ReadData<{name}, {readType}>().ToArray();");
            sel.AppendLine("}");

            var del = new CodeWriter();
            var upd = new CodeWriter();
            var ins = new CodeWriter();
            var fin = new CodeWriter();
            var lst = new CodeWriter();
            if (!string.IsNullOrWhiteSpace(lastPk))
            {
                lst.AppendLine($"public static {name}[] List({connType} conn, int limit, int offset)");
                lst.AppendLine("{");
                lst.AppendLine("using var cmd = conn.CreateCommand();");
                lst.AppendLine($@"cmd.CommandText = @""SELECT * FROM ""{table}"" ORDER BY {lastPk} LIMIT @p0 OFFSET @p1;"";");
                lst.AppendLine($@"cmd.Parameters.AddWithValue(""@p0"", limit);");
                lst.AppendLine($@"cmd.Parameters.AddWithValue(""@p1"", offset);");
                lst.AppendLine("using var reader = cmd.ExecuteReader();");
                lst.AppendLine($"return reader.ReadData<{name}, {readType}>().ToArray();");
                lst.AppendLine("}");

                fin.AppendLine($"public static {name} Find({connType} conn, {lastPkT} id)");
                fin.AppendLine("{");
                fin.AppendLine("using var cmd = conn.CreateCommand();");
                fin.AppendLine($@"cmd.CommandText = @""SELECT * FROM ""{table}"" WHERE {lastPk} = @p0;"";");
                fin.AppendLine(@"cmd.Parameters.AddWithValue(""@p0"", id);");
                fin.AppendLine("using var reader = cmd.ExecuteReader();");
                fin.AppendLine($"return reader.ReadData<{name}, {readType}>().FirstOrDefault();");
                fin.AppendLine("}");

                ins.AppendLine($"public {lastPkT} Insert({connType} conn)");
                ins.AppendLine("{");
                ins.AppendLine("using var cmd = conn.CreateCommand();");
                ins.AppendLine("this.WriteSql(cmd);");
                ins.AppendLine($@"cmd.CommandText = cmd.GetColumns().CreateInsert({table}, ""{lastPk}"");");
                ins.AppendLine($"return cmd.ExecuteScalar().ConvertTo<{lastPkT}>();");
                ins.AppendLine("}");

                upd.AppendLine($"public bool Update({connType} conn)");
                upd.AppendLine("{");
                upd.AppendLine("using var cmd = conn.CreateCommand();");
                upd.AppendLine("this.WriteSql(cmd);");
                upd.AppendLine($@"cmd.CommandText = cmd.GetColumns().CreateUpdate({table}, ""{lastPk}"");");
                upd.AppendLine("return cmd.ExecuteNonQuery() == 1;");
                upd.AppendLine("}");

                del.AppendLine($"public bool Delete({connType} conn)");
                del.AppendLine("{");
                del.AppendLine("using var cmd = conn.CreateCommand();");
                del.AppendLine($@"cmd.CommandText = @""DELETE FROM ""{table}"" WHERE {lastPk} = @p0;"";");
                del.AppendLine($@"cmd.Parameters.AddWithValue(""@p0"", this.{lastPk});");
                del.AppendLine("return cmd.ExecuteNonQuery() == 1;");
                del.AppendLine("}");
            }

            if (isMap && mapPk.Count >= 1)
            {
                ins.AppendLine($"public bool Insert({connType} conn)");
                ins.AppendLine("{");
                ins.AppendLine("using var cmd = conn.CreateCommand();");
                ins.AppendLine("this.WriteSql(cmd);");
                ins.AppendLine($@"cmd.CommandText = cmd.GetColumns().CreateInsert({table});");
                ins.AppendLine($"return cmd.ExecuteNonQuery() == 1;");
                ins.AppendLine("}");

                del.AppendLine($"public bool Delete({connType} conn)");
                del.AppendLine("{");
                del.AppendLine("using var cmd = conn.CreateCommand();");
                del.AppendLine("this.WriteSql(cmd);");
                del.AppendLine($@"cmd.CommandText = cmd.GetColumns().CreateDelete({table});");
                del.AppendLine("return cmd.ExecuteNonQuery() == 1;");
                del.AppendLine("}");
            }

            var body = new CodeWriter();
            body.AppendLines(crea);
            body.AppendLine();
            body.AppendLines(sel);
            body.AppendLine();
            if (lst.Lines.Count >= 1)
            {
                body.AppendLines(lst);
                body.AppendLine();
            }
            if (fin.Lines.Count >= 1)
            {
                body.AppendLines(fin);
                body.AppendLine();
            }
            body.AppendLines(ins);
            body.AppendLine();
            if (upd.Lines.Count >= 1)
            {
                body.AppendLines(upd);
                body.AppendLine();
            }
            body.AppendLines(del);

            code.AppendLines(body);
            code.AppendLine("}");
            code.AppendLine("}");
            ctx.AddSource(fileName, code.ToString());
        }
    }
}