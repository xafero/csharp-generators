using System;
using System.IO;
using System.Linq;
using System.Text;
using Cscg.AdoNet.Lib;
using SourceGenerated.Sql;
﻿using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace SourceGenerated
{
    public static partial class Program
    {
        public static void Main()
        {
            // MainCg();
            // MainBg();

            const string sqlFile = "example.db";

            var connStr = AdoTool.CreateConnStr<SqliteConnectionStringBuilder>(
                s => s.DataSource = sqlFile,
                s => s.Mode = SqliteOpenMode.ReadWriteCreate,
                s => s.Pooling = true,
                s => s.ForeignKeys = true,
                s => s.RecursiveTriggers = true
            );
            using var conn = AdoTool.OpenConn<SqliteConnection>(connStr);
            Console.WriteLine(connStr + " / " + conn);

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

            using var cmd1 = conn.CreateCommand();
            person.WriteSql(cmd1);
            cmd1.CommandText = cmd1.GetColumns().CreateInsert("Persons", "Id");
            var newId = cmd1.ExecuteScalar();
            Console.WriteLine(newId);

            person = new Person { Name = "Timmy Scott" };

            using var cmd2 = conn.CreateCommand();
            cmd2.CommandText = "UPDATE Persons SET Name = @pName WHERE Id = @p0;";
            cmd2.Parameters.AddWithValue("@p0", newId);
            person.WriteSql(cmd2);
            var updCount = cmd2.ExecuteNonQuery();
            Console.WriteLine(updCount == 1);

            using var cmd3 = conn.CreateCommand();
            cmd3.CommandText = "SELECT p.* FROM Persons p WHERE p.Id = @p0;";
            cmd3.Parameters.AddWithValue("@p0", newId);
            using var read3 = cmd3.ExecuteReader();
            Console.WriteLine(read3);

            person = read3.ReadData<Person, SqliteDataReader>().Single();
            var json = JsonConvert.SerializeObject(person, Formatting.None);
            Console.WriteLine(json);

            var cmd4 = conn.CreateCommand();
            cmd4.CommandText = "SELECT p.* FROM Persons p ORDER BY p.Id LIMIT @p0 OFFSET @p1;";
            cmd4.Parameters.AddWithValue("@p0", 5);
            cmd4.Parameters.AddWithValue("@p1", 0);
            using var read4 = cmd4.ExecuteReader();
            Console.WriteLine(read4);

            var persons = read4.ReadData<Person, SqliteDataReader>().ToArray();
            json = JsonConvert.SerializeObject(persons, Formatting.None);
            Console.WriteLine(json);

            var wasDeleted = person.Delete(conn);
            Console.WriteLine($"Deleted? {wasDeleted}");

            var tableNames = conn.GetAllTableNames().Take(1).ToArray();
            var tblInfo = tableNames.Select(conn.GetTableColumns).ToArray();
            json = JsonConvert.SerializeObject(tblInfo, Formatting.None);
            Console.WriteLine(json);

            /*
               users = User.all
               user = User.first
               david = User.find_by(name: 'David')
               users = User.where(name: 'David', occupation: 'Code Artist').order(created_at: :desc)
             */
        }
    }
}