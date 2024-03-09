﻿namespace Cscg.AdoNet
{
    internal static class AdoSource
    {
        internal const string LibSpace = "Cscg.AdoNet.Lib";

        internal const string TableAttrName = "Table";
        internal const string ColAttrName = "Column";
        internal const string KeyAttrName = "Key";

        public static string BuildPlural(string name)
        {
            return $"{name}s";
        }
    }
}