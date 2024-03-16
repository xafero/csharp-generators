using System.Collections.Generic;
using System.Xml.Serialization;

namespace Cscg.StubCreator.Model
{
    [XmlRoot("doc")]
    public class XmlDoc
    {
        [XmlElement("assembly")]
        public XmlAssembly Assembly { get; set; }

        [XmlArray("members")]
        [XmlArrayItem("member")]
        public List<XmlMember> Members { get; set; }
    }
}