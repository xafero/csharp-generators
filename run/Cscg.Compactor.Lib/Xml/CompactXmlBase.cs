using System.IO;
using System.Xml;

namespace Cscg.Compactor.Lib.Xml
{
    public static class CompactXmlBase
    {
        public static void ReadXml(this IXmlCompacted obj, Stream stream)
        {
            var reader = XmlReader.Create(stream, new XmlReaderSettings
            {
                CloseInput = false, ConformanceLevel = ConformanceLevel.Auto
            });
            obj.ReadXml(ref reader);
            reader.Dispose();
        }

        public static void WriteXml(this IXmlCompacted obj, Stream stream)
        {
            var writer = XmlWriter.Create(stream, new XmlWriterSettings
            {
                CloseOutput = false, ConformanceLevel = ConformanceLevel.Auto,
                WriteEndDocumentOnClose = true
            });
            obj.WriteXml(ref writer);
            writer.Flush();
            stream.Flush();
            writer.Dispose();
        }
    }
}