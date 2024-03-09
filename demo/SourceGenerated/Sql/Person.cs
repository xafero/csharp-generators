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
}