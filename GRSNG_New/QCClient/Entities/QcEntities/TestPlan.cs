using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using QCClient.Client;
using QcClient.Entities.XmlEntities;

namespace QcClient.Entities.QcEntities
{
    public class TestPlan
    {

        private readonly RestClient _client;
        private const String XmlHead = "<?xml version=\"1.0\" encoding=\"utf-16\"?>";

        public TestPlan(RestClient client)
        {
            _client = client;
        }


        public List<Entity> GetFolderTree()
        {
            var resp = _client.DoHttp<XmlEntities.Entities>("GET", "test-folders", null);
            return ((XmlEntities.Entities)resp).Entity.ToList();
        }


        public TestPlanFolderEntity CreateFolder(String subjectName, String parentId = null)
        {
            var folders = GetFolderTree();
            var folder = folders.SingleOrDefault(x => x.Fields[9].Value == subjectName);
            if (folder == null)
            {
                parentId = String.IsNullOrEmpty(parentId) ? GetFolderTree().ToList()[0].Fields[1].Value : parentId;
                var entity = new Entity
                {
                    Type = "test-folder",
                    Fields = new List<EntitiesEntityField> {  
                        new EntitiesEntityField {Name = "parent-id", Value = parentId},
                        new EntitiesEntityField {Name = "name", Value = subjectName},
                    }
                };
                var xmlBody = entity.Serialize().Replace(XmlHead, "");
                return (TestPlanFolderEntity)_client.DoHttp<TestPlanFolderEntity>("POST", @"test-folders", xmlBody);
            }
            return new TestPlanFolderEntity { Id = folder.Fields[1].Value };
        }

        public TestPlanFolderEntity CreateFolderByName(String subjectName, string parentId, string description)
        {
            var folder = ((XmlEntities.Entities)_client.DoHttp<XmlEntities.Entities>("GET",
                "test-folders?query={name['" + HttpUtility.UrlEncode(subjectName) + "'];parent-id[" + parentId + "]}",
                null))
                .Entity.ToList().SingleOrDefault();
            if (folder != null) return new TestPlanFolderEntity { Id = folder.Fields[1].Value };
            var entity = new Entity
            {
                Type = "test-folder",
                Fields = new List<EntitiesEntityField> {  
                    new EntitiesEntityField {Name = "parent-id", Value = parentId.ToString()},
                    new EntitiesEntityField {Name = "name", Value = subjectName},
                    new EntitiesEntityField {Name = "description", Value = subjectName},
                }
            };
            var xmlBody = entity.Serialize().Replace(XmlHead, "");
            return (TestPlanFolderEntity)_client.DoHttp<TestPlanFolderEntity>("POST", @"test-folders", xmlBody);
        }

        public int GetRootId()
        {
            return ((XmlEntities.Entities)_client.DoHttp<XmlEntities.Entities>("GET",
                "test-folders?query={name[Subject]}",
                null))
                .Entity.ToList().Select(p => Convert.ToInt32(p.Fields[1].Value)).Min();
        }

        public int GetFolderId(string folderName, int parentId)
        {
            int id = 0;
            try
            {
                id = ((XmlEntities.Entities)_client.DoHttp<XmlEntities.Entities>("GET",
                    "test-folders?query={name[" + folderName + "]}",
                    null))
                    .Entity.ToList()
                    .Where(p => Convert.ToInt32(p.Fields[3].Value) == parentId)
                    .Select(p => Convert.ToInt32(p.Fields[1].Value)).ToList()[0];
            }
            catch (Exception)
            {
                id = -1;
            }
            return id;
        }

