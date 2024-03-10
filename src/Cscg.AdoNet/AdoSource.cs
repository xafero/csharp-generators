using System.Collections.Generic;
using Cscg.Core;

namespace Cscg.AdoNet
{
    internal static class AdoSource
    {
        internal const string LibSpace = "Cscg.AdoNet.Lib";

        internal const string TableAttrName = "Table";
        internal const string MappingAttrName = "Mapping";
        internal const string ColAttrName = "Column";
        internal const string KeyAttrName = "Key";
        internal const string ForeignAttrName = "Foreign";

        public static string BuildPlural(string name)
        {
            if (name.EndsWith("y"))
                return $"{name.Substring(0, name.Length - 1)}ies";

            return $"{name}s";
        }

        // SELECT, INSERT, UPDATE, DELETE

        public static string GetTransaction(IEnumerable<string> inner)
        {
            var lines = new List<string> { "BEGIN TRANSACTION;" };
            foreach (var item in inner)
                lines.Add(item);
            lines.Add("COMMIT;");
            return string.Join(Texts.NewLine, lines);
        }
    }
}