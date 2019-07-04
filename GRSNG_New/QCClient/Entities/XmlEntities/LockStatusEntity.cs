using System;
using QcClient.Tools;
using System.Xml.Serialization;

namespace QcClient.Entities.XmlEntities
{

    [XmlType(AnonymousType = true)]
    [XmlRoot(Namespace = "", IsNullable = false)]
    public class LockStatusEntity : Serializer<LockStatusEntity>
    {
        [XmlElement("LockStatus")]
        public String LockStatus { get; set; }

        [XmlElement("LockUser")]
        public String LockUser { get; set; }

        [XmlElement("LockedByMe")]
        public String LockedByMe { get; set; }
    }

    //  <LockStatusEntity>
    //      <LockStatus>LOCKED_BY_OTHER</LockStatus> 
    //      <LockUser>anna.grachova</LockUser> 
    //      <LockedByMe>false</LockedByMe> 
    //  </LockStatusEntity>
}