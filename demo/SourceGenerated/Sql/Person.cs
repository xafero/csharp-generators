using System.Collections.Generic;
using Cscg.AdoNet.Lib;
using SourceGenerated.Sql;

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

    [Table]
    public partial class Blog
    {
        [Key]
        [Column]
        public int BlogId { get; set; }

        [Column]
        public string Url { get; set; }

        [Column]
        public int Rating { get; set; }

        [Column]
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

        [Column]
        public int BlogId { get; set; }

        [Column]
        public Blog Blog { get; set; }
    }
}