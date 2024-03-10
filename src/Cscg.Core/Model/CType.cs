using System.Collections.Generic;
using System.Xml.Serialization;

namespace Cscg.Core.Model
{
    [XmlRoot("Type")]
    public class CType
    {
        [XmlAttribute] public string Parent { get; set; }

        [XmlAttribute] public TypeKind Kind { get; set; }

        [XmlAttribute] public string Name { get; set; }

        [XmlElement("Member")] public List<CMember> Members { get; set; }
    }
}