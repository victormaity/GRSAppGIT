using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Xml.Serialization;
using Castle.Core.Internal;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Office2010.PowerPoint;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem;
using GlobalReportingSystem.Core.Models.Executor;
using GlobalReportingSystem.Core.Models.GRS;
using GRSExecutor.GrsAgent;
using Microsoft.Practices.ServiceLocation;
using System.Data;
using Ionic.Zip;
using Mono.Cecil;
using Mono.Cecil.Cil;
using Exception = System.Exception;

namespace GRSExecutor.Support
{
    public class CommonMethods
    {
        public static SocketResponse RunLinearElement(int executionId)
        {
            var testRun = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>().GetSingleOrDefault(p => p.ID == executionId);
            SendRequestToClient(testRun.Client1.RemoteMachineIP, new SocketRequest { RequestType = "removeLastReports" });
            if (SendRequestToClient(testRun.Client1.RemoteMachineIP, new SocketRequest { RequestType = "ping" }).isSuccess)
            {

                Task.Run(async delegate
                {
                    List<int> testSetIds;
                    Report report = null;
                    await Task.Delay(10000);
                    while (report == null)
                    {
                        report = CommonMethods.LinearResultsWaiter(testRun.Client1.RemoteMachineIP, testRun, out testSetIds, false);
                        //await Task.Delay(10000);
                    }
                });

                return SendRequestToClient(testRun.Client1.RemoteMachineIP,
                    new SocketRequest { RequestType = "linear", RequestParameters = executionId.ToString() });
            }
            return null;
        }

        public static SocketResponse SendRequestToClient(string IP, SocketRequest Request)
        {
            var tcpClient = new TcpClientWithTimeout(IP, 56565, 5000);
            var clientSocket = tcpClient.Connect();
            clientSocket.SendTimeout = 100000;
            clientSocket.ReceiveTimeout = 150000;
            var serverStream = clientSocket.GetStream();
            var request = Request.Serialize();
            byte[] outStream = Encoding.ASCII.GetBytes(request);
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();

            byte[] inStream = new byte[524288];
            serverStream.Read(inStream, 0, inStream.Length);
            string returndata = Encoding.ASCII.GetString(inStream);
            tcpClient.Disconnect();
            return new SocketResponse().Deserialize(returndata);
        }

        public static List<string> GetArrayOfTests(NunitTests Tests)
        {
            return (from testFix in Tests.TestFixtures from testcase in testFix.Tests select testFix.Namespace + "." + testFix.Name + "." + testcase).ToList();
        }

        public static SocketResponse PingTheClient(string IP, int ID)
        {
            var singleOrDefault = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>().GetSingleOrDefault(p => p.ID == ID);

            var clientItems = ServiceLocator.Current.GetInstance<IRepository<Client>>();
            var client = singleOrDefault != null ? singleOrDefault.Client1 : clientItems.GetSingleOrDefault(p => p.RemoteMachineIP == IP);
            if (client != null)
            {
                var response = SendRequestToClient(client.RemoteMachineIP, new SocketRequest { RequestType = "ping", });
                //update client
                if (response.clientInfo != null)
                {
                    client.Firefox = response.clientInfo.ff;
                    client.Ie = response.clientInfo.ie;
                    client.Chome = response.clientInfo.ch;
                    client.windowsversion = response.clientInfo.windows;
                    client.freespace = response.clientInfo.freespace;
                    clientItems.SaveChanges();
                }
                return response;
            }
            return null;
        }

        public static void RunSchenario(int machineId)
        {
            var theClient = ServiceLocator.Current.GetInstance<IRepository<Client>>().GetSingleOrDefault(p => p.ID == machineId);
            var response = SendRequestToClient(theClient.RemoteMachineIP, new SocketRequest { RequestType = "executeScenario", RequestParameters = theClient.BelongToProject.ToString() });
            if (!response.isSuccess) throw new Exception(response.Message);
        }

        public static string getJunitTestSuite(TestSuit testSuite)
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
        } // From JenkinsExecutor

