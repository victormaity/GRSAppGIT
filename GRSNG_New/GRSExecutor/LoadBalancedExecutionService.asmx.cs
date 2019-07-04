using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Script.Services;
using System.Web.Services;
using System.Xml;
using System.Xml.Serialization;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.Executor;
using System.Text;
using GRSExecutor.Support;
using Microsoft.Practices.ServiceLocation;
//using GlobalReportingSystem.Core.Models.GRS;

namespace GRSExecutor
{
    /// <summary>
    /// Summary description for LoadBalancedExecutionService
    /// </summary>
    [WebService(Namespace = "http://loadbalanced.webclient.agent.grs.thomsonreuters.com/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [System.ComponentModel.ToolboxItem(false)]
    // To allow this Web Service to be called from script, using ASP.NET AJAX, uncomment the following line. 
    // [System.Web.Script.Services.ScriptService]
    public class LoadBalancedExecutionService : System.Web.Services.WebService
    {
        [WebMethod]
        public LoadBalancedElement GetLoadBalancedElement(int id)
        {
            var lbElement = ServiceLocator.Current.GetInstance<IRepository<NewLoadBalanced>>().GetSingleOrDefault(p => p.ID == id);
            if (lbElement != null)
            {
                var item = new LoadBalancedElement();
                item.Machines = new List<Machine>();
                item.Tests = new List<Test>();

                foreach (var machine in ServiceLocator.Current.GetInstance<IRepository<LoadBalancedMachine>>().GetAllToList(p => p.NewLoadBalancedId == id))
                {
                    item.Machines.Add(new Machine()
                    {
                        ID = machine.ID,
                        Account = machine.AccountId ?? 0,
                        Browser = machine.Browser,
                        Configuration = machine.ExecutionConfigurationId ?? 0,
                        Exclude = machine.Exclude,
                        Include = machine.Include,
                        Ip = machine.Client.RemoteMachineIP,
                        Priority = machine.Priority,
                        TargetTestCycle = machine.TargetTestCycle,
                        Profile = machine.EnvironmentId ?? 0
                    });
                }

                foreach (var test in ServiceLocator.Current.GetInstance<IRepository<LoadBalancedTest>>().GetAllToList(p => p.NewLoadBalancedId == id))
                {
                    item.Tests.Add(new Test()
                    {
                        ID = test.ID,
                        Finished = test.FinishedAt,
                        Name = test.TestName,
                        Started = test.StartedAt,
                        Status = test.Status
                    });
                }

                item.Framework = lbElement.FrameworkId.ToString();
                item.ProjectId = lbElement.BelongToProject ?? 0;

                return item;
            }
            return null;
        }

        //[WebMethod]
        //public Machine GetMachineDetails(int modelId, string ip)
        //{
        //    using (var db = new GRSEntities())
        //    {
        //        var model = db.LoadBalanceds.SingleOrDefault(p => p.ID == modelId);
        //        if (model != null)
        //        {
        //            var lbm = new LoadBalancedModel().Deserialize(Encoding.UTF8.GetString(model.SerializableContent));
        //            if (lbm != null)
        //            {
        //                return lbm.Elements.First(p => p.Machines.Exists(z => z.Ip == ip)).Machines.First(p => p.Ip == ip);
        //            }
        //        }
        //    }
        //    return null;
        //}

        [WebMethod]
        public AccountForTestRunModel GetAccount(int id)
        {
            var accountDb = ServiceLocator.Current.GetInstance<IRepository<AccountForTestRun>>().GetSingleOrDefault(p => p.ID == id);
            if (accountDb != null)
            {
                var account = new AccountForTestRunModel
                {
                    AccountName = accountDb.AccountName,
                    ID = accountDb.ID,
                    Comments = accountDb.Comments,
                    UserLogin = accountDb.UserLogin,
                    UserPassword = accountDb.UserPassword
                };
                return account;
            }
            return null;
        }

        [WebMethod]
        public TestCycleModel GetTestCycle(int id)
        {
            var testCyclesDb = ServiceLocator.Current.GetInstance<IRepository<TestCycle>>().GetSingleOrDefault(p => p.ID == id);
            if (testCyclesDb != null)
            {
                var testCycles = new TestCycleModel
                {
                    ID = testCyclesDb.ID,
                    CycleComments = testCyclesDb.CycleComments,
                    CycleEnd = testCyclesDb.CycleEnd,
                    CycleIsCurrent = testCyclesDb.CycleIsCurrent ?? false,
                    CycleName = testCyclesDb.CycleName,
                    CycleStart = testCyclesDb.CycleStart,
                    isInnactive = testCyclesDb.isInnactive ?? false
                };
                return testCycles;
            }
            return null;
        }

        [WebMethod]
        public HostsConfigurationModel GetProfile(int id)
        {
            var configDb = ServiceLocator.Current.GetInstance<IRepository<HostsConfiguration>>().GetSingleOrDefault(p => p.ID == id);
            if (configDb != null)
            {
                var config = new HostsConfigurationModel
                {
                    ApplicationURL = configDb.ApplicationURL,
                    EnvironmentName = configDb.EnvironmentName,
                    HostFileContent = configDb.HostFileContent,
                    ID = configDb.ID,
                    isBackDoor = configDb.isBackDoor ?? false
                };
                return config;
            }
            return null;
        }

        [WebMethod]
        public ExecutionConfigurationModel GetConfiguration(int id)
        {
            var executionConfigurationsDb = ServiceLocator.Current.GetInstance<IRepository<ExecutionConfiguration>>().GetSingleOrDefault(p => p.ID == id);
            if (executionConfigurationsDb != null)
            {
                var executionConfigurations = new ExecutionConfigurationModel
                {
                    ID = executionConfigurationsDb.ID,
                    Content = executionConfigurationsDb.Content,
                    FileName = executionConfigurationsDb.FileName,
                    Name = executionConfigurationsDb.Name
                };
                return executionConfigurations;
            }
            return null;
        }

        [WebMethod]
        public string GetPortion(int elementId, string ip)
        {
            //var model = db.LoadBalanceds.SingleOrDefault(p => p.ID == elementId);
            var newLoadBalancedItems = ServiceLocator.Current.GetInstance<IRepository<NewLoadBalanced>>();
            var model = newLoadBalancedItems.GetSingleOrDefault(p => p.ID == elementId);
            if (model != null)
            {
                //var lbm = new LoadBalancedModel().Deserialize(Encoding.UTF8.GetString(model.SerializableContent));
                //if (lbm != null)
                //{
                //    var innerGuid = Guid.Parse(modelId);
                //    var loadBalancedElement = lbm.Elements.SingleOrDefault(p => p.Id == innerGuid);
                //    if (loadBalancedElement != null)
                //    {
                //var tests = loadBalancedElement.Tests.Where(p => p.Started == null).ToList();
                var tests = model.LoadBalancedTests.Where(p => p.StartedAt == null).ToList();

                if (tests.Count != 0)
                {
                    var theTest = tests.First();
                    theTest.Status = "In progress";
                    theTest.MachineIP = ip;
                    theTest.StartedAt = DateTime.Now;
                    //model.SerializableContent = Encoding.UTF8.GetBytes(lbm.Serialize());
                    newLoadBalancedItems.SaveChanges();
                    return theTest.TestName;
                }
                //    }
                //}
            }
            return null;
        }
        
        [WebMethod]
        public void FinishPortion(int elementId, string portion)
        {
            var newLoadBalancedItems = ServiceLocator.Current.GetInstance<IRepository<NewLoadBalanced>>();
            var model = newLoadBalancedItems.GetSingleOrDefault(p => p.ID == elementId);
            if (model != null)
            {
                //var lbm = new LoadBalancedModel().Deserialize(Encoding.UTF8.GetString(model.SerializableContent));
                //if (lbm != null)
                //{
                //    var innerGuid = Guid.Parse(modelId);
                var loadBalancedElement = model.LoadBalancedTests;
                if (loadBalancedElement != null)
                {
                    var test =
                        loadBalancedElement.FirstOrDefault(p => p.TestName == portion);
                    //.Tests.Find(p => p.Name == portion);
                    if (test != null)
                    {
                        //test.Finished = DateTime.Now;
                        test.FinishedAt = DateTime.Now;
                        test.Status = "Finished";
                        //model.SerializableContent = Encoding.UTF8.GetBytes(lbm.Serialize());
                        newLoadBalancedItems.SaveChanges();
                    }
                }
                //}
            }
        }

        [WebMethod]
        public byte[] GetFramework(int id)
        {
            var r = ServiceLocator.Current.GetInstance<IRepository<FilesStorage>>().GetSingleOrDefault(p => p.ID == id);
            if (r != null)
                return r.FileContent;
            return null;
        }

        [WebMethod]
        public TestExecutionsModel GetTestExecutionById(int id)
        {
            var testExec = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>();
            var execution = testExec.GetSingleOrDefault(p => p.ID == id);

            if (execution != null)
            {
                var testExecutions = new TestExecutionsModel
                {
                    ID = execution.ID,
                    BelongToProject = execution.BelongToProject,
                    AccaptanceCriteria = execution.AccaptanceCriteria,
                    Account = execution.Account,
                    Browser = execution.Browser,
                    CategoriesExclude = execution.CategoriesExclude,
                    CategoriesInclude = execution.CategoriesInclude,
                    Client = execution.Client,
                    Comments = execution.Comments,
                    Configuration = execution.Configuration,
                    Tests = execution.Tests,
                    ProfileHost = execution.ProfileHost,
                    RemoteExecutionLink = execution.RemoteExecutionLink,
                    FrameworkVersion = execution.FrameworkVersion,
                    FailsToAlert = execution.FailsToAlert,
                    Priroty = execution.Priroty,
                    ExecutionType = execution.ExecutionType,
                    Scheduling = execution.Scheduling,
                    TargetTestCycle = execution.TargetTestCycle,
                    LoginPageURL = execution.LoginPageURL
                };
                return testExecutions;
            }
            return null;
        }

        [WebMethod]
        public List<TestExecutionsModel> GetTestExecutionsByIp(string ip, int projectId, string type)
        {
            var testExec = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>();
            var executions = testExec.GetAll(p => p.Client1.RemoteMachineIP == ip && p.BelongToProject == projectId && p.ExecutionType == type).ToList();

            var listTestExecutions = new List<TestExecutionsModel>();

            foreach (var execution in executions)
            {
                listTestExecutions.Add(new TestExecutionsModel
                {
                    ID = execution.ID,
                    BelongToProject = execution.BelongToProject,
                    AccaptanceCriteria = execution.AccaptanceCriteria,
                    Account = execution.Account,
                    Browser = execution.Browser,
                    CategoriesExclude = execution.CategoriesExclude,
                    CategoriesInclude = execution.CategoriesInclude,
                    Client = execution.Client,
                    Comments = execution.Comments,
                    Configuration = execution.Configuration,
                    Tests = execution.Tests,
                    ProfileHost = execution.ProfileHost,
                    RemoteExecutionLink = execution.RemoteExecutionLink,
                    FrameworkVersion = execution.FrameworkVersion,
                    FailsToAlert = execution.FailsToAlert,
                    Priroty = execution.Priroty,
                    ExecutionType = execution.ExecutionType,
                    Scheduling = execution.Scheduling,
                    TargetTestCycle = execution.TargetTestCycle,
                    LoginPageURL = execution.LoginPageURL
                });
            }

            return listTestExecutions;
        }

        [WebMethod]
        public void PutTestRunsByREL(int id, string text)
        {
            var ids = new List<string>();
            var ts = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>();

            if (text != null)
            {
                var guids = text.Split(';').ToList();
                foreach (var guid in guids)
                {
                    try
                    {
                        var g = Guid.Parse(guid);

                        var testSuite = ts.GetSingleOrDefault(p => p.UI == g);
                        testSuite.DedicatedLinearExecution = id;
                        ids.Add(testSuite.ID.ToString());
                    }
                    catch
                    { }
                }
            }
            var tEx = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>();
            tEx.GetSingleOrDefault(p => p.ID == id).Comments = string.Join(";", ids);
            tEx.SaveChanges();
            ts.SaveChanges();
        }

        [WebMethod]
        public void PutTestRunsByRelLb(int id, string text)
        {
            var ids = new List<string>();
            var ts = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>();

            if (text != null)
            {
                var guids = text.Split(';').ToList();
                foreach (var guid in guids)
                {
                    try
                    {
                        var g = Guid.Parse(guid);

                        var testSuite = ts.GetSingleOrDefault(p => p.UI == g);
                        testSuite.DedicatedLbExecution = id;
                        ids.Add(testSuite.ID.ToString());
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
            var lb = ServiceLocator.Current.GetInstance<IRepository<NewLoadBalanced>>();
            lb.GetSingleOrDefault(p => p.ID == id).Comments = string.Join(";", ids);
            lb.SaveChanges();
            ts.SaveChanges();
        }

        private void LogMessage(string message)
        {
            using (var writer = File.AppendText(@"D:\GRS\log.txt"))
            {
                writer.WriteLine(message + " - " + DateTime.Now);
            }
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void StartLoadBalanced(int execution)
        {
            var executionModel = ServiceLocator.Current.GetInstance<IRepository<NewLoadBalanced>>().GetSingleOrDefault(p => p.ID == execution);
            if (executionModel != null)
            {
                var machines = executionModel.LoadBalancedMachines.ToList();
                if (machines.Count == 0) throw new Exception("No machines were added to this load balanced execution");
                if (executionModel.LoadBalancedTests.ToList().Count == 0) throw new Exception("No tests were added to this load balanced execution");
                foreach (var machine in machines)
                {
                    CommonMethods.SendRequestToClient(machine.Client.RemoteMachineIP, new SocketRequest { RequestType = "removeLastReports" });
                    if (CommonMethods.SendRequestToClient(machine.Client.RemoteMachineIP, new SocketRequest { RequestType = "ping" }).isSuccess)
                        CommonMethods.SendRequestToClient(machine.Client.RemoteMachineIP, new SocketRequest
                        {
                            RequestType = "loadbalanced",
                            RequestParameters = execution + "|"
                            //executionModel.ID.ToString() + "|" + execution
                        });

                    Thread.Sleep(5000);
                    //CommonMethods.ExecutionMonitor(0, machine);

                    Task.Run(async delegate
                    {
                        List<int> testSetIds;
                        Report report = null;
                        await Task.Delay(10000);
                        while (report == null)
                        {
                            report = CommonMethods.LoadBalancedResultsWaiter(machine.Client.RemoteMachineIP, executionModel,
                                out testSetIds, false);
                        }
                    });
                }
            }
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string StartREL(int executionId)
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
                        responses = CommonMethods.LoadBalancedResultsWaiter(machineIp, execution, out testSetIds);

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

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public ProjectModel GetProjectById(int id)
        {
            //var model = new ProjectModel();
            //var proj = ServiceLocator.Current.GetInstance<IRepository<Project>>().GetSingleOrDefault(p => p.ID == id);
            //model.DisplayName = proj.DisplayName;
            //model.ProjectName = proj.ProjectName;
            //model.SvnPath = proj.SVNpath;
            //model.SvnLogin = proj.SVNlogin;
            //model.SvnPassword = proj.SVNpassword;
            //return model;
            var model = new ProjectModel();
            var proj = ServiceLocator.Current.GetInstance<IRepository<Project>>().GetSingleOrDefault(p => p.ID == id);
            model.DisplayName = proj.DisplayName;
            model.ProjectName = proj.ProjectName;
            model.IsGITDefault = (proj.IsGITDefault == "Y" || proj.IsGITDefault == "y") ? true : false;
            model.SvnPath = proj.SVNpath;
            model.SvnLogin = proj.SVNlogin;
            model.SvnPassword = proj.SVNpassword;
            model.GitPath = proj.GITPath;
            model.GitLogin = proj.GITUsername;
            model.GitPassword = proj.GITPassword;
            return model;
        }

        //[WebMethod(EnableSession = false)]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        //public Int64 CreateRelExecutionStatus(int ExecutionId, string IP, bool IsExecutionCompleted, string currentstatus, string StatusComment)
        //{
        //    try
        //    {
        //        //Get the execution details                
        //        Int64 lastrowcount = 0;
        //        Int64 lastrowid = 0;
        //        var execution = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>().GetSingleOrDefault(p => p.ID == ExecutionId);
        //        if (execution != null)
        //        {
        //            var totalTestSuitList = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>().GetAllToList(TS => TS.ParentProject == execution.BelongToProject && TS.ParentTestCycle == execution.TargetTestCycle);
        //            List<int?> PCIID = (from x in totalTestSuitList select x.ParentClientInfo).ToList();
        //            var client = ServiceLocator.Current.GetInstance<IRepository<Client>>().GetSingleOrDefault(C => C.ID == execution.Client);
        //            var getAllClientList = ServiceLocator.Current.GetInstance<IRepository<ClientsInformation>>().GetAllToList(CI => PCIID.Contains(CI.ID) && CI.ClientIP == client.RemoteMachineIP);
        //            List<int> CIID = (from x in getAllClientList select x.ID).ToList();
        //            var TSTotalList = (from x in totalTestSuitList where x.ParentClientInfo.HasValue && CIID.Contains(x.ParentClientInfo.Value) select x).ToList();
        //            lastrowcount = (from x in TSTotalList select x.ID).Count();
        //            lastrowid = (from x in TSTotalList select x.ID).Max();
        //        }

        //        RelExecutionStatu relExecutionstatus = new RelExecutionStatu();
        //        relExecutionstatus.ExecutionId = ExecutionId;
        //        relExecutionstatus.MachineIP = IP;
        //        relExecutionstatus.ExecutionStarted = DateTime.Now;
        //        relExecutionstatus.IsExecutionCompleted = IsExecutionCompleted;
        //        if (!string.IsNullOrEmpty(currentstatus))
        //            relExecutionstatus.CurrentStatus = currentstatus;
        //        if (!string.IsNullOrEmpty(StatusComment))
        //            relExecutionstatus.Comment = StatusComment;
        //        relExecutionstatus.LastStatusCheckedAt = (DateTime)System.Data.SqlTypes.SqlDateTime.Null;
        //        relExecutionstatus.CurrentStatusCheckedAt = DateTime.Now;
        //        relExecutionstatus.LastRowCount = lastrowcount;
        //        relExecutionstatus.LastRowId = lastrowid;
        //        var relexecutionlocator = ServiceLocator.Current.GetInstance<IRepository<RelExecutionStatu>>();
        //        relexecutionlocator.Add(relExecutionstatus);
        //        relexecutionlocator.SaveChanges();
        //        return relExecutionstatus.ID;
        //    }
        //    catch
        //    {
        //        return 0;
        //    }
        //}

        //[WebMethod(EnableSession = false)]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]        
        //public Int64 UpdateLatestRelExecutionStatus(Int64 RelStatusID, int ExecutionId, string IP, bool IsExecutionCompleted, string currentstatus, string StatusComment)
        //{
        //    try
        //    {
        //        var relexecutionstatus = ServiceLocator.Current.GetInstance<IRepository<RelExecutionStatu>>();
        //        var data = relexecutionstatus.GetSingleOrDefault(p => p.ID == RelStatusID);
        //        if (relexecutionstatus != null)
        //        {
        //            Int64 newrowcount = 0;
        //            Int64 newrowid = 0;
        //            string testsuitidUpdate = "0";
        //            if (IsExecutionCompleted == true)
        //            {
        //                Thread.Sleep(120000);
        //                //Update database
        //                Int64 lastcount = data.LastRowCount ?? 0;
        //                Int64 lastrowid = data.LastRowId ?? 0;
        //                var testexecution = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>();
        //                var execution = testexecution.GetSingleOrDefault(TE => TE.ID == data.ExecutionId);
        //                if (execution != null)
        //                {
        //                    var testsuits = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>();
        //                    var totalTestSuitList = testsuits.GetAllToList(TS => TS.ParentProject == execution.BelongToProject && TS.ParentTestCycle == execution.TargetTestCycle);
        //                    List<int?> PCIID = (from x in totalTestSuitList select x.ParentClientInfo).ToList();
        //                    var clientinfomation = ServiceLocator.Current.GetInstance<IRepository<ClientsInformation>>();
        //                    var getAllClientList = clientinfomation.GetAllToList(CI => PCIID.Contains(CI.ID) && CI.ClientIP == data.MachineIP);
        //                    List<int> CIID = (from x in getAllClientList select x.ID).ToList();
        //                    var TSTotalList = (from x in totalTestSuitList where x.ParentClientInfo.HasValue && CIID.Contains(x.ParentClientInfo.Value) select x).ToList();
        //                    newrowcount = TSTotalList.Count;
        //                    newrowid = (from x in TSTotalList orderby x.ID descending select x.ID).FirstOrDefault();
        //                    var finaltestsuit = (from x in TSTotalList where x.ID > lastrowid select x).ToList();
        //                    //int counttoupdate = finaltestsuit.Count;
        //                    List<int> TestSuitIDs = (from x in finaltestsuit select x.ID).ToList();
        //                    testsuitidUpdate = string.Join(",", TestSuitIDs);
        //                    //int maxtsid = TestSuitIDs.Max();
        //                    //foreach (var itemts in TestSuitIDs)
        //                    //{
        //                    //    int tsid = itemts;
        //                    //    if (tsid > 0)
        //                    //    {
        //                    //        var testsuitdetails = testsuits.GetSingleOrDefault(TS => TS.ID == tsid);
        //                    //        if (testsuitdetails != null)
        //                    //        {
        //                    //            testsuitdetails.TSStart = data.ExecutionCompleted;
        //                    //            testsuitdetails.DeliveryTime = DateTime.Now;
        //                    //            testsuits.SaveChanges();
        //                    //        }
        //                    //    }
        //                    //}
        //                }
        //            }

        //            if (data.IsExecutionCompleted == false)
        //            {
        //                data.IsExecutionCompleted = IsExecutionCompleted;
        //                data.Comment = StatusComment;
        //                data.LastStatusCheckedAt = data.CurrentStatusCheckedAt;
        //                data.CurrentStatusCheckedAt = DateTime.Now;
        //                if (IsExecutionCompleted == true)
        //                {
        //                    data.NewRowCount = newrowcount;
        //                    data.NewRowId = newrowid;
        //                    data.TestSuitIds = testsuitidUpdate;
        //                    data.CurrentStatus = "Completed";
        //                }
        //                else
        //                {
        //                    data.CurrentStatus = currentstatus;
        //                }
        //                relexecutionstatus.SaveChanges();
        //            }

        //            return data.ID;
        //        }
        //        else
        //        {
        //            //Int64 relexestatusid = CreateRelExecutionStatus(ExecutionId, IP, IsExecutionCompleted, currentstatus, StatusComment);
        //            //return relexestatusid;
        //            return 0;
        //        }
        //    }
        //    catch
        //    {
        //        return 0;
        //    }
        //}

        /*
         
         //public Int64 UpdateLatestRelExecutionStatus(Int64 RelStatusID, int ExecutionId, string IP, bool IsExecutionCompleted, string currentstatus, string StatusComment)
        //{
        //    try
        //    {
        //        var relexecutionstatus = ServiceLocator.Current.GetInstance<IRepository<RelExecutionStatu>>();
        //        var data = relexecutionstatus.GetSingleOrDefault(p => p.ID == RelStatusID);
        //        if (relexecutionstatus != null)
        //        {
        //            Int64 newrowcount = 0;
        //            Int64 newrowid = 0;
        //            string testsuitidUpdate = "0";
        //            if (IsExecutionCompleted == true)
        //            {
        //                Thread.Sleep(120000);
        //                //Update database
        //                Int64 lastcount = data.LastRowCount ?? 0;
        //                Int64 lastrowid = data.LastRowId ?? 0;

        //                var testexecution = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>();
        //                var execution = testexecution.GetSingleOrDefault(TE => TE.ID == data.ExecutionId);
        //                if (execution != null)
        //                {
        //                    var testsuits = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>();
        //                    var totalTestSuitList = testsuits.GetAllToList(TS => TS.ParentProject == execution.BelongToProject && TS.ParentTestCycle == execution.TargetTestCycle);
        //                    List<int?> PCIID = (from x in totalTestSuitList select x.ParentClientInfo).ToList();
        //                    var clientinfomation = ServiceLocator.Current.GetInstance<IRepository<ClientsInformation>>();
        //                    var getAllClientList = clientinfomation.GetAllToList(CI => PCIID.Contains(CI.ID) && CI.ClientIP == data.MachineIP);
        //                    List<int> CIID = (from x in getAllClientList select x.ID).ToList();
        //                    var TSTotalList = (from x in totalTestSuitList where x.ParentClientInfo.HasValue && CIID.Contains(x.ParentClientInfo.Value) select x).ToList();
        //                    newrowcount = TSTotalList.Count;
        //                    newrowid = (from x in TSTotalList orderby x.ID descending select x.ID).FirstOrDefault();
        //                    var finaltestsuit = (from x in TSTotalList where x.ID > lastrowid select x).ToList();
        //                    //int counttoupdate = finaltestsuit.Count;
        //                    List<int> TestSuitIDs = (from x in finaltestsuit select x.ID).ToList();
        //                    testsuitidUpdate = string.Join(",", TestSuitIDs);
        //                    int maxtsid = TestSuitIDs.Max();
        //                    foreach (var itemts in TestSuitIDs)
        //                    {
        //                        int tsid = itemts;
        //                        if (tsid > 0)
        //                        {
        //                            var testsuitdetails = testsuits.GetSingleOrDefault(TS => TS.ID == tsid);
        //                            if (testsuitdetails != null)
        //                            {
        //                                testsuitdetails.TSStart = data.ExecutionCompleted;
        //                                testsuitdetails.DeliveryTime = DateTime.Now;
        //                                data.TestSuitIds = testsuitidUpdate;
        //                                testsuits.SaveChanges();
        //                            }
        //                        }
        //                    }
        //                }
        //            }

        //            if (data.IsExecutionCompleted == false)
        //            {
        //                data.IsExecutionCompleted = IsExecutionCompleted;                        
        //                data.Comment = StatusComment;
        //                data.LastStatusCheckedAt = data.CurrentStatusCheckedAt;
        //                data.CurrentStatusCheckedAt = DateTime.Now;
        //                if (IsExecutionCompleted == true)
        //                {
        //                    data.NewRowCount = newrowcount;
        //                    data.NewRowId = newrowid;

        //                }
        //                relexecutionstatus.SaveChanges();
        //            }

        //            return data.ID;
        //        }
        //        else
        //        {
        //            //Int64 relexestatusid = CreateRelExecutionStatus(ExecutionId, IP, IsExecutionCompleted, currentstatus, StatusComment);
        //            //return relexestatusid;
        //            return 0;
        //        }
        //    }
        //    catch
        //    {
        //        return 0;
        //    }
        //}
         
         */

        //[WebMethod(EnableSession = false)]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        //public void UpdateLatestRelExecutionStatusEntryLevelRecord(Int64 RelStatusID, Int64 LastRowCount, Int64 LastRowId, string ToAddress)
        //{
        //    try
        //    {
        //        var relexecutionstatus = ServiceLocator.Current.GetInstance<IRepository<RelExecutionStatu>>();
        //        var data = relexecutionstatus.GetSingleOrDefault(p => p.ID == RelStatusID);
        //        if (data != null)
        //        {
        //            //data.LastRowCount = LastRowCount;
        //            //data.LastRowId = LastRowId;
        //            data.ToAddress = ToAddress;
        //            relexecutionstatus.SaveChanges();
        //        }
        //    }
        //    catch
        //    {

        //    }
        //}

        //[WebMethod(EnableSession = false)]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        //public void UpdateLatestRelExecutionStatusResultLevelRecord(Int64 RelStatusID, Int64 NewRowCount, Int64 NewRowId, string TestSuitIds)
        //{
        //    try
        //    {
        //        var relexecutionstatus = ServiceLocator.Current.GetInstance<IRepository<RelExecutionStatu>>();
        //        var data = relexecutionstatus.GetSingleOrDefault(p => p.ID == RelStatusID);
        //        if (relexecutionstatus != null)
        //        {
        //            if (data.IsExecutionCompleted == false)
        //            {
        //                var execution = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>().GetSingleOrDefault(p => p.ID == data.ExecutionId);
        //                if (execution != null)
        //                {
        //                    Thread.Sleep(120000);
        //                    var totalTestSuitList = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>().GetAllToList(TS => TS.ParentProject == execution.BelongToProject && TS.ParentTestCycle == execution.TargetTestCycle);
        //                    List<int?> PCIID = (from x in totalTestSuitList select x.ParentClientInfo).ToList();
        //                    var client = ServiceLocator.Current.GetInstance<IRepository<Client>>().GetSingleOrDefault(C => C.ID == execution.Client);
        //                    var getAllClientList = ServiceLocator.Current.GetInstance<IRepository<ClientsInformation>>().GetAllToList(CI => PCIID.Contains(CI.ID) && CI.ClientIP == client.RemoteMachineIP);
        //                    List<int> CIID = (from x in getAllClientList select x.ID).ToList();
        //                    var TSTotalList = (from x in totalTestSuitList where x.ParentClientInfo.HasValue && CIID.Contains(x.ParentClientInfo.Value) select x).ToList();
        //                    List<int> TestSuitIDs = (from x in TSTotalList select x.ID).ToList();
        //                    data.NewRowCount = (from x in TSTotalList select x.ID).Count();
        //                    data.NewRowId = (from x in TSTotalList select x.ID).Max();
        //                    data.TestSuitIds = string.Join(",", TestSuitIDs); ;
        //                }
        //                else
        //                {
        //                    data.NewRowCount = NewRowCount;
        //                    data.NewRowId = NewRowId;
        //                    data.TestSuitIds = TestSuitIds;
        //                }
        //                relexecutionstatus.SaveChanges();
        //            }
        //        }
        //    }
        //    catch
        //    {

        //    }
        //}

        //[WebMethod(EnableSession = false)]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        //public string GetExecutionStatus(Int64 RelStatusID)
        //{
        //    var relexecutionstatus = ServiceLocator.Current.GetInstance<IRepository<RelExecutionStatu>>();
        //    var data = relexecutionstatus.GetSingleOrDefault(p => p.ID == RelStatusID);
        //    if (data != null)
        //    {
        //        return data.IsExecutionCompleted ? "TRUE" : "FALSE";
        //    }
        //    else
        //    {
        //        return "INVALID REL STATUS ID";
        //    }
        //}

        //[WebMethod(EnableSession = false)]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        //public DateTime GetServiceCurrentDateTime()
        //{
        //    return DateTime.Now;
        //}


        /*-----------------------------------------------------------------------*/

        //[WebMethod(EnableSession = false)]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        //public Int64 CreateRelExecutionStatus(int ExecutionId, string IP, bool IsExecutionCompleted, string currentstatus, string StatusComment)
        //{
        //    WriteLogToFile(IP, "Create Rel Execution Status", ExecutionId, "Request to create RelExecutionStatus initiated from GAgent at " + DateTime.Now.ToString("dd/MM/yyyy  hh:mm:ss tt"));
        //    try
        //    {
        //        //Get the execution details                
        //        Int64 lastrowcount = 0;
        //        Int64 lastrowid = 0;
        //        var execution = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>().GetSingleOrDefault(p => p.ID == ExecutionId);
        //        if (execution != null)
        //        {
        //            var client = ServiceLocator.Current.GetInstance<IRepository<Client>>().GetSingleOrDefault(C => C.ID == execution.Client);

        //            var totalTestSuitList = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>().GetAllToList(TS => TS.ParentProject == execution.BelongToProject && TS.ParentTestCycle == execution.TargetTestCycle);
        //            List<int?> PCIID = (from x in totalTestSuitList select x.ParentClientInfo).ToList();

        //            var getAllClientList = ServiceLocator.Current.GetInstance<IRepository<ClientsInformation>>().GetAllToList(CI => PCIID.Contains(CI.ID) && CI.ClientIP == client.RemoteMachineIP);

        //            List<int> CIID = (from x in getAllClientList select x.ID).ToList();

        //            //var TSTotalList = (from x in totalTestSuitList where x.ParentClientInfo.HasValue && CIID.Contains(x.ParentClientInfo.Value) select x).ToList();

        //            var TSTotalList = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>().GetAllToList(
        //                TS => TS.ParentProject == execution.BelongToProject
        //                    && TS.ParentTestCycle == execution.TargetTestCycle
        //                    && TS.ParentClientInfo.HasValue
        //                    && CIID.Contains(TS.ParentClientInfo.Value));

        //            lastrowcount = (from x in TSTotalList select x.ID).Count();
        //            lastrowid = (from x in TSTotalList select x.ID).Max();
        //        }

        //        RelExecutionStatu relExecutionstatus = new RelExecutionStatu();
        //        relExecutionstatus.ExecutionId = ExecutionId;
        //        relExecutionstatus.MachineIP = IP;
        //        relExecutionstatus.ExecutionStarted = DateTime.Now;
        //        relExecutionstatus.IsExecutionCompleted = IsExecutionCompleted;
        //        if (!string.IsNullOrEmpty(currentstatus))
        //            relExecutionstatus.CurrentStatus = currentstatus;
        //        if (!string.IsNullOrEmpty(StatusComment))
        //            relExecutionstatus.Comment = StatusComment;
        //        relExecutionstatus.LastStatusCheckedAt = (DateTime)System.Data.SqlTypes.SqlDateTime.Null;
        //        relExecutionstatus.CurrentStatusCheckedAt = DateTime.Now;
        //        relExecutionstatus.LastRowCount = lastrowcount;
        //        relExecutionstatus.LastRowId = lastrowid;
        //        var relexecutionlocator = ServiceLocator.Current.GetInstance<IRepository<RelExecutionStatu>>();
        //        relexecutionlocator.Add(relExecutionstatus);
        //        relexecutionlocator.SaveChanges();
        //        return relExecutionstatus.ID;
        //    }
        //    catch
        //    {
        //        return 0;
        //    }
        //}

        //[WebMethod(EnableSession = false)]
        //[ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        //public Int64 UpdateLatestRelExecutionStatus(Int64 RelStatusID, int ExecutionId, string IP, bool IsExecutionCompleted, string currentstatus, string StatusComment)
        //{
        //    WriteLogToFile(IP, "Update Latest RelExecution Status", ExecutionId, "Request to update RelExecutionStatus initiated at " + DateTime.Now.ToString("dd/MM/yyyy  hh:mm:ss tt"));
        //    try
        //    {
        //        var relexecutionstatus = ServiceLocator.Current.GetInstance<IRepository<RelExecutionStatu>>();
        //        var data = relexecutionstatus.GetSingleOrDefault(p => p.ID == RelStatusID);
        //        if (relexecutionstatus != null)
        //        {
        //            Int64 newrowcount = 0;
        //            Int64 newrowid = 0;
        //            string testsuitidUpdate = "0";
        //            if (IsExecutionCompleted == true)
        //            {
        //                Thread.Sleep(120000);
        //                //Update database
        //                Int64 lastcount = data.LastRowCount ?? 0;
        //                Int64 lastrowid = data.LastRowId ?? 0;
        //                var testexecution = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>();
        //                var execution = testexecution.GetSingleOrDefault(TE => TE.ID == data.ExecutionId);
        //                if (execution != null)
        //                {
        //                    var testsuits = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>();
        //                    var totalTestSuitList = testsuits.GetAllToList(TS => TS.ParentProject == execution.BelongToProject && TS.ParentTestCycle == execution.TargetTestCycle);
        //                    List<int?> PCIID = (from x in totalTestSuitList select x.ParentClientInfo).ToList();
        //                    var clientinfomation = ServiceLocator.Current.GetInstance<IRepository<ClientsInformation>>();
        //                    var getAllClientList = clientinfomation.GetAllToList(CI => PCIID.Contains(CI.ID) && CI.ClientIP == data.MachineIP);
        //                    List<int> CIID = (from x in getAllClientList select x.ID).ToList();
        //                    //var TSTotalList = (from x in totalTestSuitList where x.ParentClientInfo.HasValue && CIID.Contains(x.ParentClientInfo.Value) select x).ToList();

        //                    var TSTotalList = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>().GetAllToList(
        //                        TS => TS.ParentProject == execution.BelongToProject
        //                            && TS.ParentTestCycle == execution.TargetTestCycle
        //                            && TS.ParentClientInfo.HasValue
        //                            && CIID.Contains(TS.ParentClientInfo.Value));

        //                    newrowcount = (from x in TSTotalList select x.ID).Count();
        //                    newrowid = (from x in TSTotalList select x.ID).Max();
        //                    var finaltestsuit = (from x in TSTotalList where x.ID > lastrowid select x).ToList();
        //                    //int counttoupdate = finaltestsuit.Count;
        //                    List<int> TestSuitIDs = (from x in finaltestsuit select x.ID).ToList();
        //                    testsuitidUpdate = string.Join(",", TestSuitIDs);
        //                    //int maxtsid = TestSuitIDs.Max();
        //                    //foreach (var itemts in TestSuitIDs)
        //                    //{
        //                    //    int tsid = itemts;
        //                    //    if (tsid > 0)
        //                    //    {
        //                    //        var testsuitdetails = testsuits.GetSingleOrDefault(TS => TS.ID == tsid);
        //                    //        if (testsuitdetails != null)
        //                    //        {
        //                    //            testsuitdetails.TSStart = data.ExecutionCompleted;
        //                    //            testsuitdetails.DeliveryTime = DateTime.Now;
        //                    //            testsuits.SaveChanges();
        //                    //        }
        //                    //    }
        //                    //}
        //                }
        //            }

        //            if (data.IsExecutionCompleted == false)
        //            {
        //                data.IsExecutionCompleted = IsExecutionCompleted;
        //                data.Comment = StatusComment;
        //                data.LastStatusCheckedAt = data.CurrentStatusCheckedAt;
        //                data.CurrentStatusCheckedAt = DateTime.Now;
        //                if (IsExecutionCompleted == true)
        //                {
        //                    data.NewRowCount = newrowcount;
        //                    data.NewRowId = newrowid;
        //                    data.TestSuitIds = testsuitidUpdate;
        //                    data.CurrentStatus = "Completed";
        //                }
        //                else
        //                {
        //                    data.CurrentStatus = currentstatus;
        //                }
        //                relexecutionstatus.SaveChanges();
        //            }

        //            return data.ID;
        //        }
        //        else
        //        {
        //            //Int64 relexestatusid = CreateRelExecutionStatus(ExecutionId, IP, IsExecutionCompleted, currentstatus, StatusComment);
        //            //return relexestatusid;
        //            return 0;
        //        }
        //    }
        //    catch
        //    {
        //        return 0;
        //    }
        //}

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Int64 CreateRelExecutionStatus(int ExecutionId, string IP, bool IsExecutionCompleted, string currentstatus, string StatusComment)
        {
            WriteLogToFile(IP, "Create Rel Execution Status", ExecutionId, "Request to create RelExecutionStatus initiated from GAgent at " + DateTime.Now.ToString("dd/MM/yyyy  hh:mm:ss tt"));
            try
            {
                //Get the execution details                
                Int64 lastrowcount = 0;
                Int64 lastrowid = 0;
                var execution = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>().GetSingleOrDefault(p => p.ID == ExecutionId);
                if (execution != null)
                {
                    WriteLogToFile(IP, "Create Rel Execution Status", ExecutionId, "Valid Execution ID. Checked at " + DateTime.Now.ToString("dd/MM/yyyy  hh:mm:ss tt"));
                    var client = ServiceLocator.Current.GetInstance<IRepository<Client>>().GetSingleOrDefault(C => C.ID == execution.Client);

                    var totalTestSuitList = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>().GetAllToList(TS => TS.ParentProject == execution.BelongToProject && TS.ParentTestCycle == execution.TargetTestCycle);
                    List<int?> PCIID = (from x in totalTestSuitList select x.ParentClientInfo).ToList();

                    var getAllClientList = ServiceLocator.Current.GetInstance<IRepository<ClientsInformation>>().GetAllToList(CI => PCIID.Contains(CI.ID) && CI.ClientIP == client.RemoteMachineIP);

                    List<int> CIID = (from x in getAllClientList select x.ID).ToList();

                    //var TSTotalList = (from x in totalTestSuitList where x.ParentClientInfo.HasValue && CIID.Contains(x.ParentClientInfo.Value) select x).ToList();

                    var TSTotalList = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>().GetAllToList(
                        TS => TS.ParentProject == execution.BelongToProject
                            && TS.ParentTestCycle == execution.TargetTestCycle
                            && TS.ParentClientInfo.HasValue
                            && CIID.Contains(TS.ParentClientInfo.Value));

                    System.Threading.Thread.Sleep(5000);
                    if (TSTotalList.Count > 0)
                    {
                        lastrowcount = (from x in TSTotalList select x.ID).Count();
                        lastrowid = (from x in TSTotalList select x.ID).Max();
                    }
                }

                WriteLogToFile(IP, "Create Rel Execution Status", ExecutionId, "Moment to create new tracking row. Initiated at " + DateTime.Now.ToString("dd/MM/yyyy  hh:mm:ss tt"));
                RelExecutionStatu relExecutionstatus = new RelExecutionStatu();
                relExecutionstatus.ExecutionId = ExecutionId;
                relExecutionstatus.MachineIP = IP;
                relExecutionstatus.ExecutionStarted = DateTime.Now;
                relExecutionstatus.IsExecutionCompleted = IsExecutionCompleted;
                if (!string.IsNullOrEmpty(currentstatus))
                    relExecutionstatus.CurrentStatus = currentstatus;
                if (!string.IsNullOrEmpty(StatusComment))
                    relExecutionstatus.Comment = StatusComment;
                relExecutionstatus.LastStatusCheckedAt = (DateTime)System.Data.SqlTypes.SqlDateTime.Null;
                relExecutionstatus.CurrentStatusCheckedAt = DateTime.Now;
                relExecutionstatus.LastRowCount = lastrowcount;
                relExecutionstatus.LastRowId = lastrowid;
                var relexecutionlocator = ServiceLocator.Current.GetInstance<IRepository<RelExecutionStatu>>();
                relexecutionlocator.Add(relExecutionstatus);
                relexecutionlocator.SaveChanges();
                WriteLogToFile(IP, "Create Rel Execution Status", ExecutionId, "New Tracking Row Created. Row Id is " + relExecutionstatus.ID + " Checked at " + DateTime.Now.ToString("dd/MM/yyyy  hh:mm:ss tt"));
                return relExecutionstatus.ID;

            }
            catch (Exception ex)
            {
                var st = new StackTrace(ex, true);
                var frame = st.GetFrame(st.FrameCount-1);
                var line = frame.GetFileLineNumber();
                WriteLogToFile(IP, "Create Rel Execution Status", ExecutionId, "Failed to create new tracking row. Exception Occurred. Exception Message: " + ex.Message + ". Exception Line Number: " + Convert.ToString(line) + ". Checked at " + DateTime.Now.ToString("dd/MM/yyyy  hh:mm:ss tt"));
                //WriteLogToFile(IP, "Create Rel Execution Status", ExecutionId, "Failed to create new tracking row. Exception Occurred. Exception Message: " + ex.Message + " Checked at " + DateTime.Now.ToString("dd/MM/yyyy  hh:mm:ss tt"));
                return 0;
            }
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public Int64 UpdateLatestRelExecutionStatus(Int64 RelStatusID, int ExecutionId, string IP, bool IsExecutionCompleted, string currentstatus, string StatusComment)
        {
            WriteLogToFile(IP, "Update Latest RelExecution Status", ExecutionId, "Request to update RelExecutionStatus initiated at " + DateTime.Now.ToString("dd/MM/yyyy  hh:mm:ss tt"));
            try
            {
                var relexecutionstatus = ServiceLocator.Current.GetInstance<IRepository<RelExecutionStatu>>();
                var data = relexecutionstatus.GetSingleOrDefault(p => p.ID == RelStatusID);
                if (relexecutionstatus != null)
                {
                    WriteLogToFile(IP, "Update Latest RelExecution Status", ExecutionId, "Valid relExecutionStatus ID. Checked at " + DateTime.Now.ToString("dd/MM/yyyy  hh:mm:ss tt"));
                    Int64 newrowcount = 0;
                    Int64 newrowid = 0;
                    string testsuitidUpdate = "0";
                    if (IsExecutionCompleted == true)
                    {
                        WriteLogToFile(IP, "Update Latest RelExecution Status", ExecutionId, "Execution Completed. Updating entry in DB. Checked at " + DateTime.Now.ToString("dd/MM/yyyy  hh:mm:ss tt"));
                        Thread.Sleep(120000);
                        //Update database
                        Int64 lastcount = data.LastRowCount ?? 0;
                        Int64 lastrowid = data.LastRowId ?? 0;
                        var testexecution = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>();
                        var execution = testexecution.GetSingleOrDefault(TE => TE.ID == data.ExecutionId);
                        if (execution != null)
                        {
                            var testsuits = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>();
                            var totalTestSuitList = testsuits.GetAllToList(TS => TS.ParentProject == execution.BelongToProject && TS.ParentTestCycle == execution.TargetTestCycle);
                            List<int?> PCIID = (from x in totalTestSuitList select x.ParentClientInfo).ToList();
                            var clientinfomation = ServiceLocator.Current.GetInstance<IRepository<ClientsInformation>>();
                            var getAllClientList = clientinfomation.GetAllToList(CI => PCIID.Contains(CI.ID) && CI.ClientIP == data.MachineIP);
                            List<int> CIID = (from x in getAllClientList select x.ID).ToList();
                            //var TSTotalList = (from x in totalTestSuitList where x.ParentClientInfo.HasValue && CIID.Contains(x.ParentClientInfo.Value) select x).ToList();

                            var TSTotalList = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>().GetAllToList(
                                TS => TS.ParentProject == execution.BelongToProject
                                    && TS.ParentTestCycle == execution.TargetTestCycle
                                    && TS.ParentClientInfo.HasValue
                                    && CIID.Contains(TS.ParentClientInfo.Value));

                            newrowcount = (from x in TSTotalList select x.ID).Count();
                            newrowid = (from x in TSTotalList select x.ID).Max();
                            var finaltestsuit = (from x in TSTotalList where x.ID > lastrowid select x).ToList();
                            //int counttoupdate = finaltestsuit.Count;
                            List<int> TestSuitIDs = (from x in finaltestsuit select x.ID).ToList();
                            testsuitidUpdate = string.Join(",", TestSuitIDs);
                            //int maxtsid = TestSuitIDs.Max();
                            //foreach (var itemts in TestSuitIDs)
                            //{
                            //    int tsid = itemts;
                            //    if (tsid > 0)
                            //    {
                            //        var testsuitdetails = testsuits.GetSingleOrDefault(TS => TS.ID == tsid);
                            //        if (testsuitdetails != null)
                            //        {
                            //            testsuitdetails.TSStart = data.ExecutionCompleted;
                            //            testsuitdetails.DeliveryTime = DateTime.Now;
                            //            testsuits.SaveChanges();
                            //        }
                            //    }
                            //}
                        }
                    }

                    if (data.IsExecutionCompleted == false)
                    {
                        data.IsExecutionCompleted = IsExecutionCompleted;
                        data.Comment = StatusComment;
                        data.LastStatusCheckedAt = data.CurrentStatusCheckedAt;
                        data.CurrentStatusCheckedAt = DateTime.Now;
                        if (IsExecutionCompleted == true)
                        {
                            data.NewRowCount = newrowcount;
                            data.NewRowId = newrowid;
                            data.TestSuitIds = testsuitidUpdate;
                            data.CurrentStatus = "Completed";
                        }
                        else
                        {
                            data.CurrentStatus = currentstatus;
                        }
                        relexecutionstatus.SaveChanges();
                    }

                    WriteLogToFile(IP, "Update Latest RelExecution Status", ExecutionId, "Request to update RelExecutionStatus. Status ID: " + data.ID + ". Checked at " + DateTime.Now.ToString("dd/MM/yyyy  hh:mm:ss tt"));

                    return data.ID;
                }
                else
                {
                    WriteLogToFile(IP, "Update Latest RelExecution Status", ExecutionId, "Invalid relExecutionStatus ID. Checked at " + DateTime.Now.ToString("dd/MM/yyyy  hh:mm:ss tt"));                    
                    return 0;
                }
            }
            catch (Exception ex)
            {
                WriteLogToFile(IP, "Update Latest RelExecution Status", ExecutionId, "Exception Occurred while updating entry. Exception Message: " + ex.Message + ". Exception Time: " + DateTime.Now.ToString("dd/MM/yyyy  hh:mm:ss tt"));
                return 0;
            }
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void UpdateLatestRelExecutionStatusEntryLevelRecord(Int64 RelStatusID, Int64 LastRowCount, Int64 LastRowId, string ToAddress)
        {
            string ip = "";
            int executionid = 0;
            try
            {
                var relexecutionstatus = ServiceLocator.Current.GetInstance<IRepository<RelExecutionStatu>>();
                var data = relexecutionstatus.GetSingleOrDefault(p => p.ID == RelStatusID);
                if (data != null)
                {
                    //data.LastRowCount = LastRowCount;
                    //data.LastRowId = LastRowId;
                    data.ToAddress = ToAddress;
                    relexecutionstatus.SaveChanges();
                    ip = data.MachineIP;
                    executionid = data.ExecutionId;
                }
            }
            catch
            {

            }
            WriteLogToFile(ip, "Update Latest RelExecution Status Entry Level", executionid, "Request to update RelExecutionStatus initiated at " + DateTime.Now.ToString("dd/MM/yyyy  hh:mm:ss tt"));
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public void UpdateLatestRelExecutionStatusResultLevelRecord(Int64 RelStatusID, Int64 NewRowCount, Int64 NewRowId, string TestSuitIds)
        {
            string ip = "";
            int executionid = 0;
            try
            {
                var relexecutionstatus = ServiceLocator.Current.GetInstance<IRepository<RelExecutionStatu>>();
                var data = relexecutionstatus.GetSingleOrDefault(p => p.ID == RelStatusID);
                ip = data.MachineIP;
                executionid = data.ExecutionId;
                if (relexecutionstatus != null)
                {
                    if (data.IsExecutionCompleted == false)
                    {
                        var execution = ServiceLocator.Current.GetInstance<IRepository<TestsExecution>>().GetSingleOrDefault(p => p.ID == data.ExecutionId);
                        if (execution != null)
                        {
                            Thread.Sleep(120000);
                            var totalTestSuitList = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>().GetAllToList(TS => TS.ParentProject == execution.BelongToProject && TS.ParentTestCycle == execution.TargetTestCycle);
                            List<int?> PCIID = (from x in totalTestSuitList select x.ParentClientInfo).ToList();
                            var client = ServiceLocator.Current.GetInstance<IRepository<Client>>().GetSingleOrDefault(C => C.ID == execution.Client);
                            var getAllClientList = ServiceLocator.Current.GetInstance<IRepository<ClientsInformation>>().GetAllToList(CI => PCIID.Contains(CI.ID) && CI.ClientIP == client.RemoteMachineIP);
                            List<int> CIID = (from x in getAllClientList select x.ID).ToList();
                            //var TSTotalList = (from x in totalTestSuitList where x.ParentClientInfo.HasValue && CIID.Contains(x.ParentClientInfo.Value) select x).ToList();

                            var TSTotalList = ServiceLocator.Current.GetInstance<IRepository<TestSuit>>().GetAllToList(
                                TS => TS.ParentProject == execution.BelongToProject
                                    && TS.ParentTestCycle == execution.TargetTestCycle
                                    && TS.ParentClientInfo.HasValue
                                    && CIID.Contains(TS.ParentClientInfo.Value));

                            List<int> TestSuitIDs = (from x in TSTotalList select x.ID).ToList();
                            data.NewRowCount = (from x in TSTotalList select x.ID).Count();
                            data.NewRowId = (from x in TSTotalList select x.ID).Max();
                            data.TestSuitIds = string.Join(",", TestSuitIDs);
                            data.CurrentStatus = "Completed";
                        }
                        else
                        {
                            data.NewRowCount = NewRowCount;
                            data.NewRowId = NewRowId;
                            data.TestSuitIds = TestSuitIds;
                            data.CurrentStatus = "Completed";
                        }
                        relexecutionstatus.SaveChanges();
                    }
                }
            }
            catch
            {

            }
            WriteLogToFile(ip, "Update Latest RelExecution Status Result Level", executionid, "Request to update RelExecutionStatus initiated at " + DateTime.Now.ToString("dd/MM/yyyy  hh:mm:ss tt"));
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public string GetExecutionStatus(Int64 RelStatusID)
        {
            var relexecutionstatus = ServiceLocator.Current.GetInstance<IRepository<RelExecutionStatu>>();
            var data = relexecutionstatus.GetSingleOrDefault(p => p.ID == RelStatusID);
            if (data != null)
            {
                return data.IsExecutionCompleted ? "TRUE" : "FALSE";
            }
            else
            {
                return "INVALID REL STATUS ID";
            }
        }

        [WebMethod(EnableSession = false)]
        [ScriptMethod(ResponseFormat = ResponseFormat.Json)]
        public DateTime GetServiceCurrentDateTime()
        {
            return DateTime.Now;
        }

        private void WriteLogToFile(string MachineIP, string MessageFrom, int ExecutionId, string LogMessage)
        {
            try
            {
                //string path = @"E:\Jenkins\workspace\GRSNG\GRSExecutor\LogFiles\";
                string path = @"D:\Jenkins\workspace\GRSNG\GRSExecutor\LogFiles\";
                string logFileName = "Web_Service_Log_" + MachineIP + ".txt";
                string logFilePath = path + logFileName;
                if (!File.Exists(logFilePath))
                {
                    File.Create(logFilePath);
                }
                string message = Environment.NewLine;
                message += Environment.NewLine;
                message += "------------------------" + string.Format("Date & Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")) + "------------------------";
                message += Environment.NewLine;
                message += Environment.NewLine;
                message += "Request from IP: " + MachineIP;
                message += Environment.NewLine;
                message += "Message request method from: " + MessageFrom;
                message += Environment.NewLine;
                message += "Execution ID: " + ExecutionId;
                message += Environment.NewLine;
                message += "Log: " + LogMessage;
                message += Environment.NewLine;
                message += Environment.NewLine;
                message += "-----------------------------------------------------------------------------------";
                message += Environment.NewLine;
                using (StreamWriter writer = new StreamWriter(logFilePath, true))
                {
                    writer.WriteLine(message);
                    writer.Close();
                }
            }
            catch
            { }
        }

    }
}
