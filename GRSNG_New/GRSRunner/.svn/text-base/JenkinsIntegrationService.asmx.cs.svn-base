using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using GRSRunner.DB;

namespace GRSRunner
{
    /// <summary>
    /// Summary description for JenkinsIntegrationService
    /// </summary>
    [WebService(Namespace = "http://tempuri.org/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class JenkinsIntegrationService : System.Web.Services.WebService
    {
        [WebMethod]
        public string LaunchTestSession(string guid)
        {
            var relId = Guid.Parse(guid);
            using (var db = new GRSDataBaseEntities())
            {
                var testExecution = db.TestsExecutions.SingleOrDefault(p => p.RemoteExecutionLink == relId);
                if (testExecution == null) throw new SoapException("No tasks were found found for requested execution id.", SoapException.ServerFaultCode);
                if (testExecution.ExecutionType != "linear") throw new SoapException("Jenkins integration works only with linear execution type.", SoapException.ServerFaultCode);
                var pingResponse = RenderExecution.PingTheClient(null, testExecution.ID);
                if (pingResponse == null) throw new SoapException(string.Format("Machine for tests execution is turned off or GAgent is not installed"), SoapException.ServerFaultCode);
                if (!pingResponse.isSuccess) throw new SoapException(string.Format("Machine for tests execution is busy at the moment. {0}", pingResponse.Message), SoapException.ServerFaultCode);
                RenderExecution.SendRequestToClient(testExecution.Client1.RemoteMachineIP,
                                                    new SocketRequest { RequestType = "removeLastReports" });
                var response = RenderExecution.RunLinearElement(testExecution.ID, 1);
                if (!response.isSuccess) throw new SoapException(string.Format("System was not able to launch tests bacuse {0}", response.Message), SoapException.ServerFaultCode);
                return testExecution.Client1.RemoteMachineIP;
            }
        }

        [WebMethod]
        public List<string> WaitForResults(string ip)
        {
            var junitReports = new List<string>();
            //ping the client
            var pingResponse = RenderExecution.SendRequestToClient(ip,
                                                new SocketRequest { RequestType = "ping" });
            if (!pingResponse.isSuccess)
                return null;
            //grab test suits ids
            var response = RenderExecution.SendRequestToClient(ip,
                                                               new SocketRequest
                                                               {
                                                                   RequestType = "getLastReportIds"
                                                               });
            if (response.isSuccess)
            {
                var reportIds = response.Message.Split(';');
                foreach (var reportId in reportIds)
                {
                    var guid = Guid.Parse(reportId);
                    using (var db = new GRSDataBaseEntities())
                    {
                        var testSuite = db.TestSuits.SingleOrDefault(p => p.UI == guid);
                        if (testSuite != null)
                        {
                            junitReports.Add(getJunitTestSuite(testSuite));
                        }
                    }
                }

            }
            return junitReports;
        }

        private string getJunitTestSuite(TestSuit testSuite)
        {
            var serTs = new testsuite { tests = testSuite.TestCases.Count, failures = testSuite.TestCases.Count(p => p.TCState != "pass"), timestamp = DateTime.Now };
            var tests = new List<testsuiteTestcase>();
            foreach (var testCase in testSuite.TestCases)
            {
                var time = (decimal)(testCase.TCEndTime - testCase.TCStartTime).TotalSeconds;
                var testToAdd = new testsuiteTestcase { classname = testCase.TestSuit.TSName, name = testCase.TCName + " - " + testCase.TCDescription, time = time };
                if (testCase.TCState != "pass")
                {
                    var message = testCase.TestSteps.Last().StepActual;
                    testToAdd.Item = new testsuiteTestcaseFailure { type = testCase.TCState, Value = message, message = message };
                }
                tests.Add(testToAdd);
            }
            serTs.testcase = tests.ToArray();
            var serializer = new XmlSerializer(typeof(testsuite));
            using (var writer = new StringWriter())
            {
                serializer.Serialize(writer, serTs);
                var xmldoc = writer.ToString().Split('\n').Skip(1);
                return string.Join("\n", xmldoc.ToArray());
            }
        }
    }
}
