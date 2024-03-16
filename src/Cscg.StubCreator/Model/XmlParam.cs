using System.Xml.Serialization;

namespace Cscg.StubCreator.Model
{
    [XmlRoot("param")]
    public class XmlParam
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlText]
        public string Description { get; set; }
    }
}