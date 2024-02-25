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
    }
}