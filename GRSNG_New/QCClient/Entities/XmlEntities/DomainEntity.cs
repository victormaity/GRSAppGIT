using System.Xml.Serialization;
using QcClient.Tools;

namespace QcClient.Entities.XmlEntities
{
    public class Domains : Serializer<Domains>
    {
        [XmlElement("Domain")]
        public DomainsDomain[] Domain { get; set; }
    }


    public class DomainsDomain
    {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlText]
        public string Value { get; set; }
    }
}