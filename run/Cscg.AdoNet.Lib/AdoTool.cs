using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;

// ReSharper disable SuspiciousTypeConversion.Global

namespace Cscg.AdoNet.Lib
{
    public static class AdoTool
    {
        private const string NewLine = "\n";

        public static string CreateConnStr<T>(params Action<T>[] actions)
            where T : DbConnectionStringBuilder, new()
        {
            var builder = new T();
            foreach (var action in actions)
                action(builder);
            return builder.ConnectionString;
        }

        public static T OpenConn<T>(string connStr, bool open = true)
            where T : DbConnection, new()
        {
            var conn = new T { ConnectionString = connStr };
            if (open)
                conn.Open();
            return conn;
        }

        public static string GetTransaction(params string[] inner)
        {
            var lines = new List<string> { "BEGIN TRANSACTION;", "" };
            foreach (var item in inner)
            {
                lines.Add(item);
                lines.Add("");
            }
            lines.AddRange(["", "COMMIT;"]);
            return string.Join(NewLine, lines);
        }

        public static (string sql, int rows) RunTransaction(
            this DbConnection conn, params string[] inner)
        {
            using var cmd = conn.CreateCommand();
            var sql = GetTransaction(inner);
            cmd.CommandText = sql;
            var rows = cmd.ExecuteNonQuery();
            return (sql, rows);
        }

        public static IEnumerable<TData> ReadData<TData, TReader>(this TReader reader)
            where TReader : DbDataReader
            where TData : IActiveRead<TReader>, new()
        {
            while (reader.Read())
            {
                var item = new TData();
                for (var index = 0; index < reader.FieldCount; index++)
                {
                    var key = reader.GetName(index);
                    item.ReadSql(reader, key, index);
                }
                if (item is IActiveNested<TData> an)
                    item = an.Inner;
                yield return item;
            }
        }

        public static Dictionary<string, string> GetColumns<TCommand>(this TCommand cmd,
            string prefix = "@p")
            where TCommand : DbCommand
        {
            var dict = new Dictionary<string, string>();
            foreach (DbParameter parameter in cmd.Parameters)
            {
                var key = parameter.ParameterName;
                var val = key.Replace(prefix, string.Empty);
                dict[val] = key;
            }
            return dict;
        }

        public static string CreateInsert(this IDictionary<string, string> cols, string tbl,
            string? id = null)
        {
            var bld = new StringBuilder();
            bld.Append("INSERT INTO ");
            bld.Append('"');
            bld.Append(tbl);
            bld.Append('"');
            bld.Append($" ({string.Join(", ", cols.Keys)})");
            bld.Append(" VALUES");
            bld.Append($" ({string.Join(", ", cols.Values)})");
            if (!string.IsNullOrWhiteSpace(id))
            {
                bld.Append(" RETURNING");
                bld.Append($" {id};");
            }
            return bld.ToString();
        }

        public static string CreateUpdate(this IDictionary<string, string> cols, string tbl, string id,
            string prefix = "@p", int skip = 1)
        {
            var bld = new StringBuilder();
            bld.Append("UPDATE ");
            bld.Append('"');
            bld.Append(tbl);
            bld.Append('"');
            bld.Append(" SET");
            var tmp = cols.Skip(skip).Select(c => $"{c.Key} = {c.Value}");
            bld.Append($" {string.Join(", ", tmp)}");
            bld.Append(" WHERE");
            bld.Append($" {id} = {prefix}{id};");
            return bld.ToString();
        }

        public static string CreateSelect(this IDictionary<string, string> cols, string tbl)
        {
            var bld = new StringBuilder();
            bld.Append("SELECT * FROM ");
            bld.Append('"');
            bld.Append(tbl);
            bld.Append('"');
            bld.Append(" WHERE");
            var tmp = cols.Select(c => $"{c.Key} = {c.Value}");
            bld.Append($" {string.Join(" AND ", tmp)};");
            return bld.ToString();
        }

        public static string CreateDelete(this IDictionary<string, string> cols, string tbl)
        {
            var bld = new StringBuilder();
            bld.Append("DELETE FROM ");
            bld.Append('"');
            bld.Append(tbl);
            bld.Append('"');
            bld.Append(" WHERE");
            var tmp = cols.Select(c => $"{c.Key} = {c.Value}");
            bld.Append($" {string.Join(" AND ", tmp)};");
            return bld.ToString();
        }
    }
}