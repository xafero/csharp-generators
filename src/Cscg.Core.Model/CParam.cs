using System.Xml.Serialization;

namespace Cscg.Core.Model
{
    [XmlRoot("Param")]
    public class CParam
    {
        [XmlAttribute] public string Name { get; set; }

        [XmlAttribute] public string Value { get; set; }
    }
}