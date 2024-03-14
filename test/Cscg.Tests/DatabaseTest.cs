using System;
using System.Data;
using System.Linq;
using Cscg.AdoNet.Lib;
using Microsoft.Data.Sqlite;
using SourceGenerated.Sql;
using Xunit;
using Xunit.Abstractions;
using IAD = Cscg.AdoNet.Lib.IActiveData<Microsoft.Data.Sqlite.SqliteDataReader, Microsoft.Data.Sqlite.SqliteCommand>;
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
        [InlineData("p", "1")]
        [InlineData("f", "1")]
        public void TestSqlite(string cla, string mode)
        {
            if (cla == "p")
            {
                DoDatabase(cla, mode, () => new Person { Name = "Tom" },
                    (x, conn) => x.Id = x.Insert(conn),
                    (x, conn) =>
                    {
                        x.Name = "Harry Potter";
                        return x.Update(conn);
                    },
                    Person.Find, conn => Person.List(conn, 10, 0).Single(),
                    (conn, y) => Person.FindSame(conn, x => x.Name = y.Name).Single(),
                    (conn, y) => y.Delete(conn)
                );
                return;
            }
            if (cla == "f")
            {
                DoDatabase(cla, mode, () => new Funny
                    {
                        H = 42, P = "Tom", T = 289, A = true, B = 129, C = [3, 4, 22, 43], 
                        E = DateOnly.FromDateTime(DateTime.Now), F = new DateTime(2011,2,3),
                        D = 't', G = DateTimeOffset.UnixEpoch, I = 39.2933, K = 2830, J = Guid.NewGuid(),
                        L = 292, N = -13, M = 39383, O = 99.99223f, U = 39399, S = 2911,
                        Q = TimeOnly.FromDateTime(DateTime.UtcNow), R = TimeSpan.FromMinutes(29)
                    },
                    (x, conn) => x.Id = x.Insert(conn),
                    (x, conn) =>
                    {
                        x.P = "Harry";
                        return x.Update(conn);
                    },
                    Funny.Find, conn => Funny.List(conn, 10, 0).Single(),
                    (conn, y) => Funny.FindSame(conn, x => x.P = y.P).Single(),
                    (conn, y) => y.Delete(conn)
                );
            }
        }

        private void DoDatabase<T>(string cla, string mode, Func<T> create,
            Func<T, SqliteConnection, int> getId, Func<T, SqliteConnection, bool> getUpdate,
            Func<SqliteConnection, int, T> find, Func<SqliteConnection, T> list,
            Func<SqliteConnection, T, T> same, Func<SqliteConnection, T, bool> delete)
            where T : IAD
        {
            var fileName = IoTool.DeleteIfExists($"blog_{cla}_{mode}.db");
            using var conn = CreateConnection(fileName);

            var input = create();

            var id = getId(input, conn);
            Assert.Equal(1, id);

            var upd = getUpdate(input, conn);
            Assert.True(upd);

            var output = find(conn, id);
            Assert.NotNull(output);

            output = list(conn);
            Assert.NotNull(output);

            output = same(conn, input);
            Assert.True(delete(conn, output));

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

            var (sql, rows) = conn.RunTransaction(Person.CreateTable(),
                Funny.CreateTable());
            Assert.Equal(816, sql.Length);
            Assert.Equal(0, rows);

            return conn;
        }
    }
}