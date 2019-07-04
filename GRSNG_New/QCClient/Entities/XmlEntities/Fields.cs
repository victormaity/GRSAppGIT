using System.Collections.Generic;
using System.Xml.Serialization;

namespace QcClient.Entities.XmlEntities
{
    public class Fields
    {
        [XmlElement("Field")]
        public List<Field> Field { get; set; }
    }
}
