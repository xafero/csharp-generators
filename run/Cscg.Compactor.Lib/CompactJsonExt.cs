using System.IO;
using System.Text.Json;

namespace Cscg.Compactor.Lib
{
    public static class CompactJsonExt
    {
        public static void ReadJson(this ICompacted obj, Stream stream)
        {
            var array = stream.ToBytes();
            var reader = new Utf8JsonReader(array, new JsonReaderOptions
            {
                CommentHandling = JsonCommentHandling.Disallow
            });
            // TODO obj.ReadJson(ref reader);
        }

        public static void WriteJson(this ICompacted obj, Stream stream)
        {
            using var writer = new Utf8JsonWriter(stream, new JsonWriterOptions
            {
                Indented = true
            });
            // TODO obj.WriteJson(ref writer);
            writer.Flush();
            stream.Flush();
        }
    }
}