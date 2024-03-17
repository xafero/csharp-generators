using System.IO;
using System.Text;

namespace Cscg.Compactor.Lib.Xml
{
    public static class XmlCompactor
    {
        public static byte[] SerializeObject<T>(T item) where T : IXmlCompacted
        {
            using var mem = new MemoryStream();
            item.WriteXml(mem);
            return mem.ToArray();
        }

        public static string SerializeObjectS<T>(T product) where T : IXmlCompacted
        {
            var bytes = SerializeObject(product);
            return Encoding.UTF8.GetString(bytes);
        }

        public static T DeserializeObject<T>(byte[] bytes) where T : IXmlCompacted, new()
        {
            var item = new T();
            using var mem = new MemoryStream(bytes);
            item.ReadXml(mem);
            return item;
        }

        public static T DeserializeObjectS<T>(string text) where T : IXmlCompacted, new()
        {
            var bytes = Encoding.UTF8.GetBytes(text);
            return DeserializeObject<T>(bytes);
        }
    }
}