using System.Collections.Generic;
using System.Linq;
using Cscg.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cscg.AdoNet
{
    internal static class AdoSource
    {
        internal const string LibSpace = "Cscg.AdoNet.Lib";

        internal const string ContextAttrName = "Context";
        internal const string TableAttrName = "Table";
        internal const string MappingAttrName = "Mapping";
        internal const string ColAttrName = "Column";
        internal const string KeyAttrName = "Key";
        internal const string ForeignAttrName = "Foreign";

        internal const string DiscriminatorFld = "Discriminator";

        public static string BuildPlural(string name)
        {
            if (name.EndsWith("y"))
                return $"{name.Substring(0, name.Length - 1)}ies";

            return $"{name}s";
        }

        internal static void Exec(SourceProductionContext ctx, SyntaxWrap syntax)
            => ctx.WrapForError(syntax, Exec);

        private static void Exec(ClassDeclarationSyntax cds, string name, CodeWriter code, SyntaxWrap syntax)
        {
            var cdy = syntax.Symbol;
            var ccs = cdy.FindArgs(simple: true);
            var space = cds.GetParentName() ?? Coding.AutoNamespace;
            code.AddUsings(LibSpace, "System", "System.Linq", "Microsoft.Data.Sqlite");
            code.AppendLine();
            code.AppendLine($"namespace {space}");
            code.AppendLine("{");

            const string cbldType = "SqliteConnectionStringBuilder";
            const string connType = "SqliteConnection";
            const string writType = "SqliteCommand";
            const string readType = "SqliteDataReader";
            var adiType = $"DbContext<{connType}>"; // IActiveData<{readType}, {writType}>";
            var adiTypes = new List<string> { adiType };
            code.WriteClassLine(name, interfaces: adiTypes.ToArray());
            code.AppendLine("{");

            var objects = cds.Members.OfType<FieldDeclarationSyntax>().Select(p =>
            {
                if (p.Declaration.Type.GetGeneric(out var fh, out var fa) && fh == "DbSet")
                    return new { o = fa[0], n = p.GetFields().Single() };
                return null;
            }).Where(x => x != null).ToArray();

            var dbSets = new List<string>();
            var creators = new List<string>();
            foreach (var p in objects)
            {
                creators.Add($"    {p.o}.CreateTable(),");
                var dbSetName = $"{p.o}DbSet";
                var dbSetProp = p.n.n.Trim('_').ToTitle();
                dbSets.Add("");
                dbSets.Add($"public {dbSetName} {dbSetProp}");
                dbSets.Add("{");
                dbSets.Add($"get => {p.n.n} as {dbSetName} ?? ({dbSetName})({p.n.n} = new {dbSetName}(this));");
                dbSets.Add("}");
            }
            creators.ModifyLast(f => f.TrimEnd(','));

            var body = new List<string>
            {
                "private string GetDataSource()",
                "{",
                "if (!string.IsNullOrWhiteSpace(DataSource)) return DataSource;",
                $"return (DataSource = \"{name}.db\");",
                "}",
                "",
                $"protected override bool IsDatabaseEmpty({connType} conn)",
                "{",
                "return !conn.GetAllTableNames().Any();",
                "}",
                "",
                "protected override string GetConnStr()",
                "{",
                $"var connStr = AdoTool.CreateConnStr<{cbldType}>(",
                "   s => s.DataSource = GetDataSource(),",
                "   s => s.Mode = SqliteOpenMode.ReadWriteCreate,",
                "   s => s.Pooling = true,",
                "   s => s.ForeignKeys = true,",
                "   s => s.RecursiveTriggers = true",
                ");",
                "return connStr;",
                "}",
                "",
                $"protected override void CreateTables({connType} conn)",
                "{",
                "_ = conn.RunTransaction(["
            };
            body.AddRange(creators);
            body.AddRange(["]);", "}"]);
            body.AddRange(dbSets);

            code.AppendLines(body, trim: false);
            code.AppendLine("}");
            code.AppendLine("}");
        }

        internal static List<string> NewSet(string name, string connType)
        {
            var code = new List<string>();
            var setName = $"{name}DbSet";
            code.Add($"public partial class {setName} : DbSet<{name}>");
            code.Add("{");
            code.Add($"public {setName}(DbContext<{connType}> ctx)");
            code.Add("{");
            code.Add("Context = ctx;");
            code.Add("}");
            code.Add("}");
            return code;
        }
    }
}