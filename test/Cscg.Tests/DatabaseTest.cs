using System.Data;
using System.Linq;
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

            var input = new Person { Name = "Tom" };

            var id = input.Id = input.Insert(conn);
            Assert.Equal(1, id);

            input.Name = "Harry Potter";
            Assert.True(input.Update(conn));

            var output = Person.Find(conn, id);
            Assert.NotNull(output);

            output = Person.List(conn, 10, 0).Single();
            Assert.NotNull(output);

            output = Person.FindSame(conn, x => x.Name = input.Name).Single();
            Assert.True(output.Delete(conn));

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