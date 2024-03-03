using System.IO;
using System.Text.Json;
using Cscg.Compactor.Lib.Tools;

namespace Cscg.Compactor.Lib.Json
{
    public static class CompactJsonBase
    {
        public static void ReadJson(this IJsonCompacted obj, Stream stream)
        {
            var array = stream.ToBytes();
            var reader = new Utf8JsonReader(array, new JsonReaderOptions
            {
                CommentHandling = JsonCommentHandling.Disallow
            });
            obj.ReadJson(ref reader);
        }

        public static void WriteJson(this IJsonCompacted obj, Stream stream)
        {
            var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Indented = false
            });
            obj.WriteJson(ref writer);
            writer.Flush();
            stream.Flush();
            writer.Dispose();
        }
    }
}