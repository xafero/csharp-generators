using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Cscg.Core.Model
{
    public static class XmlTool
    {
        private static readonly XmlSerializer Serializer = new(typeof(CType));

        public static CType Read(TextReader input)
        {
            var reader = XmlReader.Create(input, new()
            {
                IgnoreWhitespace = false,
                DtdProcessing = DtdProcessing.Ignore
            });
            var value = Serializer.Deserialize(reader);
            return (CType)value!;
        }

        public static void Write(CType value, TextWriter output)
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