        public static void SendNolioExcelReport(List<int> testSetIds, string report, int executionId)
        {

            //_theSession = db.isSessionActive(Request);

            List<TestSuit> testSuits = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>().GetAll(p => testSetIds.Contains(p.ID)).ToList();
            var project = testSuits.First().Project;

            /*var listForExport = new List<ExportToExcel>();
                        
            foreach (var testSuit in testSuits)
            {
                var path = ServiceLocator.Current.GetInstance<IRepository<QCExportAssignment>>().GetSingleOrDefault(
                        p => p.BelongToProject == project.ID && p.TestSetName == testSuit.TSName);

                listForExport.AddRange(testSuit.TestCases.Select(test => new ExportToExcel
                {
                    BusinessCriticality = test.Criticality,
                    Description = test.TCDescription,
                    Steps = string.Join("\n", test.TestSteps.Select(p => p.StepDescription)),
                    TestName = test.TCName,
                    Subject = path != null ? path.QCPath : "",
                    TCState = test.TCState,
                    ClientIP = testSuit.ClientsInformation.ClientIP,
                    EndTime = testSuit.ClientsInformation.EndTime.ToString(),
                    Attachments = string.IsNullOrEmpty(test.TCAttachments)
                        ? ""
                        : string.Join("\r\n",
                            test.TCAttachments.Split(';')
                                .Select(
                                    p =>
                                        ("http://" + HttpContext.Current.Request.Url.Authority + "/nggrs/harvest/Attachments/" + project.ID + "/" + p))),
                    Screenshots = string.Join("\r\n", GetScreenShots(test).
                        Select(
                            p =>
                                ("http://" + HttpContext.Current.Request.Url.Authority + "/nggrs/harvest/Screenshots/" +
                                 p)))
                }));
            }

            //export to excel
            var workbook = new XLWorkbook();
            workbook.Worksheets.Add(listForExport.ToDataTable(), "Export");
            var fileName = Guid.NewGuid() + ".xlsx";
            workbook.SaveAs(HttpContext.Current.Server.MapPath("~") + "/Reports/" + fileName);

            //export fails to excel
            var workbookError = new XLWorkbook();
            workbookError.Worksheets.Add(listForExport.Where(p => p.TCState != "pass").ToList().ToDataTable(), "Exported Fails");
            var fileNameError = Guid.NewGuid() + ".xlsx";
            workbookError.SaveAs(HttpContext.Current.Server.MapPath("~") + "/Reports/" + fileNameError);

            string downloadLinkFull, downloadLinkError;
            downloadLinkFull = String.Concat("<a href='", "http://", HttpContext.Current.Request.Url.Host, ":", HttpContext.Current.Request.Url.Port, HttpContext.Current.Request.ApplicationPath, "/Reports/", fileName,
                "'>Download full EXCEL report</a><br/><br/><br/>");
            downloadLinkError = String.Concat("<a href='", "http://", HttpContext.Current.Request.Url.Host, ":", HttpContext.Current.Request.Url.Port, HttpContext.Current.Request.ApplicationPath, "/Reports/", fileNameError,
                "'>Download EXCEL report for failed results</a><br/><br/><br/>");
            */
            var message = Resource.SimpleEmailTemplate;
            message = message.Replace("%main_text%", HttpUtility.HtmlEncode(report));

            //ServiceLocator.Current.GetInstance<IEmailer>().SendMail(project.Accesses.Where(p => p.User != null).Select(p => p.User.UserEmail).ToList(),
            ServiceLocator.Current.GetInstance<IEmailer>().SendMail(ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>().GetSingleOrDefault(p => p.ID == executionId).Subscribers.Split(',').ToList(),
                "grs@clarivate.com",
                "[Global Reporting System - " + "Acceptance testing results " + (project == null ? "" : project.ProjectName) +
                "] ", message, new string[] { }, true);
        }

        public static List<string> GetScreenShots(TestCase testCase)
        {
            var list = new List<string>();

            CollectionExtensions.ForEach(testCase.TestSteps, p =>
                                CollectionExtensions.ForEach(p.SubSteps.Where(g => !string.IsNullOrEmpty(g.SubStepScreenShotDriver)), g => list.Add(g.SubStepScreenShotDriver)));
            return list;
        }

