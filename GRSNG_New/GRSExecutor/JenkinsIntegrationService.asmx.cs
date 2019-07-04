using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Models.Entities;
using GRSExecutor.Support;
using System.Threading.Tasks;

//using GlobalReportingSystem.DataBase;
using Microsoft.Practices.ServiceLocation;


namespace GRSExecutor
{
    /// <summary>
    /// Summary description for JenkinsIntegrator
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
            var testExecution = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>().GetSingleOrDefault(p => p.RemoteExecutionLink == relId);
            if (testExecution == null) throw new SoapException("No tasks were found found for requested execution id.", SoapException.ServerFaultCode);
            if (testExecution.ExecutionType != "linear") throw new SoapException("Jenkins integration works only with linear execution type.", SoapException.ServerFaultCode);
            var isNewLinear = testExecution.isNew == null ? false : (bool)testExecution.isNew;
            if (!isNewLinear)
            {
                var pingResponse = CommonMethods.PingTheClient(null, testExecution.ID);
                if (pingResponse == null) throw new SoapException(string.Format("Machine for tests execution is turned off or GAgent is not installed"), SoapException.ServerFaultCode);
                if (!pingResponse.isSuccess) throw new SoapException(string.Format("Machine for tests execution is busy at the moment. {0}", pingResponse.Message), SoapException.ServerFaultCode);
                CommonMethods.SendRequestToClient(testExecution.Client1.RemoteMachineIP,
                                                    new SocketRequest { RequestType = "removeLastReports" });
                var response = CommonMethods.RunLinearElement(testExecution.ID);
                if (!response.isSuccess) throw new SoapException(string.Format("System was not able to launch tests bacuse {0}", response.Message), SoapException.ServerFaultCode);
            }
            else
            {
                var client = CommonMethods.GetClient(testExecution.Client1.RemoteMachineIP);
                    //Thread t = new Thread(delegate() { client.startLinear(executionId); });
                    //t.Start();
                if (client.ping())
                {
                    client.removeLastReports();
                    
                    Task.Run(delegate
                    {
                        client.startLinear(testExecution.ID);
                    });
                }
                else
                {
                    throw new SoapException("Machine for tests execution is busy at the moment. ", SoapException.ServerFaultCode);
                }
            }
                return testExecution.Client1.RemoteMachineIP + ";" + testExecution.ID;
        }

        [WebMethod]
        public List<string> WaitForResults(string ip, int id)
        {
            var junitReports = new List<string>();

            var testExecution = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>()
                            .GetSingleOrDefault(p => p.ID == id);
            var isNewLinear = testExecution.isNew == null ? false : (bool)testExecution.isNew;
            if (!isNewLinear)
            {
                //ping the client
                var pingResponse = CommonMethods.SendRequestToClient(ip,
                                                    new SocketRequest { RequestType = "ping" });

                if (!pingResponse.isSuccess)
                    return null;
                //grab test suits ids

                var response = CommonMethods.SendRequestToClient(ip,
                                                                   new SocketRequest
                                                                   {
                                                                       RequestType = "getLastReportIds",
                                                                       RequestParameters = id.ToString()
                                                                   });

                if (response.isSuccess)
                {
                    List<string> reportIds = new List<string>();

                    try
                    {
                        reportIds = testExecution.Comments.Split(';').ToList();
                    }
                    catch { }

                    foreach (var reportId in reportIds)
                    {
                        int tsId = 0;
                        Int32.TryParse(reportId, out tsId);
                        var testSuite = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>().GetSingleOrDefault(p => p.ID == tsId);
                        if (testSuite != null)
                        {
                            junitReports.Add(CommonMethods.getJunitTestSuite(testSuite));
                        }
                    }
                }
            }
            else
            {
                var client = CommonMethods.GetClient(ip);
                if(!client.ping())
                {
                    return null;
                }
                if(client.getLastReportIdsLb(id, false))
                {
                    List<string> reportIds = new List<string>();

                    try
                    {
                        reportIds = testExecution.Comments.Split(';').ToList();
                    }
                    catch { }

                    foreach (var reportId in reportIds)
                    {
                        int tsId = 0;
                        Int32.TryParse(reportId, out tsId);
                        var testSuite = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>().GetSingleOrDefault(p => p.ID == tsId);
                        if (testSuite != null)
                        {
                            junitReports.Add(CommonMethods.getJunitTestSuite(testSuite));
                        }
                    }
                }
            }
            return junitReports;
        }
    }
}
