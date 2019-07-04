using System.Xml.Serialization;
using QcClient.Tools;

namespace QcClient.Entities.XmlEntities
{
    [System.SerializableAttribute]
    [System.Diagnostics.DebuggerStepThroughAttribute]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [XmlType(AnonymousType=true)]
    [XmlRoot(Namespace="", IsNullable=false)]

    public class TestSetEntities : Serializer<TestSetEntities> {

        private TestSetEntity[] _testSetEntity;
    
        private string[] _textField;
    
        private short _totalResultsField;
    
        private bool _totalResultsFieldSpecified;


        [XmlElement("TestSetEntity")]
        public TestSetEntity[] Entity
        {
            get { return _testSetEntity; }
            set { _testSetEntity = value; }
        }
    

        [XmlText]
        public string[] Text {
            get { return _textField; }
            set { _textField = value; }
        }
    

        [XmlAttribute]
        public short TotalResults {
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