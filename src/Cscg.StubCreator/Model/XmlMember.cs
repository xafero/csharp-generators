using System.Collections.Generic;
using System.Xml.Serialization;

namespace Cscg.StubCreator.Model
{
    [XmlRoot("member")]
    public class XmlMember
    {
        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("summary")]
        public string Summary { get; set; }

        [XmlElement("param")]
        public List<XmlParam> Params { get; set; }

        [XmlElement("returns")]
        public string Returns { get; set; }

        [XmlElement("remarks")]
        public string Remarks { get; set; }

        [XmlElement("value")]
        public string Value { get; set; }
    }
}