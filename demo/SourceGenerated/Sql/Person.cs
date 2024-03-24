using System;
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
    }

    [Table(Name = "House")]
    public partial class House
    {
        [Key]
        [Column]
        public int MyId { get; set; }

        [Column]
        public string Street { get; set; }
    }

    [Mapping]
    public partial class HousePerson
    {
        [Foreign(Table = "House", Column = nameof(House.MyId))]
        [Column]
        public int HousesMyId { get; set; }

        [Foreign(Table = "Persons", Column = nameof(Person.Id))]
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

        [Include]
        public Profile Profile { get; set; }

        [Column]
        public string UserName { get; set; }

        [Column]
        public string Email { get; set; }
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
    }

    [Table]
    public partial class Funny
    {
        [Key] [Column] public int Id { get; set; }
        [Column] public bool A { get; set; }
        [Column] public byte B { get; set; }
        [Column] public byte[] C { get; set; }
        [Column] public char D { get; set; }
        [Column] public DateOnly E { get; set; }
        [Column] public DateTime F { get; set; }
        [Column] public DateTimeOffset G { get; set; }
        [Column] public decimal H { get; set; }
        [Column] public double I { get; set; }
        [Column] public Guid J { get; set; }
        [Column] public short K { get; set; }
        [Column] public int L { get; set; }
        [Column] public long M { get; set; }
        [Column] public sbyte N { get; set; }
        [Column] public float O { get; set; }
        [Column] public string P { get; set; }
        [Column] public TimeOnly Q { get; set; }
        [Column] public TimeSpan R { get; set; }
        [Column] public ushort S { get; set; }
        [Column] public uint T { get; set; }
        [Column] public ulong U { get; set; }
    }
}