        public static Report LinearResultsWaiter(string ip, TestsExecution execution, out List<int> tsIds, bool isREL = true, string host = null, string path = null)
        {
            var junitReports = new Report();
            tsIds = new List<int>();
            var exec = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>();

            //ping the client
            var pingResponse = SendRequestToClient(ip,
                                                new SocketRequest { RequestType = "ping" });
            if (!pingResponse.isSuccess)
            {
                exec.GetSingleOrDefault(p => p.ID == execution.ID).Status = "Running";
                exec.SaveChanges();
                Thread.Sleep(120000);
                return null;
            }

            Thread.Sleep(120000);
            //grab test suits ids
            var response = SendRequestToClient(ip,
                                                               new SocketRequest
                                                               {
                                                                   RequestType = "getLastReportIds",
                                                                   RequestParameters = execution.ID.ToString()
                                                               });

            if (response.isSuccess && isREL)
            {
                List<string> reportIds = new List<string>();// = (response.Message as string).Split(';');

                try
                {
                    reportIds = exec
                        .GetSingleOrDefault(p => p.ID == execution.ID).Comments.Split(';').ToList();
                }
                catch
                { }

                var tCycleLink = "http://" + (host ?? HttpContext.Current.Request.Url.Host) +
                        ":" + HttpContext.Current.Request.Url.Port +
                        (path ?? HttpContext.Current.Request.ApplicationPath.Replace("/executor", "")) +
                        "/PublicAccess/Report";

                junitReports.TestSuiteField = FillJunitReports(reportIds, tCycleLink, out tsIds);
            }

            exec.GetSingleOrDefault(p => p.ID == execution.ID).Status = "Finished";

            Task.Run(async delegate
            {
                await Task.Delay(10000);
                ChangeStatusAction(execution.ID);
            });
            return junitReports;
        }

        public static void LogMessage(string message)
        {
            using (var writer = File.AppendText(@"D:\GRS\log.txt"))
            {
                writer.WriteLine(DateTime.Now + " : " + message);
            }
        }
        
        private static void ChangeStatusAction(int execution)
        {
            var exec = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>();
            exec.GetSingleOrDefault(p => p.ID == execution).Status = "Ready";
            exec.SaveChanges();
        }

        public static TestsExecution GetLinearExecutionById(int id)
        {
            return ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>().GetSingleOrDefault(p => p.ID == id);
        }

        public static NewLoadBalanced GetLbExecutionById(int id)
        {
            return ServiceLocator.Current.GetInstance<IRepository<NewLoadBalanced>>().GetSingleOrDefault(p => p.ID == id);
        }

        public static Report LoadBalancedResultsWaiter(string ip, NewLoadBalanced execution, out List<int> tsIds, bool isREL = true, string host = null, string path = null)
        {
            var junitReports = new Report();

            tsIds = new List<int>();
            //ping the client
            var pingResponse = SendRequestToClient(ip,
                                                new SocketRequest { RequestType = "ping" });
            if (!pingResponse.isSuccess)
            {
                Thread.Sleep(120000);
                return null;
            }

            Thread.Sleep(120000);

            pingResponse = SendRequestToClient(ip,
                                    new SocketRequest { RequestType = "ping" });
            if (!pingResponse.isSuccess)
            {
                Thread.Sleep(10000);
                return null;
            }

            //grab test suits ids
            var response = SendRequestToClient(ip,
                                                               new SocketRequest
                                                               {
                                                                   RequestType = "getLastReportIdsLb",
                                                                   RequestParameters = execution.ID.ToString()
                                                               });

            Thread.Sleep(10000);

            if (response.isSuccess && isREL)
            {
                List<string> reportIds = new List<string>();// = (response.Message as string).Split(';');

                try
                {
                    reportIds = ServiceLocator.Current.GetInstance<IRepository<NewLoadBalanced>>()
                        .GetSingleOrDefault(p => p.ID == execution.ID).Comments.Split(';').ToList();
                }
                catch
                { }


                string tc = execution.FilesStorage.TestsExecutions.First().TargetTestCycle.ToString();

                var tCycleLink = "http://" + (host ?? HttpContext.Current.Request.Url.Host) +
                        ":" + HttpContext.Current.Request.Url.Port +
                        (path ?? HttpContext.Current.Request.ApplicationPath.Replace("/executor", "")) +
                        "/TestCycle/Index/" + tc;

                junitReports.TestSuiteField = FillJunitReports(reportIds, tCycleLink, out tsIds);

            }
            return junitReports;
        }

