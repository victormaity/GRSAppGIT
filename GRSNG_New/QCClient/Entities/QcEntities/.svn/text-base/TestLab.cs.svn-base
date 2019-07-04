using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using QCClient.Client;
using QcClient.Entities.XmlEntities;
using System.Web;

namespace QcClient.Entities.QcEntities
{
    public class TestLab
    {
        private readonly RestClient _client;
        private const String XmlHead = "<?xml version=\"1.0\" encoding=\"utf-16\"?>";


        public TestLab(RestClient client) {
            _client = client;
        }


        public List<Entity> GetFolderTree() {
            return ((XmlEntities.Entities)_client.DoHttp<XmlEntities.Entities>("GET", "test-set-folders ", null)).Entity.ToList();
        }



        public TestLabFolderEntity CreateFolder(String subjectName, String parentId = null) {
            var folders = GetFolderTree();
            var folder = folders.SingleOrDefault(x => x.Fields[8].Value == subjectName);
            if (folder == null) {
                parentId = String.IsNullOrEmpty(parentId) ? GetFolderTree().ToList()[0].Fields[1].Value : parentId;
                var entity = new Entity {
                    Type = "test-set-folder",
                    Fields = new List<EntitiesEntityField> {  
                        new EntitiesEntityField {Name = "parent-id", Value = parentId},
                        new EntitiesEntityField {Name = "name", Value = subjectName}
                    }
                };
                var xmlBody = entity.Serialize().Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
                return (TestLabFolderEntity)_client.DoHttp<TestLabFolderEntity>("POST", @"test-set-folders", xmlBody);
            }
            return new TestLabFolderEntity { Id = folder.Fields[0].Value }; 
        }



        public TestSetEntity CreateTestSet(String testsetname, String parentId) {
            var entity = new Entity {
                Type = "test-set",
                Fields = new List<EntitiesEntityField> {
                    new EntitiesEntityField { Name = "name", Value = testsetname },
                    new EntitiesEntityField { Name = "parent-id", Value = parentId },
                    new EntitiesEntityField { Name = "subtype-id", Value = "hp.qc.test-set.default" }
                }
            };
            var xmlBody = entity.Serialize().Replace(XmlHead, "");

            var testSets = (TestSetEntities)_client.DoHttp<TestSetEntities>("GET", "test-sets", null);
            var testSet = testSets.Entity.SingleOrDefault(x => x.ParentId == parentId && x.Name == testsetname);

            if (testSet != null) {
                _client.DeleteEntity(@"test-sets", testSet.Id, String.Empty);
            }
            return (TestSetEntity)_client.DoHttp<TestSetEntity>("POST", "test-sets", xmlBody);
        }


        public TestInstanceEntity AddTestToTestSet(TestEntity test, TestSetEntity testset) {
            var status = String.Empty;
            if (!String.IsNullOrEmpty(test.Status))
            {
                if (test.Status.SequenceEqual("pass")) { status = "Passed"; }
                else if (test.Status.SequenceEqual("fail")) { status = "Failed"; }
                else if (test.Status.SequenceEqual("norun")) { status = "No Run"; }
                else if (test.Status.SequenceEqual("notcompleted")) { status = "Not Complited"; }
            }
            var entity = new Entity {
                Type = "test-instance",
                Fields = new List<EntitiesEntityField> {
                    new EntitiesEntityField { Name = "cycle-id", Value = testset.Id },
                    new EntitiesEntityField { Name = "test-id", Value = test.Id }, 
                    new EntitiesEntityField { Name = "test-order", Value = "1" }, 
                    new EntitiesEntityField { Name = "owner", Value = test.Owner },
                    new EntitiesEntityField { Name = "actual-tester",Value = test.Owner },
                    new EntitiesEntityField { Name = "subtype-id", Value = "hp.qc.test-instance.MANUAL" }, // testset.SubtypeId
                    new EntitiesEntityField { Name = "status", Value = status }
                }
            };
            var xmlBody = entity.Serialize().Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
            var testInstance = ((TestInstanceEntities)_client.DoHttp<TestInstanceEntities>("GET", "test-instances ", null));
            var testSet = testInstance.TestInstances.SingleOrDefault(x => x.CycleId == testset.Id && x.TestId == test.Id);

            if (testSet != null)
            {
                _client.DoHttp<Entity>("DELETE", "test-instances/" + testSet.Id, String.Empty);
            }

            return (TestInstanceEntity)_client.DoHttp<TestInstanceEntity>("POST", "test-instances", xmlBody);
        }

