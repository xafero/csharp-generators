namespace Cscg.AdoNet.Lib
{
    public sealed class Column
    {
        public string Name { get; }
        public bool IsPrimaryKey { get; }
        public string ForeignTable { get; }
        public string ForeignColumn { get; }

        public Column(string name, bool primaryKey, string foreignTable, string foreignCol)
        {
            Name = name;
            IsPrimaryKey = primaryKey;
            ForeignTable = foreignTable;
            ForeignColumn = foreignCol;
        }
    }
}