        public static Report LoadBalancedResultsWaiter_newAgent(AgentWebMethodsClient client, NewLoadBalanced execution, out List<int> tsIds, bool isREL = true, string host = null, string path = null)
        {
            var junitReports = new Report();

            tsIds = new List<int>();

            var pingResponse = client.ping();
            if (!pingResponse)
            {
                Thread.Sleep(120000);
                return null;
            }

            Thread.Sleep(120000);

            pingResponse = client.ping();
            if (!pingResponse)
            {
                Thread.Sleep(10000);
                return null;
            }

            LogMessage("Gathering reports");
            var response = client.getLastReportIdsLb(execution.ID, true);
            LogMessage("Reports were gathered: " + response);

            Thread.Sleep(10000);

            if (response && isREL)
            {
                List<string> reportIds = new List<string>();// = (response.Message as string).Split(';');

                try
                {
                    reportIds = ServiceLocator.Current.GetInstance<IRepository<NewLoadBalanced>>()
                        .GetSingleOrDefault(p => p.ID == execution.ID).Comments.Split(';').ToList();
                }
                catch
                { }


                string tc = execution.FilesStorage.TestsExecutions.First().TargetTestCycle.ToString();

                var tCycleLink = "http://" + (host ?? HttpContext.Current.Request.Url.Host) +
                        ":" + HttpContext.Current.Request.Url.Port +
                        (path ?? HttpContext.Current.Request.ApplicationPath.Replace("/executor", "")) +
                        "/TestCycle/Index/" + tc;

                junitReports.TestSuiteField = FillJunitReports(reportIds, tCycleLink, out tsIds);

            }
            return junitReports;
        }

        public static Report LinearResultsWaiter_newAgent(AgentWebMethodsClient client, TestsExecution execution, out List<int> tsIds, bool isREL = true, string host = null, string path = null)
        {
            var junitReports = new Report();
            tsIds = new List<int>();
            var exec = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>();

            var pingResponse = client.ping();
            if (!pingResponse)
            {
                Thread.Sleep(120000);
                return null;
            }

            Thread.Sleep(120000);

            pingResponse = client.ping();
            if (!pingResponse)
            {
                Thread.Sleep(10000);
                return null;
            }

            LogMessage("Gathering reports");
            //grab test suits ids
            var response = client.getLastReportIdsLb(execution.ID, false);
            LogMessage("Reports were gathered: " + response);

            Thread.Sleep(10000);

            if (response && isREL)
            {
                List<string> reportIds = new List<string>();// = (response.Message as string).Split(';');

                try
                {
                    reportIds = exec
                        .GetSingleOrDefault(p => p.ID == execution.ID).Comments.Split(';').ToList();
                }
                catch
                { }

                var tCycleLink = "http://" + (host ?? HttpContext.Current.Request.Url.Host) +
                        ":" + HttpContext.Current.Request.Url.Port +
                        (path ?? HttpContext.Current.Request.ApplicationPath.Replace("/executor", "")) +
                        "/PublicAccess/Report";

                junitReports.TestSuiteField = FillJunitReports(reportIds, tCycleLink, out tsIds);
            }

            exec.GetSingleOrDefault(p => p.ID == execution.ID).Status = "Finished";

            Task.Run(async delegate
            {
                await Task.Delay(10000);
                ChangeStatusAction(execution.ID);
            });
            return junitReports;
        }