        public void MoveFolderInTrash(int folderToMoveId)
        {
            var trashId = GetFolderId("Trash", GetRootId());
            var folder = _client.GetTestLabFolder(folderToMoveId);

            var entity = new Entity
            {
                Type = "test-set-folder",
                Fields = new List<EntitiesEntityField>
                    {
                        new EntitiesEntityField {Name = "parent-id", Value = trashId.ToString()},
                        new EntitiesEntityField {Name = "name", Value = String.Concat(folder.Name, " - ", DateTime.Now.ToString(), "(GRS)")},
                        new EntitiesEntityField {Name = "description", Value = folder.Description},
                        new EntitiesEntityField {Name = "id", Value = folder.Id},
                        new EntitiesEntityField {Name = "ver-stamp", Value = folder.VerStamp},
                        new EntitiesEntityField {Name = "hierarchical-path", Value = folder.HierarchicalPath},
                        new EntitiesEntityField {Name = "view-order", Value = folder.ViewOrder},
                        new EntitiesEntityField {Name = "attachment", Value = folder.Attachment}
                    }
            };
            var xmlBody = entity.Serialize().Replace(XmlHead, "");
            _client.UpdateFolder(folderToMoveId.ToString(), xmlBody, "test-set-folders");
        }

        public int GetFolderId(string folderName, int parentId)
        {
            int id = 0;
            try
            {
                id = ((XmlEntities.Entities)_client.DoHttp<XmlEntities.Entities>("GET",
                    "test-set-folders?query={name[" + folderName + "]}",
                    null))
                    .Entity.ToList()
                    .Where(p => Convert.ToInt32(p.Fields[3].Value) == parentId)
                    .Select(p => Convert.ToInt32(p.Fields[0].Value)).ToList()[0];
            }
            catch (Exception)
            {
                id = -1;
            }
            return id;
        }

        public int GetRootId()
        {
            return ((XmlEntities.Entities)_client.DoHttp<XmlEntities.Entities>("GET",
                "test-set-folders?query={name[Root]}",
                null))
                .Entity.ToList().Select(p => Convert.ToInt32(p.Fields[0].Value)).Min();
        }

        public TestLabFolderEntity CreateFolderByName(String subjectName, string parentId, string description)
        {
            var folder = ((XmlEntities.Entities)_client.DoHttp<XmlEntities.Entities>("GET",
                "test-set-folders?query={name['" + HttpUtility.UrlEncode(subjectName) + "'];parent-id[" + parentId + "]}",
                null))
                .Entity.ToList().SingleOrDefault();
            if (folder != null) return new TestLabFolderEntity { Id = folder.Fields[0].Value };
            var entity = new Entity
            {
                Type = "test-set-folder",
                Fields = new List<EntitiesEntityField> {  
                    new EntitiesEntityField {Name = "parent-id", Value = parentId.ToString()},
                    new EntitiesEntityField {Name = "name", Value = subjectName},
                    new EntitiesEntityField {Name = "description", Value = subjectName},
                }
            };
            var xmlBody = entity.Serialize().Replace(XmlHead, "");
            return (TestLabFolderEntity)_client.DoHttp<TestLabFolderEntity>("POST", @"test-set-folders", xmlBody);
        }
    }
}
