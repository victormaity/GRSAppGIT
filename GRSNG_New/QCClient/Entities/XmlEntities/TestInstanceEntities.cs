using System.Xml.Serialization;
using QcClient.Tools;

namespace QcClient.Entities.XmlEntities
{
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    [XmlRoot(Namespace="", IsNullable=false)]

    public class TestInstanceEntities : Serializer<TestInstanceEntities>{

        private TestInstanceEntity[] _testInstanceEntityField;
    
        private string[] _textField;
    
        private int _totalResultsField;
    
        private bool _totalResultsFieldSpecified;

        [XmlElement("TestInstanceEntity")]
        public TestInstanceEntity[] TestInstances
        {
            get { return _testInstanceEntityField; }
            set { _testInstanceEntityField = value; }
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