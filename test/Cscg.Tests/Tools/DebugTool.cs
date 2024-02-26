using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using PeterO.Cbor;

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

        public static string ToCborJson(byte[] bytes)
        {
            var cbor = CBORObject.DecodeFromBytes(bytes);
            var json = cbor.ToJSONString();
            return json;
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