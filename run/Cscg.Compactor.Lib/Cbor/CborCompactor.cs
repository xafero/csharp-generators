using System;
using System.IO;

namespace Cscg.Compactor.Lib.Cbor
{
    public static class CborCompactor
    {
        public static byte[] SerializeObject<T>(T item) where T : ICborCompacted
        {
            using var mem = new MemoryStream();
            item.WriteCbor(mem);
            return mem.ToArray();
        }

        public static string SerializeObjectS<T>(T product) where T : ICborCompacted
        {
            var bytes = SerializeObject(product);
            return Convert.ToBase64String(bytes);
        }

        public static T DeserializeObject<T>(byte[] bytes) where T : ICborCompacted, new()
        {
            var item = new T();
            using var mem = new MemoryStream(bytes);
            item.ReadCbor(mem);
            return item;
        }

        public static T DeserializeObjectS<T>(string text) where T : ICborCompacted, new()
        {
            var bytes = Convert.FromBase64String(text);
            return DeserializeObject<T>(bytes);
        }
    }
}