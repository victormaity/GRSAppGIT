using System;
using System.Net;
using NUnit.Framework;
using QcClient.Entities.XmlEntities;
using QcClient.Tools;

namespace QcClient.UnitTests
{
    [TestFixture]
    public class BasicFeatures
    {

        [Test]
        public void Test()
        {
            using (var client = new QCClient.Client.RestClient())
            {
                const string login = "anna.grachova";
                const string passw = "addmin1!";

                client.WebClient = new CookieWebClient(new CookieContainer()) { Credentials = new NetworkCredential(login, passw) };
                client.WebClient.Headers.Add("Content-Type", "application/xml");
                client.WebClient.Headers.Add("Accept", "application/xml");
                client.ServerUrl = "http://10.206.16.33:8080/";
                client.Domain = "INNOVATION";
                client.Project = "CPC";
                client.Auth(login, passw);

                try
                {
                    const string testSetName = "X-Test01";
                    const string testStepName1 = "X-Step01";

                    // -------------------CREATE-TESTPLAN-------------------------------

                    var testData = new TestEntity { Name = "X-Ovo-Test", Status = "Passed", Owner = "1", Description = "2" };

                    var obj = client.TestPlan.CreateFolder(testSetName);
                    var test = client.TestPlan.AddTestCasesToTestPlanFolder(obj.Id, testData,client.GetNameOfPriorityField());

                    var tstep = new DesignStepEntity {Name = testStepName1, Description = "1", Expected = "2"};
                    client.TestPlan.AddTestStepToTestCase(test.Id, tstep);

                    // ------------------CREATE--TESTLAB--------------------------------

                    var f1 = client.TestLab.CreateFolder("Release 777");
                    var f2 = client.TestLab.CreateFolder(testSetName, f1.Id);

                    var testset = client.TestLab.CreateTestSet(testSetName, f2.Id);

                    // -------------------TESTPLAN -> binding -> TESTLAB-----------------

                    var inst = client.TestLab.AddTestToTestSet(test, testset);

                    // --------------------TESTRUN---------------------------------------

                    var trun = client.TestRun.AddTestRun(inst, test, testset);
                }
                catch (Exception ex) {}
            }
        }
    }
}
