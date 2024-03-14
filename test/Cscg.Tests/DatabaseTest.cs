using System.Data;
using Cscg.AdoNet.Lib;
using Microsoft.Data.Sqlite;
using SourceGenerated.Sql;
using Xunit;
using Xunit.Abstractions;
using static Cscg.Tests.Tools.DebugTool;

namespace Cscg.Tests
{
    public class DatabaseTest
    {
        private readonly ITestOutputHelper _out;

        public DatabaseTest(ITestOutputHelper @out)
        {
            _out = @out;
        }

        [Theory]
        [InlineData("b", "e")]
        public void TestSqlite(string cla, string mode)
        {
            var fileName = IoTool.DeleteIfExists($"blog_{cla}_{mode}.db");
            using var conn = CreateConnection(fileName);

            var input = new Person { Name = "Harry Potter" };

            var id = input.Id = input.Insert(conn);
            Assert.Equal(1, id);

            var output = Person.Find(conn, id);

            var expected = ToJson(input);
            var actual = ToJson(output);
            _out.WriteLine(actual);
            Assert.Equal(expected, actual);
        }

        private static SqliteConnection CreateConnection(string fileName)
        {
            var connStr = AdoTool.CreateConnStr<SqliteConnectionStringBuilder>(
                s => s.DataSource = fileName
            );
            Assert.Equal(23, connStr.Length);

            var conn = AdoTool.OpenConn<SqliteConnection>(connStr);
            Assert.Equal(ConnectionState.Open, conn.State);

            var (sql, rows) = conn.RunTransaction(Person.CreateTable());
            Assert.Equal(172, sql.Length);
            Assert.Equal(0, rows);

            return conn;
        }
    }
}