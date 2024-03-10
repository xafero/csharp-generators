using System.Xml.Serialization;

namespace Cscg.Core.Model
{
    [XmlRoot("Member")]
    public class CMember
    {
        [XmlAttribute] public MemberKind Kind { get; set; }

        [XmlAttribute] public string Name { get; set; }

        [XmlAttribute] public string Type { get; set; }
    }
}