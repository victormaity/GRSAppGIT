using System.Collections.Generic;
using QcClient.Tools;
using System.Xml.Serialization;

namespace QcClient.Entities.XmlEntities
{

    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Entities : Serializer<Entities>
    {
        [XmlElement("Entity")]
        public List<Entity> Entity { get; set; }

        [XmlAttribute]
        public int TotalResults { get; set; }
    }



    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class Entity : Serializer<Entity>
    {
        [XmlArrayItem("Field", IsNullable = false)]
        public List<EntitiesEntityField> Fields { get; set; }

        [XmlAttribute]
        public string Type { get; set; }
    }



    [XmlType(AnonymousType = true)]
    public class EntitiesEntityField : Serializer<Entity>
    {
        public string Value { get; set; }

        [XmlAttribute]
        public string Name { get; set; }
    }
}