using System;
using System.IO;

namespace Cscg.Compactor.Lib.Binary
{
    public static class BinaryCompactor
    {
        public static byte[] SerializeObject<T>(T item) where T : IBinCompacted
        {
            using var mem = new MemoryStream();
            item.WriteBinary(mem);
            return mem.ToArray();
        }

        public static string SerializeObjectS<T>(T product) where T : IBinCompacted
        {
            var bytes = SerializeObject(product);
            return Convert.ToBase64String(bytes);
        }

        public static T DeserializeObject<T>(byte[] bytes) where T : IBinCompacted, new()
        {
            var item = new T();
            using var mem = new MemoryStream(bytes);
            item.ReadBinary(mem);
            return item;
        }

        public static T DeserializeObjectS<T>(string text) where T : IBinCompacted, new()
        {
            var bytes = Convert.FromBase64String(text);
            return DeserializeObject<T>(bytes);
        }
    }
}