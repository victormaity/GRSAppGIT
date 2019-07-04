using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NUnit.Framework;
using QcClient.Entities.QcEntities;
using QcClient.Entities.XmlEntities;
using QcClient.Tools;

namespace QCClient.Client
{
    public class RestClient : IDisposable
    {
        public String ServerUrl { get; set; }
        public String Domain { get; set; }
        public String Project { get; set; }
        public CookieWebClient WebClient { get; set; }

        public TestPlan TestPlan { get { return new TestPlan(this); } }
        public TestLab TestLab { get { return new TestLab(this); } }
        public TestRun TestRun { get { return new TestRun(this); } }

        public String Auth(String user, String password)
        {
            WebClient = new CookieWebClient(new CookieContainer()) { Credentials = new NetworkCredential(user, password) };
            WebClient.Headers.Add("Content-Type", "application/xml");
            WebClient.Headers.Add("Accept", "application/xml");
            try
            {
                WebClient.DownloadString(string.Concat(ServerUrl, "qcbin/authentication-point/authenticate"));
                return String.Empty;
            }
            catch (Exception ex)
            {
                if (ex.Message == "Unable to connect to the remote server")
                {
                    return ex.Message + ", check the VPN connection";
                }
                return ex.Message + ", check login/password";
            }
        }


        public Domains GetDomains()
        {
            try
            {
                var domainsXml = WebClient.DownloadString(ServerUrl + "qcbin/rest/domains/");
                return new Domains().Deserialize(domainsXml);
            }
            catch (Exception)
            {
                return new Domains();
            }
        }


        public Projects GetProjects()
        {
            try
            {
                var projectsXml = WebClient.DownloadString(ServerUrl + "qcbin/rest/domains/" + Domain + "/projects");
                return new Projects().Deserialize(projectsXml);
            }
            catch (Exception)
            {
                return new Projects();
            }
        }

        public TestPlanFolderEntity GetTestPlanFolder(int folderId)
        {
            try
            {
                WebClient.Headers.Add("Content-Type", "application/xml");
                WebClient.Headers.Add("Accept", "application/xml");
                var resp = string.Format("{0}qcbin/rest/domains/{1}/projects/{2}/test-folders/{3}", ServerUrl, Domain, Project, folderId);
                resp = WebClient.DownloadString(resp);
                return new TestPlanFolderEntity().Deserialize(resp.Replace("Entity", "TestPlanFolderEntity"));
            }
            catch (WebException e)
            {
                string responseText;
                if (e.Response != null)
                {
                    using (var reader = new StreamReader(e.Response.GetResponseStream()))
                    {
                        responseText = reader.ReadToEnd();
                    }
                }
                else
                {
                    responseText = e.ToString();
                }
                throw new Exception(responseText);
            }
        }

        public TestLabFolderEntity GetTestLabFolder(int folderId)
        {
            try
            {
                WebClient.Headers.Add("Content-Type", "application/xml");
                WebClient.Headers.Add("Accept", "application/xml");
                var resp = string.Format("{0}qcbin/rest/domains/{1}/projects/{2}/test-set-folders/{3}", ServerUrl, Domain, Project, folderId);
                resp = WebClient.DownloadString(resp);
                return new TestLabFolderEntity().Deserialize(resp.Replace("Entity", "TestLabFolderEntity"));
            }
            catch (WebException e)
            {
                string responseText;
                if (e.Response != null)
                {
                    using (var reader = new StreamReader(e.Response.GetResponseStream()))
                    {
                        responseText = reader.ReadToEnd();
                    }
                }
                else
                {
                    responseText = e.ToString();
                }
                throw new Exception(responseText);
            }
        }

        public EntityResourceDescriptor GetProjectCustomization()
        {
            const string customization = @"customization/entities/test?page-size=10&amp;start-index=0";
            var resp = WebClient.DownloadString(string.Format("{0}qcbin/rest/domains/{1}/projects/{2}/" + customization, ServerUrl, Domain, Project));
            return new EntityResourceDescriptor().Deserialize(resp);
        }


