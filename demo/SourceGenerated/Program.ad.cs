using System;
using System.IO;
using System.Linq;
using System.Text;
using Cscg.AdoNet.Lib;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using SourceGenerated.Sql;

namespace SourceGenerated
{
    public static partial class Program
    {
        public static void MainAd()
        {
            var sqlFile = IoTool.DeleteIfExists("example.db");

            var connStr = AdoTool.CreateConnStr<SqliteConnectionStringBuilder>(
                s => s.DataSource = sqlFile,
                s => s.Mode = SqliteOpenMode.ReadWriteCreate,
                s => s.Pooling = true,
                s => s.ForeignKeys = true,
                s => s.RecursiveTriggers = true
            );
            using var conn = AdoTool.OpenConn<SqliteConnection>(connStr);
            Console.WriteLine($"{connStr} / {conn.State}");

            var (sql, rows) = conn.RunTransaction(
                Blog.CreateTable(),
                Post.CreateTable(),
                Funny.CreateTable(),
                House.CreateTable(),
                Person.CreateTable(),
                HousePerson.CreateTable(),
                Profile.CreateTable(),
                User.CreateTable()
            );
            File.WriteAllText("test.sql", sql, Encoding.UTF8);
            Console.WriteLine(rows);

            var person = new Person { Name = "Willy Scott" };
            person.Id = person.Insert(conn);
            Console.WriteLine(person.Id);

            person.Name = "Timmy Scott";
            var wasUpdated = person.Update(conn);
            Console.WriteLine(wasUpdated);

            person = Person.Find(conn, person.Id);
            var json = JsonConvert.SerializeObject(person, Formatting.None);
            Console.WriteLine(json);

            var persons = Person.List(conn, 5, 0);
            json = JsonConvert.SerializeObject(persons, Formatting.None);
            Console.WriteLine(json);

            var house = new House { Street = "Main Street 123" };
            house.MyId = house.Insert(conn);
            Console.WriteLine(house.MyId);

            var hp = new HousePerson { HousesMyId = house.MyId, OwnersId = person.Id };
            var wasInserted = hp.Insert(conn);
            Console.WriteLine(wasInserted);

            var hPerson = HousePerson.FindSame(conn, p => p.OwnersId = 88).SingleOrDefault();
            json = JsonConvert.SerializeObject(hPerson, Formatting.None);
            Console.WriteLine(json);

            var wasDeleted = hPerson?.Delete(conn);
            Console.WriteLine($"Deleted? {wasDeleted}");

            wasDeleted = person.Delete(conn);
            Console.WriteLine($"Deleted? {wasDeleted}");

            var dbVersion = conn.GetDatabaseVersion();
            Console.WriteLine(dbVersion);

            var tableNames = conn.GetAllTableNames().Take(1).ToArray();
            var tblInfo = tableNames.Select(conn.GetTableColumns).ToArray();
            json = JsonConvert.SerializeObject(tblInfo, Formatting.None);
            Console.WriteLine(json);

            var blog1 = new Blog { Rating = 5, Url = "www.google.com" };
            blog1.MyBlogId = blog1.Insert(conn);
            Console.WriteLine(blog1.MyBlogId);
            Console.WriteLine(Blog.List(conn, 10, 0).Length);

            var funny1 = new Funny
            {
                A = true, B = 234, C = [9, 3, 2], D = 'z', E = DateOnly.FromDateTime(DateTime.Now),
                F = DateTime.UtcNow, G = DateTimeOffset.Now, H = 392.22m, I = 42.2113,
                J = Guid.NewGuid(), K = 22, L = 29202, M = 230902, N = -112, O = 9202.292892f,
                P = "Some text!", Q = TimeOnly.FromDateTime(DateTime.Now), R = TimeSpan.FromDays(39),
                S = 199, T = 33394, U = 32939022
            };
            funny1.Id = funny1.Insert(conn);
            Console.WriteLine(funny1.Id);
            Console.WriteLine(Funny.List(conn, 10, 0).Length);

            var person2 = new Person { Name = "Tim" };
            person2.Id = person2.Insert(conn);
            Console.WriteLine(person2.Id);
            Console.WriteLine(Person.List(conn, 10, 0).Length);

            var house2 = new House { Street = "Super Way 43" };
            house2.MyId = house2.Insert(conn);
            Console.WriteLine(house2.MyId);
            Console.WriteLine(House.List(conn, 10, 0).Length);

            var hp2 = new HousePerson { HousesMyId = house2.MyId, OwnersId = person2.Id };
            hp2.Insert(conn);

            var post2 = new Post
            {
                Title = "Good title", Content = "Everything is well and right.", BlogId = blog1.MyBlogId
            };
            post2.PostId = post2.Insert(conn);
            Console.WriteLine(post2.PostId);
            Console.WriteLine(Post.List(conn, 10, 0).Length);

            var prof2 = new Profile
            {
                Bio = "Good CV or not", Birthdate = DateTime.Now, Image = [2, 38, 92, 34]
            };
            prof2.ProfileId = prof2.Insert(conn);
            Console.WriteLine(prof2.ProfileId);
            Console.WriteLine(Profile.List(conn, 10, 0).Length);

            var user2 = new User
            {
                UserName = "Testo Macko", Email = "test@mail.com.uk",
                ProfileId = prof2.ProfileId, UserId = "testy"
            };
            user2.UserId = user2.Insert(conn);
            Console.WriteLine(user2.UserId);
            Console.WriteLine(User.List(conn, 10, 0).Length);
        }
    }
}