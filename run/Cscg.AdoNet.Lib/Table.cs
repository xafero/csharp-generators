namespace Cscg.AdoNet.Lib
{
    public sealed class Table
    {
        public string Name { get; }
        public Column[] Columns { get; }

        public Table(string name, Column[] columns)
        {
            Name = name;
            Columns = columns;
        }
    }
}