        internal Object DoHttp<T>(string method, string functionality, string body)
        { // design-steps?page-size=5&amp;start-index=0"
            try
            {
                WebClient.Headers.Add("Content-Type", "application/xml");
                WebClient.Headers.Add("Accept", "application/xml");
                var resp = string.Format("{0}qcbin/rest/domains/{1}/projects/{2}/{3}", ServerUrl, Domain, Project, functionality);
                switch (method)
                {
                    case "GET": { resp = WebClient.DownloadString(resp); break; }
                    case "POST": { resp = WebClient.UploadString(resp, method, body); break; }
                    case "DELETE": { resp = WebClient.UploadString(resp, method, String.Empty); break; }
                    case "PUT": { resp = WebClient.UploadString(resp, method, body); break; }
                }

                switch (typeof(T).Name)
                {
                    case "Entity": { return new Entity().Deserialize(resp); }
                    case "Entities": { return new Entities().Deserialize(resp); }

                    case "TestPlanFolderEntity": { return new TestPlanFolderEntity().Deserialize(resp.Replace("Entity", "TestPlanFolderEntity")); }
                    case "TestLabFolderEntity": { return new TestLabFolderEntity().Deserialize(resp.Replace("Entity", "TestLabFolderEntity")); }

                    case "DesignStepEntities": { return new DesignStepEntities().Deserialize(resp.Replace("Entities", "DesignStepEntities").Replace("Entity", "DesignStepEntity")); }
                    case "DesignStepEntity": { return new DesignStepEntity().Deserialize(resp.Replace("Entity", "DesignStepEntity")); }

                    case "TestEntities": { return new TestEntities().Deserialize(resp.Replace("Entities", "TestEntities").Replace("Entity", "TestEntity")); }
                    case "TestEntity": { return new TestEntity().Deserialize(resp.Replace("Entity", "TestEntity")); }

                    case "TestSetEntities": { return new TestSetEntities().Deserialize(resp.Replace("Entities", "TestSetEntities").Replace("Entity", "TestSetEntity")); }
                    case "TestSetEntity": { return new TestSetEntity().Deserialize(resp.Replace("Entity", "TestSetEntity")); }

                    case "TestInstanceEntities": { return new TestInstanceEntities().Deserialize(resp.Replace("Entities", "TestInstanceEntities").Replace("Entity", "TestInstanceEntity")); }
                    case "TestInstanceEntity": { return new TestInstanceEntity().Deserialize(resp.Replace("Entity", "TestInstanceEntity")); }

                    case "RunEntities": { return new RunEntities().Deserialize(resp.Replace("Entities", "RunEntities").Replace("Entity", "RunEntity")); }
                    case "RunEntity": { return new RunEntity().Deserialize(resp.Replace("Entity", "RunEntity")); }

                    case "LockStatusEntity": { return new LockStatusEntity().Deserialize(resp); }

                    default: { return default(T); }
                }
            }
            catch (WebException e)
            {
                string responseText;
                if (e.Response != null)
                {
                    using (var reader = new StreamReader(e.Response.GetResponseStream()))
                    {
                        responseText = reader.ReadToEnd();
                    }
                }
                else
                {
                    responseText = e.ToString();
                }
                throw new Exception(responseText);
            }
        }

        public Object UpdateTest(String entityId, String xmlBody)
        {
            var subPath = @"tests" + "/" + entityId + "/";
            TestEntity result = null;
            var checkedOutTest = LockTest(@"tests", entityId);
            if (checkedOutTest != null && !String.IsNullOrEmpty(checkedOutTest.CheckOutUserName))
            {
                result = (TestEntity)DoHttp<TestEntity>("PUT", subPath, xmlBody);
            }
            return result;
        }

        public Entity UpdateFolder(String folderId, String xmlBody, string folderType)
        {
            var subPath = folderType + "/" + folderId + "/";
            Entity result = null;
            result = (Entity)DoHttp<Entity>("PUT", subPath, xmlBody);
            return result;
        }

        public Object UpdateStep(String entityId, String xmlBody)
        {
            var subPath = @"design-steps" + "/" + entityId + "/";
            DesignStepEntity result = null;
            result = (DesignStepEntity)DoHttp<DesignStepEntity>("PUT", subPath, xmlBody);
            return result;
        }

        public Boolean DeleteEntity(String entities, String entityId, String xmlBody)
        {
            return DoHttp<Object>("DELETE", entities + "/" + entityId, String.Empty) != null;
        }


        public string RunRawQuery(string request)
        {
            return WebClient.DownloadString(string.Concat(ServerUrl, request));
        }


        public void Dispose()
        {
            WebClient.DownloadString(string.Concat(ServerUrl, "qcbin/authentication-point/logout"));
            WebClient.Dispose();
        }

