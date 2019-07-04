using System;
using System.Web.Mvc;
using System.Collections.Generic;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Constants;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Web.UI.ExecutorNg;
using GRSExecutor.Support;
using System.Threading;
using GlobalReportingSystem.Core.Models.GRS;
using System.Linq;
using System.IO;
using System.Web.Script.Serialization;


namespace GlobalReportingSystem.Web.UI.Controllers
{
    public class LaunchController : Controller
    {
        private readonly IManageLaunchProvider _launchProvider;
        private readonly IManageExecutionProvider _executionProvider;
        private readonly IMangaeRelReportProvider _relreportProvider;
        private readonly IEmailer _emailer;
        private readonly IManageUserProvider _userProvider;
        private readonly IManageConfigurationProvider _configurationProvider;

        public LaunchController(IManageLaunchProvider launchProvider, IManageExecutionProvider executionProvider,
            IMangaeRelReportProvider relreportProvider, IEmailer emailer, IManageUserProvider userProvider,
            IManageConfigurationProvider configurationProvider)
        {
            _launchProvider = launchProvider;
            _executionProvider = executionProvider;
            _relreportProvider = relreportProvider;
            _emailer = emailer;
            _userProvider = userProvider;
            _configurationProvider = configurationProvider;
        }

        [Authorize]
        public ActionResult Linear()
        {
            var linearExec = _launchProvider.GetLinearExecutions(User, Server.MapPath("~"));

            return View(linearExec);
        }

        [Authorize]
        public ActionResult LoadBalanced(int? id)
        {
            var lbExec = _launchProvider.GetLoadBalancedExecutions(User, id, Server.MapPath("~"));

            return View(lbExec);
        }

