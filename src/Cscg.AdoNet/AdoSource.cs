using System;
using System.Collections.Generic;
using Cscg.Core;
using Microsoft.CodeAnalysis;

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
        {
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

            const string cbldType = "SqliteConnectionStringBuilder";
            const string connType = "SqliteConnection";
            const string writType = "SqliteCommand";
            const string readType = "SqliteDataReader";
            var adiType = $"DbContext<{connType}>"; // IActiveData<{readType}, {writType}>";
            var adiTypes = new List<string> { adiType };
            code.WriteClassLine(name, interfaces: adiTypes.ToArray());
            code.AppendLine("{");

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
                "_ = conn.RunTransaction([",
                "]);",
                "}"
            };

            code.AppendLines(body, trim: false);
            code.AppendLine("}");
            code.AppendLine("}");
            code.AppendLine(" // " + DateTime.Now.ToString("O")); // TODO
            ctx.AddSource(fileName, code.ToString());
        }
    }
}