using System.Collections.Generic;
using Cscg.AdoNet.Lib;
using SourceGenerated.Sql;

namespace SourceGenerated.Sql
{
    [Table(Name = "FuckYou")]
    public partial class Person
    {
        [Key] 
        [Column(Name = "ShitId")] 
        public int Id { get; set; }

        [Column] 
        public string Name { get; set; }
    }

    [Table(Name = "FuckYou2")]
    public partial class Blog
    {
        [Key]
        [Column(Name = "ShitId")]
        public int BlogId { get; set; }

        [Key]
        [Column(Name = "ShitId")]
        public string Url { get; set; }

        [Key]
        [Column(Name = "ShitId")]
        public int Rating { get; set; }

        [Key]
        [Column(Name = "ShitId")]
        public List<Post> Posts { get; set; }
    }

    [Table(Name = "FuckYou3")]
    public partial class Post
    {
        [Key]
        [Column(Name = "ShitId")]
        public int PostId { get; set; }

        [Key]
        [Column(Name = "ShitId")]
        public string Title { get; set; }

        [Key]
        [Column(Name = "ShitId")]
        public string Content { get; set; }

        [Key]
        [Column(Name = "ShitId")]
        public int BlogId { get; set; }

        [Key]
        [Column(Name = "ShitId")]
        public Blog Blog { get; set; }
    }
}