using System.IO;
using System.Xml;

namespace Cscg.Compactor.Lib
{
    public static class CompactXmlExt
    {
        public static void ReadXml(this ICompacted obj, Stream stream)
        {
            using var reader = XmlReader.Create(stream, new XmlReaderSettings
            {
                CloseInput = false
            });
            // TODO obj.ReadXml(ref reader);
        }

        public static void WriteXml(this ICompacted obj, Stream stream)
        {
            using var writer = XmlWriter.Create(stream, new XmlWriterSettings
            {
                CloseOutput = false
            });
            // TODO obj.WriteXml(ref writer);
            writer.Flush();
            stream.Flush();
        }
    }
}