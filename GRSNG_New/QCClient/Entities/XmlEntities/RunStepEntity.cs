using System;
using System.Xml.Serialization;
using QcClient.Tools;

namespace QcClient.Entities.XmlEntities
{
    public class RunStepEntity : Serializer<RunStepEntity>
    {
        [XmlElement("Fields")]
        public Fields Fields { get; set; }

        private String _user01;
        public String User01 {
            get {
                return Fields != null ? _user01 = Field.GetField(Fields.Field, "user-01") : _user01;
            }
            set {
                if (String.IsNullOrEmpty(User01)) {
                    _user01 = value;
                }
            }
        }

        private String _user02;
        public String User02 {
            get {
                return Fields != null ? _user02 = Field.GetField(Fields.Field, "user-02") : _user02;
            }
            set {
                if (String.IsNullOrEmpty(User02)) {
                    _user02 = value;
                }
            }
        }

        private String _user03;
        public String User03 {
            get {
                return Fields != null ? _user03 = Field.GetField(Fields.Field, "user-03") : _user03;
            }
            set {
                if (String.IsNullOrEmpty(User03)) {
                    _user03 = value;
                }
            }
        }

        private String _user04;
        public String User04 {
            get {
                return Fields != null ? _user04 = Field.GetField(Fields.Field, "user-04") : _user04;
            }
            set {
                if (String.IsNullOrEmpty(User04)) {
                    _user04 = value;
                }
            }
        }

        private String _attachment;
        public String Attachment {
            get {
                return Fields != null ? _attachment = Field.GetField(Fields.Field, "attachment") : _attachment;
            }
            set {
                if (String.IsNullOrEmpty(Attachment)) {
                    _attachment = value;
                }
            }
        }

        private String _id;
        public String Id {
            get {
                return Fields != null ? _id = Field.GetField(Fields.Field, "id") : _id;
            }
            set {
                if (String.IsNullOrEmpty(Id)) {
                    _id = value;
                }
            }
        }

        private String _parentId;
        public String ParentId {
            get {
                return Fields != null ? _parentId = Field.GetField(Fields.Field, "parent-id") : _parentId;
            }
            set {
                if (String.IsNullOrEmpty(ParentId)) {
                    _parentId = value;
                }
            }
        }

        private String _description;
        public String Description {
            get { 
                return Fields != null ? _description = Field.GetField(Fields.Field, "description") : _description;
            }
            set {
                if (String.IsNullOrEmpty(Description)) {
                    _description = value;
                }
            }
        }

        private String _name;
        public String Name {
            get {
                return Fields != null ? _name = Field.GetField(Fields.Field, "name") : _name;
            }
            set {
                if (String.IsNullOrEmpty(Name)) {
                    _name = value;
                }
            }
        }

        private String _linkTest;
        public String LinkTest {
            get {
                return Fields != null ? _linkTest = Field.GetField(Fields.Field, "link-test") : _linkTest;
            }
            set {
                if (String.IsNullOrEmpty(LinkTest)) {
                    _linkTest = value;
                }
            }
        }

        private String _vts;
        public String Vts {
            get {
                return Fields != null ? _vts = Field.GetField(Fields.Field, "vts") : _vts;
            }
            set {
                if (String.IsNullOrEmpty(Vts)) {
                    _vts = value;
                }
            }
        }

        private String _hasParams;
        public String HasParams {
            get {
                return Fields != null ? _hasParams = Field.GetField(Fields.Field, "has-params") : _hasParams;
            }
            set {
                if (String.IsNullOrEmpty(HasParams)) {
                    _hasParams = value;
                }
            }
        }

        private String _vcUserName;
        public String VcUserName {
            get {
                return Fields != null ? _vcUserName = Field.GetField(Fields.Field, "vc-user-name") : _vcUserName;
            }
            set {
                if (String.IsNullOrEmpty(VcUserName)) {
                    _vcUserName = value;
                }
            }
        }

        private String _verStamp;
        public String VerStamp {
            get {
                return Fields != null ? _verStamp = Field.GetField(Fields.Field, "ver-stamp") : _verStamp;
            }
            set {
                if (String.IsNullOrEmpty(VerStamp)) {
                    _verStamp = value;
                }
            }
        }

        private String _expected;
        public String Expected {
            get {
                return Fields != null ? _expected = Field.GetField(Fields.Field, "expected") : _expected;
            }
            set {
                if (String.IsNullOrEmpty(Expected)) {
                    _expected = value;
                }
            }
        }
        private String _actual;
        public String Actual
        {
            get
            {
                return Fields != null ? _actual = Field.GetField(Fields.Field, "actual") : _actual;
            }
            set
            {
                if (String.IsNullOrEmpty(Actual))
                {
                    _actual = value;
                }
            }
        }

        private String _status;
        public String Status
        {
            get
            {
                return Fields != null ? _status = Field.GetField(Fields.Field, "status") : _status;
            }
            set
            {
                if (String.IsNullOrEmpty(Status))
                {
                    _status = value;
                }
            }
        }

        private String _desstepId;
        public String DesStepId
        {
            get
            {
                return Fields != null ? _desstepId = Field.GetField(Fields.Field, "desstep-id") : _desstepId;
            }
            set
            {
                if (String.IsNullOrEmpty(DesStepId))
                {
                    _desstepId = value;
                }
            }
        }

        private String _testId;
        public String TestId
        {
            get
            {
                return Fields != null ? _testId = Field.GetField(Fields.Field, "test-id") : _testId;
            }
            set
            {
                if (String.IsNullOrEmpty(TestId))
                {
                    _testId = value;
                }
            }
        }

        private String _stepOrder;
        public String StepOrder {
            get {
                return Fields != null ? _stepOrder = Field.GetField(Fields.Field, "step-order") : _stepOrder;
            }
            set {
                if (String.IsNullOrEmpty(StepOrder)) {
                    _stepOrder = value;
                }
            }
        }

    }
}


//  <Fields>
//      <Field Name="user-04">
//          <Value></Value> 
//      </Field>
//      <Field Name="user-03">
//          <Value /> 
//      </Field>
//      <Field Name="user-02">
//          <Value></Value> 
//      </Field>
//      <Field Name="user-01">
//          <Value />  
//      </Field>
//      <Field Name="link-test" /> 
//      <Field Name="attachment">     
//          <Value />  
//      </Field>
//      <Field Name="vts">
//          <Value></Value> 
//      </Field>
//      <Field Name="has-params">
//          <Value />  
//      </Field>
//      <Field Name="vc-user-name">
//          <Value />  
//      </Field>
//      <Field Name="id">
//          <Value></Value> 
//      </Field>
//      <Field Name="ver-stamp">
//          <Value></Value> 
//      </Field>
//      <Field Name="parent-id">
//          <Value></Value> 
//      </Field>
//      <Field Name="expected">
//          <Value />  
//      </Field>
//      <Field Name="description">
//          <Value></Value> 
//      </Field>
//      <Field Name="name">
//          <Value></Value> 
//      </Field>
//      <Field Name="step-order">
//          <Value></Value> 
//      </Field>
//  </Fields>
/// <remarks/>

