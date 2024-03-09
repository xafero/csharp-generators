using Cscg.Core;
using Microsoft.CodeAnalysis;

namespace Cscg.AdoNet
{
    public sealed class SqliteSource
    {
        public static string Quote(string text)
        {
            return text.StartsWith("\"") ? text : $"\"{text}\"";
        }

        public static (string t, string c) GetType(ITypeSymbol type, string tblKey)
        {
            var text = type.ToTrimDisplay();
            string res;
            var cond = string.Empty;
            var canNull = true;
            switch (text)
            {
                case "int":
                    res = "INTEGER";
                    canNull = false;
                    if (tblKey != null)
                        cond += $"CONSTRAINT \"{tblKey}\" PRIMARY KEY AUTOINCREMENT";
                    break;
                case "string":
                    res = "TEXT";
                    break;
                default:
                    res = "!TODO!";
                    break;
            }
            cond = $"{(canNull ? "NULL" : "NOT NULL")} {cond}";
            return (res, cond.Trim());
        }

        public static (string[] i, string[] o) GetForeign(
            string table, string prop, string ds, string dp)
        {
            ds = ds.Trim('"');
            var cstr = $"CONSTRAINT \"\"FK_{table}_{ds}_{prop}\"\" " +
                       $"FOREIGN KEY (\"\"{prop}\"\") REFERENCES" +
                       $" \"\"{ds}\"\" (\"{dp}\") ON DELETE CASCADE";
            cstr = "@\"    " + cstr + ",\",";
            var index = $"IX_{table}_{prop}";
            var ix = $"CREATE INDEX \"\"{index}\"\" ON \"\"{table}\"\" (\"\"{prop}\"\");";
            ix = "@\"" + ix + "\"";
            return ([cstr], [ix]);
        }
    }
}