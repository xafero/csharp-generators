using System.Collections.Generic;
using System.Data.Common;

namespace Cscg.AdoNet.Lib
{
    public static class SqliteTool
    {
        public static IEnumerable<string> GetAllTableNames(this DbConnection conn)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT name FROM sqlite_master WHERE type=='table' ORDER BY name";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                var name = reader.GetString(0);
                yield return name;
            }
        }

        public static IEnumerable<TableInfo> GetTableColumns(this DbConnection conn, string table)
        {
            using var cmd = conn.CreateCommand();
            cmd.CommandText = $"SELECT '{table}' AS tname, * FROM pragma_table_info('{table}')";
            using var read = cmd.ExecuteReader();
            while (read.Read())
            {
                yield return new TableInfo
                {
                    Table = read.GetString(0),
                    Id = read.GetInt32(1),
                    Name = read.GetString(2),
                    Type = read.GetString(3),
                    NotNull = read.GetBoolean(4),
                    Default = read.GetValue(5),
                    Primary = read.GetBoolean(6)
                };
            }
        }

        public class TableInfo
        {
            public string? Table { get; set; }
            public int? Id { get; set; }
            public string? Name { get; set; }
            public string? Type { get; set; }
            public bool? NotNull { get; set; }
            public object? Default { get; set; }
            public bool? Primary { get; set; }
        }
    }
}