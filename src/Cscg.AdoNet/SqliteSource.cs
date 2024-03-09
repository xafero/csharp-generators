using Cscg.Core;
using Microsoft.CodeAnalysis;

namespace Cscg.AdoNet
{
    public sealed class SqliteSource
    {
        public static string Quote(string text)
        {
            return $"\"{text}\"";
        }

        public static (string t, string c) GetType(ITypeSymbol type)
        {
            var text = type.ToTrimDisplay();
            string res;
            var cond = default(string);
            var canNull = true;
            switch (text)
            {
                case "int":
                    res = "INTEGER";
                    canNull = false;
                    break;
                case "string":
                    res = "TEXT";
                    break;
                default:
                    res = "!TODO!";
                    break;
            }
            cond += canNull ? "NULL" : "NOT NULL";
            return (res, cond);
        }
    }
}