using System.Collections.Generic;
using System.Linq;
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
                case "double":
                case "float":
                    res = "REAL";
                    canNull = false;
                    break;
                case "ulong":
                case "long":
                case "uint":
                case "int":
                case "ushort":
                case "short":
                case "sbyte":
                case "byte":
                case "bool":
                    res = "INTEGER";
                    canNull = false;
                    if (tblKey != null)
                        cond += $"CONSTRAINT \"{tblKey}\" PRIMARY KEY AUTOINCREMENT";
                    break;
                case "System.DateTimeOffset":
                case "System.DateTime":
                case "System.DateOnly":
                case "System.TimeSpan":
                case "System.TimeOnly":
                case "System.Guid":
                case "decimal":
                case "char":
                    res = "TEXT";
                    canNull = false;
                    break;
                case "string":
                    res = "TEXT";
                    if (tblKey != null)
                    {
                        cond += $"CONSTRAINT \"{tblKey}\" PRIMARY KEY";
                        canNull = false;
                    }
                    break;
                case "byte[]":
                    res = "BLOB";
                    break;
                default:
                    res = $"!?{text}?!";
                    break;
            }
            cond = $"{(canNull ? "NULL" : "NOT NULL")} {cond}";
            return (res, cond.Trim());
        }

        public static (string[] i, string[] o) GetForeign(string table, string prop,
            string ds, string dp, bool unique)
        {
            ds = ds.Trim('"');
            var cstr = $"CONSTRAINT \"\"FK_{table}_{ds}_{prop}\"\" " +
                       $"FOREIGN KEY (\"\"{prop}\"\") REFERENCES" +
                       $" \"\"{ds}\"\" (\"{dp}\") ON DELETE CASCADE";
            cstr = "@\"    " + cstr + ",\",";
            var index = $"IX_{table}_{prop}";
            const string s = " IF NOT EXISTS";
            var mod = " ";
            if (unique)
                mod += "UNIQUE ";
            var ix = $"CREATE{mod}INDEX{s} \"\"{index}\"\" ON \"\"{table}\"\" (\"\"{prop}\"\");";
            ix = "@\"" + ix + "\",";
            return ([cstr], [ix]);
        }

        public static string GetMapKey(string table, IEnumerable<string> keys)
        {
            var props = string.Join(", ", keys.Select(k => $"\"{Quote(k)}\""));
            var cstr = $"CONSTRAINT \"\"PK_{table}\"\" " +
                       $"PRIMARY KEY ({props})";
            cstr = "@\"    " + cstr + ",\",";
            return cstr;
        }

        public static string GetRead(ITypeSymbol type)
        {
            var text = type.ToTrimDisplay();
            string res;
            switch (text)
            {
                // Native functions
                case "bool": res = "r.GetBoolean(i)"; break;
                case "byte": res = "r.GetByte(i)"; break;
                case "char": res = "r.GetChar(i)"; break;
                case "short": res = "r.GetInt16(i)"; break;
                case "int": res = "r.GetInt32(i)"; break;
                case "long": res = "r.GetInt64(i)"; break;
                case "float": res = "r.GetFloat(i)"; break;
                case "double": res = "r.GetDouble(i)"; break;
                case "string": res = "r.GetString(i)"; break;
                case "decimal": res = "r.GetDecimal(i)"; break;
                case "System.Guid": res = "r.GetGuid(i)"; break;
                case "System.DateTime": res = "r.GetDateTime(i)"; break;
                case "System.DateTimeOffset": res = "r.GetDateTimeOffset(i)"; break;
                case "System.TimeSpan": res = "r.GetTimeSpan(i)"; break;
                // Simple casts
                case "sbyte": res = "(sbyte)r.GetByte(i)"; break;
                case "ushort": res = "(ushort)r.GetInt16(i)"; break;
                case "uint": res = "(uint)r.GetInt32(i)"; break;
                case "ulong": res = "(ulong)r.GetInt64(i)"; break;
                // Non-duplicated code
                case "byte[]": res = "r.GetFieldValue<byte[]>(i)"; break;
                case "System.TimeOnly": res = "r.GetFieldValue<TimeOnly>(i)"; break;
                case "System.DateOnly": res = "r.GetFieldValue<DateOnly>(i)"; break;
                // Nothing fits...
                default: res = $"TODO({text})"; break;
            }
            return res;
        }
    }
}