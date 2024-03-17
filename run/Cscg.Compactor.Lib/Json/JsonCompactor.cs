using System.IO;
using System.Text;

namespace Cscg.Compactor.Lib.Json
{
    public static class JsonCompactor
    {
        public static byte[] SerializeObject<T>(T item) where T : IJsonCompacted
        {
            using var mem = new MemoryStream();
            item.WriteJson(mem);
            return mem.ToArray();
        }

        public static string SerializeObjectS<T>(T product) where T : IJsonCompacted
        {
            var bytes = SerializeObject(product);
            return Encoding.UTF8.GetString(bytes);
        }

        public static T DeserializeObject<T>(byte[] bytes) where T : IJsonCompacted, new()
        {
            var item = new T();
            using var mem = new MemoryStream(bytes);
            item.ReadJson(mem);
            return item;
        }

        public static T DeserializeObjectS<T>(string text) where T : IJsonCompacted, new()
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            return DeserializeObject<T>(bytes);
        }
    }
}