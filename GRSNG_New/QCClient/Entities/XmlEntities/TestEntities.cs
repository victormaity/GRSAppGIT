using System.Xml.Serialization;
using QcClient.Tools;

namespace QcClient.Entities.XmlEntities
{

    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    [XmlRoot(Namespace="", IsNullable=false)]

    public class TestEntities : Serializer<TestEntities>
    {

        private TestEntity[] _testEntity;
    
        private string[] _textField;
    
        private int _totalResultsField;
    
        private bool _totalResultsFieldSpecified;


        [XmlElement("TestEntity")]
        public TestEntity[] TestEntity
        {
            get { return _testEntity; }
            set { _testEntity = value; }
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