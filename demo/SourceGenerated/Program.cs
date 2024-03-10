using System;
using System.IO;
using System.Text;
using Cscg.AdoNet.Lib;
using SourceGenerated.Sql;
﻿using Microsoft.Data.Sqlite;

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

            var person = new Person { Name = "Hans Scott" };

            using var cmd1 = conn.CreateCommand();
            cmd1.CommandText = "INSERT INTO Persons (Name) VALUES (@p0) RETURNING Id;";
            cmd1.Parameters.AddWithValue("@p0", person.Name);
            var newId = cmd1.ExecuteScalar();
            Console.WriteLine(newId);

            person = new Person { Name = "Timmy Scott" };

            using var cmd2 = conn.CreateCommand();
            cmd2.CommandText = "UPDATE Persons SET Name = @p1 WHERE Id = @p0;";
            cmd2.Parameters.AddWithValue("@p0", newId);
            cmd2.Parameters.AddWithValue("@p1", person.Name);
            var updCount = cmd2.ExecuteNonQuery();
            Console.WriteLine(updCount == 1);

            using var cmd3 = conn.CreateCommand();
            cmd3.CommandText = "SELECT p.* FROM Persons p WHERE p.Id = @p0;";
            cmd3.Parameters.AddWithValue("@p0", newId);
            using var read3 = cmd3.ExecuteReader();
            Console.WriteLine(read3);

            var cmd4 = conn.CreateCommand();
            cmd4.CommandText = "SELECT p.* FROM Persons p ORDER BY p.Id LIMIT @p0 OFFSET @p1;";
            // cmd4.CommandText = "SELECT p.Id as p_Id, q.Id as q_Id FROM Persons p, Persons q";
            cmd4.Parameters.AddWithValue("@p0", 5);
            cmd4.Parameters.AddWithValue("@p1", 15);
            using var read4 = cmd4.ExecuteReader();
            Console.WriteLine(read4);

            while (read4.Read())
                for (var i = 0; i < read4.FieldCount; i++)
                {
                    var fieldKey = read4.GetName(i);
                    var fieldVal = read4.GetValue(i); // use specific if possible?
                    Console.WriteLine(fieldKey + " = " + fieldVal);
                    // TODO Generate ?!
                    // yield return new Person();
                }

            using var cmd5 = conn.CreateCommand();
            cmd5.CommandText = "DELETE FROM Persons WHERE Id = @p0;";
            cmd5.Parameters.AddWithValue("@p0", newId);
            var delCount = cmd5.ExecuteNonQuery();
            Console.WriteLine(delCount == 1);


            /*
               -- get all table names
               SELECT name FROM sqlite_master WHERE type=='table' ORDER BY name;

               -- get table columns
               SELECT 'Blogs' AS tname, * FROM pragma_table_info('Blogs')
             */

            /*
               users = User.all
               user = User.first
               david = User.find_by(name: 'David')
               users = User.where(name: 'David', occupation: 'Code Artist').order(created_at: :desc)               
             */

            /*
            SqliteDataReader d = cmd1.ExecuteReader();
            DbDataReader r = d;                    
            */
        }
    }
}