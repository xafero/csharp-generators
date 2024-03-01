using System.IO;
using System.Xml;

namespace Cscg.Compactor.Lib
{
    public static class CompactXmlBase
    {
        public static void ReadXml(this ICompacted obj, Stream stream)
        {
            var reader = XmlReader.Create(stream, new XmlReaderSettings
            {
                CloseInput = false
            });
            obj.ReadXml(ref reader);
            reader.Dispose();
        }

        public static void WriteXml(this ICompacted obj, Stream stream)
        {
            var writer = XmlWriter.Create(stream, new XmlWriterSettings
            {
                CloseOutput = false
            });
            obj.WriteXml(ref writer);
            writer.Flush();
            stream.Flush();
            writer.Dispose();
        }
    }
}