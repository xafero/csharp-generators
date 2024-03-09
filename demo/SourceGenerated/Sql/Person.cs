using System;
using System.Collections.Generic;
using Cscg.AdoNet.Lib;

namespace SourceGenerated.Sql
{
    [Table]
    public partial class Person
    {
        [Key] 
        [Column] 
        public int Id { get; set; }

        [Column] 
        public string Name { get; set; }

        public List<House> Houses { get; set; }
    }

    [Table(Name = "House")]
    public partial class House
    {
        [Key]
        [Column]
        public int MyId { get; set; }

        [Column]
        public string Street { get; set; }

        public List<Person> Owners { get; set; }
    }

    [Mapping]
    public partial class HousePerson
    {
        [Foreign(Table = "House", Column = "MyId")]
        [Column]
        public int HousesMyId { get; set; }

        [Foreign(Table = "Persons", Column = "Id")]
        [Column]
        public int OwnersId { get; set; }
    }

    [Table]
    public partial class Blog
    {
        [Key]
        [Column]
        public int MyBlogId { get; set; }

        [Column]
        public string Url { get; set; }

        [Column]
        public int Rating { get; set; }

        public List<Post> Posts { get; set; }
    }

    [Table]
    public partial class Post
    {
        [Key]
        [Column]
        public int PostId { get; set; }

        [Column]
        public string Title { get; set; }

        [Column]
        public string Content { get; set; }

        [Foreign(Table = "Blogs", Column = nameof(Blog.MyBlogId))]
        [Column]
        public int BlogId { get; set; }

        public Blog Blog { get; set; }
    }

    [Table]
    public partial class User
    {
        [Key]
        [Column]
        public string UserId { get; set; }

        [Foreign(Table = "Profile", Column = nameof(Profile.ProfileId), Unique = true)]
        [Column]
        public long ProfileId { get; set; }

        [Column]
        public string UserName { get; set; }

        [Column]
        public string Email { get; set; }

        public Profile Profile { get; set; }
    }

    [Table(Name = "Profile")]
    public partial class Profile
    {
        [Key]
        [Column]
        public long ProfileId { get; set; }

        [Column]
        public DateTime Birthdate { get; set; }

        [Column]
        public byte[] Image { get; set; }

        [Column]
        public string Bio { get; set; }

        public User User { get; set; }
    }
}