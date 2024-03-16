using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Cscg.Core
{
    public static class XmlTool<T>
    {
        private static readonly XmlSerializer Serializer = new(typeof(T));

        public static T Read(TextReader input)
        {
            var reader = XmlReader.Create(input, new()
            {
                IgnoreWhitespace = false,
                DtdProcessing = DtdProcessing.Ignore
            });
            var value = Serializer.Deserialize(reader);
            return (T)value!;
        }

        public static void Write(T value, TextWriter output)
        {
            var writer = XmlWriter.Create(output, new()
            {
                OmitXmlDeclaration = true,
                Encoding = Encoding.UTF8,
                Indent = true
            });
            Serializer.Serialize(writer, value);
        }
    }
}