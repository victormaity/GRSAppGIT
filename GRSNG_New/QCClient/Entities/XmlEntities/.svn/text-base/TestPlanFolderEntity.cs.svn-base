using System;
using System.Xml.Serialization;
using QcClient.Tools;

namespace QcClient.Entities.XmlEntities
{
    public class TestPlanFolderEntity : Serializer<TestPlanFolderEntity>
    {
        [XmlElement("Fields")]
        public Fields Fields { get; set; }

        private String _itemVersion;
        public String ItemVersion {
            get {
                return Fields != null ? _itemVersion = Field.GetField(Fields.Field, "item-version") : _itemVersion;
            }
            set {
                if (String.IsNullOrEmpty(ItemVersion)) {
                    _itemVersion = value;
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

        private String _system;
        public String System {
            get {
                return Fields != null ? _system = Field.GetField(Fields.Field, "system") : _system;
            }
            set {
                if (String.IsNullOrEmpty(System)) {
                    _system = value;
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
    }
}

//  <Entity Type="test-folder">
//      <Fields>
//          <Field Name="item-version" /> 
//          <Field Name="id">
//              <Value></Value> 
//          </Field>
//          <Field Name="ver-stamp">
//              <Value></Value> 
//          </Field>
//          <Field Name="parent-id">
//              <Value></Value> 
//          </Field>
//          <Field Name="system">
//              <Value></Value> 
//          </Field>
//          <Field Name="last-modified">
//              <Value></Value> 
//          </Field>
//          <Field Name="description">
//              <Value></Value> 
//          </Field>
//          <Field Name="hierarchical-path">
//              <Value></Value> 
//          </Field>
//          <Field Name="view-order">
//              <Value></Value> 
//          </Field>
//          <Field Name="name">
//              <Value></Value> 
//          </Field>
//          <Field Name="attachment">
//              <Value></Value> 
//          </Field>
//      </Fields>
//      <RelatedEntities /> 
//  </Entity>