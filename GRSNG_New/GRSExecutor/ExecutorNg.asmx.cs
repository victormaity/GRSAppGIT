using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Script.Services;
using System.Web.Services;
using GlobalReportingSystem.Core.Models.Executor;
using GRSExecutor.GrsAgent;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Models.Entities;
using GRSExecutor.Support;
using Microsoft.Practices.ServiceLocation;
using Exception = System.Exception;
using System.Xml.Serialization;
using System.IO;
using System.Xml;

namespace GRSExecutor
{
    /// <summary>
    /// Summary description for ExecutorNg
    /// </summary>
    [WebService(Namespace = "http://ng.executor.webclient.agent.grs.thomsonreuters.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class ExecutorNg : System.Web.Services.WebService
    {
        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string PingById(int id)
        {
            var singleOrDefault = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>().GetSingleOrDefault(p => p.ID == id);
            if (singleOrDefault != null)
            {
                var client = CommonMethods.GetClient(singleOrDefault.Client1.RemoteMachineIP);
                return client.ping() ? "free" : "busy";
            }
            return null;
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string PingByIp(string ip)
        {
            try
            {
                var client = CommonMethods.GetClient(ip);
                return client.ping() ? "free" : "busy";
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void StartLinear(int executionId)
        {
            try
            {
                var singleOrDefault = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>().GetSingleOrDefault(p => p.ID == executionId);
                if (singleOrDefault != null)
                {
                    var client = CommonMethods.GetClient(singleOrDefault.Client1.RemoteMachineIP);
                    //Thread t = new Thread(delegate() { client.startLinear(executionId); });
                    //t.Start();
                    if (client.ping())
                    {
                        client.startLinear(executionId);
                    }
                    else
                    {
                        throw new Exception("Execution wasn't started because ping command wasn't received");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void StartLoadBalanced(int executionId)
        {
            try
            {
                CommonMethods.LogMessage("Inside ExecutorNg");
                int pingCount = 0;
                var executionModel =
                    ServiceLocator.Current.GetInstance<IRepository<NewLoadBalanced>>()
                        .GetSingleOrDefault(p => p.ID == executionId);
                if (executionModel != null)
                {
                    var machines = executionModel.LoadBalancedMachines.ToList();
                    if (machines.Count == 0)
                        throw new Exception("No machines were added to this load balanced execution");
                    if (executionModel.LoadBalancedTests.ToList().Count == 0)
                        throw new Exception("No tests were added to this load balanced execution");
                    foreach (var machine in machines)
                    {
                        CommonMethods.LogMessage("For ip: " + machine.Client.RemoteMachineIP);
                        var client = CommonMethods.GetClient(machine.Client.RemoteMachineIP);
                        if (client.ping())
                        {
                            pingCount ++;
                            client.removeLastReportsAsync();
                            client.startLoadbalancedAsync(executionId);

                            Thread.Sleep(5000);
                            //CommonMethods.ExecutionMonitor(0, machine);

                            Task.Run(async delegate
                            {
                                List<int> testSetIds;
                                Report report = null;
                                await Task.Delay(10000);
                                while (report == null)
                                {
                                    report = CommonMethods.LoadBalancedResultsWaiter_newAgent(client,
                                        executionModel,
                                        out testSetIds, false);
                                }
                            });
                        }
                    }
                    if (pingCount != machines.Count)
                    {
                        throw new Exception("For some machines execution wasn't started because ping command wasn't received");
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void Stop(string ip)
        {
            try
            {
                var client = CommonMethods.GetClient(ip);
                client.stop();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        //[WebMethod(EnableSession = false)]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        //public string WaitForLinearRelResults(int executionId)
        //{
        //    Report responses = null;

        //    while (responses == null)
        //    {
        //        try
        //        {
        //            List<int> testSetIds;
        //            var execution = CommonMethods.GetLinearExecutionById(executionId);
        //            var machineIp = execution.Client1.RemoteMachineIP;
        //            responses = CommonMethods.LinearResultsWaiter(machineIp, execution, out testSetIds);
        //            if (responses != null)
        //            {
        //                XmlSerializer xsSubmit = new XmlSerializer(responses.GetType());
        //                StringWriter sww = new StringWriter();
        //                XmlWriter writer = XmlWriter.Create(sww);
        //                xsSubmit.Serialize(writer, responses);
        //                var xml = sww.ToString();

        //                try
        //                {
        //                    CommonMethods.SendNolioExcelReport(testSetIds, xml, executionId);
        //                }
        //                catch
        //                {
        //                }
        //                return xml;
        //            }
        //        }
        //        catch
        //        { return null; }
        //    }
        //    return null;
        //}

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string WaitForLinearRelResults(int executionId)
        {
            Report responses = null;

            while (responses == null)
            {
                try
                {
                    List<int> testSetIds;
                    var execution = CommonMethods.GetLinearExecutionById(executionId);
                    var machineIp = execution.Client1.RemoteMachineIP;
                    responses = CommonMethods.LinearResultsWaiter(machineIp, execution, out testSetIds);
                    if (responses != null)
                    {
                        XmlSerializer xsSubmit = new XmlSerializer(responses.GetType());
                        StringWriter sww = new StringWriter();
                        XmlWriter writer = XmlWriter.Create(sww);
                        xsSubmit.Serialize(writer, responses);
                        var xml = sww.ToString();

                        try
                        {
                            CommonMethods.SendNolioExcelReport(testSetIds, xml, executionId);
                        }
                        catch
                        {
                        }
                        return xml;
                    }
                }
                catch
                { return null; }
            }
            return null;
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string WaitForLbRelResults(int executionId)
        {
            //StartLoadBalanced(executionId);

            try
            {
                List<int> testSetIds;
                var execution = CommonMethods.GetLbExecutionById(executionId);
                var machineIps = execution.LoadBalancedMachines.Select(p => p.Client.RemoteMachineIP);

                var allReports = new Report();
                var allTestSetIds = new List<int>();

                foreach (var machineIp in machineIps)
                {
                    Report responses = null;
                    while (responses == null)
                    {
                        var client = CommonMethods.GetClient(machineIp);
                        responses = CommonMethods.LoadBalancedResultsWaiter_newAgent(client, execution, out testSetIds);

                        if (responses != null)
                        {
                            allTestSetIds.AddRange(testSetIds);
                            allReports.TestSuiteField.AddRange(responses.TestSuiteField);
                        }
                    }
                }

                XmlSerializer xsSubmit = new XmlSerializer(allReports.GetType());
                StringWriter sww = new StringWriter();
                XmlWriter writer = XmlWriter.Create(sww);
                xsSubmit.Serialize(writer, allReports);
                var xml = sww.ToString();

                try
                {
                    CommonMethods.SendNolioExcelReport(allTestSetIds, xml, executionId);
                }
                catch
                {
                }
                return xml;
            }
            catch
            { return null; }
        }
    }
}
