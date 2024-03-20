using System;
using System.Linq;
using Cscg.AdoNet.Lib;
using Newtonsoft.Json;
using SourceGenerated.Sql;

namespace SourceGenerated.Sql
{
    [Context]
    public partial class AdContext
    {
        private DbSet<Blog> _blogs;
        private DbSet<Post> _posts;
        private DbSet<Funny> _funnies;
        private DbSet<House> _houses;
        private DbSet<Person> _persons;
        private DbSet<HousePerson> _housePersons;
        private DbSet<Profile> _profiles;
        private DbSet<User> _users;
    }
}

namespace SourceGenerated
{
    public static partial class Program
    {
        public static void MainAd()
        {
            var sqlFile = IoTool.DeleteIfExists("example.db");
            using var ctx = new AdContext();

            var person = new Person { Name = "Willy Scott" };
            Console.WriteLine(person.Id);

            person.Id = ctx.Persons.Insert(person);
            Console.WriteLine(person.Id);

            person.Name = "Timmy Scott";
            var wasUpdated = ctx.Persons.Update(person);
            Console.WriteLine(wasUpdated);

            person = ctx.Persons.Find(person.Id);
            var json = JsonConvert.SerializeObject(person, Formatting.None);
            Console.WriteLine(json);

            var persons = ctx.Persons.List(5, 0);
            json = JsonConvert.SerializeObject(persons, Formatting.None);
            Console.WriteLine(json);

            var house = new House { Street = "Main Street 123" };
            Console.WriteLine(house.MyId);

            house.MyId = ctx.Houses.Insert(house);
            Console.WriteLine(house.MyId);

            var hp = new HousePerson { HousesMyId = house.MyId, OwnersId = person.Id };
            var wasInserted = ctx.HousePersons.Insert(hp);
            Console.WriteLine(wasInserted);

            var hPerson = ctx.HousePersons.FindSame(p => p.OwnersId = 88).SingleOrDefault();
            json = JsonConvert.SerializeObject(hPerson, Formatting.None);
            Console.WriteLine(json);

            var wasDeleted = hPerson != null && ctx.HousePersons.Delete(hPerson);
            Console.WriteLine($"Deleted? {wasDeleted}");

            wasDeleted = ctx.Persons.Delete(person);
            Console.WriteLine($"Deleted? {wasDeleted}");

            var conn = ctx.GetOpenConn();
            var dbVersion = conn.GetDatabaseVersion();
            Console.WriteLine(dbVersion);

            var tableNames = conn.GetAllTableNames().Take(1).ToArray();
            var tblInfo = tableNames.Select(conn.GetTableColumns).ToArray();
            json = JsonConvert.SerializeObject(tblInfo, Formatting.None);
            Console.WriteLine(json);

            var blog1 = new Blog { Rating = 5, Url = "www.google.com" };
            Console.WriteLine(blog1.MyBlogId);

            blog1.MyBlogId = ctx.Blogs.Insert(blog1);
            Console.WriteLine(blog1.MyBlogId);
            Console.WriteLine(ctx.Blogs.List().Length);

            var funny1 = new Funny
            {
                A = true, B = 234, C = [9, 3, 2], D = 'z', E = DateOnly.FromDateTime(DateTime.Now),
                F = DateTime.UtcNow, G = DateTimeOffset.Now, H = 392.22m, I = 42.2113,
                J = Guid.NewGuid(), K = 22, L = 29202, M = 230902, N = -112, O = 9202.292892f,
                P = "Some text!", Q = TimeOnly.FromDateTime(DateTime.Now), R = TimeSpan.FromDays(39),
                S = 199, T = 33394, U = 32939022
            };
            funny1.Id = ctx.Funnies.Insert(funny1);
            Console.WriteLine(funny1.Id);
            Console.WriteLine(ctx.Funnies.List().Length);

            var person2 = new Person { Name = "Tim" };
            person2.Id = ctx.Persons.Insert(person2);
            Console.WriteLine(person2.Id);
            Console.WriteLine(ctx.Persons.List().Length);

            var house2 = new House { Street = "Super Way 43" };
            house2.MyId = ctx.Houses.Insert(house2);
            Console.WriteLine(house2.MyId);
            Console.WriteLine(ctx.Houses.List().Length);

            var hp2 = new HousePerson { HousesMyId = house2.MyId, OwnersId = person2.Id };
            ctx.HousePersons.Insert(hp2);

            var post2 = new Post
            {
                Title = "Good title", Content = "Everything is well and right.", BlogId = blog1.MyBlogId
            };
            post2.PostId = ctx.Posts.Insert(post2);
            Console.WriteLine(post2.PostId);
            Console.WriteLine(ctx.Posts.List().Length);

            var prof2 = new Profile
            {
                Bio = "Good CV or not", Birthdate = DateTime.Now, Image = [2, 38, 92, 34]
            };
            prof2.ProfileId = ctx.Profiles.Insert(prof2);
            Console.WriteLine(prof2.ProfileId);
            Console.WriteLine(ctx.Profiles.List().Length);

            var user2 = new User
            {
                UserName = "Testo Macko", Email = "test@mail.com.uk",
                ProfileId = prof2.ProfileId, UserId = "testy"
            };
            user2.UserId = ctx.Users.Insert(user2);
            Console.WriteLine(user2.UserId);
            Console.WriteLine(ctx.Users.List().Length);
        }
    }
}