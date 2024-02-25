using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Cscg.Tests.Tools
{
    public static class DebugTool
    {
        public static string ToJson(object obj)
        {
            return JsonConvert.SerializeObject(obj, new JsonSerializerSettings
            {
                Formatting = Formatting.None, Converters = { new StringEnumConverter() }
            });
        }

        public static IDictionary<string, object> ToDict(Type type)
        {
            var dict = new SortedDictionary<string, object>();
            foreach (var field in type.GetFields())
            {
                var key = field.Name;
                var val = field.GetValue(null);
                dict[key] = val;
            }
            return dict;
        }
    }
}