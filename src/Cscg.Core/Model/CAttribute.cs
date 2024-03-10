using System.Collections.Generic;
using System.Xml.Serialization;

namespace Cscg.Core.Model
{
    [XmlRoot("Attribute")]
    public class CAttribute
    {
        [XmlAttribute] public string Type { get; set; }

        [XmlElement("Param")] public List<CParam> Params { get; set; }
    }
}