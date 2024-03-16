using System.Xml.Serialization;

namespace Cscg.StubCreator.Model
{
    [XmlRoot("assembly")]
    public class XmlAssembly
    {
        [XmlElement("name")]
        public string Name { get; set; }
    }
}