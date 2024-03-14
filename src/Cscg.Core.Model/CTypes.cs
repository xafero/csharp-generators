using System.Collections.Generic;
using System.Xml.Serialization;

namespace Cscg.Core.Model
{
    [XmlRoot("Types")]
    public class CTypes
    {
        [XmlElement("Type")] public List<CType> Types { get; set; }
    }
}