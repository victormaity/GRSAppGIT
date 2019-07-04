using System.Xml.Serialization;
using QcClient.Tools;

namespace QcClient.Entities.XmlEntities
{
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    [XmlRoot(Namespace="", IsNullable=false)]

    public class DesignStepEntities : Serializer<DesignStepEntities>
    {

        private DesignStepEntity[] _designStepEntity;
    
        private short _totalResultsField;
    
        private bool _totalResultsFieldSpecified;

        [XmlElement("DesignStepEntity")]
        public DesignStepEntity[] DesignStepEntity
        {
            get { return _designStepEntity; }
            set { _designStepEntity = value; }
        }
    
        [XmlAttribute]
        public short TotalResults
        {
            get { return _totalResultsField; }
            set { _totalResultsField = value; }
        }
    

        [XmlIgnore]
        public bool TotalResultsSpecified
        {
            get { return _totalResultsFieldSpecified; }
            set { _totalResultsFieldSpecified = value; }
        }
    }
}