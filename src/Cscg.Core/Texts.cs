using System;
using System.Collections.Generic;

namespace Cscg.Core
{
    public static class Texts
    {
        public const string NewLine = "\n";

        public static string TrimNull(this string text)
        {
            return string.IsNullOrWhiteSpace(text) ? null : text.Trim();
        }

        public static string ToTitle(this string text)
        {
            return text.Substring(0, 1).ToUpper() + text.Substring(1);
        }

        public static string ToSnake(this string text)
        {
            return text.Substring(0, 1).ToLower() + text.Substring(1);
        }

        public static void ModifyLast(this IList<string> lines, Func<string, string> func)
        {
            var idx = lines.Count - 1;
            var oldLine = lines[idx];
            var newLine = func(oldLine);
            lines[idx] = newLine;
        }
    }
}