        public void GetEntityField(String entityUrl, String fieldName)
        {
            if (entityUrl == null) throw new Exception("entityUrl can't be null");
            if (fieldName == null) throw new Exception("fieldName can't be null");
            var entity = DoHttp<Object>("GET",
                entityUrl + "/" + (entityUrl.Contains("?") ? "&" : "?") + "fields=" + fieldName, String.Empty);
            //server.get(entityUrl + ( entityUrl.contains("?") ? "&" :"?" ) + "fields=" + fieldName); 
            //  return ALMUtils.getXMLEntityField(entity, fieldName); 
        }

        public TestEntity LockTest(String entities, String entityId)
        {
            TestEntity checkedOutTest = null;
            var subPath = entities + "/" + entityId + "/";
            var lockstatus = (LockStatusEntity)DoHttp<LockStatusEntity>("GET", subPath + "lock", String.Empty);
            if (lockstatus.LockStatus == "LOCKED_BY_ME")
            {
                var testCases =
                    (TestEntities)(DoHttp<TestEntities>("GET", @"tests/?query={id[" + entityId + "]}", null));
                return testCases.TestEntity.SingleOrDefault(p => p != null);
            }
            if (lockstatus.LockStatus == "UNLOCKED")
            {
                DoHttp<Object>("POST", subPath + "lock", String.Empty);
                lockstatus = (LockStatusEntity)DoHttp<LockStatusEntity>("GET", subPath + "lock", String.Empty);
            }
            if (lockstatus.LockStatus == "LOCKED_BY_ME")
            {
                try
                {
                    var chOBody =
                        new CheckOutParameters {Comment = "Auto", Version = String.Empty}.Serialize()
                            .Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
                    checkedOutTest = (TestEntity) DoHttp<TestEntity>("POST", subPath + "versions/check-out", chOBody);
                }
                catch
                {
                    checkedOutTest =
                        ((TestEntities) (DoHttp<TestEntities>("GET", @"tests/?query={id[" + entityId + "]}", null)))
                            .TestEntity.SingleOrDefault(p => p != null);
                }
            }
            return checkedOutTest;
        }

        public void UnLockTest(string entityId)
        {
            var subPath = @"tests" + "/" + entityId + "/";
            var lockstatus = (LockStatusEntity)DoHttp<LockStatusEntity>("GET", subPath + "lock", String.Empty);

            if (lockstatus.LockStatus == "LOCKED_BY_ME")
            {
                var chIBody =
                     new CheckInParameters { Comment = "Auto", OverrideLastVersion = String.Empty }.Serialize()
                         .Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
                DoHttp<Object>("POST", subPath + "versions/check-in", chIBody);
                DoHttp<Object>("DELETE", subPath + "lock", String.Empty);
            }
        }

        public string GetNameOfPriorityField()
        {
            const string tmpNameField1 = "user-05";
            const string tmpNameField2 = "user-06";
            TestEntity[] testEntities1, testEntities2;
            try
            {
                testEntities1 = GetTestEntities(tmpNameField1);
                try
                {
                    testEntities2 = GetTestEntities(tmpNameField2);
                }
                catch
                {
                    return tmpNameField1;
                }
            }
            catch
            {
                return tmpNameField2;
            }
           
            if (testEntities1 != null && testEntities2 != null)
            {
                if (testEntities1.Count() > testEntities2.Count())
                {
                    return tmpNameField1;
                }
                if (testEntities1.Count() < testEntities2.Count())
                {
                    return tmpNameField2;
                }
                try
                {
                    TestPlan.AddTestCasesToTestPlanFolder("2", new TestEntity
                    {
                        Name = Regex.Replace("tmp" + DateTime.Now, "[^a-zA-Z0-9\\s]", string.Empty),
                        Description = "",
                        User06 = "High",
                        Status = "Ready"
                    }, tmpNameField1);
                    return tmpNameField1;
                }
                catch
                {
                    return tmpNameField2;
                }
            }
            return testEntities1 == null ? tmpNameField2 : tmpNameField1;
        }

        private TestEntity[] GetTestEntities(string priorityFieldName1)
        {
            var functionality = @"tests/?query={" + priorityFieldName1 + "[High]}";
            WebClient.Headers.Add("Content-Type", "application/xml");
            WebClient.Headers.Add("Accept", "application/xml");
            var req = string.Format("{0}qcbin/rest/domains/{1}/projects/{2}/{3}", ServerUrl, Domain, Project, functionality);
            var resp = WebClient.DownloadString(req);
            var rx = new Regex("Entities TotalResults=\"[0-9]+\"");
            resp = rx.Replace(resp, "Entities").Replace("Entities", "TestEntities").Replace("Entity", "TestEntity");
            return new TestEntities().Deserialize(resp).TestEntity;
        }
    }
}
