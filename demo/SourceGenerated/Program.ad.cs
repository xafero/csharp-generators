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
            const string sqlFile = "example.db";

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
        }
    }
}