using System;
using System.Xml.Serialization;
using QcClient.Tools;

namespace QcClient.Entities.XmlEntities
{
    public class TestLabFolderEntity : Serializer<TestLabFolderEntity>
    {
        [XmlElement("Fields")]
        public Fields Fields { get; set; }

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

        private String _hierarchicalPath;
        public String HierarchicalPath {
            get {
                return Fields != null ? _hierarchicalPath = Field.GetField(Fields.Field, "hierarchical-path") : _hierarchicalPath;
            }
            set {
                if (String.IsNullOrEmpty(HierarchicalPath)) {
                    _hierarchicalPath = value;
                }
            }
        }

        private String _viewOrder;
        public String ViewOrder {
            get {
                return Fields != null ? _viewOrder = Field.GetField(Fields.Field, "view-order") : _viewOrder;
            }
            set {
                if (String.IsNullOrEmpty(ViewOrder)) {
                    _viewOrder = value;
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

        private String _noOfSons;
        public String NoOfSons {
            get {
                return Fields != null ? _noOfSons = Field.GetField(Fields.Field, "no-of-sons") : _noOfSons;
            }
            set {
                if (String.IsNullOrEmpty(NoOfSons)) {
                    _noOfSons = value;
                }
            }
        }

        private String _assignRcyc;
        public String AssignRcyc {
            get {
                return Fields != null ? _assignRcyc = Field.GetField(Fields.Field, "system") : _assignRcyc;
            }
            set {
                if (String.IsNullOrEmpty(AssignRcyc)) {
                    _assignRcyc = value;
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

        private String _workflow;
        public String Workflow {
            get {
                return Fields != null ? _workflow = Field.GetField(Fields.Field, "last-modified") : _workflow;
            }
            set {
                if (String.IsNullOrEmpty(Workflow))
                {
                    _workflow = value;
                }
            }
        }

    }
}

//<Entity Type="test-folder">
//  <Fields>
//    <Field Name="id">
//      <Value></Value> 
//    </Field>
//    <Field Name="no-of-sons" /> 
//    <Field Name="ver-stamp">
//      <Value></Value> 
//    </Field>
//    <Field Name="parent-id">
//      <Value></Value> 
//    </Field>
//    <Field Name="assign-rcyc">
//      <Value /> 
//    </Field>
//    <Field Name="last-modified">
//      <Value></Value> 
//    </Field>
//    <Field Name="hierarchical-path">
//      <Value></Value> 
//    </Field>
//    <Field Name="description">
//      <Value /> 
//    </Field>
//    <Field Name="name">
//      <Value></Value> 
//    </Field>
//    <Field Name="view-order" /> 
//    <Field Name="attachment">
//      <Value /> 
//    </Field>
//    <Field Name="workflow">
//      <Value /> 
//    </Field>
//    <RelatedEntities /> 
//  </Fields>
//</Entity>