        public TestEntity AddTestCasesToTestPlanFolder(String folderId, TestEntity testCase, string priorityFieldName)
        {
            var entity = new Entity
            {
                Type = "test",
                Fields = new List<EntitiesEntityField> {
                    new EntitiesEntityField {Name = "parent-id", Value = folderId},
                    new EntitiesEntityField {Name = "name", Value = testCase.Name},
                    new EntitiesEntityField {Name = "subtype-id", Value = @"MANUAL"},
                    new EntitiesEntityField {Name = priorityFieldName, Value = testCase.User06},
                    new EntitiesEntityField {Name = "owner", Value = testCase.Owner},
                    new EntitiesEntityField {Name = "status", Value = testCase.Status},
                    new EntitiesEntityField {Name = "description", Value = testCase.Status}
                }
            };
            var xmlBody = entity.Serialize().Replace(XmlHead, "");

            // var tests = (TestEntities)_client.DoHttp<TestEntities>("GET", "tests", null);
            //  var tcases = tests.TestEntity.SingleOrDefault(x => x.ParentId == folderId && x.Name == testCase.Name);
            var tcases = ((TestEntities)_client.DoHttp<TestEntities>("GET",
                "tests?query={name['" + HttpUtility.UrlPathEncode(testCase.Name.Trim()) + "*'];parent-id[" + folderId + "]}",
                null));
            var tcase = tcases.TestEntity != null ? tcases.TestEntity.ToList().SingleOrDefault(p => p.ParentId == folderId && p.Name.Trim() == testCase.Name.Trim()) : null;
            if (tcase != null)
            {
                if (Convert.ToBoolean(_client.GetProjectCustomization().SupportsVC.Value))
                {
                    return (TestEntity)_client.UpdateTest(tcase.Id, xmlBody);
                }
                return null;
            }
            return (TestEntity)_client.DoHttp<TestEntity>("POST", @"tests", xmlBody);
        }

        public XmlEntities.Entities GetChildTestFolders(int folderId)
        {
            var tcases = ((XmlEntities.Entities)_client.DoHttp<XmlEntities.Entities>("GET",
                "test-folders?query={parent-id[" + folderId + "]}",
                null));
            return tcases;
        }

        public void MoveFolderInTrash(int folderToMoveId)
        {
            var trashId = GetFolderId("Trash", GetRootId());
            var folder = _client.GetTestPlanFolder(folderToMoveId);

            var entity = new Entity
            {
                Type = "test-folder",
                Fields = new List<EntitiesEntityField>
                    {
                        new EntitiesEntityField {Name = "parent-id", Value = trashId.ToString()},
                        new EntitiesEntityField {Name = "name", Value = String.Concat(folder.Name, " - ", DateTime.Now.ToString(), "(GRS)")},
                        new EntitiesEntityField {Name = "description", Value = folder.Description},
                        new EntitiesEntityField {Name = "id", Value = folder.Id},
                        new EntitiesEntityField {Name = "ver-stamp", Value = folder.VerStamp},
                        new EntitiesEntityField {Name = "item-version", Value = folder.ItemVersion},
                        new EntitiesEntityField {Name = "hierarchical-path", Value = folder.HierarchicalPath},
                        new EntitiesEntityField {Name = "view-order", Value = folder.ViewOrder},
                        new EntitiesEntityField {Name = "attachment", Value = folder.Attachment}
                    }
            };
            var xmlBody = entity.Serialize().Replace(XmlHead, "");
            _client.UpdateFolder(folderToMoveId.ToString(), xmlBody, "test-folders");
        }

        public DesignStepEntity AddTestStepToTestCase(String testCasesId, DesignStepEntity testStep)
        { // Only for ALM ver >= 12
            var checkedOutTest = _client.LockTest(@"tests", testCasesId);
            if (checkedOutTest != null && !String.IsNullOrEmpty(checkedOutTest.CheckOutUserName))
            {
                var entity = new Entity
                {
                    Type = "design-step",
                    Fields = new List<EntitiesEntityField>
                    {
                        new EntitiesEntityField {Name = "parent-id", Value = testCasesId},
                        new EntitiesEntityField {Name = "name", Value = testStep.Name},
                        new EntitiesEntityField {Name = "description", Value = testStep.Description==null?"":testStep.Description.Replace("@", "").Replace("…", "...").Replace("™", "")},
                        new EntitiesEntityField {Name = "expected", Value = testStep.Expected==null?"":testStep.Expected.Replace("@", "").Replace("…", "...").Replace("™", "")},
                    }
                };
                var xmlBody = entity.Serialize().Replace(XmlHead, "");

                return (DesignStepEntity)_client.DoHttp<DesignStepEntity>("POST", @"design-steps", xmlBody);
            }
            return null;
        }

        public void DeleteAllSteps(string testCaseId)
        {
            var designsteps =
                   (DesignStepEntities)
                       _client.DoHttp<DesignStepEntities>("GET",
                           "design-steps/?query={parent-id[" + testCaseId + "]}", null);
            if (designsteps.DesignStepEntity != null)
            {
                foreach (var item in designsteps.DesignStepEntity)
                {
                    _client.DeleteEntity("design-steps", item.Id, null);
                }
            }
        }
    }
}
