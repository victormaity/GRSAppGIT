using System;
using System.Xml.Serialization;
using QcClient.Tools;

namespace QcClient.Entities.XmlEntities
{
    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class CheckOutParameters : Serializer<CheckOutParameters>
    {
        [XmlElement("Comment")]
        public String Comment { get; set; }

        [XmlElement("Version")]
        public String Version { get; set; }
    }


    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class CheckInParameters : Serializer<CheckInParameters>
    {
        [XmlElement("Comment")]
        public String Comment { get; set; }

        [XmlElement("OverrideLastVersion")]
        public String OverrideLastVersion { get; set; }
    }
}