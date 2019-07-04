using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Web.UI.Attributes;
using GlobalReportingSystem.Web.UI.ExecutorService;
using GlobalReportingSystem.Web.UI.Linear;
using GlobalReportingSystem.Web.UI.LoadBalancedExecutionService;
using Messages = GlobalReportingSystem.Core.Constants.Messages;
using GlobalReportingSystem.Core.Models.GRS;

namespace GlobalReportingSystem.Web.UI.Controllers
{
    public class ExecutionController : Controller
    {
        //
        // GET: /Execution/

        private readonly IManageExecutionProvider _executionProvider;
        private readonly IMangaeRelReportProvider _relreportProvider;
        private readonly IEmailer _emailer;
        private readonly IManageLaunchProvider _launchProvider;

        public ExecutionController(IManageExecutionProvider executionProvider, IMangaeRelReportProvider relreportProvider,IEmailer emailer, IManageLaunchProvider launchProvider)
        {
            _executionProvider = executionProvider;
            _relreportProvider = relreportProvider;
            _emailer = emailer;
            _launchProvider = launchProvider;
        }

        [Authorize]
        public ActionResult Linear()
        {
            //SetLinearTestCycle(0,0);
            return View(_executionProvider.GetLinearExecutions(User));
        }
        //[Authorize]
        //public ActionResult LoadBalanced(int? id)
        //{
        //    if (id.HasValue)
        //        Session["fullscreenid"] = id.Value;
        //    var sessionElement = Session["fullscreenid"];
        //    if (sessionElement != null)
        //        id = (int)Session["fullscreenid"];
        //    return View(_executionProvider.GetLoadBalancedExecutions(User, id));
        //}
        [Authorize]
        public ActionResult AddLoadBalancedSection(int? frameworkId)
        {
            try
            {
                return PartialView("LoadBalancedSection", _executionProvider.AddLoadBalancedExecution(User, frameworkId));
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }
        [Authorize]
        public ActionResult RemoveLbTest(string tests)
        {
            try
            {
                _executionProvider.DeleteLbTests(tests.Split(',').ToList());
                return null;
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }
        [Authorize]
        public ActionResult ResetLbTest(string tests)
        {
            try
            {
                _executionProvider.ResetLbTests(tests.Split(',').ToList());
                return null;
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }
        [Authorize]
        public ActionResult RemoveLoadBalancedSection(int id)
        {
            try
            {
                _executionProvider.DeleteLoadBalancedSection(User, id);
                return null;
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }
        [Authorize]
        public ActionResult AddLoadBalancedMachine(int sectionId)
        {
            try
            {
                return PartialView("LoadBalancedMachine", _executionProvider.AddLoadBalancedMachine(User, sectionId));
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }
        [Authorize]
        public ActionResult RemoveLoadBalancedMachine(int id)
        {
            try
            {
                _executionProvider.DeleteLoadBalancedMachine(User, id);
                return null;
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }
        [Authorize]
        public ActionResult AddLinearElement(int frameworkId)
        {
            _executionProvider.AddTestsExecution(User, frameworkId);
            return RedirectToAction("Linear");
        }
        [Authorize]
        public ActionResult SetLbMachine(int clientId, int machineId)
        {
            try
            {
                _executionProvider.SetLoadBalancedMachine(machineId, clientId);
                return PartialView("LoadBalancedMachine", _executionProvider.GetLoadBalancedMachines(machineId));
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }
        [Authorize]
        public ActionResult SetLbAccount(int accountid, int machineid)
        {
            try
            {
                return PartialView("LoadBalancedMachine", _executionProvider.SetLoadBalancedAccount(accountid, machineid));
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }
        [Authorize]
        public ActionResult SetLbEnvironment(int environmentid, int machineid)
        {
            try
            {
                return PartialView("LoadBalancedMachine",
                    _executionProvider.SetLoadBalancedEnvironment(environmentid, machineid));
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }
        [Authorize]
        public ActionResult SetLbBrowser(string browser, int machineid)
        {
            try
            {
                return PartialView("LoadBalancedMachine",
                    _executionProvider.SetLoadBalancedBrowser(browser, machineid));
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }
        [Authorize]
        public ActionResult SetLbExecutionConfig(int confid, int machineid)
        {
            try
            {
                return PartialView("LoadBalancedMachine",
                    _executionProvider.SetLoadBalancedExecutionConfig(confid, machineid));
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        [Authorize]
        public ActionResult SetLbTargetTc(int cycleid, int machineid)
        {
            try
            {
                return PartialView("LoadBalancedMachine", _executionProvider.SetLoadBalancedTargetTc(cycleid, machineid));
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }
        [Authorize]
        public ActionResult SetLbPriority(string priority, int machineid)
        {
            _executionProvider.SetLoadBalancedPriority(priority, machineid);
            return RedirectToAction("LoadBalanced");
        }
        [Authorize]
        public ActionResult RenderTestsForFramework(int id)
        {
            try
            {
                return PartialView("TestsForFramework", _executionProvider.GetTestsForFramework(id));
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
                return PartialView("ChildrenNodes", _executionProvider.GetChildren(id, parent));
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
        [HttpPost]
        public ActionResult AddLbTests(int sectionId, string tests, string key)
        {
            _executionProvider.AddLoadBalancedTests(sectionId, tests, key);
            return Json("");
        }

        public ActionResult RemoveFullScreenMode()
        {
            Session["fullscreenid"] = null;
            return RedirectToAction("LoadBalanced");
        }
        [Authorize]
        public ActionResult StartLoadBalanced(int id)
        {
            string status, value;
            try
            {
                var executor = new LoadBalancedExecutionServiceSoapClient();
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
        [Authorize]
        public ActionResult StopExecution(int executionId, string type)
        {
            var status = "Message";
            var value = String.Empty;
            var stopped = new List<string>();
            var failed = new List<string>();
            try
            {
                var machines = _executionProvider.StopExecution(executionId, type);
                var executor = new ExecutorSoapClient();
                foreach (var machine in machines)
                {
                    try
                    {
                        executor.SendCommandToMachine(machine, "stop");
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
        public ActionResult RemoveLinearElement(int id)
        {
            _executionProvider.DeleteLinearElement(id);
            return RedirectToAction("Linear");
        }
        [Authorize]
        public ActionResult SetLinearMachine(int clientid, int executionid)
        {
            _executionProvider.SetLinearMachine(clientid, executionid);
            return RedirectToAction("Linear");
        }
        [Authorize]
        public ActionResult SetLinearBrowser(string browser, int executionid)
        {
            _executionProvider.SetLinearBrowser(browser, executionid);
            return RedirectToAction("Linear");
        }
        [Authorize]
        public ActionResult SetLinearExecutionConfiguration(int confid, int executionid)
        {
            _executionProvider.SetLinearExecutionConfiguration(confid, executionid);
            return RedirectToAction("Linear");
        }
        [Authorize]
        public ActionResult SetLinearEnvironment(int environmentid, int executionid)
        {
            _executionProvider.SetLinearEnvironment(environmentid, executionid);
            return RedirectToAction("Linear");
        }
        [Authorize]
        public ActionResult SetLinearTestCycle(int cycleid, int executionid)
        {
            _executionProvider.SetLinearTestCycle(cycleid, executionid);
            return RedirectToAction("Linear");
        }
        [Authorize]
        public ActionResult SetLinearAccount(int accountid, int executionid)
        {
            _executionProvider.SetLinearAccount(accountid, executionid);
            return RedirectToAction("Linear");
        }
        [Authorize]
        public ActionResult AddLiearTests(int executionId, string tests, string key)
        {
            try
            {
                _executionProvider.AddLiearTests(executionId, tests, key);
                return Json("");
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }
        [Authorize]
        public int AddFullLinearExecution(string user, int projId, string tests, int frameworkId, string browser, int? client, int? account,
            int? hostConf, int? execConf, string subscribers)
        {
            if (execConf == 0)
                return _executionProvider.AddFullExecution(user, projId, tests, frameworkId, browser, client, account, hostConf, null, subscribers);
            return _executionProvider.AddFullExecution(user, projId, tests, frameworkId, browser, client, account, hostConf, execConf, subscribers);
        }
        [Authorize]
        public ActionResult RemoveLinearTest(int executionid, string testname)
        {
            _executionProvider.DeleteLinearTest(executionid, testname);
            return RedirectToAction("Linear");
        }
        [Authorize]
        public ActionResult StartLinear(int executionId)
        {
            string status, value;
            try
            {
                var executor = new LinearSoapClient();
                //var executor = new GRSExecutor();
                executor.RunTests(executionId);

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
        public string PingLinear(int executionId)
        {
            try
            {
                var executor = new ExecutorSoapClient();
                return (executor.PingByID(executionId));
            }
            catch (Exception)
            {
                return "null";
            }
        }
        [Authorize]
        public JsonResult GetLoadBalancedMashines(int executionId)
        {
            try
            {
                var mashines = _executionProvider.GetLoadBalancedMashines(executionId);
                return Json(mashines);
            }
            catch (Exception ex)
            {
                return Json("");
            }
        }
        [Authorize]
        public string PingLB(string mashineIp)
        {
            try
            {
                var executor = new ExecutorSoapClient();
                return (executor.PingByIP(mashineIp));
            }
            catch (Exception ex)
            {
                return "null";
            }
        }
        [Authorize]
        public JsonResult GetCategoriesForFramework(int id)
        {
            try
            {
                var categoriesDistinct = _executionProvider.GetCategoriesForFramework(id);
                return Json(categoriesDistinct);
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }
        [Authorize]
        public ActionResult SetCategoriesForMachine(int id, string categories, bool include)
        {
            try
            {
                return PartialView("LoadBalancedMachine", _executionProvider.SetCategoriesForMachine(id, categories, include));
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }
        [Authorize]
        public ActionResult SetPriorityForMachine(int id, string priorities)
        {
            try
            {
                return PartialView("LoadBalancedMachine", _executionProvider.SetPriorityForMachine(id, priorities));
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }

        }
        [Authorize]
        public ActionResult SetPriorityForExecution(int id, string priorities)
        {
            _executionProvider.SetPriorityForExecution(id, priorities);
            return Json("");
        }
        [Authorize]
        public ActionResult SetSubscriberForExecution(int id, string subscribers)
        {
            _executionProvider.SetSubscriberForExecution(id, subscribers);
            return Json("");
        }
        [Authorize]
        public ActionResult SetCategotyForExecution(int id, string categories, bool include)
        {
            try
            {
                _executionProvider.SetCategotyForExecution(id, categories, include);
                return Json("");
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }
        [Authorize]
        public ActionResult GetCategoriesForExecution(int id, bool include)
        {
            try
            {
                var categories = _executionProvider.GetCategoryForExecution(id, include);
                return Json(categories);
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }
        [Authorize]
        public ActionResult GetCategoriesForMachine(int id, bool include)
        {
            try
            {
                var categories = _executionProvider.GetCategoryForMachine(id, include);
                return Json(categories);
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        //public void RunRel(string type, string rel, string vm, string env, string account)
        //{
        //    try
        //    {
        //        if (type == "linear" || type == null)
        //        {
        //            var executionId = _executionProvider.GetLinearIdByRel(rel, vm, env, account);
        //            if (executionId != 0)
        //            {
        //                var executor = new LinearSoapClient();
        //                executor.RunTests(executionId);
        //                string message = null;
        //                try
        //                {
        //                    while (message == null)
        //                    {
        //                        message = executor.WaitForRelResults(executionId);
        //                        if (message != null)
        //                        {
        //                            _executionProvider.SubmitResponse(message, false);
        //                        }
        //                        else
        //                        {
        //                            Thread.Sleep(30000);
        //                        }
        //                    }
        //                }
        //                catch (Exception)
        //                {
        //                    _executionProvider.SubmitResponse(string.Empty, true);
        //                }
        //            }
        //        }
        //        else if (type == "lb")
        //        {
        //            var executionId = _executionProvider.GetLoadBalanceIdByRel(rel);
        //            if (executionId != 0)
        //            {
        //                var executor = new LoadBalancedExecutionServiceSoapClient();
        //                //GRSExecutor.LoadBalancedExecutionService();
        //                executor.StartLoadBalanced(executionId);
        //                //var message = 
        //                executor.StartREL(executionId);
        //            }
        //        }
        //        //return Json("");
        //    }
        //    catch (Exception ex)
        //    {
        //        _executionProvider.SubmitResponse(ex.Message, true);
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
        //            var executionId = _executionProvider.GetLinearIdByRel(rel, vm, env, account);
        //            int testsuitId = 0;
        //
        //            var initialtotaltestsuitcount = _relreportProvider.TotalTestSuitCount(rel, vm);
        //            var finaltcount = initialtotaltestsuitcount;
        //            if (executionId != 0)
        //            {
        //                var executor = new LinearSoapClient();
        //                StartDateTime = DateTime.Now;
        //                executor.RunTests(executionId);
        //                string message = null;
        //                try
        //                {
        //                    //while (message == null)
        //                    finaltcount = _relreportProvider.TotalTestSuitCount(rel, vm);
        //                    while (finaltcount <= initialtotaltestsuitcount)
        //                    {
        //                        message = executor.WaitForRelResults(executionId);
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
        //                var executor = new LoadBalancedExecutionServiceSoapClient();
        //                //GRSExecutor.LoadBalancedExecutionService();
        //                executor.StartLoadBalanced(executionId);
        //                //var message = 
        //                executor.StartREL(executionId);
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

        public JsonResult RunRel(string type, string rel, string vm, string env, string account)
        {
            try
            {
                DateTime StartDateTime = DateTime.Now;
                DateTime EndDateTime = DateTime.Now;
                if (type == "linear" || type == null)
                {
                    var executionId = _executionProvider.GetLinearIdByRel(rel, vm, env, account);
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
                        var executor = new LinearSoapClient();
                        StartDateTime = DateTime.Now;
                        executor.RunTests(executionId);
                        EndDateTime = DateTime.Now;
                        Thread.Sleep(30000);
                        List<TestSuit> FinalAllTestsuit = _relreportProvider.GetTestSuitOnRelAndVM(rel, vm);

                        int count = FinalAllTestsuit.Count;
                        while (count == initialtotaltestsuitcount)
                        {
                            FinalAllTestsuit = _relreportProvider.GetTestSuitOnRelAndVM(rel, vm);
                            count = FinalAllTestsuit.Count;
                        }

                        if (FinalAllTestsuit.Count > initialtotaltestsuitcount)
                        {
                            Thread.Sleep(15000);
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

                            return Json(NewTestsuit, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            List<RelReturn> NewTestsuit = new List<RelReturn>();
                            NewTestsuit.Add(new RelReturn()
                            {
                                ExecutionId = executionId,
                                TestSuitId = 0,
                                Message = vm + "-101"
                            });
                            return Json(NewTestsuit, JsonRequestBehavior.AllowGet);
                        }
                    }
                    else
                    {
                        List<RelReturn> NewTestsuit = new List<RelReturn>();
                        NewTestsuit.Add(new RelReturn()
                        {
                            ExecutionId = executionId,
                            TestSuitId = 0,
                            Message = vm + "-102"
                        });
                        return Json(NewTestsuit, JsonRequestBehavior.AllowGet);
                    }
                }
                else if (type == "lb")
                {
                    var executionId = _executionProvider.GetLoadBalanceIdByRel(rel);
                    if (executionId != 0)
                    {
                        var executor = new LoadBalancedExecutionServiceSoapClient();
                        executor.StartLoadBalanced(executionId);
                        executor.StartREL(executionId);
                    }
                    List<RelReturn> NewTestsuit = new List<RelReturn>();
                    NewTestsuit.Add(new RelReturn()
                    {
                        ExecutionId = executionId,
                        TestSuitId = 0,
                        Message = vm + "-103"
                    });
                    return Json(NewTestsuit, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    List<RelReturn> NewTestsuit = new List<RelReturn>();
                    NewTestsuit.Add(new RelReturn()
                    {
                        ExecutionId = 0,
                        TestSuitId = 0,
                        Message = vm + "-102"
                    });
                    return Json(NewTestsuit, JsonRequestBehavior.AllowGet);
                }
            }
            catch (Exception ex)
            {
                //_executionProvider.SubmitResponse(ex.Message, true);
                if (ex.Message.ToString().ToUpper().Contains("SYSTEM.WEB.SERVICES.PROTOCOLS.SOAPEXCEPTION: SERVER WAS UNABLE TO PROCESS REQUEST."))
                {
                    List<RelReturn> NewTestsuit = new List<RelReturn>();
                    NewTestsuit.Add(new RelReturn()
                    {
                        ExecutionId = 0,
                        TestSuitId = 0,
                        Message = vm + "-105"
                    });
                    return Json(NewTestsuit, JsonRequestBehavior.AllowGet);
                }
                else
                {
                    List<RelReturn> NewTestsuit = new List<RelReturn>();
                    NewTestsuit.Add(new RelReturn()
                    {
                        ExecutionId = 0,
                        TestSuitId = 0,
                        Message = vm + "-104"
                    });
                    return Json(NewTestsuit, JsonRequestBehavior.AllowGet);
                }
            }
        }


        [Authorize]
        public JsonResult RunWithWaiter(string type, int executionId)
        {
            string status = "Message", value = "";
            try
            {
                if (type == "linear" || type == null)
                {
                    if (executionId != 0)
                    {
                        var executor = new LinearSoapClient();
                        executor.RunTests(executionId);
                        string message = null;
                        try
                        {
                            while (message == null)
                            {
                                message = executor.WaitForRelResults(executionId);
                                if (message != null)
                                {
                                    XmlDocument xmlDoc = new XmlDocument();
                                    xmlDoc.LoadXml(message);

                                    string xpath = "//TestSuite";
                                    var urls = xmlDoc.SelectNodes(xpath);

                                    if (urls != null)
                                        foreach (XmlNode url in urls)
                                        {
                                            var urlVal = url.SelectSingleNode("//QuickView");
                                            if (urlVal != null)
                                                value += "<a target='_blank' href='" + urlVal.InnerText + "'>" + urlVal.InnerText + "</a><br/>";
                                        }
                                }
                                else
                                {
                                    Thread.Sleep(30000);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            status = "Error";
                            value = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                            value = value.Length > 461 ? (value.Substring(0, 458) + "...") : (value);
                            return Json(new { type = status, text = value });
                        }
                    }
                }
                else if (type == "lb")
                {
                    if (executionId != 0)
                    {
                        var executor = new LoadBalancedExecutionServiceSoapClient();
                        //GRSExecutor.LoadBalancedExecutionService();
                        executor.StartLoadBalanced(executionId);
                        //var message = 
                        executor.StartREL(executionId);
                    }
                }
                return Json(new { type = status, text = value });
            }
            catch (Exception ex)
            {
                status = "Error";
                value = ex.InnerException == null ? ex.Message : ex.InnerException.Message;
                value = value.Length > 461 ? (value.Substring(0, 458) + "...") : (value);
                return Json(new { type = status, text = value });
            }
        }
        private static void LogMessage(string message)
        {
            using (var writer = System.IO.File.AppendText(@"D:\GRS\log.txt"))
            {
                writer.WriteLine(message + " - " + DateTime.Now);
            }
        }



        /*---------------------------------------------------------------------------------------------------------*/

        /*--------------------RUN REL BREAKED---------------------------------*/

        /*Step 1: Check client machine health (whether machine is ready or busy or null)*/
        public string RunRelIPHealthCheck(string type, string rel, string vm, string env, string account, string ToAddress)
        {
            string Status = "Machine is not ready.";
            bool cansendemail = false;
            try
            {

                if (type == "linear" || type == null)
                {
                    int executionId = _executionProvider.GetLinearIdByRel(rel, vm, env, account);
                    if (executionId != 0)
                    {
                        var executor = new ExecutorSoapClient();
                        string clientIpHealth = (executor.PingByID(executionId));
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
            return Status;
        }

        /*Step 2: Calculate Total Testsuit count against that execution id against machine IP, Last TestsuitId,
        *         Initiate anonymous execution on client machine, wait for entry from GAgent side, check entry in DB,
        *         Initiate anonymous for continuous check in DB for execution completion and respond back with Error Id, Message, RelExecutionStatusId*/
        public JsonResult RunRelInitiateTesting(string type, string rel, string vm, string env, string account, string ToAddress)
        {
            try
            {
                var timeservice = new GlobalReportingSystem.Web.UI.LoadBalancedExecutionService.LoadBalancedExecutionServiceSoapClient();
                CreateRelLogs(vm, "Command for rel execution recived.");
                DateTime StartDateTime = timeservice.GetServiceCurrentDateTime();
                if (type == "linear" || type == null)
                {
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
                        var executor = new LinearSoapClient();
                        StartDateTime = DateTime.Now;
                        var lastRExStData = _relreportProvider.GetLastIdAndCountOfRelExecutionStatus(executionId, vm);
                        Thread anonymousServiceCall = new Thread(delegate()
                        {
                            try
                            {
                                executor.RunTests(executionId);
                            }
                            catch (Exception ex)
                            {
                                string[] toaddressarray = ToAddress.Split(',');
                                List<string> ToAddressList = new List<string>(toaddressarray);
                                string messagebody = "<table style='border-collapse: collapse; width: auto!important;'><tr><td align='left' colspan='2' style='font-weight: 500; font-size: 14px!important; font-family: verdana; padding: 10px 10px 10px 10px!important;'><br />Hi,<br /><br />Client machine {IPADDRESS} is not responding back to GRS. <br /><br />Possible causes of error...<br /><ul><li>Client machine is busy with other execution.</li><li>Client machine is shutdown.</li><li>G-Agent is not running on client machine.</li><li>Client machine is not connected with internet.</li><li>Client machine is not part of intranet.</li></ul><br />Please check this machine.<br /><br /><br />GRS Admin</td></tr><tr><td align='left' valign='middle' style='color: #DD5A43; font-weight: 500; font-size: 22px!important; width: 40px!important; padding: 0px 0px 0px 10px!important;'><img src='https://grs.lstools.int.clarivate.com/nggrs/Content/GRS.png' style='height: 32px; width: 32px;' /></td><td align='left' valign='middle' style='vertical-align: middle; color: #DD5A43; font-weight: 600; font-size: 22px!important; padding: 0px!important;'>GRS</td></tr><tr><td colspan='2' align='left' style='font-weight: 600; padding-top: 2px!important; padding-bottom: 0px!important;'>&copy;&nbsp;&nbsp;Clarivate Analytics {CURRENTYEAR}</td></tr></table>";
                                messagebody = messagebody.Replace("{IPADDRESS}", vm);
                                messagebody = messagebody.Replace("{CURRENTYEAR}", Convert.ToString(DateTime.Now.Year));
                                _emailer.SendMail(ToAddressList, "grs@clarivate.com", "Client Machine " + vm + " Not Responding", messagebody);
                            }
                        });
                        anonymousServiceCall.Start();

                        CreateRelLogs(vm, "Linear execution initiated");
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
                                CreateRelLogs(vm, "Tracking row generated by GAgent. (checked at max row count)");
                                break;
                            }
                            else if (LastRelExecutionID > lastRExStData.LastRelExecutionId)
                            {
                                CreateRelLogs(vm, "Tracking row generated by GAgent. (checked at max row ID)");
                                break;
                            }
                            DateTime serverCurrentTime = timeservice.GetServiceCurrentDateTime();
                            if ((serverCurrentTime - Localdateitme).TotalMinutes > 6)
                            {
                                CreateRelLogs(vm, "GAgent failed to generate tracking row, Timeout occured.");
                                IsTimeout = true; break;
                            }
                            Thread.Sleep(30000);
                        }

                        if (IsTimeout)
                        {
                            Int64 relexecutionid = timeservice.CreateRelExecutionStatus(executionId, vm, false, "TIMEOUT", "GAgent did not accept the command.");
                            timeservice.UpdateLatestRelExecutionStatusEntryLevelRecord(LastRelExecutionID, initialtotaltestsuitcount, lastTestsuitID, ToAddress);
                            RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = relexecutionid, MachineIP = vm, Status = "TIMEOUT" };
                            var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                            jsonResponseResult.MaxJsonLength = int.MaxValue;
                            return jsonResponseResult;
                        }
                        else
                        {
                            timeservice.UpdateLatestRelExecutionStatusEntryLevelRecord(LastRelExecutionID, initialtotaltestsuitcount, lastTestsuitID, ToAddress);
                            var statusdata = _relreportProvider.GetRelExecutionStatusById(LastRelExecutionID);
                            RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = statusdata.ID, MachineIP = statusdata.MachineIP, Status = (!string.IsNullOrEmpty(statusdata.CurrentStatus)) ? statusdata.CurrentStatus.ToUpper() : "NULL" };
                            var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                            jsonResponseResult.MaxJsonLength = int.MaxValue;
                            return jsonResponseResult;
                        }
                    }
                    else
                    {
                        CreateRelLogs(vm, "Execution ID not found. Invalid REL LINK");
                        RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = 0, MachineIP = vm, Status = "INVALID REL LINK" };
                        var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                        jsonResponseResult.MaxJsonLength = int.MaxValue;
                        return jsonResponseResult;
                    }
                }
                else if (type == "lb")
                {
                    CreateRelLogs(vm, "Load Balanced command received.");
                    var executionId = _executionProvider.GetLoadBalanceIdByRel(rel);
                    if (executionId != 0)
                    {
                        var executor = new LoadBalancedExecutionServiceSoapClient();
                        executor.StartLoadBalanced(executionId);
                        executor.StartREL(executionId);
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
                CreateRelLogs(vm, "Exception Occured. Exception Message => " + ex.Message);
                if (ex.Message.ToString().ToUpper().Contains("SYSTEM.WEB.SERVICES.PROTOCOLS.SOAPEXCEPTION: SERVER WAS UNABLE TO PROCESS REQUEST."))
                {
                    CreateRelLogs(vm, "Exception Occured. GAgent did not accept request. Preparing to return.");
                    RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = 0, MachineIP = vm, Status = "INVALID GAGENT" };
                    var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                    jsonResponseResult.MaxJsonLength = int.MaxValue;
                    return jsonResponseResult;
                }
                else
                {
                    CreateRelLogs(vm, "Exception Occured. Preparing to return. Exception => " + ex.Message);
                    RelExecutionResponseP2 response = new RelExecutionResponseP2() { RelExecutionStatusId = 0, MachineIP = vm, Status = "INVALID" };
                    var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                    jsonResponseResult.MaxJsonLength = int.MaxValue;
                    return jsonResponseResult;
                }
            }
        }

        /*Step 4: Get the client machine execution status by client machine IP*/
        public JsonResult GetLatestStatusOfRelExecutionByIP(string MachineIP)
        {
            var timeservice = new GlobalReportingSystem.Web.UI.LoadBalancedExecutionService.LoadBalancedExecutionServiceSoapClient();
            var response = Json(_relreportProvider.GetLatestRelStatusByIP(MachineIP, timeservice.GetServiceCurrentDateTime()), JsonRequestBehavior.AllowGet);
            response.MaxJsonLength = int.MaxValue;
            return response;
        }

        /*Step 4: Get the client machine execution status by client machine ID */
        public JsonResult GetLatestStatusOfRelExecutionByID(Int64 RelExecutionStatusId)
        {
            var timeservice = new GlobalReportingSystem.Web.UI.LoadBalancedExecutionService.LoadBalancedExecutionServiceSoapClient();
            var response = Json(_relreportProvider.GetLatestRelStatusByID(RelExecutionStatusId, timeservice.GetServiceCurrentDateTime()), JsonRequestBehavior.AllowGet);
            response.MaxJsonLength = int.MaxValue;
            return response;
        }

        /*Step 5: Generate report for Run Rel execution. It accepts RelExecutionStatusIds as comma seperated string and email address as comma seperated string*/
        public JsonResult TriggerEmailForReport(string RelExecutionStatusIds, string ToAddress)
        {
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
                        string ToListString = ToAddress.Trim();
                        string[] ToListArray = ToListString.Split(',');
                        List<string> ToList = new List<string>(ToListArray);
                        string messagebody = _relreportProvider.HTMLForLaunchRELReportEmail(RelRelateIDs, ListMachineLastStatus);
                        _emailer.SendMail(ToList, "grs@clarivate.com", "Automated Test Execution Report", messagebody);
                        return Json("Email Send.", JsonRequestBehavior.AllowGet);
                    }
                    else
                    {
                        return Json("Result entry from testing framework is not found in GRS. Email not send.", JsonRequestBehavior.AllowGet);
                    }
                }
                else
                {
                    return Json("Invalid input data - RelExecutionStatusIds. Email not send.", JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                return Json("Invlid input data - ToAddress. Email not send.", JsonRequestBehavior.AllowGet);
            }
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

        /*---------------------------------------------------------------------------------------------------------*/




    }
}
