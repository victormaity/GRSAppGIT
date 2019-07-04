using System;
using System.Xml.Serialization;
using QcClient.Tools;

namespace QcClient.Entities.XmlEntities
{
    public class RunEntity : Serializer<RunEntity>
    {
        [XmlElement("Fields")]
        public Fields Fields { get; set; }


        private String _vcVersionNumber;
        public String VcVersionNumber {
            get {
                return Fields != null ? _vcVersionNumber = Field.GetField(Fields.Field, "vc-version-number") : _vcVersionNumber;
            }
            set {
                if (String.IsNullOrEmpty(VcVersionNumber)) {
                    _vcVersionNumber = value;
                }
            }
        }

        private String _status;
        public String Status {
            get {
                return Fields != null ? _status = Field.GetField(Fields.Field, "status") : _status;
            }
            set {
                if (String.IsNullOrEmpty(Status)) {
                    _status = value;
                }
            }
        }

        private String _owner;
        public String Owner {
            get {
                return Fields != null ? _owner = Field.GetField(Fields.Field, "owner") : _owner;
            }
            set {
                if (String.IsNullOrEmpty(Owner)) {
                    _owner = value;
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

        private String _hasLinkage;
        public String HasLinkage {
            get {
                return Fields != null ? _hasLinkage = Field.GetField(Fields.Field, "has-linkage") : _hasLinkage;
            }
            set {
                if (String.IsNullOrEmpty(HasLinkage)) {
                    _hasLinkage = value;
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

        private String _vcStatus;
        public String VcStatus {
            get {
                return Fields != null ? _vcStatus = Field.GetField(Fields.Field, "vc-status") : _vcStatus;
            }
            set {
                if (String.IsNullOrEmpty(VcStatus)) {
                    _vcStatus = value;
                }
            }
        }

        private String _lastModified;
        public String LastModified {
            get {
                return Fields != null ? _lastModified = Field.GetField(Fields.Field, "last-modified") : _lastModified;
            }
            set {
                if (String.IsNullOrEmpty(LastModified)) {
                    _lastModified = value;
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

        private String _subtypeId;
        public String SubtypeId {
            get {
                return Fields != null ? _subtypeId = Field.GetField(Fields.Field, "subtype-id") : _subtypeId;
            }
            set {
                if (String.IsNullOrEmpty(SubtypeId)) {
                    _subtypeId = value;
                }
            }
        }

        private String _textSync;
        public String TextSync {
            get {
                return Fields != null ? _textSync = Field.GetField(Fields.Field, "text-sync") : _textSync;
            }
            set {
                if (String.IsNullOrEmpty(TextSync)) {
                    _textSync = value;
                }
            }
        }

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

        private String _testInstance;
        public String TestInstance {
            get {
                return Fields != null ? _testInstance = Field.GetField(Fields.Field, "test-instance") : _testInstance;
            }
            set {
                if (String.IsNullOrEmpty(TestInstance)) {
                    _testInstance = value;
                }
            }
        }

        private String _executionDate;
        public String ExecutionDate {
            get {
                return Fields != null ? _executionDate = Field.GetField(Fields.Field, "execution-date") : _executionDate;
            }
            set {
                if (String.IsNullOrEmpty(ExecutionDate)) {
                    _executionDate = value;
                }
            }
        }

        private String _state;
        public String State {
            get {
                return Fields != null ? _state = Field.GetField(Fields.Field, "state") : _state;
            }
            set {
                if (String.IsNullOrEmpty(State)) {
                    _state = value;
                }
            }
        }

        private String _osConfig;
        public String OsConfig {
            get {
                return Fields != null ? _osConfig = Field.GetField(Fields.Field, "os-config") : _osConfig;
            }
            set {
                if (String.IsNullOrEmpty(OsConfig)) {
                    _osConfig = value;
                }
            }
        }

        private String _testConfigId;
        public String TestConfigId {
            get {
                return Fields != null ? _testConfigId = Field.GetField(Fields.Field, "test-config-id") : _testConfigId;
            }
            set {
                if (String.IsNullOrEmpty(TestConfigId)) {
                    _testConfigId = value;
                }
            }
        }

        private String _path;
        public String Path {
            get {
                return Fields != null ? _path = Field.GetField(Fields.Field, "path") : _path;
            }
            set {
                if (String.IsNullOrEmpty(Path)) {
                    _path = value;
                }
            }
        }

        private String _vcLockedBy;
        public String VcLockedBy {
            get {
                return Fields != null ? _vcLockedBy = Field.GetField(Fields.Field, "vc-locked-by") : _vcLockedBy;
            }
            set {
                if (String.IsNullOrEmpty(VcLockedBy)) {
                    _vcLockedBy = value;
                }
            }
        }

        private String _comments;
        public String Comments {
            get {
                return Fields != null ? _comments = Field.GetField(Fields.Field, "comments") : _comments;
            }
            set {
                if (String.IsNullOrEmpty(Comments)) {
                    _comments = value;
                }
            }
        }

        private String _osSp;
        public String OsSp {
            get {
                return Fields != null ? _osSp = Field.GetField(Fields.Field, "os-sp") : _osSp;
            }
            set {
                if (String.IsNullOrEmpty(OsSp)) {
                    _osSp = value;
                }
            }
        }

        private String _pinnedBaseline;
        public String PinnedBaseline {
            get {
                return Fields != null ? _pinnedBaseline = Field.GetField(Fields.Field, "pinned-baseline") : _pinnedBaseline;
            }
            set {
                if (String.IsNullOrEmpty(PinnedBaseline)) {
                    _pinnedBaseline = value;
                }
            }
        }

        private String _runVerStamp;
        public String RunVerStamp {
            get {
                return Fields != null ? _runVerStamp = Field.GetField(Fields.Field, "run-ver-stamp") : _runVerStamp;
            }
            set {
                if (String.IsNullOrEmpty(RunVerStamp)) {
                    _runVerStamp = value;
                }
            }
        }

        private String _osBuild;
        public String OsBuild {
            get {
                return Fields != null ? _osBuild = Field.GetField(Fields.Field, "os-build") : _osBuild;
            }
            set {
                if (String.IsNullOrEmpty(OsBuild)) {
                    _osBuild = value;
                }
            }
        }

        private String _testcyclId;
        public String TestcyclId {
            get {
                return Fields != null ? _testcyclId = Field.GetField(Fields.Field, "testcycl-id") : _testcyclId;
            }
            set {
                if (String.IsNullOrEmpty(TestcyclId)) {
                    _testcyclId = value;
                }
            }
        }

        private String _cycleId;
        public String CycleId {
            get {
                return Fields != null ? _cycleId = Field.GetField(Fields.Field, "cycle-id") : _cycleId;
            }
            set {
                if (String.IsNullOrEmpty(CycleId)) {
                    _cycleId = value;
                }
            }
        }

        private String _cycle;
        public String Cycle {
            get {
                return Fields != null ? _cycle = Field.GetField(Fields.Field, "cycle") : _cycle;
            }
            set {
                if (String.IsNullOrEmpty(Cycle)) {
                    _cycle = value;
                }
            }
        }

        private String _host;
        public String Host {
            get {
                return Fields != null ? _host = Field.GetField(Fields.Field, "host") : _host;
            }
            set {
                if (String.IsNullOrEmpty(Host)) {
                    _host = value;
                }
            }
        }

        private String _assignRcyc;
        public String AssignRcyc {
            get {
                return Fields != null ? _assignRcyc = Field.GetField(Fields.Field, "assign-rcyc") : _assignRcyc;
            }
            set {
                if (String.IsNullOrEmpty(AssignRcyc)) {
                    _assignRcyc = value;
                }
            }
        }

        private String _osName;
        public String OsName {
            get {
                return Fields != null ? _osName = Field.GetField(Fields.Field, "os-name") : _osName;
            }
            set {
                if (String.IsNullOrEmpty(OsName)) {
                    _osName = value;
                }
            }
        }

        private String _tersParamsValues;
        public String TersParamsValues {
            get {
                return Fields != null ? _tersParamsValues = Field.GetField(Fields.Field, "ters-params-values") : _tersParamsValues;
            }
            set {
                if (String.IsNullOrEmpty(TersParamsValues)) {
                    _tersParamsValues = value;
                }
            }
        }

        private String _testId;
        public String TestId {
            get {
                return Fields != null ? _testId = Field.GetField(Fields.Field, "test-id") : _testId;
            }
            set {
                if (String.IsNullOrEmpty(TestId)) {
                    _testId = value;
                }
            }
        }

        private String _draft;
        public String Draft {
            get {
                return Fields != null ? _draft = Field.GetField(Fields.Field, "draft") : _draft;
            }
            set {
                if (String.IsNullOrEmpty(TestId)) {
                    _draft = value;
                }
            }
        }

        private String _itersSumStatus;
        public String ItersSumStatus {
            get {
                return Fields != null ? _itersSumStatus = Field.GetField(Fields.Field, "iters-sum-status") : _itersSumStatus;
            }
            set {
                if (String.IsNullOrEmpty(ItersSumStatus)) {
                    _itersSumStatus = value;
                }
            }
        }

        private String _duration;
        public String Duration {
            get {
                return Fields != null ? _duration = Field.GetField(Fields.Field, "duration") : _duration;
            }
            set {
                if (String.IsNullOrEmpty(Duration)) {
                    _duration = value;
                }
            }
        }

        private String _bptStructure;
        public String BptStructure {
            get {
                return Fields != null ? _bptStructure = Field.GetField(Fields.Field, "bpt-structure") : _bptStructure;
            }
            set {
                if (String.IsNullOrEmpty(BptStructure)) {
                    _bptStructure = value;
                }
            }
        }

        private String _bptaChangeDetected;
        public String BptaChangeDetected {
            get {
                return Fields != null ? _bptaChangeDetected = Field.GetField(Fields.Field, "bpta-change-detected") : _bptaChangeDetected;
            }
            set {
                if (String.IsNullOrEmpty(BptaChangeDetected)) {
                    _bptaChangeDetected = value;
                }
            }
        }

        private String _executionTime;
        public String ExecutionTime {
            get {
                return Fields != null ? _executionTime = Field.GetField(Fields.Field, "execution-time") : _executionTime;
            }
            set {
                if (String.IsNullOrEmpty(ExecutionTime)) {
                    _executionTime = value;
                }
            }

        }

        private String _bptaChangeAwareness;
        public String BptaChangeAwareness {
            get {
                return Fields != null ? _bptaChangeAwareness = Field.GetField(Fields.Field, "bpta-change-awareness") : _bptaChangeAwareness;
            }
            set {
                if (String.IsNullOrEmpty(BptaChangeAwareness)) {
                    _bptaChangeAwareness = value;
                }
            }
        }

    }
}



//  <Entity Type="run">
//      <Fields>
//          <Field Name="test-instance">
//              <Value>1</Value> 
//          </Field>
//          <Field Name="user-01" /> 
//          <Field Name="execution-date">
//              <Value>2013-05-21</Value> 
//          </Field>
//          <Field Name="state">
//              <Value /> 
//          </Field>
//          <Field Name="id">
//              <Value>1</Value> 
//          </Field>
//          <Field Name="os-config">
//              <Value /> 
//          </Field>
//          <Field Name="test-config-id">
//              <Value>1007</Value> 
//          </Field>
//          <Field Name="name">
//              <Value>Run_5-21_2-0-37</Value> 
//          </Field>
//          <Field Name="has-linkage">
//              <Value>N</Value> 
//          </Field>
//          <Field Name="path">
//              <Value /> 
//          </Field>
//          <Field Name="vc-status">
//              <Value>Checked_In</Value> 
//          </Field>
//          <Field Name="pinned-baseline">      
//              <Value />                       
//          </Field>
//          <Field Name="run-ver-stamp">
//              <Value>2</Value> 
//          </Field>
//          <Field Name="vc-version-number">
//              <Value>2</Value> 
//          </Field>
//          <Field Name="os-build">
//              <Value>Build 7601</Value> 
//          </Field>
//          <Field Name="testcycl-id">
//              <Value>7</Value> 
//          </Field>
//          <Field Name="cycle-id">
//              <Value>4</Value> 
//          </Field>
//          <Field Name="cycle">
//              <Value /> 
//          </Field>
//          <Field Name="host">
//              <Value>ANNAG</Value> 
//          </Field>
//          <Field Name="assign-rcyc">
//              <Value /> 
//          </Field>
//          <Field Name="last-modified">
//              <Value>2013-05-21 02:13:01</Value> 
//          </Field>
//          <Field Name="status">
//              <Value>Failed</Value> 
//          </Field>
//          <Field Name="os-name">
//              <Value>Windows 7</Value> 
//          </Field>
//          <Field Name="attachment">
//              <Value /> 
//          </Field>
//          <Field Name="iters-params-values">
//              <Value /> 
//          </Field>
//          <Field Name="test-id">
//              <Value>7</Value> 
//          </Field>
//          <Field Name="subtype-id">
//              <Value>hp.qc.run.MANUAL</Value> 
//          </Field>
//          <Field Name="draft">
//              <Value>N</Value> 
//          </Field>                                     
//          <Field Name="iters-sum-status">              
//              <Value />                                
//          </Field>                                     
//          <Field Name="duration">                      
//              <Value>742</Value> 
//          </Field>
//          <Field Name="bpt-structure">
//              <Value /> 
//          </Field>
//          <Field Name="owner">
//              <Value>Anna.Grachova</Value> 
//          </Field>
//          <Field Name="text-sync">
//              <Value /> 
//          </Field>
//          <Field Name="bpta-change-detected" /> 
//          <Field Name="execution-time">
//              <Value>02:13:01</Value> 
//          </Field>
//          <Field Name="bpta-change-awareness">
//              <Value /> 
//          </Field>
//          <Field Name="vc-locked-by">
//              <Value>anna.grachova</Value> 
//          </Field>
//          <Field Name="comments">
//              <Value /> 
//          </Field>
//          <Field Name="os-sp">
//              <Value>Service Pack 1</Value> 
//          </Field>
//      </Fields>
//      <RelatedEntities /> 
//  </Entity>



