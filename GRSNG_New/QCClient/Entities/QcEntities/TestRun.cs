using System.Collections.Generic;
using System.Linq;
using QCClient.Client;
using QcClient.Entities.XmlEntities;
using System;

namespace QcClient.Entities.QcEntities
{
    public class TestRun
    {
        private readonly RestClient _client;

        public TestRun(RestClient client) {
            _client = client;
        }


        public Entity AddTestRun(TestInstanceEntity testInstance, TestEntity test, TestSetEntity testSet)
        {

            var entity = new Entity {
                Type = "run",
                Fields = new List<EntitiesEntityField> {
                    new EntitiesEntityField { Name = "name", Value = test.Name}, //test.Name },  // <Field PhysicalName="RN_RUN_NAME" Name="name" Label="Run Name">
                    new EntitiesEntityField { Name = "test-id", Value = test.Id }, // <Field PhysicalName="RN_TEST_ID" Name="test-id" Label="Test">
                    new EntitiesEntityField { Name = "testcycl-id", Value = testInstance.Id },  // <Field PhysicalName="RN_TESTCYCL_ID" Name="testcycl-id" Label="Test Instance">
                    new EntitiesEntityField { Name = "owner", Value = testInstance.Owner }, //testInstance.Owner}, // <Field PhysicalName="RN_TESTER_NAME" Name="owner" Label="Tester">

                    new EntitiesEntityField { Name = "cycle-id", Value = testInstance.CycleId },
                    new EntitiesEntityField { Name = "subtype-id", Value = "hp.qc.run.MANUAL" },
                    new EntitiesEntityField { Name = "status", Value = testInstance.Status },
                    new EntitiesEntityField { Name = "test-instance", Value = "1" }
                }
            };
            var xmlBody = entity.Serialize().Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");

            var testRuns = ((RunEntities)_client.DoHttp<RunEntities>("GET", "runs ", null)).RunEntity;
            var testRun = testRuns.SingleOrDefault(x => x.TestId == test.Id && x.Name == test.Name);
            
            if (testRun != null) {
                _client.DoHttp<Entity>("DELETE", "runs/" + testRun.Id, null);
            }
            return (Entity)_client.DoHttp<Entity>("POST", "runs", xmlBody);
        }

        public Entity UpdateTestRunStatus(string runId, string status)
        {
            var entity = new Entity
            {
                Type = "run",
                Fields = new List<EntitiesEntityField> {
                   new EntitiesEntityField { Name = "status", Value = status }
                }
            };
            var xmlBody = entity.Serialize().Replace("<?xml version=\"1.0\" encoding=\"utf-16\"?>", "");
            return (Entity)_client.DoHttp<Entity>("PUT", String.Concat("runs/", runId) , xmlBody);
        }

        public DesignStepEntity AddTestStepToTestCase(String testRunId, RunStepEntity testStep)
        { // Only for ALM ver >= 12
            String XmlHead = "<?xml version=\"1.0\" encoding=\"utf-16\"?>";
           // var checkedOutTest = _client.LockTest(@"tests", testRunId);
            //if (checkedOutTest != null && !String.IsNullOrEmpty(checkedOutTest.CheckOutUserName))
            //{
            var status = String.Empty;
            if (testStep.Status.SequenceEqual("pass")) { status = "Passed"; }
            else if (testStep.Status.SequenceEqual("fail")) { status = "Failed"; }
            else if (testStep.Status.SequenceEqual("norun")) { status = "No Run"; }
            else if (testStep.Status.SequenceEqual("notcompleted")) { status = "Not Complited"; }
                var entity = new Entity
                {
                    Type = "run-step",
                    Fields = new List<EntitiesEntityField>
                    {
                        new EntitiesEntityField {Name = "parent-id", Value = testRunId},
                        //new EntitiesEntityField {Name = "desstep-id", Value = testStep.Id},
                        new EntitiesEntityField {Name = "status", Value = status},
                        new EntitiesEntityField {Name = "test-id", Value = testStep.ParentId},
                        new EntitiesEntityField {Name = "name", Value = testStep.Name},
                        new EntitiesEntityField {Name = "description", Value = testStep.Description==null?"":testStep.Description.Replace("@", "").Replace("…", "...").Replace("™", "")},
                        new EntitiesEntityField {Name = "expected", Value = testStep.Expected==null?"":testStep.Expected.Replace("@", "").Replace("…", "...").Replace("™", "")},
                        new EntitiesEntityField {Name = "actual", Value = testStep.Actual==null?"":testStep.Actual.Replace("@", "").Replace("…", "...").Replace("™", "")}
                    }
                };
                var xmlBody = entity.Serialize().Replace(XmlHead, "");

                return (DesignStepEntity)_client.DoHttp<DesignStepEntity>("POST", @"run-steps", xmlBody);
                // }
            //}
            return null;
        }

    }
}
