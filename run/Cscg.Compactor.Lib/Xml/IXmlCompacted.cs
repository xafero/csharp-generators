using System.Xml;

namespace Cscg.Compactor.Lib.Xml
{
    public interface IXmlCompacted
    {
        void WriteXml(ref XmlWriter w);

        void ReadXml(ref XmlReader r);
    }
}