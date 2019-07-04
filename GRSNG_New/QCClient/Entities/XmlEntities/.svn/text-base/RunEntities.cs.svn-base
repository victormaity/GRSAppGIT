using System.Xml.Serialization;
using QcClient.Tools;

namespace QcClient.Entities.XmlEntities
{
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    [XmlRoot(Namespace="", IsNullable=false)]

    public class RunEntities : Serializer<RunEntities> {

        private RunEntity[] _runEntityField;
    
        private string[] _textField;
    
        private int _totalResultsField;
    
        private bool _totalResultsFieldSpecified;

        [XmlElement("RunEntity")]
        public RunEntity[] RunEntity
        {
            get { return _runEntityField; }
            set { _runEntityField = value; }
        }
    
        [XmlText]
        public string[] Text {
            get { return _textField; }
            set { _textField = value; }
        }
    
        [XmlAttribute]
        public int TotalResults {
            get { return _totalResultsField; }
            set { _totalResultsField = value; }
        }
    
        [XmlIgnore]
        public bool TotalResultsSpecified {
            get { return _totalResultsFieldSpecified; }
            set { _totalResultsFieldSpecified = value; }
        }
    }

}