        private static List<TestSuite> FillJunitReports(List<string> reportsList, string link, out List<int> tsIds)
        {
            tsIds = new List<int>();

            var testSuites = new List<TestSuite>();

            foreach (var reportId in reportsList)
            {
                try
                {
                    int id;
                    Int32.TryParse(reportId, out id);
                    tsIds.Add(id);
                    var testSuite = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>().GetSingleOrDefault(p => p.ID == id);
                    if (testSuite != null)
                    {
                        TestSuite ts = new TestSuite();
                        ts.TestSuiteName = testSuite.TSName;
                        ts.RunTime = testSuite.TSStart.ToString();
                        ts.Total = testSuite.TestCases.Count.ToString();
                        ts.Pass = testSuite.TestCases.Count(p => p.TCState == "pass").ToString();
                        ts.Fail = testSuite.TestCases.Count(p => p.TCState == "fail").ToString();
                        ts.Blocker = testSuite.TestCases.Count(p => p.TCState == "notcompleted").ToString();
                        ts.NoRun = testSuite.TestCases.Count(p => p.TCState == "norun").ToString();
                        ts.QuickView = link != null ? link + "?testSuite=" + testSuite.ID : "";
                        //int bCount = 0;
                        testSuites.Add(ts);
                    }
                }
                catch
                { }
            }

            return testSuites;
        }

        public int GetNunitPath(int tcId)
        {
            var tc = ServiceLocator.Current.GetInstance<IRepository<TestCase>>().GetFirstOrDefault(p => p.ID == tcId);
            if (tc == null)
                return 0;
            var targetQcId = tc.qc_id;
            var executionLinear = tc.TestSuit.TestsExecution;
            var executionLb = tc.TestSuit.NewLoadBalanced;
            var executionId = 0;

            if (targetQcId != null && (executionLinear != null || executionLb != null))
            {
                var proj = tc.TestSuit.Project;

                string[] filePaths = Directory.GetFiles(@"D:\GRS\SVN_NEW2\" + proj.ProjectName, "*.cs",
                                         SearchOption.AllDirectories);
                string reg = "namespace (.+)[\\r\\s\\{a-zA-Z=\\(.\\);:\",<>0-9_'\\/\\+\\}\\[\\]]+public void (.+)\\(\\)[\\r\\s\\{a-zA-Z=\\(.\\);\",<>0-9_'\\/\\+]+\\.addTestCase\\(.+, +.+, +.+, +(" + targetQcId + ")\\);";

                foreach (var file in filePaths)
                {
                    using (StreamReader sr = new StreamReader(@file))
                    {
                        string contents = sr.ReadToEnd();
                        //"namespace (.)+[\s\{a-zA-Z=\(.\);:",0-9_'\/\\+\}\[\]]+public void (.)+[\s\{a-zA-Z=\(.\);",0-9_'\/\\+]+\.addTestCase\(.+, +.+, +.+, +(\d+)\);";

                        Match match = Regex.Match(contents, reg, RegexOptions.CultureInvariant);

                        if (match.Success)
                        {
                            var nameSpace = match.Groups[1].Value.Replace("\r","");
                            var testCase = match.Groups[2].Value.Replace("\r", "");
                            //var qcId = match.Groups[3].Value;

                            //if (qcId == targetQcId)
                            //{
                                if (executionLinear != null)
                                {
                                    executionId = AddExecution(proj.ID, String.Concat(nameSpace, ".", testCase),
                                        executionLinear.Browser, executionLinear.FrameworkVersion ?? default(int));
                                }
                                else if (executionLb != null)
                                {
                                    executionId = AddExecution(proj.ID, String.Concat(nameSpace, ".", testCase),
                                        executionLb.LoadBalancedMachines.First().Browser,
                                        executionLb.FrameworkId ?? default(int));
                                }
                                break;
                            //}
                        }
                    }
                }
            }

            return executionId;
        }

        private static int AddExecution(int pId, string test, string browser, int frameworkId)
        {
            var testsExecution = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>();
            testsExecution.Add(new TestsExecution
            {
                BelongToProject = pId,
                ExecutionType = "linear",
                RemoteExecutionLink = Guid.NewGuid(),
                Tests = test,
                Browser = browser,
                Priroty = "",
                CategoriesExclude = "",
                CategoriesInclude = "",
                FrameworkVersion = frameworkId
            });
            testsExecution.SaveChanges();
            return testsExecution.GetLastOrDefault(p => p.ID != null).ID;
        }

        public static AgentWebMethodsClient GetClient(string ip)
        {
            return new AgentWebMethodsClient("AgentWebMethodsPort",
              string.Format("http://{0}:3195/", ip));
        }

    }
}