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
        [InlineData("b", "1")]
        [InlineData("p", "1")]
        [InlineData("u", "1")]
        [InlineData("f", "1")]
        public void TestSqlite(string cla, string mode)
        {
            using var sc = CreateConn(cla, mode);
            if (cla == "b")
            {
                var blogId = default(int);
                DoDatabase(sc, () => new Blog { Rating = 4, Url = "www.tom.tk" },
                    (x, conn) => x.MyBlogId = blogId = conn.Blogs.Insert(x),
                    (x, conn) =>
                    {
                        x.Url = "www.harry.po";
                        return conn.Blogs.Update(x);
                    },
                    (conn, id) => conn.Blogs.Find((int)id),
                    conn => conn.Blogs.List().Single(),
                    (conn, y) => conn.Blogs.FindSame(x => x.Url = y.Url).Single(),
                    (_, _) => true
                );
                DoDatabase(sc, () => new Post { Title = "Super Tom", Content = "Nothing.", BlogId = blogId },
                    (x, conn) => x.PostId = conn.Posts.Insert(x),
                    (x, conn) =>
                    {
                        x.Title = "Harry Wonder";
                        return conn.Posts.Update(x);
                    },
                    (conn, id) => conn.Posts.Find((int)id),
                    conn => conn.Posts.List().Single(),
                    (conn, y) => conn.Posts.FindSame(x => x.Title = y.Title).Single(),
                    (conn, y) => conn.Posts.Delete(y)
                );
                sc.Blogs.Delete(sc.Blogs.Find(blogId));
                return;
            }
            if (cla == "p")
            {
                var personId = default(int);
                DoDatabase(sc, () => new Person { Name = "Tom" },
                    (x, conn) => x.Id = personId = conn.Persons.Insert(x),
                    (x, conn) =>
                    {
                        x.Name = "Harry Potter";
                        return conn.Persons.Update(x);
                    },
                    (conn, id) => conn.Persons.Find((int)id),
                    conn => conn.Persons.List().Single(),
                    (conn, y) => conn.Persons.FindSame(x => x.Name = y.Name).Single(),
                    (_, _) => true
                );
                var houseId = default(int);
                DoDatabase(sc, () => new House { Street = "Main Street 129" },
                    (x, conn) => x.MyId = houseId = conn.Houses.Insert(x),
                    (x, conn) =>
                    {
                        x.Street = "Scotsman Way 4";
                        return conn.Houses.Update(x);
                    },
                    (conn, id) => conn.Houses.Find((int)id),
                    conn => conn.Houses.List().Single(),
                    (conn, y) => conn.Houses.FindSame(x => x.Street = y.Street).Single(),
                    (_, _) => true
                );
                var hp = default(HousePerson);
                DoDatabase(sc, () => hp = new HousePerson { HousesMyId = houseId, OwnersId = personId },
                    (x, conn) => conn.HousePersons.Insert(x) ? 1 : 0,
                    (_, _) => true,
                    (_, _) => hp,
                    _ => hp,
                    (conn, y) => conn.HousePersons.FindSame(x => x.OwnersId = y.OwnersId).Single(),
                    (conn, y) => conn.HousePersons.Delete(y)
                );
                sc.Persons.Delete(sc.Persons.Find(personId));
                sc.Houses.Delete(sc.Houses.Find(houseId));
                return;
            }
            if (cla == "u")
            {
                var profId = default(long);
                DoDatabase(sc, () => new Profile
                    {
                        Bio = "Some text.", Birthdate = new DateTime(1987, 2, 9), Image = [9, 3, 2]
                    },
                    (x, conn) => x.ProfileId = profId = conn.Profiles.Insert(x),
                    (x, conn) =>
                    {
                        x.Bio = "Harry's text.";
                        return conn.Profiles.Update(x);
                    },
                    (conn, id) => conn.Profiles.Find((long)id),
                    conn => conn.Profiles.List().Single(),
                    (conn, y) => conn.Profiles.FindSame(x => x.Bio = y.Bio).Single(),
                    (_, _) => true
                );
                DoDatabase(sc,
                    () => new User { Email = "tom@mail.com", UserName = "Tom", UserId = "1", ProfileId = profId },
                    (x, conn) => x.UserId = conn.Users.Insert(x),
                    (x, conn) =>
                    {
                        x.UserName = "Harry";
                        return conn.Users.Update(x);
                    },
                    (conn, id) => conn.Users.Find((string)id),
                    conn => conn.Users.List().Single(),
                    (conn, y) => conn.Users.FindSame(x => x.UserName = y.UserName).Single(),
                    (conn, y) => conn.Users.Delete(y)
                );
                sc.Profiles.Delete(sc.Profiles.Find(profId));
                return;
            }
            if (cla == "f")
            {
                DoDatabase(sc, () => new Funny
                    {
                        H = 42, P = "Tom", T = 289, A = true, B = 129, C = [3, 4, 22, 43],
                        E = DateOnly.FromDateTime(DateTime.Now), F = new DateTime(2011, 2, 3),
                        D = 't', G = DateTimeOffset.UnixEpoch, I = 39.2933, K = 2830, J = Guid.NewGuid(),
                        L = 292, N = -13, M = 39383, O = 99.99223f, U = 39399, S = 2911,
                        Q = TimeOnly.FromDateTime(DateTime.UtcNow), R = TimeSpan.FromMinutes(29)
                    },
                    (x, conn) => x.Id = conn.Funnies.Insert(x),
                    (x, conn) =>
                    {
                        x.P = "Harry";
                        return conn.Funnies.Update(x);
                    },
                    (conn, id) => conn.Funnies.Find((int)id),
                    conn => conn.Funnies.List().Single(),
                    (conn, y) => conn.Funnies.FindSame(x => x.P = y.P).Single(),
                    (conn, y) => conn.Funnies.Delete(y)
                );
            }
        }

        private static AdContext CreateConn(string cla, string mode)
        {
            var fileName = IoTool.DeleteIfExists($"blog_{cla}_{mode}.db");
            var conn = new AdContext(fileName);
            return conn;
        }

        private void DoDatabase<T>(AdContext conn, Func<T> create,
            Func<T, AdContext, object> getId, Func<T, AdContext, bool> getUpdate,
            Func<AdContext, object, T> find, Func<AdContext, T> list,
            Func<AdContext, T, T> same, Func<AdContext, T, bool> delete)
            where T : IAD
        {
            var input = create();

            var id = getId(input, conn);
            Assert.Equal(1 + "", id + "");

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
                House.CreateTable(), HousePerson.CreateTable(), Funny.CreateTable(),
                User.CreateTable(), Profile.CreateTable(),
                Blog.CreateTable(), Post.CreateTable());
            Assert.Equal(816, sql.Length);
            Assert.Equal(0, rows);

            return conn;
        }
    }
}