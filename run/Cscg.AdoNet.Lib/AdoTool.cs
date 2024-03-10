using System;
using System.Collections.Generic;
using System.Data.Common;

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
            where TData : IActiveData<TReader>, new()
        {
            while (reader.Read())
            {
                var item = new TData();
                for (var index = 0; index < reader.FieldCount; index++)
                {
                    var key = reader.GetName(index);
                    item.ReadSql(reader, key, index);
                }
                yield return item;
            }
        }
    }
}