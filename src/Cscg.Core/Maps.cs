using System;
using System.Collections.Generic;
using System.Linq;

namespace Cscg.Core
{
    public static class Maps
    {
        public static IEnumerable<(string k, string v)> SplitMap(string text)
        {
            foreach (var item in text.Trim('{', '}')
                         .Split([", "], StringSplitOptions.None)
                         .Select(t => t.Split(['='], 2)))
                yield return (k: item[0].TrimStart('"'), v: item[1].TrimEnd('"'));
        }

        public static Dictionary<string, string> ToDict(this IEnumerable<(string k, string v)> items)
        {
            return items.ToDictionary(k => k.k, v => v.v);
        }
    }
}