        [Authorize]
        public ActionResult AddLinearElement()
        {
            _launchProvider.AddTestsExecution(User);
            return RedirectToAction("Linear");
        }
        [Authorize]
        public ActionResult AddLiearTests(int executionId, string tests, string key, string isJava)
        {
            try
            {
                _launchProvider.AddLiearTests(executionId, tests, key, Server.MapPath("~"), bool.Parse(isJava));
                return Json("");
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        [Authorize]
        public ActionResult AddLinearCucumberTests(int executionId, string features, string tags)
        {
            try
            {

                _launchProvider.AddLinearCucumberTests(executionId, features, tags);
                return Json("");
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddLbTests(int sectionId, string tests, string key, string isJava)
        {
            _launchProvider.AddLoadBalancedTests(sectionId, tests, key, Server.MapPath("~"), bool.Parse(isJava));
            return Json("");
        }

        [Authorize]
        public ActionResult RenderTests()
        {
            try
            {
                return PartialView("TestsForFramework", _launchProvider.GetTests(User, Server.MapPath("~")));
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        [Authorize]
        public ActionResult DownloadChildren(int id, string parent)
        {
            try
            {
                return PartialView("ChildrenNodes", _launchProvider.GetChildren(User, Server.MapPath("~"), parent));
            }
            catch (Exception ex)
            {
                return
                    Json(
                        new
                        {
                            type = "Error",
                            text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message)
                        });
            }
        }

        [Authorize]
        public string PingLinear(int executionId)
        {
            try
            {
                var validationMessage = _launchProvider.ValidateRelExecutionBeforeSendRequest(executionId);
                if (string.IsNullOrEmpty(validationMessage))
                {
                    var executor = new ExecutorNgSoapClient();
                    return (executor.PingById(executionId));
                }
                else
                {
                    return "ERROR " + validationMessage;
                }
            }
            catch (Exception)
            {
                return "null";
            }
        }

        [Authorize]
        public string PingLb(string mashineIp)
        {
            try
            {
                var executor = new ExecutorNgSoapClient();
                return (executor.PingByIp(mashineIp));
            }
            catch (Exception)
            {
                return "null";
            }
        }

        [Authorize]
        public ActionResult StartLinear(int executionId)
        {
            string status, value;
            try
            {
                var executor = new ExecutorNgSoapClient();
                executor.StartLinear(executionId);

                status = "Message";
                value = Messages.StartExecutionMessage;
            }
            catch (Exception ex)
            {
                status = "Error";
                value = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                value = value.Length > 461 ? (value.Substring(0, 458) + "...") : (value);
            }
            return Json(new { type = status, text = value });
        }

        [Authorize]
        public ActionResult StartLoadBalanced(int id)
        {
            string status, value;
            try
            {
                LogMessage("Incide StartLoadBalanced Launch");
                var executor = new ExecutorNgSoapClient();
                LogMessage("Client was created");
                LogMessage("Id is: " + id);
                //GRSExecutor.LoadBalancedExecutionService();
                executor.StartLoadBalanced(id);

                status = "Message";
                value = Messages.StartExecutionMessage;
            }
            catch (Exception ex)
            {
                status = "Error";
                value = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message);
            }
            return Json(new { type = status, text = value });
        }
        private static void LogMessage(string message)
        {
            using (var writer = System.IO.File.AppendText(@"D:\GRS\log.txt"))
            {
                writer.WriteLine(message + " - " + DateTime.Now);
            }
        }
        [Authorize]
        public ActionResult StopExecution(int executionId, string type)
        {
            var status = "Message";
            var value = String.Empty;
            var stopped = new List<string>();
            var failed = new List<string>();
            try
            {
                var machines = _launchProvider.GetMachineIp(executionId, type);
                var executor = new ExecutorNgSoapClient();
                foreach (var machine in machines)
                {
                    try
                    {
                        executor.Stop(machine);
                        stopped.Add(machine);
                    }
                    catch (Exception ex)
                    {
                        failed.Add(machine);
                        var msg = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                        LogMessage(msg.Length > 461 ? (msg.Substring(0, 458) + "...") : (msg));
                    }
                }
                if (failed.Count > 0)
                {
                    value = String.Concat("Execution wasn't stopped on: ", String.Join("; ", failed.ToArray()), " due to errors occurred");
                }
                if (stopped.Count > 0)
                {
                    value += String.Concat("<br />", Messages.StopExecutionMessage, " on: ", String.Join("; ", stopped.ToArray()));
                }
            }
            catch (Exception ex)
            {
                status = "Error";
                value = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                value = value.Length > 461 ? (value.Substring(0, 458) + "...") : (value);

            }
            return Json(new { type = status, text = value });
        }

        [Authorize]
        public JsonResult Stop(int imageId)
        {
            try
            {
                var machineIp = _launchProvider.GetMachineIpById(imageId);
                var executor = new ExecutorNgSoapClient();
                executor.Stop(machineIp);
                return Json(new { type = "Message", text = "Execution was stopped for " + machineIp + " image" });
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        [Authorize]
        public JsonResult GetCategoriesForFramework()
        {
            try
            {
                var categoriesDistinct = _launchProvider.GetCategories(User, Server.MapPath("~"));
                return Json(categoriesDistinct);
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        /*DO NOT DELETE*/
        //public JsonResult RunRel(string type, string rel, string vm, string env, string account)
        //{
        //    try
        //    {
        //        DateTime StartDateTime = DateTime.Now;
        //        DateTime EndDateTime = DateTime.Now;
        //        if (type == "linear" || type == null)
        //        {
        //            int executionId = _executionProvider.GetLinearIdByRel(rel, vm, env, account);
        //            int testsuitId = 0;

        //            List<TestSuit> getalltestsuit = _relreportProvider.GetTestSuitOnRelAndVM(rel, vm);
        //            var initialtotaltestsuitcount = getalltestsuit.Count;
        //            Int64 lastTestsuitID = (from x in getalltestsuit select x.ID).Max();

        //            Int64 finaltcount = initialtotaltestsuitcount;
        //            if (executionId != 0)
        //            {
        //                var executor = new ExecutorNgSoapClient();
        //                StartDateTime = DateTime.Now;
        //                executor.StartLinear(executionId);
        //                string message = null;
        //                try
        //                {
        //                    //while(message == null) 
        //                    finaltcount = _relreportProvider.TotalTestSuitCount(rel, vm);
        //                    while (finaltcount <= initialtotaltestsuitcount)
        //                    {
        //                        try
        //                        {
        //                            message = executor.WaitForLinearRelResults(executionId);
        //                        }
        //                        catch
        //                        {
        //                        }
        //                        if (message != null)
        //                        {
        //                            //_executionProvider.SubmitResponse(message, false);
        //                        }
        //                        else
        //                        {
        //                            Thread.Sleep(30000);
        //                        }

        //                        finaltcount = _relreportProvider.TotalTestSuitCount(rel, vm);
        //                    }

        //                    EndDateTime = DateTime.Now;
        //                    var testsuitdetails = _relreportProvider.GetLastTestAsListSuitBasedOnRelAndVM(rel, vm);
        //                    if (testsuitdetails.Count > 0) { foreach (var item in testsuitdetails) { testsuitId = item.ID; } }
        //                    if (testsuitId > 0)
        //                    {
        //                        _relreportProvider.UpdateTestSuitStartEndDateTime(testsuitId, StartDateTime, EndDateTime);
        //                    }
        //                    return Json((new RelReturn() { ExecutionId = executionId, TestSuitId = testsuitId, Message = "" }), JsonRequestBehavior.AllowGet);
        //                }
        //                catch (Exception ex)
        //                {
        //                    //_executionProvider.SubmitResponse(string.Empty, true);                            
        //                    return Json((new RelReturn() { ExecutionId = 0, TestSuitId = 0, Message = ex.Message }), JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //            else
        //            {
        //                //return Json(ex.Message, JsonRequestBehavior.AllowGet);
        //                return Json((new RelReturn() { ExecutionId = 0, TestSuitId = 0, Message = "Invalid REL link." }), JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        else if (type == "lb")
        //        {
        //            var executionId = _executionProvider.GetLoadBalanceIdByRel(rel);
        //            if (executionId != 0)
        //            {
        //                var executor = new ExecutorNgSoapClient();
        //                //GRSExecutor.LoadBalancedExecutionService();
        //                executor.StartLoadBalanced(executionId);
        //                executor.WaitForLbRelResults(executionId);
        //            }
        //            return Json((new RelReturn() { ExecutionId = 0, TestSuitId = 0, Message = "This is Load balanced REL link." }), JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            return Json((new RelReturn() { ExecutionId = 0, TestSuitId = 0, Message = "Invalid REL link." }), JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        //_executionProvider.SubmitResponse(ex.Message, true);                
        //        return Json((new RelReturn() { ExecutionId = 0, TestSuitId = 0, Message = ex.Message }), JsonRequestBehavior.AllowGet);
        //    }
        //}

        //public JsonResult RunRel(string type, string rel, string vm, string env, string account)
        //{
        //    try
        //    {
        //        DateTime StartDateTime = DateTime.Now;
        //        DateTime EndDateTime = DateTime.Now;
        //        if (type == "linear" || type == null)
        //        {
        //            int executionId = _executionProvider.GetLinearIdByRel(rel, vm, env, account);
        //            List<TestSuit> getalltestsuit = _relreportProvider.GetTestSuitOnRelAndVM(rel, vm);
        //            var initialtotaltestsuitcount = getalltestsuit.Count;
        //            Int64 lastTestsuitID = 0;
        //            if (initialtotaltestsuitcount > 0)
        //            {
        //                lastTestsuitID = (from x in getalltestsuit select x.ID).Max();
        //            }
        //            Int64 finaltcount = initialtotaltestsuitcount;
        //            if (executionId != 0)
        //            {
        //                var executor = new ExecutorNgSoapClient();
        //                StartDateTime = DateTime.Now;
        //                executor.StartLinear(executionId);
        //                EndDateTime = DateTime.Now;
        //                Thread.Sleep(30000);
        //                List<TestSuit> FinalAllTestsuit = _relreportProvider.GetTestSuitOnRelAndVM(rel, vm);

        //                if (FinalAllTestsuit.Count > initialtotaltestsuitcount)
        //                {
        //                    Thread.Sleep(15000);
        //                    FinalAllTestsuit = _relreportProvider.GetTestSuitOnRelAndVM(rel, vm);
        //                    List<RelReturn> NewTestsuit = (from x in FinalAllTestsuit
        //                                                   where x.ID > lastTestsuitID
        //                                                   select new RelReturn
        //                                                   {
        //                                                       ExecutionId = executionId,
        //                                                       TestSuitId = x.ID,
        //                                                       Message = vm + "-000"
        //                                                   }).ToList();

        //                    foreach (var item in NewTestsuit)
        //                    {
        //                        int tsid = item.TestSuitId;
        //                        if (tsid > 0)
        //                            _relreportProvider.UpdateTestSuitStartEndDateTime(tsid, StartDateTime, DateTime.Now);
        //                    }

        //                    return Json(NewTestsuit, JsonRequestBehavior.AllowGet);
        //                }
        //                else
        //                {
        //                    List<RelReturn> NewTestsuit = new List<RelReturn>();
        //                    NewTestsuit.Add(new RelReturn()
        //                    {
        //                        ExecutionId = executionId,
        //                        TestSuitId = 0,
        //                        Message = vm + "-101"
        //                    });
        //                    return Json(NewTestsuit, JsonRequestBehavior.AllowGet);
        //                }
        //            }
        //            else
        //            {
        //                List<RelReturn> NewTestsuit = new List<RelReturn>();
        //                NewTestsuit.Add(new RelReturn()
        //                {
        //                    ExecutionId = executionId,
        //                    TestSuitId = 0,
        //                    Message = vm + "-102"
        //                });
        //                return Json(NewTestsuit, JsonRequestBehavior.AllowGet);
        //            }
        //        }
        //        else if (type == "lb")
        //        {
        //            var executionId = _executionProvider.GetLoadBalanceIdByRel(rel);
        //            if (executionId != 0)
        //            {
        //                var executor = new ExecutorNgSoapClient();
        //                executor.StartLoadBalanced(executionId);
        //                executor.WaitForLbRelResults(executionId);
        //            }

        //            List<RelReturn> NewTestsuit = new List<RelReturn>();
        //            NewTestsuit.Add(new RelReturn()
        //            {
        //                ExecutionId = executionId,
        //                TestSuitId = 0,
        //                Message = vm + "-103"
        //            });
        //            return Json(NewTestsuit, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            List<RelReturn> NewTestsuit = new List<RelReturn>();
        //            NewTestsuit.Add(new RelReturn()
        //            {
        //                ExecutionId = 0,
        //                TestSuitId = 0,
        //                Message = vm + "-102"
        //            });
        //            return Json(NewTestsuit, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.Message.ToString().ToUpper().Contains("SYSTEM.WEB.SERVICES.PROTOCOLS.SOAPEXCEPTION: SERVER WAS UNABLE TO PROCESS REQUEST."))
        //        {
        //            List<RelReturn> NewTestsuit = new List<RelReturn>();
        //            NewTestsuit.Add(new RelReturn()
        //            {
        //                ExecutionId = 0,
        //                TestSuitId = 0,
        //                Message = vm + "-105"
        //            });
        //            return Json(NewTestsuit, JsonRequestBehavior.AllowGet);
        //        }
        //        else
        //        {
        //            List<RelReturn> NewTestsuit = new List<RelReturn>();
        //            NewTestsuit.Add(new RelReturn()
        //            {
        //                ExecutionId = 0,
        //                TestSuitId = 0,
        //                Message = vm + "-104"
        //            });
        //            return Json(NewTestsuit, JsonRequestBehavior.AllowGet);
        //        }
        //    }
        //}

        //public JsonResult RunRel(string type, string rel, string vm, string env, string account)
        //{
        //    try
        //    {
        //        if (type == "linear" || type == null)
        //        {
        //            int executionId = _executionProvider.GetLinearIdByRel(rel, vm, env, account);
        //            List<TestSuit> getalltestsuit = _relreportProvider.GetTestSuitOnRelAndVM(rel, vm);
        //            var initialtotaltestsuitcount = getalltestsuit.Count;
        //            Int64 lastTestsuitID = 0;
        //            if (initialtotaltestsuitcount > 0)
        //            {
        //                lastTestsuitID = (from x in getalltestsuit select x.ID).Max();
        //            }
        //            Int64 finaltcount = initialtotaltestsuitcount;
        //            if (executionId != 0)
        //            {
        //                var executor = new ExecutorNgSoapClient();
        //                var timeservice = new GlobalReportingSystem.Web.UI.LoadBalancedExecutionService.LoadBalancedExecutionServiceSoapClient();
        //                DateTime StartDateTime = timeservice.GetServiceCurrentDateTime();
        //                var lastRExStData = _relreportProvider.GetLastIdAndCountOfRelExecutionStatus(executionId, vm);
        //                Thread anonymousServiceCallDelegate = new Thread(delegate() { try { executor.StartLinear(executionId); } catch { } });
        //                anonymousServiceCallDelegate.Start();

        //                //Thread threadStartServiceCall = new Thread(new ThreadStart(() => executor.StartLinear(executionId)));
        //                //threadStartServiceCall.Start();
        //                //}
        //                //catch 
        //                //{ 
        //                //}

        //                DateTime Localdateitme = timeservice.GetServiceCurrentDateTime();
        //                bool IsTimeout = false;
        //                Int64 MaxCount = lastRExStData.TotalRelExecutionCount;
        //                Int64 LastRelExecutionID = lastRExStData.LastRelExecutionId;
        //                RelExecutionStatusLastAndCount nextRExStData;
        //                while (MaxCount <= lastRExStData.TotalRelExecutionCount && LastRelExecutionID <= lastRExStData.LastRelExecutionId && IsTimeout == false)
        //                {
        //                    nextRExStData = _relreportProvider.GetLastIdAndCountOfRelExecutionStatus(executionId, vm);
        //                    MaxCount = nextRExStData.TotalRelExecutionCount;
        //                    LastRelExecutionID = nextRExStData.LastRelExecutionId;
        //                    if (MaxCount > lastRExStData.TotalRelExecutionCount) { break; }
        //                    else if (LastRelExecutionID > lastRExStData.LastRelExecutionId) { break; }
        //                    DateTime serverCurrentDateTime = timeservice.GetServiceCurrentDateTime();
        //                    if ((serverCurrentDateTime - Localdateitme).TotalMinutes > 6) { IsTimeout = true; break; }
        //                    Thread.Sleep(30000);
        //                }
        //                bool IsCompleted = false;
        //                bool IsInternalTimeOut = false;
        //                if (IsTimeout == false)
        //                {
        //                    Int64 finalRelExStID = LastRelExecutionID;
        //                    RelExecutionStatu relexdata;
        //                    while (IsCompleted == false && IsInternalTimeOut == false)
        //                    {
        //                        relexdata = _relreportProvider.GetRelExecutionStatusById(finalRelExStID);
        //                        IsCompleted = relexdata.IsExecutionCompleted;
        //                        if (IsCompleted == true) { break; }
        //                        var serverCurrentDateTime = timeservice.GetServiceCurrentDateTime();
        //                        DateTime serverdatetime = new DateTime(serverCurrentDateTime.Year, serverCurrentDateTime.Month, serverCurrentDateTime.Day, serverCurrentDateTime.Hour, serverCurrentDateTime.Minute, serverCurrentDateTime.Second);
        //                        DateTime dbdatetimeCurrentStatus = new DateTime(relexdata.CurrentStatusCheckedAt.Year, relexdata.CurrentStatusCheckedAt.Month, relexdata.CurrentStatusCheckedAt.Day, relexdata.CurrentStatusCheckedAt.Hour, relexdata.CurrentStatusCheckedAt.Minute, relexdata.CurrentStatusCheckedAt.Second);
        //                        double difference = (serverdatetime - dbdatetimeCurrentStatus).TotalMinutes;
        //                        //if ((difference - 330) > 16) // FOR Server 
        //                        if ((difference - 330) > 16) // FOR Local
        //                        {
        //                            IsInternalTimeOut = true;
        //                            break;
        //                        }
        //                        Thread.Sleep(30000);
        //                    }
        //                }

        //                if (IsTimeout == true)
        //                {
        //                    //Test case execution not started within 5 minute
        //                    List<RelReturn> NewTestsuit = new List<RelReturn>();
        //                    NewTestsuit.Add(new RelReturn()
        //                    {
        //                        ExecutionId = executionId,
        //                        TestSuitId = 0,
        //                        Message = vm + "-106"
        //                    });
        //                    var returnJsonResult = Json(NewTestsuit, JsonRequestBehavior.AllowGet);
        //                    returnJsonResult.MaxJsonLength = int.MaxValue;
        //                    return returnJsonResult;
        //                    //return new JsonResult() { Data = NewTestsuit, MaxJsonLength = 2147483644 };
        //                }
        //                else if (IsInternalTimeOut == true)
        //                {
        //                    //return with error - row generated but not updateing first time or interrepted GAgent
        //                    List<RelReturn> NewTestsuit = new List<RelReturn>();
        //                    NewTestsuit.Add(new RelReturn()
        //                    {
        //                        ExecutionId = executionId,
        //                        TestSuitId = 0,
        //                        Message = vm + "-107"
        //                    });
        //                    var returnJsonResult = Json(NewTestsuit, JsonRequestBehavior.AllowGet);
        //                    returnJsonResult.MaxJsonLength = int.MaxValue;
        //                    return returnJsonResult;
        //                }
        //                else if (IsCompleted == true)
        //                {
        //                    Thread.Sleep(120000);
        //                    List<TestSuit> FinalAllTestsuit = _relreportProvider.GetTestSuitOnRelAndVM(rel, vm);
        //                    if (FinalAllTestsuit.Count > initialtotaltestsuitcount)
        //                    {
        //                        FinalAllTestsuit = _relreportProvider.GetTestSuitOnRelAndVM(rel, vm);
        //                        List<RelReturn> NewTestsuit = (from x in FinalAllTestsuit
        //                                                       where x.ID > lastTestsuitID
        //                                                       select new RelReturn
        //                                                       {
        //                                                           ExecutionId = executionId,
        //                                                           TestSuitId = x.ID,
        //                                                           Message = vm + "-000"
        //                                                       }).ToList();

        //                        foreach (var item in NewTestsuit)
        //                        {
        //                            int tsid = item.TestSuitId;
        //                            if (tsid > 0)
        //                                _relreportProvider.UpdateTestSuitStartEndDateTime(tsid, StartDateTime, DateTime.Now);
        //                        }
        //                        var returnJsonResult = Json(NewTestsuit, JsonRequestBehavior.AllowGet);
        //                        returnJsonResult.MaxJsonLength = int.MaxValue;
        //                        return returnJsonResult;
        //                    }
        //                    else
        //                    {
        //                        List<RelReturn> NewTestsuit = new List<RelReturn>();
        //                        NewTestsuit.Add(new RelReturn()
        //                        {
        //                            ExecutionId = executionId,
        //                            TestSuitId = 0,
        //                            Message = vm + "-101"
        //                        });
        //                        var returnJsonResult = Json(NewTestsuit, JsonRequestBehavior.AllowGet);
        //                        returnJsonResult.MaxJsonLength = int.MaxValue;
        //                        return returnJsonResult;
        //                    }
        //                }
        //                else
        //                {
        //                    // Return with error - GAGENT interrrupted after starting the TestCase execution
        //                    List<RelReturn> NewTestsuit = new List<RelReturn>();
        //                    NewTestsuit.Add(new RelReturn()
        //                    {
        //                        ExecutionId = executionId,
        //                        TestSuitId = 0,
        //                        Message = vm + "-107"
        //                    });
        //                    var returnJsonResult = Json(NewTestsuit, JsonRequestBehavior.AllowGet);
        //                    returnJsonResult.MaxJsonLength = int.MaxValue;
        //                    return returnJsonResult;
        //                }
        //            }
        //            else
        //            {
        //                List<RelReturn> NewTestsuit = new List<RelReturn>();
        //                NewTestsuit.Add(new RelReturn()
        //                {
        //                    ExecutionId = executionId,
        //                    TestSuitId = 0,
        //                    Message = vm + "-102"
        //                });
        //                var returnJsonResult = Json(NewTestsuit, JsonRequestBehavior.AllowGet);
        //                returnJsonResult.MaxJsonLength = int.MaxValue;
        //                return returnJsonResult;
        //            }
        //        }
        //        else if (type == "lb")
        //        {
        //            var executionId = _executionProvider.GetLoadBalanceIdByRel(rel);
        //            if (executionId != 0)
        //            {
        //                var executor = new ExecutorNgSoapClient();
        //                executor.StartLoadBalanced(executionId);
        //                executor.WaitForLbRelResults(executionId);
        //            }

        //            List<RelReturn> NewTestsuit = new List<RelReturn>();
        //            NewTestsuit.Add(new RelReturn()
        //            {
        //                ExecutionId = executionId,
        //                TestSuitId = 0,
        //                Message = vm + "-103"
        //            });
        //            var returnJsonResult = Json(NewTestsuit, JsonRequestBehavior.AllowGet);
        //            returnJsonResult.MaxJsonLength = int.MaxValue;
        //            return returnJsonResult;
        //        }
        //        else
        //        {
        //            List<RelReturn> NewTestsuit = new List<RelReturn>();
        //            NewTestsuit.Add(new RelReturn()
        //            {
        //                ExecutionId = 0,
        //                TestSuitId = 0,
        //                Message = vm + "-102"
        //            });
        //            var returnJsonResult = Json(NewTestsuit, JsonRequestBehavior.AllowGet);
        //            returnJsonResult.MaxJsonLength = int.MaxValue;
        //            return returnJsonResult;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        if (ex.Message.ToString().ToUpper().Contains("SYSTEM.WEB.SERVICES.PROTOCOLS.SOAPEXCEPTION: SERVER WAS UNABLE TO PROCESS REQUEST."))
        //        {
        //            List<RelReturn> NewTestsuit = new List<RelReturn>();
        //            NewTestsuit.Add(new RelReturn()
        //            {
        //                ExecutionId = 0,
        //                TestSuitId = 0,
        //                Message = vm + "-105"
        //            });
        //            var returnJsonResult = Json(NewTestsuit, JsonRequestBehavior.AllowGet);
        //            returnJsonResult.MaxJsonLength = int.MaxValue;
        //            return returnJsonResult;
        //        }
        //        else
        //        {
        //            List<RelReturn> NewTestsuit = new List<RelReturn>();
        //            NewTestsuit.Add(new RelReturn()
        //            {
        //                ExecutionId = 0,
        //                TestSuitId = 0,
        //                Message = vm + "-104"
        //            });
        //            var returnJsonResult = Json(NewTestsuit, JsonRequestBehavior.AllowGet);
        //            returnJsonResult.MaxJsonLength = int.MaxValue;
        //            return returnJsonResult;
        //        }
        //    }
        //}

        public JsonResult RunRel(string type, string rel, string vm, string env, string account)
        {
            try
            {
                var timeservice = new GlobalReportingSystem.Web.UI.LoadBalancedExecutionService.LoadBalancedExecutionServiceSoapClient();
                //GRSLogWrite(vm, timeservice.GetServiceCurrentDateTime().ToString("dd-MM-yyyy HH:mm:ss"), "Command for rel execution recived.");
                DateTime StartDateTime = DateTime.Now;
                if (type == "linear" || type == null)
                {
                    //GRSLogWrite(vm, timeservice.GetServiceCurrentDateTime().ToString("dd-MM-yyyy HH:mm:ss"), "Command type for rel execution is Linear.");
                    int executionId = _executionProvider.GetLinearIdByRel(rel, vm, env, account);
                    List<TestSuit> getalltestsuit = _relreportProvider.GetTestSuitOnRelAndVM(rel, vm);
                    var initialtotaltestsuitcount = getalltestsuit.Count;
                    Int64 lastTestsuitID = 0;
                    if (initialtotaltestsuitcount > 0)
                    {
                        lastTestsuitID = (from x in getalltestsuit select x.ID).Max();
                    }
                    Int64 finaltcount = initialtotaltestsuitcount;
                    if (executionId != 0)
                    {
                        var executor = new ExecutorNgSoapClient();
                        StartDateTime = DateTime.Now;
                        var lastRExStData = _relreportProvider.GetLastIdAndCountOfRelExecutionStatus(executionId, vm);
                        Thread anonymousServiceCall = new Thread(delegate () { executor.StartLinear(executionId); });
                        anonymousServiceCall.Start();
                        //GRSLogWrite(vm, timeservice.GetServiceCurrentDateTime().ToString("dd-MM-yyyy HH:mm:ss"), "Linear execution initiated");
                        //Thread threadStartServiceCall = new Thread(new ThreadStart(() => executor.StartLinear(executionId)));
                        //threadStartServiceCall.Start();
                        DateTime Localdateitme = timeservice.GetServiceCurrentDateTime();
                        bool IsTimeout = false;
                        Int64 MaxCount = lastRExStData.TotalRelExecutionCount;
                        Int64 LastRelExecutionID = lastRExStData.LastRelExecutionId;
                        while (MaxCount <= lastRExStData.TotalRelExecutionCount && LastRelExecutionID <= lastRExStData.LastRelExecutionId && IsTimeout == false)
                        {
                            var nextRExStData = _relreportProvider.GetLastIdAndCountOfRelExecutionStatus(executionId, vm);
                            MaxCount = nextRExStData.TotalRelExecutionCount;
                            LastRelExecutionID = nextRExStData.LastRelExecutionId;
                            if (MaxCount > lastRExStData.TotalRelExecutionCount)
                            {
                                //GRSLogWrite(vm, timeservice.GetServiceCurrentDateTime().ToString("dd-MM-yyyy HH:mm:ss"), "Tracking row generated by GAgent. (checked at max row count)");
                                break;
                            }
                            else if (LastRelExecutionID > lastRExStData.LastRelExecutionId)
                            {
                                //GRSLogWrite(vm, timeservice.GetServiceCurrentDateTime().ToString("dd-MM-yyyy HH:mm:ss"), "Tracking row generated by GAgent. (checked at max row ID)");
                                break;
                            }
                            DateTime serverCurrentTime = timeservice.GetServiceCurrentDateTime();
                            if ((serverCurrentTime - Localdateitme).TotalMinutes > 6)
                            {
                                //GRSLogWrite(vm, timeservice.GetServiceCurrentDateTime().ToString("dd-MM-yyyy HH:mm:ss"), "GAgent failed to generate tracking row, Timeout occurred.");
                                IsTimeout = true; break;
                            }
                            Thread.Sleep(30000);
                        }
                        bool IsCompleted = false;
                        bool IsInternalTimeOut = false;
                        if (IsTimeout == false)
                        {
                            timeservice.UpdateLatestRelExecutionStatusEntryLevelRecord(LastRelExecutionID, initialtotaltestsuitcount, lastTestsuitID, "");
                            Int64 finalRelExStID = LastRelExecutionID;
                            while (IsCompleted == false && IsInternalTimeOut == false)
                            {
                                RelExecutionStatu relexdata = _relreportProvider.GetRelExecutionStatusById(finalRelExStID);
                                IsCompleted = relexdata.IsExecutionCompleted;
                                if (IsCompleted == true) { break; }
                                DateTime serverCDTime = timeservice.GetServiceCurrentDateTime();
                                DateTime serverCurrentDateTime = new DateTime(serverCDTime.Year, serverCDTime.Month, serverCDTime.Day, serverCDTime.Hour, serverCDTime.Minute, serverCDTime.Second);
                                DateTime lastupdatedCurrentStatus = new DateTime(relexdata.CurrentStatusCheckedAt.Year, relexdata.CurrentStatusCheckedAt.Month, relexdata.CurrentStatusCheckedAt.Day, relexdata.CurrentStatusCheckedAt.Hour, relexdata.CurrentStatusCheckedAt.Minute, relexdata.CurrentStatusCheckedAt.Second);
                                double difference = (serverCurrentDateTime - lastupdatedCurrentStatus).TotalMinutes;
                                //if (relexdata.LastStatusCheckedAt == (DateTime)System.Data.SqlTypes.SqlDateTime.Null && difference > 7 && difference < 15)
                                //{ IsInternalTimeOut = true; break; }
                                //else 
                                if (difference > 16)
                                {
                                    //GRSLogWrite(vm, timeservice.GetServiceCurrentDateTime().ToString("dd-MM-yyyy HH:mm:ss"), "Update checked -> Time. Last update received at " + lastupdatedCurrentStatus.ToString("dd-MM-yyyy HH:mm:ss"));
                                    IsInternalTimeOut = true; break;
                                }
                                else
                                {
                                    //GRSLogWrite(vm, timeservice.GetServiceCurrentDateTime().ToString("dd-MM-yyyy HH:mm:ss"), "Update checked -> No Timeout. Last update received at " + lastupdatedCurrentStatus.ToString("dd-MM-yyyy HH:mm:ss"));
                                }
                                Thread.Sleep(30000);
                            }
                        }

                        if (IsTimeout == true)
                        {
                            //GRSLogWrite(vm, timeservice.GetServiceCurrentDateTime().ToString("dd-MM-yyyy HH:mm:ss"), "Row generatoin level -> Timeout. Prepare to return errorcode 106 with ExecutionId " + executionId + " TestSuitID 0");
                            //Test case execution not started within 5 minute
                            List<RelReturn> NewTestsuit = new List<RelReturn>();
                            NewTestsuit.Add(new RelReturn()
                            {
                                ExecutionId = executionId,
                                TestSuitId = 0,
                                Message = vm + "-106"
                            });
                            var returnJsonResult = Json(NewTestsuit, JsonRequestBehavior.AllowGet);
                            returnJsonResult.MaxJsonLength = int.MaxValue;
                            return returnJsonResult;
                        }
                        else if (IsInternalTimeOut == true)
                        {
                            //GRSLogWrite(vm, timeservice.GetServiceCurrentDateTime().ToString("dd-MM-yyyy HH:mm:ss"), "Row update level -> Timeout. Prepare to return errorcode 107 with ExecutionId " + executionId + " TestSuitID 0");
                            //return with error - row generated but not updateing first time or interrepted GAgent
                            List<RelReturn> NewTestsuit = new List<RelReturn>();
                            NewTestsuit.Add(new RelReturn()
                            {
                                ExecutionId = executionId,
                                TestSuitId = 0,
                                Message = vm + "-107"
                            });
                            var returnJsonResult = Json(NewTestsuit, JsonRequestBehavior.AllowGet);
                            returnJsonResult.MaxJsonLength = int.MaxValue;
                            return returnJsonResult;
                        }
                        else if (IsCompleted == true)
                        {
                            //GRSLogWrite(vm, timeservice.GetServiceCurrentDateTime().ToString("dd-MM-yyyy HH:mm:ss"), "Execution Completed. Comparing Test suits for Execution ID" + executionId);
                            Thread.Sleep(120000);
                            List<TestSuit> FinalAllTestsuit = _relreportProvider.GetTestSuitOnRelAndVM(rel, vm);
                            if (FinalAllTestsuit.Count > initialtotaltestsuitcount)
                            {
                                //GRSLogWrite(vm, timeservice.GetServiceCurrentDateTime().ToString("dd-MM-yyyy HH:mm:ss"), "Execution Completed. Testsuit found. Preparing to return result with execution ID " + executionId);
                                FinalAllTestsuit = _relreportProvider.GetTestSuitOnRelAndVM(rel, vm);
                                List<RelReturn> NewTestsuit = (from x in FinalAllTestsuit
                                                               where x.ID > lastTestsuitID
                                                               select new RelReturn
                                                               {
                                                                   ExecutionId = executionId,
                                                                   TestSuitId = x.ID,
                                                                   Message = vm + "-000"
                                                               }).ToList();

                                //foreach (var item in NewTestsuit)
                                //{
                                //    int tsid = item.TestSuitId;
                                //    if (tsid > 0)
                                //        _relreportProvider.UpdateTestSuitStartEndDateTime(tsid, StartDateTime, DateTime.Now);
                                //}

                                var returnJsonResult = Json(NewTestsuit, JsonRequestBehavior.AllowGet);
                                returnJsonResult.MaxJsonLength = int.MaxValue;
                                return returnJsonResult;
                            }
                            else
                            {
                                //GRSLogWrite(vm, timeservice.GetServiceCurrentDateTime().ToString("dd-MM-yyyy HH:mm:ss"), "Execution Completed. Testsuit not found. Preparing to return result with execution ID " + executionId);
                                List<RelReturn> NewTestsuit = new List<RelReturn>();
                                NewTestsuit.Add(new RelReturn()
                                {
                                    ExecutionId = executionId,
                                    TestSuitId = 0,
                                    Message = vm + "-101"
                                });
                                var returnJsonResult = Json(NewTestsuit, JsonRequestBehavior.AllowGet);
                                returnJsonResult.MaxJsonLength = int.MaxValue;
                                return returnJsonResult;
                            }
                        }
                        else
                        {
                            //GRSLogWrite(vm, timeservice.GetServiceCurrentDateTime().ToString("dd-MM-yyyy HH:mm:ss"), "No Timeout and No Execution completed. Else part detected (Unidentified exception). Preparing to return with errorcode 107 with Execution ID" + executionId);
                            // Return with error - GAGENT interrrupted after starting the TestCase execution
                            List<RelReturn> NewTestsuit = new List<RelReturn>();
                            NewTestsuit.Add(new RelReturn()
                            {
                                ExecutionId = executionId,
                                TestSuitId = 0,
                                Message = vm + "-107"
                            });
                            var returnJsonResult = Json(NewTestsuit, JsonRequestBehavior.AllowGet);
                            returnJsonResult.MaxJsonLength = int.MaxValue;
                            return returnJsonResult;
                        }
                    }
                    else
                    {
                        //GRSLogWrite(vm, timeservice.GetServiceCurrentDateTime().ToString("dd-MM-yyyy HH:mm:ss"), "Execution ID not found. Invalid REL LINK");
                        List<RelReturn> NewTestsuit = new List<RelReturn>();
                        NewTestsuit.Add(new RelReturn()
                        {
                            ExecutionId = executionId,
                            TestSuitId = 0,
                            Message = vm + "-102"
                        });
                        var returnJsonResult = Json(NewTestsuit, JsonRequestBehavior.AllowGet);
                        returnJsonResult.MaxJsonLength = int.MaxValue;
                        return returnJsonResult;
                    }
                }
                else if (type == "lb")
                {
                    //GRSLogWrite(vm, timeservice.GetServiceCurrentDateTime().ToString("dd-MM-yyyy HH:mm:ss"), "Load Balanced command received.");
                    var executionId = _executionProvider.GetLoadBalanceIdByRel(rel);
                    if (executionId != 0)
                    {
                        var executor = new ExecutorNgSoapClient();
                        executor.StartLoadBalanced(executionId);
                        executor.WaitForLbRelResults(executionId);
                    }

                    List<RelReturn> NewTestsuit = new List<RelReturn>();
                    NewTestsuit.Add(new RelReturn()
                    {
                        ExecutionId = executionId,
                        TestSuitId = 0,
                        Message = vm + "-103"
                    });
                    var returnJsonResult = Json(NewTestsuit, JsonRequestBehavior.AllowGet);
                    returnJsonResult.MaxJsonLength = int.MaxValue;
                    //GRSLogWrite(vm, timeservice.GetServiceCurrentDateTime().ToString("dd-MM-yyyy HH:mm:ss"), "Execution Completed for Load Balanced. Preparing to return result.");
                    return returnJsonResult;
                }
                else
                {
                    //GRSLogWrite(vm, timeservice.GetServiceCurrentDateTime().ToString("dd-MM-yyyy HH:mm:ss"), "Invalid command type received. Command type is " + type + ". Preparing to return with errorcode 102.");
                    List<RelReturn> NewTestsuit = new List<RelReturn>();
                    NewTestsuit.Add(new RelReturn()
                    {
                        ExecutionId = 0,
                        TestSuitId = 0,
                        Message = vm + "-102"
                    });
                    var returnJsonResult = Json(NewTestsuit, JsonRequestBehavior.AllowGet);
                    returnJsonResult.MaxJsonLength = int.MaxValue;
                    return returnJsonResult;
                }
            }
            catch (Exception ex)
            {
                //GRSLogWrite(vm, "GRS Local server Time => " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), "Exception Occurred. Exception Message => " + ex.Message);
                if (ex.Message.ToString().ToUpper().Contains("SYSTEM.WEB.SERVICES.PROTOCOLS.SOAPEXCEPTION: SERVER WAS UNABLE TO PROCESS REQUEST."))
                {
                    //GRSLogWrite(vm, "GRS Local server Time => " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), "Exception Occurred. GAgent did not accept request. Preparing to return.");
                    List<RelReturn> NewTestsuit = new List<RelReturn>();
                    NewTestsuit.Add(new RelReturn()
                    {
                        ExecutionId = 0,
                        TestSuitId = 0,
                        Message = vm + "-105"
                    });
                    var returnJsonResult = Json(NewTestsuit, JsonRequestBehavior.AllowGet);
                    returnJsonResult.MaxJsonLength = int.MaxValue;
                    return returnJsonResult;
                }
                else
                {
                    //GRSLogWrite(vm, "GRS Local server Time => " + DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss"), "Exception Occurred. Preparing to return.");
                    List<RelReturn> NewTestsuit = new List<RelReturn>();
                    NewTestsuit.Add(new RelReturn()
                    {
                        ExecutionId = 0,
                        TestSuitId = 0,
                        Message = vm + "-104"
                    });
                    var returnJsonResult = Json(NewTestsuit, JsonRequestBehavior.AllowGet);
                    returnJsonResult.MaxJsonLength = int.MaxValue;
                    return returnJsonResult;
                }
            }
        }

        public JsonResult LaunchRenRelReportSendEmail(string executionId_testsuitId, string ToAddresss, string Message)
        {
            if ((!string.IsNullOrEmpty(executionId_testsuitId)) && (!string.IsNullOrEmpty(ToAddresss)))
            {
                try
                {
                    List<string> ToList;
                    if (!string.IsNullOrEmpty(ToAddresss))
                    {
                        string[] toaddressarr = ToAddresss.Split(',');
                        ToList = new List<string>(toaddressarr);
                    }
                    else
                    {
                        ToList = new List<string>();
                    }

                    List<string> exetest;
                    if (!string.IsNullOrEmpty(executionId_testsuitId))
                    {
                        string[] ET = executionId_testsuitId.Split(',');
                        exetest = new List<string>(ET);
                    }
                    else
                    {
                        exetest = new List<string>();
                    }

                    List<RelReturn> RelRelateIDs = new List<RelReturn>();

                    foreach (var item in exetest)
                    {
                        if (!string.IsNullOrEmpty(item))
                        {
                            string[] etind = item.Split(':');
                            if (etind.Length == 2)
                            {
                                string executionid = etind[0];
                                string testsuitid = etind[1];

                                int executionId = 0;
                                int testsuitId = 0;

                                Int32.TryParse(executionid, out executionId);
                                Int32.TryParse(testsuitid, out testsuitId);
                                if (executionId > 0 && testsuitId > 0)
                                {
                                    RelRelateIDs.Add(new RelReturn() { ExecutionId = executionId, TestSuitId = testsuitId });
                                }
                            }
                        }
                    }

                    if (RelRelateIDs.Count > 0 && ToList.Count > 0)
                    {
                        string messagebody = _relreportProvider.HTMLForLaunchRELReportEmail(RelRelateIDs, Message);
                        _emailer.SendMail(ToList, "grs@clarivate.com", "Automated Test Execution Report", messagebody);
                        return Json("Email Send.", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Sending email failed. Invalid parameter.", JsonRequestBehavior.AllowGet);
                    }
                }
                catch (Exception ex)
                {
                    return Json(ex.Message, JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Invalid Request.", JsonRequestBehavior.AllowGet);
            }
        }

        public void ErrorDetailsGRSReport(string exid, string tsid)
        {
            int executionid = 0;
            if ((!string.IsNullOrEmpty(exid)) && (!string.IsNullOrEmpty(tsid)))
            {
                Int32.TryParse(exid, out executionid);
            }

            var relexdata = _relreportProvider.GetTestSuitIdString(executionid, tsid);

            string tfinaltsid = string.Empty;
            if (!string.IsNullOrEmpty(relexdata))
                tfinaltsid = relexdata;
            else
                tfinaltsid = tsid;

            string[] tsidarr = tfinaltsid.Split(',');
            List<string> tsidlist = new List<string>(tsidarr);
            if (tsidlist.Count == 1)
            {
                Int64 testsuitid = 0;

                if ((!string.IsNullOrEmpty(tsid)))
                {
                    Int32.TryParse(exid, out executionid);
                    Int64.TryParse(tsid, out testsuitid);
                }
                string responsestring = _relreportProvider.HTMLForLaunchRELReportBrowser(executionid, testsuitid);
                responsestring = responsestring.Replace("http://10.236.4.153/nggrs/Launch/ErrorDetailsGRSReport", "https://grs.lstools.int.clarivate.com/nggrs/Launch/ErrorDetailsGRSReportIndividual");
                Response.Write(responsestring);
            }

            else if (tsidlist.Count > 1)
            {
                List<Int64> TSLIST = new List<Int64>();
                foreach (var itemts in tsidlist)
                {
                    Int64 temptsid = 0;
                    bool isconverted = Int64.TryParse(itemts, out temptsid);
                    if (isconverted)
                    {
                        TSLIST.Add(temptsid);
                    }
                }

                if (TSLIST.Count > 0 && executionid > 0)
                {
                    string responsestring = _relreportProvider.HTMLForLaunchRELTestSuitDetails(executionid, TSLIST);
                    responsestring = responsestring.Replace("http://10.236.4.153/nggrs/Launch/ErrorDetailsGRSReport", "https://grs.lstools.int.clarivate.com/nggrs/Launch/ErrorDetailsGRSReportIndividual");
                    Response.Write(responsestring);
                }
                else
                {
                    string responsestring = _relreportProvider.HTMLForLaunchRELReportBrowser(0, 0);
                    responsestring = responsestring.Replace("http://10.236.4.153/nggrs/Launch/ErrorDetailsGRSReport", "https://grs.lstools.int.clarivate.com/nggrs/Launch/ErrorDetailsGRSReportIndividual");
                    Response.Write(responsestring);
                }
            }
            else
            {
                string responsestring = _relreportProvider.HTMLForLaunchRELReportBrowser(0, 0);
                responsestring = responsestring.Replace("http://10.236.4.153/nggrs/Launch/ErrorDetailsGRSReport", "https://grs.lstools.int.clarivate.com/nggrs/Launch/ErrorDetailsGRSReportIndividual");
                Response.Write(responsestring);
            }
        }

        public void ErrorDetailsGRSReportIndividual(string exid, string tsid)
        {
            int executionid = 0;
            if ((!string.IsNullOrEmpty(exid)) && (!string.IsNullOrEmpty(tsid)))
            {
                Int32.TryParse(exid, out executionid);
            }

            //var relexdata = _relreportProvider.GetTestSuitIdString(executionid, tsid);

            string tfinaltsid = string.Empty;
            //if (!string.IsNullOrEmpty(relexdata))
            //    tfinaltsid = relexdata;
            //else
            tfinaltsid = tsid;

            string[] tsidarr = tfinaltsid.Split(',');
            List<string> tsidlist = new List<string>(tsidarr);
            if (tsidlist.Count == 1)
            {
                Int64 testsuitid = 0;

                if ((!string.IsNullOrEmpty(tsid)))
                {
                    Int32.TryParse(exid, out executionid);
                    Int64.TryParse(tsid, out testsuitid);
                }
                string responsestring = _relreportProvider.HTMLForLaunchRELReportBrowser(executionid, testsuitid);
                responsestring = responsestring.Replace("http://10.236.4.153/nggrs/", "https://grs.lstools.int.clarivate.com/nggrs/");
                Response.Write(responsestring);
            }

            else if (tsidlist.Count > 1)
            {
                List<Int64> TSLIST = new List<Int64>();
                foreach (var itemts in tsidlist)
                {
                    Int64 temptsid = 0;
                    bool isconverted = Int64.TryParse(itemts, out temptsid);
                    if (isconverted)
                    {
                        TSLIST.Add(temptsid);
                    }
                }

                if (TSLIST.Count > 0 && executionid > 0)
                {
                    string responsestring = _relreportProvider.HTMLForLaunchRELTestSuitDetails(executionid, TSLIST);
                    responsestring = responsestring.Replace("http://10.236.4.153/nggrs/", "https://grs.lstools.int.clarivate.com/nggrs/");
                    Response.Write(responsestring);
                }
                else
                {
                    string responsestring = _relreportProvider.HTMLForLaunchRELReportBrowser(0, 0);
                    responsestring = responsestring.Replace("http://10.236.4.153/nggrs/", "https://grs.lstools.int.clarivate.com/nggrs/");
                    Response.Write(responsestring);
                }
            }
            else
            {
                string responsestring = _relreportProvider.HTMLForLaunchRELReportBrowser(0, 0);
                responsestring = responsestring.Replace("http://10.236.4.153/nggrs/", "https://grs.lstools.int.clarivate.com/nggrs/");
                Response.Write(responsestring);
            }
        }


        /*--------------------RUN REL BREAKED---------------------------------*/

        /*Step 1: Check client machine health (whether machine is ready or busy or null)*/
        public string RunRelIPHealthCheck(string type, string rel, string vm, string env, string account, string ToAddress)
        {
            WriteControllerLogToFile("RunRel IP Health Check", "IP health check initiated", type, rel, vm, env, account, ToAddress, "");
            string Status = "Machine is not ready.";
            bool cansendemail = false;
            try
            {

                if (type == "linear" || type == null)
                {
                    int executionId = _executionProvider.GetLinearIdByRel(rel, vm, env, account);
                    if (executionId != 0)
                    {
                        var executor = new ExecutorNgSoapClient();
                        string clientIpHealth = (executor.PingById(executionId));
                        if (clientIpHealth.ToUpper() == "FREE")
                        {
                            Status = "FREE";
                            cansendemail = false;
                        }
                        else
                        {
                            Status = "Machine is not ready.";
                            cansendemail = true;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                cansendemail = true;
            }

            if (cansendemail)
            {
                if (!string.IsNullOrEmpty(ToAddress))
                {
                    string[] toaddressarray = ToAddress.Split(',');
                    List<string> ToAddressList = new List<string>(toaddressarray);
                    string messagebody = "<table style='border-collapse: collapse; width: auto!important;'><tr><td align='left' colspan='2' style='font-weight: 500; font-size: 14px!important; font-family: verdana; padding: 10px 10px 10px 10px!important;'><br />Hi,<br /><br />Client machine {IPADDRESS} is not responding back to GRS. <br /><br />Possible causes of error...<br /><ul><li>Client machine is busy with other execution.</li><li>Client machine is shutdown.</li><li>G-Agent is not running on client machine.</li><li>Client machine is not connected with internet.</li><li>Client machine is not part of intranet.</li></ul><br />Please check this machine.<br /><br /><br />GRS Admin</td></tr><tr><td align='left' valign='middle' style='color: #DD5A43; font-weight: 500; font-size: 22px!important; width: 40px!important; padding: 0px 0px 0px 10px!important;'><img src='https://grs.lstools.int.clarivate.com/nggrs/Content/GRS.png' style='height: 32px; width: 32px;' /></td><td align='left' valign='middle' style='vertical-align: middle; color: #DD5A43; font-weight: 600; font-size: 22px!important; padding: 0px!important;'>GRS</td></tr><tr><td colspan='2' align='left' style='font-weight: 600; padding-top: 2px!important; padding-bottom: 0px!important;'>&copy;&nbsp;&nbsp;Clarivate Analytics {CURRENTYEAR}</td></tr></table>";
                    messagebody = messagebody.Replace("{IPADDRESS}", vm);
                    messagebody = messagebody.Replace("{CURRENTYEAR}", Convert.ToString(DateTime.Now.Year));
                    _emailer.SendMail(ToAddressList, "grs@clarivate.com", "Client Machine " + vm + " Not Responding", messagebody);
                }
            }

            //var response = Json(Status, JsonRequestBehavior.AllowGet);
            //response.MaxJsonLength = int.MaxValue;
            //return response;
            return Status;
        }

        /*Step 2: Calculate Total Testsuit count against that execution id against machine IP, Last TestsuitId, 
        *         Initiate anonymous execution on client machine, wait for entry from GAgent side, check entry in DB, 
        *         Initiate anonymous for continuous check in DB for execution completion and respond back with Error Id, Message, RelExecutionStatusId*/
        /*public JsonResult RunRelInitiateTesting(string type, string rel, string vm, string env, string account, string ToAddress)
        //{
        //    WriteControllerLogToFile("Run Rel Initiate Testing", "Remote machine testing method initiated", type, rel, vm, env, account, ToAddress, "");
        //    try
        //    {
        //        var timeservice = new GlobalReportingSystem.Web.UI.LoadBalancedExecutionService.LoadBalancedExecutionServiceSoapClient();
        //        CreateRelLogs(vm, "Command for rel execution recived.");
        //        DateTime StartDateTime = timeservice.GetServiceCurrentDateTime();
        //        if (type == "linear" || type == null)
        //        {
        //            Int64 prelexid = 0;
        //            CreateRelLogs(vm, "Command type for rel execution is Linear.");
        //            int executionId = _executionProvider.GetLinearIdByRel(rel, vm, env, account);
        //            List<TestSuit> getalltestsuit = _relreportProvider.GetTestSuitOnRelAndVM(rel, vm);
        //            var initialtotaltestsuitcount = getalltestsuit.Count;
        //            Int64 lastTestsuitID = 0;
        //            if (initialtotaltestsuitcount > 0)
        //            {
        //                lastTestsuitID = (from x in getalltestsuit select x.ID).Max();
        //            }
        //            Int64 finaltcount = initialtotaltestsuitcount;
        //            if (executionId != 0)
        //            {
        //                var executor = new ExecutorNgSoapClient();
        //                StartDateTime = DateTime.Now;
        //                var lastRExStData = _relreportProvider.GetLastIdAndCountOfRelExecutionStatus(executionId, vm);
        //                Thread anonymousServiceCall = new Thread(delegate()
        //                {
        //                    try
        //                    {
        //                        executor.StartLinear(executionId);
        //                    }
        //                    catch (Exception ex)
        //                    {
        //                        if (!string.IsNullOrEmpty(ToAddress))
        //                        {
        //                            string[] toaddressarray = ToAddress.Split(',');
        //                            List<string> ToAddressList = new List<string>(toaddressarray);
        //                            string messagebody = "<table style='border-collapse: collapse; width: auto!important;'><tr><td align='left' colspan='2' style='font-weight: 500; font-size: 14px!important; font-family: verdana; padding: 10px 10px 10px 10px!important;'><br />Hi,<br /><br />Client machine {IPADDRESS} is not responding back to GRS. <br /><br />Possible causes of error...<br /><ul><li>Client machine is busy with other execution.</li><li>Client machine is shutdown.</li><li>G-Agent is not running on client machine.</li><li>Client machine is not connected with internet.</li><li>Client machine is not part of intranet.</li></ul><br />Please check this machine.<br /><br /><br />GRS Admin</td></tr><tr><td align='left' valign='middle' style='color: #DD5A43; font-weight: 500; font-size: 22px!important; width: 40px!important; padding: 0px 0px 0px 10px!important;'><img src='https://grs.lstools.int.clarivate.com/nggrs/Content/GRS.png' style='height: 32px; width: 32px;' /></td><td align='left' valign='middle' style='vertical-align: middle; color: #DD5A43; font-weight: 600; font-size: 22px!important; padding: 0px!important;'>GRS</td></tr><tr><td colspan='2' align='left' style='font-weight: 600; padding-top: 2px!important; padding-bottom: 0px!important;'>&copy;&nbsp;&nbsp;Clarivate Analytics {CURRENTYEAR}</td></tr></table>";
        //                            messagebody = messagebody.Replace("{IPADDRESS}", vm);
        //                            messagebody = messagebody.Replace("{CURRENTYEAR}", Convert.ToString(DateTime.Now.Year));
        //                            _emailer.SendMail(ToAddressList, "grs@clarivate.com", "Client Machine " + vm + " Not Responding", messagebody);
        //                        }
        //                    }
        //                });
        //                anonymousServiceCall.Start();

        //                CreateRelLogs(vm, "Linear execution initiated");
        //                DateTime Localdateitme = timeservice.GetServiceCurrentDateTime();
        //                bool IsTimeout = false;
        //                Int64 MaxCount = lastRExStData.TotalRelExecutionCount;
        //                Int64 LastRelExecutionID = lastRExStData.LastRelExecutionId;
        //                while (MaxCount <= lastRExStData.TotalRelExecutionCount
        //                    && LastRelExecutionID <= lastRExStData.LastRelExecutionId
        //                    && IsTimeout == false)
        //                {
        //                    var nextRExStData = _relreportProvider.GetLastIdAndCountOfRelExecutionStatus(executionId, vm);
        //                    MaxCount = nextRExStData.TotalRelExecutionCount;
        //                    LastRelExecutionID = nextRExStData.LastRelExecutionId;
        //                    if (MaxCount > lastRExStData.TotalRelExecutionCount)
        //                    {
        //                        CreateRelLogs(vm, "Tracking row generated by GAgent. (checked at max row count)");
        //                        try
        //                        {
        //                            prelexid = nextRExStData.TotalRelExecutionCount;
        //                        }
        //                        catch
        //                        {
        //                            prelexid = 0;
        //                        }
        //                        break;
        //                    }
        //                    else if (LastRelExecutionID > lastRExStData.LastRelExecutionId)
        //                    {
        //                        CreateRelLogs(vm, "Tracking row generated by GAgent. (checked at max row ID)");
        //                        try
        //                        {
        //                            prelexid = nextRExStData.TotalRelExecutionCount;
        //                        }
        //                        catch
        //                        {
        //                            prelexid = 0;
        //                        }
        //                        break;
        //                    }
        //                    DateTime serverCurrentTime = timeservice.GetServiceCurrentDateTime();
        //                    if ((serverCurrentTime - Localdateitme).TotalMinutes > 6)
        //                    {
        //                        CreateRelLogs(vm, "GAgent failed to generate tracking row, Timeout occurred.");
        //                        try
        //                        {
        //                            prelexid = nextRExStData.TotalRelExecutionCount;
        //                        }
        //                        catch
        //                        {
        //                            prelexid = 0;
        //                        }
        //                        IsTimeout = true; break;
        //                    }
        //                    Thread.Sleep(30000);
        //                }

        //                if (IsTimeout)
        //                {
        //                    // create entry with 0 record
        //                    //Int64 relexecutionid = timeservice.CreateRelExecutionStatus(executionId, vm, false, "TIMEOUT", "GAgent did not accept the command.");
        //                    Int64 relexecutionid = _relreportProvider.CreateRelExecutionStatus(executionId, vm, false, "TIMEOUT", "GAgent did not accept the command.");
        //                    if (relexecutionid > 0)
        //                        _relreportProvider.UpdateLatestRelExecutionStatusEntryLevelRecord(relexecutionid, initialtotaltestsuitcount, lastTestsuitID, ToAddress);
        //                    RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = relexecutionid, MachineIP = vm, Status = "TIMEOUT" };
        //                    var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
        //                    jsonResponseResult.MaxJsonLength = int.MaxValue;
        //                    return jsonResponseResult;
        //                }
        //                else if (prelexid == 0)
        //                {
        //                    //Int64 relexecutionid = timeservice.CreateRelExecutionStatus(executionId, vm, false, "TIMEOUT", "GAgent did not accept the command.");
        //                    Int64 relexecutionid = _relreportProvider.CreateRelExecutionStatus(executionId, vm, false, "TIMEOUT", "GAgent did not accept the command.");

        //                    try
        //                    {
        //                        if (relexecutionid > 0)
        //                            _relreportProvider.UpdateLatestRelExecutionStatusEntryLevelRecord(relexecutionid, initialtotaltestsuitcount, lastTestsuitID, ToAddress);
        //                    }
        //                    catch { }
        //                    RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = prelexid, MachineIP = vm, Status = "TIMEOUT" };
        //                    var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
        //                    jsonResponseResult.MaxJsonLength = int.MaxValue;
        //                    return jsonResponseResult;
        //                }
        //                else
        //                {
        //                    //update entery 
        //                    //timeservice.UpdateLatestRelExecutionStatusEntryLevelRecord(LastRelExecutionID, initialtotaltestsuitcount, lastTestsuitID, ToAddress);
        //                    _relreportProvider.UpdateLatestRelExecutionStatusEntryLevelRecord(LastRelExecutionID, initialtotaltestsuitcount, lastTestsuitID, ToAddress);
        //                    //Call other method on execution level that will accept parameter
        //                    //Thread anonymousContinuousEntryCheck = new Thread(delegate()
        //                    //{
        //                    //    try { RunRelContinuousCheckTestCompletion(LastRelExecutionID, vm, executionId, lastTestsuitID, initialtotaltestsuitcount, StartDateTime, rel); }
        //                    //    catch (Exception ex) { }
        //                    //});
        //                    //anonymousContinuousEntryCheck.Start();

        //                    var statusdata = _relreportProvider.GetRelExecutionStatusById(LastRelExecutionID);
        //                    RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = statusdata.ID, MachineIP = statusdata.MachineIP, Status = (!string.IsNullOrEmpty(statusdata.CurrentStatus)) ? statusdata.CurrentStatus.ToUpper() : "NULL" };
        //                    var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
        //                    jsonResponseResult.MaxJsonLength = int.MaxValue;
        //                    return jsonResponseResult;
        //                }
        //            }
        //            else
        //            {
        //                CreateRelLogs(vm, "Execution ID not found. Invalid REL LINK");
        //                RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = 0, MachineIP = vm, Status = "INVALID REL LINK" };
        //                var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
        //                jsonResponseResult.MaxJsonLength = int.MaxValue;
        //                return jsonResponseResult;
        //            }
        //        }
        //        else if (type == "lb")
        //        {
        //            CreateRelLogs(vm, "Load Balanced command received.");
        //            var executionId = _executionProvider.GetLoadBalanceIdByRel(rel);
        //            if (executionId != 0)
        //            {
        //                var executor = new ExecutorNgSoapClient();
        //                executor.StartLoadBalanced(executionId);
        //                executor.WaitForLbRelResults(executionId);
        //            }

        //            CreateRelLogs(vm, "Execution Completed for Load Balanced. Preparing to return result.");
        //            RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = 0, MachineIP = vm, Status = "LOAD BALANCED" };
        //            var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
        //            jsonResponseResult.MaxJsonLength = int.MaxValue;
        //            return jsonResponseResult;
        //        }
        //        else
        //        {
        //            CreateRelLogs(vm, "Invalid command type received. Command type is " + type + ". Preparing to return with errorcode 102.");
        //            RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = 0, MachineIP = vm, Status = "INVALID TYPE" };
        //            var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
        //            jsonResponseResult.MaxJsonLength = int.MaxValue;
        //            return jsonResponseResult;
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        CreateRelLogs(vm, "Exception Occurred. Exception Message => " + ex.Message);
        //        if (ex.Message.ToString().ToUpper().Contains("SYSTEM.WEB.SERVICES.PROTOCOLS.SOAPEXCEPTION: SERVER WAS UNABLE TO PROCESS REQUEST."))
        //        {
        //            CreateRelLogs(vm, "Exception Occurred. GAgent did not accept request. Preparing to return.");
        //            RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = 0, MachineIP = vm, Status = "INVALID GAGENT" };
        //            var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
        //            jsonResponseResult.MaxJsonLength = int.MaxValue;
        //            return jsonResponseResult;
        //        }
        //        else
        //        {
        //            CreateRelLogs(vm, "Exception Occurred. Preparing to return. Exception => " + ex.Message);
        //            RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = 0, MachineIP = vm, Status = "INVALID" };
        //            var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
        //            jsonResponseResult.MaxJsonLength = int.MaxValue;
        //            return jsonResponseResult;
        //        }
        //    }
        //}
        */

        public JsonResult RunRelInitiateTesting(string type, string rel, string vm, string env, string account, string ToAddress)
        {
            WriteControllerLogToFile("Run Rel Initiate Testing", "Method accepted request.", type, rel, vm, env, account, ToAddress, "");
            try
            {
                var timeservice = new GlobalReportingSystem.Web.UI.LoadBalancedExecutionService.LoadBalancedExecutionServiceSoapClient();
                CreateRelLogs(vm, "Command for rel execution recived.");
                DateTime StartDateTime = timeservice.GetServiceCurrentDateTime();
                if (type == "linear" || type == null)
                {
                    WriteControllerLogToFile("Run Rel Initiate Testing", "Linear option acepted.", type, rel, vm, env, account, ToAddress, "");
                    try
                    {
                        Int64 prelexid = 0;
                        CreateRelLogs(vm, "Command type for rel execution is Linear.");
                        int executionId = _executionProvider.GetLinearIdByRel(rel, vm, env, account);
                        List<TestSuit> getalltestsuit = _relreportProvider.GetTestSuitOnRelAndVM(rel, vm);
                        var initialtotaltestsuitcount = getalltestsuit.Count;
                        Int64 lastTestsuitID = 0;
                        if (initialtotaltestsuitcount > 0)
                        {
                            lastTestsuitID = (from x in getalltestsuit select x.ID).Max();
                        }
                        Int64 finaltcount = initialtotaltestsuitcount;
                        if (executionId != 0)
                        {
                            var executor = new ExecutorNgSoapClient();
                            StartDateTime = DateTime.Now;
                            var lastRExStData = _relreportProvider.GetLastIdAndCountOfRelExecutionStatus(executionId, vm);
                            Thread anonymousServiceCall = new Thread(delegate ()
                            {
                                try
                                {
                                    WriteControllerLogToFile("Run Rel Initiate Testing", "Execution start scheduled by anonymous method on client machine", type, rel, vm, env, account, ToAddress, "");
                                    executor.StartLinear(executionId);
                                }
                                catch (Exception ex)
                                {
                                    WriteControllerLogToFile("Run Rel Initiate Testing", "Execution start failed on client machine. Exception: " + ex.Message, type, rel, vm, env, account, ToAddress, "");
                                    //if (!string.IsNullOrEmpty(ToAddress))
                                    //{
                                    //    string[] toaddressarray = ToAddress.Split(',');
                                    //    List<string> ToAddressList = new List<string>(toaddressarray);
                                    //    string messagebody = "<table style='border-collapse: collapse; width: auto!important;'><tr><td align='left' colspan='2' style='font-weight: 500; font-size: 14px!important; font-family: verdana; padding: 10px 10px 10px 10px!important;'><br />Hi,<br /><br />Client machine {IPADDRESS} is not responding back to GRS. <br /><br />Possible causes of error...<br /><ul><li>Client machine is busy with other execution.</li><li>Client machine is shutdown.</li><li>G-Agent is not running on client machine.</li><li>Client machine is not connected with internet.</li><li>Client machine is not part of intranet.</li></ul><br />Please check this machine.<br /><br /><br />GRS Admin</td></tr><tr><td align='left' valign='middle' style='color: #DD5A43; font-weight: 500; font-size: 22px!important; width: 40px!important; padding: 0px 0px 0px 10px!important;'><img src='https://grs.lstools.int.clarivate.com/nggrs/Content/GRS.png' style='height: 32px; width: 32px;' /></td><td align='left' valign='middle' style='vertical-align: middle; color: #DD5A43; font-weight: 600; font-size: 22px!important; padding: 0px!important;'>GRS</td></tr><tr><td colspan='2' align='left' style='font-weight: 600; padding-top: 2px!important; padding-bottom: 0px!important;'>&copy;&nbsp;&nbsp;Clarivate Analytics {CURRENTYEAR}</td></tr></table>";
                                    //    messagebody = messagebody.Replace("{IPADDRESS}", vm);
                                    //    messagebody = messagebody.Replace("{CURRENTYEAR}", Convert.ToString(DateTime.Now.Year));
                                    //    _emailer.SendMail(ToAddressList, "grs@clarivate.com", "Client Machine " + vm + " Not Responding", messagebody);
                                    //}
                                }
                            });
                            WriteControllerLogToFile("Run Rel Initiate Testing", "Ready to invoke client machine", type, rel, vm, env, account, ToAddress, "");
                            anonymousServiceCall.Start();
                            WriteControllerLogToFile("Run Rel Initiate Testing", "Remote machine invoked, Linear Execution initiated", type, rel, vm, env, account, ToAddress, "");

                            CreateRelLogs(vm, "Linear execution initiated");
                            DateTime Localdateitme = timeservice.GetServiceCurrentDateTime();
                            bool IsTimeout = false;
                            Int64 MaxCount = lastRExStData.TotalRelExecutionCount;
                            Int64 LastRelExecutionID = lastRExStData.LastRelExecutionId;
                            while (MaxCount <= lastRExStData.TotalRelExecutionCount
                                && LastRelExecutionID <= lastRExStData.LastRelExecutionId
                                && IsTimeout == false)
                            {
                                var nextRExStData = _relreportProvider.GetLastIdAndCountOfRelExecutionStatus(executionId, vm);
                                MaxCount = nextRExStData.TotalRelExecutionCount;
                                LastRelExecutionID = nextRExStData.LastRelExecutionId;
                                if (MaxCount > lastRExStData.TotalRelExecutionCount)
                                {
                                    CreateRelLogs(vm, "Tracking row generated by GAgent. (checked at max row count)");
                                    WriteControllerLogToFile("Run Rel Initiate Testing", "Tracking row generated by GAgent. (Checked by count)", type, rel, vm, env, account, ToAddress, "");
                                    try
                                    {
                                        prelexid = nextRExStData.TotalRelExecutionCount;
                                    }
                                    catch (Exception ex)
                                    {
                                        prelexid = 0;
                                        WriteControllerLogToFile("Run Rel Initiate Testing", "exception occurred while fetching the new tracking row ID (check by count). Exception: " + ex.Message, type, rel, vm, env, account, ToAddress, "");
                                    }
                                    break;
                                }
                                else if (LastRelExecutionID > lastRExStData.LastRelExecutionId)
                                {
                                    CreateRelLogs(vm, "Tracking row generated by GAgent. (checked at max row ID)");
                                    WriteControllerLogToFile("Run Rel Initiate Testing", "Tracking row generated by GAgent. (Checked by ID)", type, rel, vm, env, account, ToAddress, "");
                                    try
                                    {
                                        prelexid = nextRExStData.TotalRelExecutionCount;
                                    }
                                    catch (Exception ex)
                                    {
                                        prelexid = 0;
                                        WriteControllerLogToFile("Run Rel Initiate Testing", "Exception occurred while fetching the new tracking row ID (checked by ID). Exception: " + ex.Message, type, rel, vm, env, account, ToAddress, "");
                                    }
                                    break;
                                }
                                DateTime serverCurrentTime = timeservice.GetServiceCurrentDateTime();
                                if ((serverCurrentTime - Localdateitme).TotalMinutes > 6)
                                {
                                    CreateRelLogs(vm, "GAgent failed to generate tracking row, Timeout occurred.");
                                    WriteControllerLogToFile("Run Rel Initiate Testing", "GAgent failed to generate tracking row. Timeout occurred (wait time >= 6 minute)", type, rel, vm, env, account, ToAddress, "");
                                    try
                                    {
                                        prelexid = nextRExStData.TotalRelExecutionCount;
                                    }
                                    catch (Exception ex)
                                    {
                                        WriteControllerLogToFile("Run Rel Initiate Testing", "Exception occurred while fetching last row ID (TIMEOUT). Exception: " + ex.Message, type, rel, vm, env, account, ToAddress, "");
                                        prelexid = 0;
                                    }
                                    IsTimeout = true; break;
                                }
                                Thread.Sleep(30000);
                            }

                            if (IsTimeout)
                            {
                                WriteControllerLogToFile("Run Rel Initiate Testing", "Manual tracking row creation initiated (using web service) with timeout.", type, rel, vm, env, account, ToAddress, "");
                                Int64 relexecutionid = timeservice.CreateRelExecutionStatus(executionId, vm, false, "TIMEOUT", "GAgent did not accept the command.");
                                //Int64 relexecutionid = _relreportProvider.CreateRelExecutionStatus(executionId, vm, false, "TIMEOUT", "GAgent did not accept the command.");
                                if (relexecutionid > 0)
                                    _relreportProvider.UpdateLatestRelExecutionStatusEntryLevelRecord(relexecutionid, initialtotaltestsuitcount, lastTestsuitID, ToAddress);

                                RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = relexecutionid, MachineIP = vm, Status = "TIMEOUT" };
                                var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                                jsonResponseResult.MaxJsonLength = int.MaxValue;

                                WriteControllerLogToFile("Run Rel Initiate Testing", "Timeout. Reposnce message: " + new JavaScriptSerializer().Serialize(response), type, rel, vm, env, account, ToAddress, "");

                                return jsonResponseResult;
                            }
                            //else if (prelexid == 0)
                            //{
                            //    Int64 relexecutionid = timeservice.CreateRelExecutionStatus(executionId, vm, false, "TIMEOUT", "GAgent did not accept the command.");
                            //    //Int64 relexecutionid = _relreportProvider.CreateRelExecutionStatus(executionId, vm, false, "TIMEOUT", "GAgent did not accept the command.");
                            //
                            //    try
                            //    {
                            //        if (relexecutionid > 0)
                            //            _relreportProvider.UpdateLatestRelExecutionStatusEntryLevelRecord(relexecutionid, initialtotaltestsuitcount, lastTestsuitID, ToAddress);
                            //    }
                            //    catch { }
                            //    RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = prelexid, MachineIP = vm, Status = "TIMEOUT" };
                            //    var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                            //    jsonResponseResult.MaxJsonLength = int.MaxValue;
                            //    return jsonResponseResult;
                            //}
                            else
                            {
                                //update entery 
                                _relreportProvider.UpdateLatestRelExecutionStatusEntryLevelRecord(LastRelExecutionID, initialtotaltestsuitcount, lastTestsuitID, ToAddress);

                                var statusdata = _relreportProvider.GetRelExecutionStatusById(LastRelExecutionID);
                                RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = statusdata.ID, MachineIP = statusdata.MachineIP, Status = (!string.IsNullOrEmpty(statusdata.CurrentStatus)) ? statusdata.CurrentStatus.ToUpper() : "NULL" };
                                var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                                jsonResponseResult.MaxJsonLength = int.MaxValue;
                                WriteControllerLogToFile("Run Rel Initiate Testing", "New tracking row id generated. Response: " + new JavaScriptSerializer().Serialize(response), type, rel, vm, env, account, ToAddress, "");
                                return jsonResponseResult;
                            }
                        }
                        else
                        {
                            WriteControllerLogToFile("Run Rel Initiate Testing", "Requested RunRelInitiateTesting Execution ID not found. Invalid REL link", type, rel, vm, env, account, ToAddress, "");
                            CreateRelLogs(vm, "Execution ID not found. Invalid REL LINK");
                            RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = 0, MachineIP = vm, Status = "INVALID REL LINK" };
                            var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                            jsonResponseResult.MaxJsonLength = int.MaxValue;
                            WriteControllerLogToFile("Run Rel Initiate Testing", "Requested RunRelInitiateTesting Execution ID not found. Invalid REL link. Response: " + new JavaScriptSerializer().Serialize(response), type, rel, vm, env, account, ToAddress, "");
                            return jsonResponseResult;
                        }
                    }
                    catch (Exception ex)
                    {
                        WriteControllerLogToFile("Run Rel Initiate Testing", "Exception occurred. Exception: " + ex.Message, type, rel, vm, env, account, ToAddress, "");
                        CreateRelLogs(vm, "Exception Occurred. Exception Message => " + ex.Message);
                        if (ex.Message.ToString().ToUpper().Contains("SYSTEM.WEB.SERVICES.PROTOCOLS.SOAPEXCEPTION: SERVER WAS UNABLE TO PROCESS REQUEST."))
                        {
                            CreateRelLogs(vm, "Exception Occurred. GAgent did not accept request. Preparing to return.");
                            RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = 0, MachineIP = vm, Status = "INVALID GAGENT/CLIENT" };
                            var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                            jsonResponseResult.MaxJsonLength = int.MaxValue;
                            return jsonResponseResult;
                        }
                        else
                        {
                            CreateRelLogs(vm, "Exception Occurred. Preparing to return. Exception => " + ex.Message);
                            RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = 0, MachineIP = vm, Status = "EXCEPTION OCCURRED" };
                            var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                            jsonResponseResult.MaxJsonLength = int.MaxValue;
                            return jsonResponseResult;
                        }
                    }
                }
                else if (type == "lb")
                {
                    CreateRelLogs(vm, "Load Balanced command received.");
                    var executionId = _executionProvider.GetLoadBalanceIdByRel(rel);
                    if (executionId != 0)
                    {
                        var executor = new ExecutorNgSoapClient();
                        executor.StartLoadBalanced(executionId);
                        executor.WaitForLbRelResults(executionId);
                    }

                    CreateRelLogs(vm, "Execution Completed for Load Balanced. Preparing to return result.");
                    RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = 0, MachineIP = vm, Status = "LOAD BALANCED" };
                    var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                    jsonResponseResult.MaxJsonLength = int.MaxValue;
                    return jsonResponseResult;
                }
                else
                {
                    CreateRelLogs(vm, "Invalid command type received. Command type is " + type + ". Preparing to return with errorcode 102.");
                    RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = 0, MachineIP = vm, Status = "INVALID TYPE" };
                    var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                    jsonResponseResult.MaxJsonLength = int.MaxValue;
                    return jsonResponseResult;
                }
            }
            catch (Exception ex)
            {
                CreateRelLogs(vm, "Exception Occurred. Exception Message => " + ex.Message);
                if (ex.Message.ToString().ToUpper().Contains("SYSTEM.WEB.SERVICES.PROTOCOLS.SOAPEXCEPTION: SERVER WAS UNABLE TO PROCESS REQUEST."))
                {
                    CreateRelLogs(vm, "Exception Occurred. GAgent did not accept request. Preparing to return.");
                    RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = 0, MachineIP = vm, Status = "INVALID GAGENT" };
                    var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                    jsonResponseResult.MaxJsonLength = int.MaxValue;
                    return jsonResponseResult;
                }
                else
                {
                    CreateRelLogs(vm, "Exception Occurred. Preparing to return. Exception => " + ex.Message);
                    RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = 0, MachineIP = vm, Status = "INVALID" };
                    var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                    jsonResponseResult.MaxJsonLength = int.MaxValue;
                    return jsonResponseResult;
                }
            }
        }


        /*Looks not required*/
        /*Step 3: Anonymous method that will continuously check the result status in DB and Update DB with result once it marked it is completed*/
        //private void RunRelContinuousCheckTestCompletion(Int64 LastRelExecutionId, string vm, int executionId, Int64 lastTestsuitID, int initialtotaltestsuitcount, DateTime StartDateTime, string rel)
        //{
        //    var timeservice = new GlobalReportingSystem.Web.UI.LoadBalancedExecutionService.LoadBalancedExecutionServiceSoapClient();
        //    bool IsInternalTimeOut = false;
        //    bool IsCompleted = false;
        //    while (IsCompleted == false && IsInternalTimeOut == false)
        //    {
        //        RelExecutionStatu relexdata = _relreportProvider.GetRelExecutionStatusById(LastRelExecutionId);
        //        IsCompleted = relexdata.IsExecutionCompleted;
        //        if (IsCompleted == true) { break; }
        //        DateTime serverCDTime = timeservice.GetServiceCurrentDateTime();
        //        DateTime serverCurrentDateTime = new DateTime(serverCDTime.Year, serverCDTime.Month, serverCDTime.Day, serverCDTime.Hour, serverCDTime.Minute, serverCDTime.Second);
        //        DateTime lastupdatedCurrentStatus = new DateTime(relexdata.CurrentStatusCheckedAt.Year, relexdata.CurrentStatusCheckedAt.Month, relexdata.CurrentStatusCheckedAt.Day, relexdata.CurrentStatusCheckedAt.Hour, relexdata.CurrentStatusCheckedAt.Minute, relexdata.CurrentStatusCheckedAt.Second);
        //        double difference = (serverCurrentDateTime - lastupdatedCurrentStatus).TotalMinutes;
        //        if (difference > 16)
        //        {
        //            CreateRelLogs(vm, "Update checked -> Time. Last update received at " + lastupdatedCurrentStatus.ToString("dd-MM-yyyy HH:mm:ss"));
        //            IsInternalTimeOut = true; break;
        //        }
        //        else
        //        {
        //            CreateRelLogs(vm, "Update checked -> No Timeout. Last update received at " + lastupdatedCurrentStatus.ToString("dd-MM-yyyy HH:mm:ss"));
        //        }
        //        Thread.Sleep(30000);
        //    }

        //    if (IsInternalTimeOut == true)
        //    {
        //        CreateRelLogs(vm, "Row update level -> Timeout. Prepare to return errorcode 107 with ExecutionId " + executionId + " TestSuitID 0");
        //        // Update Row with Error TimeOut Status 
        //        timeservice.UpdateLatestRelExecutionStatus(LastRelExecutionId, executionId, vm, false, "Timeout after start", "GAgent not updating the status back to GRS");
        //    }
        //    else if (IsCompleted == true)
        //    {
        //        CreateRelLogs(vm, "Execution Completed. Comparing Test suits for Execution ID" + executionId);
        //        Thread.Sleep(120000);
        //        List<TestSuit> FinalAllTestsuit = _relreportProvider.GetTestSuitOnRelAndVM(rel, vm);
        //        if (FinalAllTestsuit.Count > initialtotaltestsuitcount)
        //        {
        //            CreateRelLogs(vm, "Execution Completed. Testsuit found. Preparing to return result with execution ID " + executionId);
        //            FinalAllTestsuit = _relreportProvider.GetTestSuitOnRelAndVM(rel, vm);
        //            int counttoupdate = FinalAllTestsuit.Count;
        //            List<int> TestSuitIDs = (from x in FinalAllTestsuit select x.ID).ToList();
        //            string testsuitidUpdate = string.Join(",", TestSuitIDs);
        //            int maxtsid = TestSuitIDs.Max();
        //            foreach (var item in TestSuitIDs)
        //            {
        //                int tsid = item; //item.TestSuitId;
        //                if (tsid > 0)
        //                    _relreportProvider.UpdateTestSuitStartEndDateTime(tsid, StartDateTime, timeservice.GetServiceCurrentDateTime());
        //            }
        //            //Update Result Level Entry
        //            timeservice.UpdateLatestRelExecutionStatusResultLevelRecord(LastRelExecutionId, counttoupdate, maxtsid, testsuitidUpdate);
        //        }
        //        else
        //        {
        //            CreateRelLogs(vm, "Execution Completed. Testsuit not found. Preparing to return result with execution ID " + executionId);
        //            timeservice.UpdateLatestRelExecutionStatusResultLevelRecord(LastRelExecutionId, initialtotaltestsuitcount, lastTestsuitID, "0");
        //        }
        //    }
        //}

        /*Step 4: Get the client machine execution status by client machine IP*/
        public JsonResult GetLatestStatusOfRelExecutionByIP(string MachineIP)
        {
            WriteControllerLogToFile("Get Latest Status of Rel Execution By IP", "Status check initiated by IP", "", "", MachineIP, "", "", "", "");
            var timeservice = new GlobalReportingSystem.Web.UI.LoadBalancedExecutionService.LoadBalancedExecutionServiceSoapClient();
            var response = Json(_relreportProvider.GetLatestRelStatusByIP(MachineIP, timeservice.GetServiceCurrentDateTime()), JsonRequestBehavior.AllowGet);
            response.MaxJsonLength = int.MaxValue;
            WriteControllerLogToFile("Get Latest Status of Rel Execution By IP", "Response: " + response, "", "", MachineIP, "", "", "", "");
            return response;
        }

        //Step 4: Get the client machine execution status by client machine ID
        public JsonResult GetLatestStatusOfRelExecutionByID(Int64 RelExecutionStatusId)
        {
            WriteControllerLogToFile("Get Latest Status of RelExecution By ID", "Status check initiated by ID", "", "", "", "", "", "", Convert.ToString(RelExecutionStatusId));
            var timeservice = new GlobalReportingSystem.Web.UI.LoadBalancedExecutionService.LoadBalancedExecutionServiceSoapClient();
            var response = Json(_relreportProvider.GetLatestRelStatusByID(RelExecutionStatusId, timeservice.GetServiceCurrentDateTime()), JsonRequestBehavior.AllowGet);
            response.MaxJsonLength = int.MaxValue;
            WriteControllerLogToFile("Get Latest Status of RelExecution By ID", "Response: " + response, "", "", "", "", "", "", Convert.ToString(RelExecutionStatusId));
            return response;
        }

        //Step 5: Generate report for Run Rel execution. It accepts RelExecutionStatusIds as comma seperated string and email address as comma seperated string
        public JsonResult TriggerEmailForReport(string RelExecutionStatusIds, string ToAddress)
        {
            string emailstatus = "";
            //bool resultCalculated = false; 
            List<RelReturn> RelRelateIDs = new List<RelReturn>();
            WriteControllerLogToFile("Trigger Email For Report", "Email Triggered", "", "", "", "", "", ToAddress, RelExecutionStatusIds);
            if (!string.IsNullOrEmpty(ToAddress))
            {
                if (!string.IsNullOrEmpty(RelExecutionStatusIds))
                {
                    //Prepare list of relexecutionstatusid in int64
                    string relid = RelExecutionStatusIds.Trim();
                    string[] relexid = relid.Split(',');
                    List<string> RelIDList = new List<string>(relexid);
                    List<Int64> reid = new List<Int64>();
                    foreach (var item in RelIDList)
                    {
                        Int64 rid = 0;
                        bool isconverted = Int64.TryParse(item, out rid);
                        if (isconverted && rid > 0)
                        {
                            reid.Add(rid);
                        }
                    }

                    List<RelExecutionResponseP2> ListMachineLastStatus = new List<RelExecutionResponseP2>();
                    //Get All The Record from DB crossponding to the IDS

                    bool canemailtrigger = false;
                    foreach (var item in reid)
                    {
                        RelExecutionStatu statusdata = _relreportProvider.GetRelExecutionStatusById(item);
                        if (statusdata != null)
                        {
                            canemailtrigger = true;
                            ListMachineLastStatus.Add(new RelExecutionResponseP2() { RelExecutionStatusId = statusdata.ID, MachineIP = statusdata.MachineIP, Status = statusdata.CurrentStatus });
                            string testsuitidstring = string.Empty;
                            Int64 newrowiddata = statusdata.NewRowId ?? 0;
                            if ((!string.IsNullOrEmpty(statusdata.TestSuitIds)) && statusdata.TestSuitIds != "0")
                            {
                                testsuitidstring = statusdata.TestSuitIds;
                                testsuitidstring = testsuitidstring.Trim();
                            }
                            else if ((string.IsNullOrEmpty(statusdata.TestSuitIds)) && (statusdata.LastRowId > 0) && (statusdata.NewRowId > 0))
                            {
                                int tsstartid = Convert.ToInt32(statusdata.LastRowId ?? 0);
                                int tsendid = Convert.ToInt32(statusdata.NewRowId ?? 0);
                                var tsids = _relreportProvider.GetTestSuitByExecutionIdWithinStartAndEndLimit(statusdata.ExecutionId, tsstartid, tsendid, statusdata.MachineIP);
                                List<int> finaltestsuitids = (from x in tsids select x.ID).ToList();
                                testsuitidstring = string.Join(",", finaltestsuitids);
                            }
                            else if ((string.IsNullOrEmpty(statusdata.TestSuitIds)) && (statusdata.LastRowId > 0))
                            {
                                int tsid = Convert.ToInt32(statusdata.LastRowId ?? 0);
                                var tsids = _relreportProvider.GetTestSuitByExecutionIdAndAfterTestSuitId(statusdata.ExecutionId, tsid, statusdata.MachineIP);
                                List<int> finaltestsuitids = (from x in tsids select x.ID).ToList();
                                testsuitidstring = string.Join(",", finaltestsuitids);
                            }
                            string[] testsuitarray = testsuitidstring.Split(',');
                            List<string> testsuitliststring = new List<string>(testsuitarray);
                            List<int> testsuitid = new List<int>();
                            foreach (var itemt in testsuitliststring)
                            {
                                int tid = 0;
                                bool isconvertedts = int.TryParse(itemt, out tid);
                                if (isconvertedts && tid > 0)
                                {
                                    testsuitid.Add(tid);
                                }
                            }

                            foreach (var itemtsid in testsuitid)
                            {
                                RelRelateIDs.Add(new RelReturn()
                                {
                                    ExecutionId = statusdata.ExecutionId,
                                    TestSuitId = itemtsid
                                });
                            }
                        }
                    }

                    //Trigger for email
                    if (canemailtrigger)
                    {
                        string ToListString = ToAddress.Trim();
                        string[] ToListArray = ToListString.Split(',');
                        List<string> ToList = new List<string>(ToListArray);
                        string messagebody = _relreportProvider.HTMLForLaunchRELReportEmail(RelRelateIDs, ListMachineLastStatus);
                        _emailer.SendMail(ToList, "grs@clarivate.com", "Automated Test Execution Report", messagebody);
                        //return Json("Email Send.", JsonRequestBehavior.AllowGet);
                        emailstatus = "Email Dispatched.";

                    }
                    else
                    {
                        //return Json("Result entry from testing framework is not found in GRS. Email not send.", JsonRequestBehavior.AllowGet);
                        emailstatus = "Result entry from testing framework is not found in GRS. Email not send.";
                    }
                }
                else
                {
                    //return Json("Invalid input data - RelExecutionStatusIds. Email not send.", JsonRequestBehavior.AllowGet);
                    emailstatus = "Invalid input data - RelExecutionStatusIds. Email not send.";
                }
            }
            else
            {
                //return Json("Invlid input data - ToAddress. Email not send.", JsonRequestBehavior.AllowGet);
                emailstatus = "Invlid input data - ToAddress. Email not dispatched.";
            }


            //Final return from here
            List<int> finaltestsuitidsforcount = (from x in RelRelateIDs select x.TestSuitId).ToList();
            JenkinsReportCountStatus getcount = _relreportProvider.GetJenkinReportFinalCountByTestSuitIds(finaltestsuitidsforcount);

            JenkinsReportCountStatus returncountitem = new JenkinsReportCountStatus()
            {
                emailStatus = emailstatus,
                totalTestCaseCount = getcount.totalTestCaseCount,
                totalPassCount = getcount.totalPassCount,
                totalFailCount = getcount.totalFailCount,
                totalNoRunCount = getcount.totalNoRunCount,
                totalNotCompletedCount = getcount.totalNotCompletedCount,
            };

            var response = Json(returncountitem, JsonRequestBehavior.AllowGet);
            response.MaxJsonLength = int.MaxValue;
            return response;

        }

        private void WriteControllerLogToFile(string MessageFrom, string LogMessage, string type = "", string rel = "", string vm = "", string env = "", string account = "", string ToAddress = "", string RelExecutionStatusIds = "")
        {
            try
            {
                //string path = @"E:\Jenkins\workspace\GRSNG\GlobalReportingSystem.Web.UI\ControllerLogFiles\";
                string path = @"D:\Jenkins\workspace\GRSNG\GlobalReportingSystem.Web.UI\ControllerLogFiles\";
                string pathvariable = "";
                if (!string.IsNullOrEmpty(vm))
                {
                    pathvariable = vm;
                }
                else if (!string.IsNullOrEmpty(RelExecutionStatusIds))
                {
                    pathvariable = "RELIDS";
                }
                string logFileName = "Controller_Log_" + pathvariable + ".txt";
                string logFilePath = path + logFileName;
                if (!System.IO.File.Exists(logFilePath))
                {
                    System.IO.File.Create(logFilePath);
                }
                string message = Environment.NewLine;
                message += Environment.NewLine;
                message += "------------------------" + string.Format("Date & Time: {0}", DateTime.Now.ToString("dd/MM/yyyy hh:mm:ss tt")) + "------------------------";
                message += Environment.NewLine;
                message += Environment.NewLine;
                message += ("Message requested from: " + MessageFrom);
                message += Environment.NewLine;
                message += ("Type of execution: " + type);
                message += Environment.NewLine;
                message += ("Remote execution link: " + rel);
                message += Environment.NewLine;
                message += ("Machine IP: " + vm);
                message += Environment.NewLine;
                message += ("Environment: " + env);
                message += Environment.NewLine;
                message += ("Account: " + account);
                message += Environment.NewLine;
                message += ("To Email Address: " + ToAddress);
                message += Environment.NewLine;
                message += ("Rel Execution Status ID: " + RelExecutionStatusIds);
                message += Environment.NewLine;
                message += ("Log Message: " + LogMessage);
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

        //---------------------SUNDRAM---------------------------------------------------------------------------------

        public const string scenarioKey = "Scenario:";
        public const string scenarioOutlineKey = "Scenario Outline:";
        public const string Background = "Background:";

        [HttpPost]
        public JsonResult GetFeatureListAndTagFromProjectFolder(int ProjectId, string ProjectName, string IsGitDefault, string SvnPath, string SvnLogin, string SvnPwd, string GITPath)
        {
            FeatureAndTags FandT = new FeatureAndTags();
            var currentUser = _userProvider.GetUserProfile(User);
            int userid = currentUser.Id;
            string projectpath = @"D:\GRS\Launch\" + userid + "\\" + ProjectName;
            try
            {
                var projectdata = _configurationProvider.GetProjectDataById(ProjectId);
                //string pathToFeatureFile = String.Concat(projectpath, "\\" + "src\\test\\resources").Replace("\\\\", "\\");
                string pathToFeatureFile = projectpath.Replace("\\\\", "\\");
                bool gitdefault = (IsGitDefault == "y" || IsGitDefault == "Y") ? true : false;
                if (gitdefault == true)
                {
                    string GitUsername = projectdata.GITUsername;
                    string GitPassword = projectdata.GITPassword;
                    bool iscloned = false;
                    int count = 0;
                    while (iscloned == false && count <= 15)
                    {
                        count += 1;
                        iscloned = _configurationProvider.CloneProjectForFeaturesAndTagsFromGIT(projectpath, true, "", "", "", GITPath, GitUsername, GitPassword, ProjectName);
                    }
                }
                else
                {
                    bool iscloned = false;
                    int count = 0;
                    while (iscloned == false && count <= 15)
                    {
                        count += 1;
                        iscloned = _configurationProvider.CloneProjectForFeaturesAndTagsFromGIT(projectpath, false, SvnPath, SvnLogin, SvnPwd, "", "", "", ProjectName);
                    }
                }
                // Feature
                try
                {
                    IEnumerable<FeatureReturn> Feature = GetAllFeatureFromProjectFolder(pathToFeatureFile);
                    FandT.Features = Feature;
                    FandT.FeatureErrorId = 0;
                    FandT.FeatureErrorMessage = string.Empty;
                }
                catch (Exception ex)
                {
                    FandT.FeatureErrorId = 1;
                    FandT.FeatureErrorMessage = ex.Message;
                }
                // Tags
                try
                {
                    IEnumerable<string> Tags = GetAllTagsFromProjectFolder(pathToFeatureFile);
                    FandT.Tags = Tags;
                    FandT.TagsErrorId = 0;
                    FandT.TagsErrorMessage = string.Empty;
                }
                catch (Exception ex)
                {
                    FandT.TagsErrorId = 1;
                    FandT.TagsErrorMessage = ex.Message;
                }
                FandT.GeneralErrorId = 0;
                FandT.GeneralErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                FandT.GeneralErrorId = 2;
                FandT.GeneralErrorMessage = ex.Message;
            }
            _configurationProvider.DeleteDirectory(projectpath);
            return Json(FandT, JsonRequestBehavior.AllowGet);
        }

        //Feature File Name
        private IEnumerable<FeatureReturn> GetAllFeatureFromProjectFolder(string FeaturePath)
        {
            string pathToFile = FeaturePath;
            var dirs = new List<string>();
            dirs.AddRange(Directory.GetDirectories(pathToFile).ToList());
            var files = new List<string>();
            files.AddRange(Directory.GetFiles(pathToFile, "*.feature").ToList());
            while (dirs.Count != 0)
            {
                string[] paths = dirs.ToArray();
                dirs.Clear();
                foreach (var path in paths)
                {
                    files.AddRange(Directory.GetFiles(path, "*.feature").ToList());
                    dirs.AddRange(Directory.GetDirectories(path).ToList());
                }
            }
            int filecount = 0;
            var listFeatures = new List<FeatureFromFolder>();
            foreach (var file in files)
            {
                filecount += 1;
                var featureName = file.Split('\\').Last().Replace(".feature", "");
                var featurePath = file.Replace(pathToFile, "").Replace(featureName + ".feature", "");
                featurePath = featurePath.EndsWith("\\") ? featurePath.Substring(0, featurePath.Length - 1) : featurePath;
                if (featurePath.StartsWith("\\")) { featurePath = featurePath.Remove(0, 1); }
                var lines = new List<string>();
                lines.AddRange(System.IO.File.ReadAllLines(file));
                lines = lines.Select(p => p.Trim()).ToList();
                lines.RemoveAll(s => s == "\t" | s == "");
                lines = DeleteFeatureComments(lines, filecount);
                string featureTag = string.Join(" ", lines.TakeWhile(p => p.TrimStart().StartsWith("@")).Select(p => p.Trim()));
                string featureDescription = GetFeatureDescription(lines);
                var scenarios = GetScenariosFromLines(lines);
                listFeatures.Add(new FeatureFromFolder
                {
                    Name = featureName,
                    Path = string.IsNullOrEmpty(featurePath) ? null : featurePath,
                    Description = featureDescription,
                    Tags = featureTag,
                    Scenarios = scenarios
                });
            }
            var features = (from x in listFeatures orderby x.Path ascending select new FeatureReturn { Name = x.Name, Path = x.Path }).ToList();
            return features;
        }

        //Feature Tags
        private IEnumerable<string> GetAllFeatureTagsFromProjectFolder(string FeaturePath)
        {
            List<string> FTagsList = new List<string>();
            string pathToFile = FeaturePath;
            var dirs = new List<string>();
            dirs.AddRange(Directory.GetDirectories(pathToFile).ToList());
            var files = new List<string>();
            files.AddRange(Directory.GetFiles(pathToFile, "*.feature").ToList());
            while (dirs.Count != 0)
            {
                string[] paths = dirs.ToArray();
                dirs.Clear();
                foreach (var path in paths)
                {
                    files.AddRange(Directory.GetFiles(path, "*.feature").ToList());
                    dirs.AddRange(Directory.GetDirectories(path).ToList());
                }
            }
            int filecount = 0;
            foreach (var file in files)
            {
                filecount += 1;
                var featureName = file.Split('\\').Last().Replace(".feature", "");
                var featurePath = file.Replace(pathToFile, "").Replace(featureName + ".feature", "");
                featurePath = featurePath.EndsWith("\\") ? featurePath.Substring(0, featurePath.Length - 1) : featurePath;
                if (featurePath.StartsWith("\\")) { featurePath = featurePath.Remove(0, 1); }
                var lines = new List<string>();
                lines.AddRange(System.IO.File.ReadAllLines(file));
                lines = lines.Select(p => p.Trim()).ToList();
                lines.RemoveAll(s => s == "\t" | s == "");
                lines = DeleteFeatureComments(lines, filecount);
                string featureTag = string.Join(" ", lines.TakeWhile(p => p.TrimStart().StartsWith("@")).Select(p => p.Trim()));
                string featureDescription = GetFeatureDescription(lines);
                var scenarios = GetScenariosFromLines(lines);

                string[] FTagsArr = featureTag.Trim().Split(' ');
                foreach (var item in FTagsArr.ToList())
                {
                    if (!string.IsNullOrEmpty(item)) { FTagsList.Add(item); }
                }
            }
            var features = (from x in FTagsList orderby x ascending select x).Distinct().ToList();
            return features;
        }

        //Scenario Tags
        private IEnumerable<string> GetAllTagsFromProjectFolder(string TagPath)
        {
            string pathToFile = TagPath;
            var dirs = new List<string>();
            dirs.AddRange(Directory.GetDirectories(pathToFile).ToList());
            var files = new List<string>();
            files.AddRange(Directory.GetFiles(pathToFile, "*.feature").ToList());
            while (dirs.Count != 0)
            {
                string[] paths = dirs.ToArray();
                dirs.Clear();
                foreach (var path in paths)
                {
                    files.AddRange(Directory.GetFiles(path, "*.feature").ToList());
                    dirs.AddRange(Directory.GetDirectories(path).ToList());
                }
            }
            int filecount = 0;
            var listFeatures = new List<FeatureFromFolder>();
            foreach (var file in files)
            {
                filecount += 1;
                var featureName = file.Split('\\').Last().Replace(".feature", "");
                var featurePath = file.Replace(pathToFile, "").Replace(featureName + ".feature", "");
                featurePath = featurePath.EndsWith("\\") ? featurePath.Substring(0, featurePath.Length - 1) : featurePath;
                if (featurePath.StartsWith("\\"))
                {
                    featurePath = featurePath.Remove(0, 1);
                }
                var lines = new List<string>();
                lines.AddRange(System.IO.File.ReadAllLines(file));
                lines = lines.Select(p => p.Trim()).ToList();
                lines.RemoveAll(s => s == "\t" | s == "");
                lines = DeleteFeatureComments(lines, filecount);
                string featureTag = string.Join(" ", lines.TakeWhile(p => p.TrimStart().StartsWith("@")).Select(p => p.Trim()));
                string featureDescription = GetFeatureDescription(lines);
                var scenarios = GetScenariosFromLines(lines);
                listFeatures.Add(new FeatureFromFolder
                {
                    Name = featureName,
                    Path = string.IsNullOrEmpty(featurePath) ? null : featurePath,
                    Description = featureDescription,
                    Tags = featureTag,
                    Scenarios = scenarios
                });
            }
            var secnariolist = (from x in listFeatures select x.Scenarios).ToList();
            List<string> scenarioTags = new List<string>();
            foreach (var item in secnariolist)
            {
                foreach (var inneritem in item)
                {
                    if (!string.IsNullOrEmpty(inneritem.Tags))
                    {
                        string[] tagmultiple = inneritem.Tags.Trim().Split(' ');
                        foreach (var ftag in tagmultiple)
                        {
                            string tag = ftag.Trim();
                            if (!string.IsNullOrEmpty(tag))
                                scenarioTags.Add(tag);
                        }
                    }
                }
            }
            List<string> FinalTags = (from x in scenarioTags orderby x ascending select x).Distinct().ToList();
            return FinalTags;
        }

        private List<string> DeleteFeatureComments(List<string> lines, int filenumber)
        {
            int linecount = 0;
            var i = 1;
            if ((lines.FindIndex(p => !p.StartsWith("@")) + i) < lines.Count())
            {
                linecount += 1;

                var line = lines[lines.FindIndex(p => !p.StartsWith("@")) + i];
                var linesToDelete = new List<string>();

                while ((lines.FindIndex(p => !p.StartsWith("@")) + i) < lines.Count() && !line.StartsWith("@"))
                {
                    try
                    {
                        if (line.StartsWith("#"))
                        {
                            linesToDelete.Add(line);
                        }
                        i++;
                        var lineitem = lines.FindIndex(p => !p.StartsWith("@"));
                        if (lineitem > 0)
                            line = lines[lineitem + i];
                    }
                    catch (Exception ex)
                    {
                    }
                }

                try
                {
                    linesToDelete.ForEach(p => lines.Remove(p));
                }
                catch (Exception ex)
                {
                }
            }
            return lines;
        }

        private string GetFeatureDescription(List<string> lines)
        {
            var description = "";
            if ((lines.FindIndex(p => !p.TrimStart().StartsWith("@")) + 1) < lines.Count())
            {
                var index = lines.FindIndex(p => !p.TrimStart().StartsWith("@")) + 1;
                while (index < lines.Count() && lines[index].Trim() != Background && !lines[index].ToLower().StartsWith("@"))
                {
                    description += lines[index].Trim() + Environment.NewLine;
                    index++;
                }
            }
            return description;
        }

        private List<ScenarioFromFolder> GetScenariosFromLines(List<string> lines)
        {
            var listScenario = new List<ScenarioFromFolder>();
            var scenarioLines = new List<string>();
            string scenarioTag = null, scenarioName = null;
            string exampleTablesString = null;
            var listScenarioLine = new List<ScenarioLineFromFolder>();
            bool isLine = false;
            //Finding the line where feature tags and name end and scenario begins
            lines.RemoveRange(0, lines.FindIndex(p => !p.TrimStart().StartsWith("@")) + 1);
            while (lines.Any() && !lines[0].Trim().StartsWith("@") && lines[0].Trim() != Background)
            {
                lines.Remove(lines[0]);
            }
            for (int i = 0; i < lines.Count(); i++)
            {
                if (!isLine)
                {
                    exampleTablesString = null;
                    if (lines[i].Trim().Replace(" ", "") == Background)
                    {
                        scenarioName = lines[i].Trim().Replace(" ", "");
                    }
                    else
                    {
                        int scenarioNamePos = i + lines.Skip(i).ToList().FindIndex(p => !p.TrimStart().StartsWith("@"));
                        scenarioTag = string.Join(" ", lines.Skip(i).Take(scenarioNamePos - i).Select(p => p.Trim()));
                        scenarioName = lines[scenarioNamePos].Replace(scenarioKey, "").Replace("Scenario Outline :", scenarioOutlineKey).Replace(scenarioOutlineKey, "").TrimStart(' ');
                        i = scenarioNamePos;
                    }
                    isLine = true;
                }
                else
                {
                    if (i != lines.Count - 1 && !lines[i + 1].TrimStart().StartsWith(scenarioKey) &&
                        !lines[i + 1].TrimStart().StartsWith(scenarioOutlineKey) &&
                        !lines[i + 1].TrimStart().StartsWith("@") &&
                        !lines[i + 1].TrimStart().StartsWith("Examples:"))
                    {
                        scenarioLines.Add(lines[i].TrimStart());
                    }
                    else
                    {
                        var index = i + 1;
                        if (index < lines.Count() && lines[i + 1].TrimStart().StartsWith("Examples:"))
                        {
                            while (index < lines.Count() && !lines[index].TrimStart().StartsWith(scenarioKey) &&
                                   !lines[index].TrimStart().StartsWith(scenarioOutlineKey) &&
                                   !lines[index].TrimStart().StartsWith("@"))
                            {
                                if (!string.IsNullOrEmpty(exampleTablesString))
                                {
                                    exampleTablesString += "\r\n";
                                }
                                if (!lines[index].TrimStart().StartsWith("Examples:") && !lines[index].TrimStart().StartsWith("|") && !lines[index].TrimStart().StartsWith("#") && !lines[index].TrimStart().StartsWith("|"))
                                {
                                    throw new Exception("Parsing error. [" + scenarioName + "] scenario has unexpected end");
                                }
                                exampleTablesString += lines[index].TrimStart();
                                index++;
                            }
                        }
                        //We reached last line of the scenario
                        scenarioLines.Add(lines[i].TrimStart());
                        for (int j = 0; j < scenarioLines.Count; j++)
                        {
                            listScenarioLine.Add(new ScenarioLineFromFolder
                            {
                                OrderId = j,
                                Line = scenarioLines[j],
                            });
                        }
                        listScenario.Add(new ScenarioFromFolder
                        {
                            Name = scenarioName,
                            Tags = scenarioTag,
                            ScenarioLines = new List<ScenarioLineFromFolder>(),
                            ExamplesTable = exampleTablesString
                        });
                        listScenarioLine.ForEach(p => listScenario.Last().ScenarioLines.Add(p));
                        scenarioLines.Clear();
                        listScenarioLine.Clear();
                        isLine = false;
                        if (!string.IsNullOrEmpty(exampleTablesString))
                        {
                            i = index - 1;
                        }
                    }
                }
            }
            return listScenario.ToList();
        }

        [HttpPost]
        public JsonResult UpdateFeaturesAndTagInDB(int ProjectId, string ProjectName, string IsGitDefault, string SvnPath, string SvnLogin, string SvnPwd, string GITPath)
        {
            ResponseResult responseResult = new ResponseResult();

            var projectdata = _configurationProvider.GetProjectDataById(ProjectId);
            if (projectdata != null)
            {
                bool IsTestCaseUpdating = (projectdata.IsTestCaseUpdating == true) ? true : false;
                int TestCaseUpdatedBy = projectdata.TestCaseUpdatedBy ?? 0;

                if (IsTestCaseUpdating == false)
                {
                    var currentUser = _userProvider.GetUserProfile(User);
                    int userid = currentUser.Id;

                    //Lock Project
                    _configurationProvider.LockProjectForFeatureTagUpdate(ProjectId, userid);

                    //Update Features Files, Feature Tags and Scenario Tags

                    //string projectpath = @"D:\GRS\Launch\" + userid + "\\" + ProjectName;
                    string projectpath = @"E:\GRS_New\Launch\" + userid + "\\" + ProjectName;

                    #region Project Cloning
                    try
                    {
                        //string pathToFeatureFile = String.Concat(projectpath, "\\" + "src\\test\\resources").Replace("\\\\", "\\");
                        string pathToFeatureFile = projectpath.Replace("\\\\", "\\");
                        bool gitdefault = (IsGitDefault == "y" || IsGitDefault == "Y") ? true : false;
                        if (gitdefault == true)
                        {
                            string GitUsername = projectdata.GITUsername;
                            string GitPassword = projectdata.GITPassword;
                            bool iscloned = false;
                            int count = 0;
                            while (iscloned == false && count <= 15)
                            {
                                count += 1;
                                iscloned = _configurationProvider.CloneProjectForFeaturesAndTagsFromGIT(projectpath, true, "", "", "", GITPath, GitUsername, GitPassword, ProjectName);
                            }
                        }
                        else
                        {
                            bool iscloned = false;
                            int count = 0;
                            while (iscloned == false && count <= 15)
                            {
                                count += 1;
                                iscloned = _configurationProvider.CloneProjectForFeaturesAndTagsFromGIT(projectpath, false, SvnPath, SvnLogin, SvnPwd, "", "", "", ProjectName);
                            }
                        }
                        #endregion Project Cloning

                        //Database part => Remove existing item form database and add fresh item in DB (Feature and Tags)
                        //Feature                        
                        try
                        {
                            IEnumerable<FeatureReturn> Feature = GetAllFeatureFromProjectFolder(pathToFeatureFile);
                            var AllFeaturesFolder = (from x in Feature select new Feature { Name = x.Name, Path = x.Path, ProjectId = ProjectId }).ToList();
                            List<Feature> AllFeaturesFromDB = _launchProvider.GetAllFeatures(ProjectId);
                            var AllFeatureNotInDB_Add = AllFeaturesFolder.Where(FD => !AllFeaturesFromDB.Any(FF => FF.Name == FD.Name && FF.Path == FD.Path)).ToList();
                            List<Int64> AllFeatureNotInFolder_Delete = new List<Int64>();
                            foreach (var item in AllFeaturesFromDB)
                            {
                                bool isExist = (from x in AllFeaturesFolder where x.Name == item.Name && x.Path == item.Path select x).Any();
                                if (!isExist)
                                {
                                    AllFeatureNotInFolder_Delete.Add(item.ID);
                                }
                            }

                            bool isAdded = _launchProvider.AddFeaturesToDB(AllFeatureNotInDB_Add);
                            bool isRemoved = _launchProvider.DeleteFeatureFromDBById(AllFeatureNotInFolder_Delete);

                            if (isAdded)
                            {
                                responseResult.FeatureErrorId = 0;
                                responseResult.FeatureErrorMessage = "";
                            }
                            else
                            {
                                responseResult.FeatureErrorId = 1;
                                responseResult.FeatureErrorMessage = "Error occurred in adding feature.";
                            }
                            if (isRemoved)
                            {
                                responseResult.FeatureErrorId = 0;
                                responseResult.FeatureErrorMessage = "";
                            }
                            else
                            {
                                responseResult.FeatureErrorId = 1;
                                responseResult.FeatureErrorMessage = "Error occurred in deleting feature.";
                            }
                        }
                        catch (Exception ex)
                        {
                            responseResult.FeatureErrorId = 3;
                            responseResult.FeatureErrorMessage = ex.Message;
                        }

                        try
                        {
                            //Get All FEATURE TAG SPECIFIC                        
                            IEnumerable<string> Ftags = GetAllFeatureTagsFromProjectFolder(pathToFeatureFile);

                            // Tags                        
                            IEnumerable<string> Tags = GetAllTagsFromProjectFolder(pathToFeatureFile);

                            List<string> FTa = Ftags.ToList();
                            List<string> STa = Tags.ToList();

                            var newTagList = FTa.Concat(STa);

                            if (_launchProvider.UpdateTags(newTagList.ToList(), ProjectId))
                            {
                                responseResult.TagErrorId = 0;
                                responseResult.TagErrorMessage = "";
                            }
                            else
                            {
                                responseResult.TagErrorId = 4;
                                responseResult.TagErrorMessage = "Error occurred in updating tags";
                            }
                        }
                        catch (Exception ex)
                        {
                            responseResult.TagErrorId = 5;
                            responseResult.TagErrorMessage = ex.Message;
                        }
                    }
                    catch (Exception ex)
                    {
                        responseResult.GeneralErrorId = 6;
                        responseResult.GeneralErrorMessage = ex.Message;
                    }
                    _configurationProvider.DeleteDirectory(projectpath);

                    //Release Project
                    _configurationProvider.ReleaseProjectForFeatureTagUpdate(ProjectId, userid);
                }
                else
                {
                    responseResult.GeneralErrorId = 99;
                    responseResult.GeneralErrorMessage = "Features and Tags of project is updating by '" + _configurationProvider.GetUserNameById(TestCaseUpdatedBy) + "'. Please wait for some time.";
                }
            }
            else
            {
                responseResult.GeneralErrorId = 98;
                responseResult.GeneralErrorMessage = "Project not found.";
            }
            return Json(responseResult, JsonRequestBehavior.AllowGet);
        }

        //[HttpPost]
        //public JsonResult UpdateFeaturesAndTagInDB(int ProjectId, string ProjectName, string IsGitDefault, string SvnPath, string SvnLogin, string SvnPwd, string GITPath)
        //{
        //    ResponseResult responseResult = new ResponseResult();

        //    var projectdata = _configurationProvider.GetProjectDataById(ProjectId);
        //    if (projectdata != null)
        //    {
        //        bool IsTestCaseUpdating = (projectdata.IsTestCaseUpdating == true) ? true : false;
        //        int TestCaseUpdatedBy = projectdata.TestCaseUpdatedBy ?? 0;

        //        if (IsTestCaseUpdating == false)
        //        {
        //            var currentUser = _userProvider.GetUserProfile(User);
        //            int userid = currentUser.Id;

        //            //Lock Project
        //            _configurationProvider.LockProjectForFeatureTagUpdate(ProjectId, userid);

        //            //Update Features and Tags

        //            string projectpath = @"E:\GRS_New\Launch\" + userid + "\\" + ProjectName;
        //            //string projectpath = @"D:\GRS\Launch\" + userid + "\\" + ProjectName;
        //            try
        //            {
        //                //string pathToFeatureFile = String.Concat(projectpath, "\\" + "src\\test\\resources").Replace("\\\\", "\\");
        //                string pathToFeatureFile = projectpath.Replace("\\\\", "\\");
        //                bool gitdefault = (IsGitDefault == "y" || IsGitDefault == "Y") ? true : false;
        //                if (gitdefault == true)
        //                {
        //                    string GitUsername = System.Configuration.ConfigurationManager.AppSettings["GITUser"];
        //                    string GitPassword = System.Configuration.ConfigurationManager.AppSettings["GITPassword"];
        //                    bool iscloned = false;
        //                    int count = 0;
        //                    while (iscloned == false && count <= 15)
        //                    {
        //                        count += 1;
        //                        iscloned = _configurationProvider.CloneProjectForFeaturesAndTagsFromGIT(projectpath, true, "", "", "", GITPath, GitUsername, GitPassword, ProjectName);
        //                    }
        //                }
        //                else
        //                {
        //                    bool iscloned = false;
        //                    int count = 0;
        //                    while (iscloned == false && count <= 15)
        //                    {
        //                        count += 1;
        //                        iscloned = _configurationProvider.CloneProjectForFeaturesAndTagsFromGIT(projectpath, false, SvnPath, SvnLogin, SvnPwd, "", "", "", ProjectName);
        //                    }
        //                }

        //                //Database part => Remove existing item form database and add fresh item in DB (Feature and Tags)
        //                //Feature
        //                try
        //                {
        //                    IEnumerable<FeatureReturn> Feature = GetAllFeatureFromProjectFolder(pathToFeatureFile);
        //                    var AllFeaturesFolder = (from x in Feature select new Feature { Name = x.Name, Path = x.Path, ProjectId = ProjectId }).ToList();
        //                    List<Feature> AllFeaturesFromDB = _launchProvider.GetAllFeatures(ProjectId);
        //                    var AllFeatureNotInDB_Add = AllFeaturesFolder.Where(FD => !AllFeaturesFromDB.Any(FF => FF.Name == FD.Name && FF.Path == FD.Path)).ToList();
        //                    List<Int64> AllFeatureNotInFolder_Delete = new List<Int64>();
        //                    foreach (var item in AllFeaturesFromDB)
        //                    {
        //                        bool isExist = (from x in AllFeaturesFolder where x.Name == item.Name && x.Path == item.Path select x).Any();
        //                        if (!isExist)
        //                        {
        //                            AllFeatureNotInFolder_Delete.Add(item.ID);
        //                        }
        //                    }

        //                    bool isAdded = _launchProvider.AddFeaturesToDB(AllFeatureNotInDB_Add);
        //                    bool isRemoved = _launchProvider.DeleteFeatureFromDBById(AllFeatureNotInFolder_Delete);

        //                    if (isAdded)
        //                    {
        //                        responseResult.FeatureErrorId = 0;
        //                        responseResult.FeatureErrorMessage = "";
        //                    }
        //                    else
        //                    {
        //                        responseResult.FeatureErrorId = 1;
        //                        responseResult.FeatureErrorMessage = "Error occurred in adding feature.";
        //                    }
        //                    if (isRemoved)
        //                    {
        //                        responseResult.FeatureErrorId = 0;
        //                        responseResult.FeatureErrorMessage = "";
        //                    }
        //                    else
        //                    {
        //                        responseResult.FeatureErrorId = 1;
        //                        responseResult.FeatureErrorMessage = "Error occurred in deleting feature.";
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    responseResult.FeatureErrorId = 3;
        //                    responseResult.FeatureErrorMessage = ex.Message;
        //                }

        //                // Tags
        //                try
        //                {
        //                    IEnumerable<string> Tags = GetAllTagsFromProjectFolder(pathToFeatureFile);
        //                    if (_launchProvider.UpdateTags(Tags.ToList(), ProjectId))
        //                    {
        //                        responseResult.TagErrorId = 0;
        //                        responseResult.TagErrorMessage = "";
        //                    }
        //                    else
        //                    {
        //                        responseResult.TagErrorId = 4;
        //                        responseResult.TagErrorMessage = "Error occurred in updating tags";
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    responseResult.TagErrorId = 5;
        //                    responseResult.TagErrorMessage = ex.Message;
        //                }
        //            }
        //            catch (Exception ex)
        //            {
        //                responseResult.GeneralErrorId = 6;
        //                responseResult.GeneralErrorMessage = ex.Message;
        //            }
        //            _configurationProvider.DeleteDirectory(projectpath);

        //            //Release Project
        //            _configurationProvider.ReleaseProjectForFeatureTagUpdate(ProjectId, userid);
        //        }
        //        else
        //        {
        //            responseResult.GeneralErrorId = 99;
        //            responseResult.GeneralErrorMessage = "Features and Tags of project is updating by '" + _configurationProvider.GetUserNameById(TestCaseUpdatedBy) + "'. Please wait for some time.";
        //        }
        //    }
        //    else
        //    {
        //        responseResult.GeneralErrorId = 98;
        //        responseResult.GeneralErrorMessage = "Project not found.";
        //    }
        //    return Json(responseResult, JsonRequestBehavior.AllowGet);
        //}

        [HttpPost]
        public JsonResult GetFeatureAndTagsFromDB(int ProjectId)
        {
            FeatureAndTags FandT = new FeatureAndTags();
            try
            {
                // Feature
                try
                {
                    List<Feature> featureList = _launchProvider.GetAllFeatures(ProjectId);
                    IEnumerable<FeatureReturn> Feature = (from x in featureList
                                                          select new FeatureReturn
                                                          {
                                                              Name = x.Name,
                                                              Path = x.Path
                                                          });
                    FandT.Features = Feature;
                    FandT.FeatureErrorId = 0;
                    FandT.FeatureErrorMessage = string.Empty;
                }
                catch (Exception ex)
                {
                    FandT.FeatureErrorId = 1;
                    FandT.FeatureErrorMessage = ex.Message;
                }
                // Tags
                try
                {
                    List<string> tagList = _launchProvider.GetAllTagsByProjectId(ProjectId);
                    IEnumerable<string> Tags = (from x in tagList select x);
                    FandT.Tags = Tags;
                    FandT.TagsErrorId = 0;
                    FandT.TagsErrorMessage = string.Empty;
                }
                catch (Exception ex)
                {
                    FandT.TagsErrorId = 1;
                    FandT.TagsErrorMessage = ex.Message;
                }
                FandT.GeneralErrorId = 0;
                FandT.GeneralErrorMessage = string.Empty;
            }
            catch (Exception ex)
            {
                FandT.GeneralErrorId = 2;
                FandT.GeneralErrorMessage = ex.Message;
            }
            return Json(FandT, JsonRequestBehavior.AllowGet);
        }

        private void CreateRelLogs(string MachineIP, string Message)
        {
            var timeservice = new GlobalReportingSystem.Web.UI.LoadBalancedExecutionService.LoadBalancedExecutionServiceSoapClient();
            RelExecutionStatusLog logs = new RelExecutionStatusLog()
            {
                MachineIP = MachineIP,
                ServerDateTime = timeservice.GetServiceCurrentDateTime(),
                Message = Message
            };
            _launchProvider.CreateRelLogs(logs);
        }

        public void RelReportGroup(string relid = "")
        {
            string responsedata = "";
            if (!string.IsNullOrEmpty(relid))
            {
                //Prepare list of relexecutionstatusid in int64
                relid = relid.Trim();
                string[] relexid = relid.Split(',');
                List<string> RelIDList = new List<string>(relexid);
                List<Int64> reid = new List<Int64>();
                foreach (var item in RelIDList)
                {
                    Int64 rid = 0;
                    bool isconverted = Int64.TryParse(item, out rid);
                    if (isconverted && rid > 0)
                    {
                        reid.Add(rid);
                    }
                }

                List<RelExecutionResponseP2> ListMachineLastStatus = new List<RelExecutionResponseP2>();
                //Get All The Record from DB crossponding to the IDS
                List<RelReturn> RelRelateIDs = new List<RelReturn>();
                bool canemailtrigger = false;
                foreach (var item in reid)
                {
                    RelExecutionStatu statusdata = _relreportProvider.GetRelExecutionStatusById(item);
                    if (statusdata != null)
                    {
                        canemailtrigger = true;
                        ListMachineLastStatus.Add(new RelExecutionResponseP2() { RelExecutionStatusId = statusdata.ID, MachineIP = statusdata.MachineIP, Status = statusdata.CurrentStatus });
                        string testsuitidstring = string.Empty;
                        Int64 newrowiddata = statusdata.NewRowId ?? 0;
                        if ((!string.IsNullOrEmpty(statusdata.TestSuitIds)) && statusdata.TestSuitIds != "0")
                        {
                            testsuitidstring = statusdata.TestSuitIds;
                            testsuitidstring = testsuitidstring.Trim();
                        }
                        else if ((string.IsNullOrEmpty(statusdata.TestSuitIds)) && (statusdata.LastRowId > 0) && (statusdata.NewRowId > 0))
                        {
                            int tsstartid = Convert.ToInt32(statusdata.LastRowId ?? 0);
                            int tsendid = Convert.ToInt32(statusdata.NewRowId ?? 0);
                            var tsids = _relreportProvider.GetTestSuitByExecutionIdWithinStartAndEndLimit(statusdata.ExecutionId, tsstartid, tsendid, statusdata.MachineIP);
                            List<int> finaltestsuitids = (from x in tsids select x.ID).ToList();
                            testsuitidstring = string.Join(",", finaltestsuitids);
                        }
                        else if ((string.IsNullOrEmpty(statusdata.TestSuitIds)) && (statusdata.LastRowId > 0))
                        {
                            int tsid = Convert.ToInt32(statusdata.LastRowId ?? 0);
                            var tsids = _relreportProvider.GetTestSuitByExecutionIdAndAfterTestSuitId(statusdata.ExecutionId, tsid, statusdata.MachineIP);
                            List<int> finaltestsuitids = (from x in tsids select x.ID).ToList();
                            testsuitidstring = string.Join(",", finaltestsuitids);
                        }
                        string[] testsuitarray = testsuitidstring.Split(',');
                        List<string> testsuitliststring = new List<string>(testsuitarray);
                        List<int> testsuitid = new List<int>();
                        foreach (var itemt in testsuitliststring)
                        {
                            int tid = 0;
                            bool isconvertedts = int.TryParse(itemt, out tid);
                            if (isconvertedts && tid > 0)
                            {
                                testsuitid.Add(tid);
                            }
                        }

                        foreach (var itemtsid in testsuitid)
                        {
                            RelRelateIDs.Add(new RelReturn()
                            {
                                ExecutionId = statusdata.ExecutionId,
                                TestSuitId = itemtsid
                            });
                        }
                    }
                }

                //Trigger for email
                if (canemailtrigger)
                {
                    responsedata = _relreportProvider.HTMLForLaunchRELReportEmail(RelRelateIDs, ListMachineLastStatus);
                }
                else
                {
                    responsedata = "<div style='color:red; font-size:14px;'>Result entry from testing framework is not found in GRS. Email not send.</div>";
                }
            }
            else
            {
                responsedata = "<div style='color:red; font-size:14px;'>Invalid input data. Report not found.</div>";
            }

            responsedata = responsedata.Replace("http://10.236.4.153", "https://grs.lstools.int.clarivate.com");

            Response.Write(responsedata);
        }

        #region Setup List For Execution As per Project Group
        //public ActionResult ExecutionGroup_Jobs()
        //{
        //}

        //public ActionResult ExecutionGroup_AddJobs()
        //{ 
        //}

        //public ActionResult ExecutionGroup_EditJobs()
        //{ 
        //}

        //public ActionResult ExecutionGroup_DeleteJobs()
        //{ 
        //}


        #endregion Setup List For Execution As per Project Group

    }
}
