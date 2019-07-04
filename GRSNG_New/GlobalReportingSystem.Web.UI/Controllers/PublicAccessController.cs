using System;
using System.Collections.Generic;
using System.DirectoryServices.AccountManagement;
using System.Drawing;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Models.GRS;
using GlobalReportingSystem.Web.UI.Linear;

namespace GlobalReportingSystem.Web.UI.Controllers
{
    public class PublicAccessController : Controller
    {
        //
        // GET: /PublicAccess/

        private readonly IManageTestCycleProvider _testCycleProvider;

        public ActionResult TestsRun()
        {
            return View();
        }

        public PublicAccessController(IManageTestCycleProvider testCycleProvider)
        {
            _testCycleProvider = testCycleProvider;
        }

        public ActionResult Report(int testSuite, int? testSuite2, string filter)
        {
            var t1 = _testCycleProvider.GetTestSuiteAndLegend(testSuite, filter);
            if (testSuite2 == null)
                return View(new List<TestCasesAndStepsModel> { t1 });
            return View(new List<TestCasesAndStepsModel> { t1, _testCycleProvider.GetTestSuiteAndLegend(testSuite2 ?? default(int), filter) });
        }

        public ActionResult PartialReport(int testSuite, string filter)
        {
            return PartialView("TestResultsPartial", _testCycleProvider.GetTestSuiteAndLegend(testSuite, filter));
        }

        public ActionResult ReturnHistory(string user)
        {
            return PartialView("HistoryDisplayForm", _testCycleProvider.GetExecutionHistory(user));
        }

        public ActionResult ReturnExecutionsHistory(string user)
        {
            return PartialView("ExecutionsPartial", _testCycleProvider.GetExecutionHistory(user));
        }

        public ActionResult ReturnProjects()
        {
            return PartialView("Step1Partial", _testCycleProvider.GetPublicAcessProjects());
        }

        public JsonResult CheckTenAuthentication(string user, string password)
        {
            using (var pc = new PrincipalContext(ContextType.Domain, "ten.thomsonreuters.com", user, password))
            {
                // validate the credentials
                if (!pc.ValidateCredentials(user, password))
                    return Json(new { type = "Error", text = "Invalid credentials" });
            }
            return Json("");
        }

        public ActionResult ReturnMachines(int projectId)
        {
            var machines = _testCycleProvider.GetAppropriateMachines(projectId);
            return PartialView("Step2Partial", machines);
        }

        public int ReturnTestFramework(int projectId)
        {
            return _testCycleProvider.GetAppropriateTests(projectId);
        }

        public ActionResult ReturnBrowsers()
        {
            return PartialView("Step4Partial");
        }

        //public ActionResult ReturnEnvironments(int projectId)
        //{
        //    var envs = _testCycleProvider.GetAppropriateEnvironments(projectId);
        //    return PartialView("Step5Partial", envs);
        //}

        //public ActionResult ReturnAccounts(int projectId)
        //{
        //    var accounts = _testCycleProvider.GetAppropriateAccounts(projectId);
        //    return PartialView("Step6Partial", accounts);
        //}

        public ActionResult ReturnConfiguration(int projectId)
        {
            var conf = _testCycleProvider.GetFullConfigurationModel(projectId);
            return PartialView("Step4Partial", conf);

            //var conf = _testCycleProvider.GetAppropriateConfigurations(projectId);
            //return PartialView("Step7Partial", conf);
        }

        public ActionResult ReturnSubscribers(int projectId)
        {
            var users = _testCycleProvider.GetAppropriateUsers(projectId);
            return PartialView("Step8Partial", users);
        }

        public ActionResult ReturnExecution(int executionId)
        {
            var exec = _testCycleProvider.GetAppropriateExecution(executionId);
            return PartialView("Step9Partial", exec);
        }

        public JsonResult GetExecutionData(int executionId)
        {
            var exec = _testCycleProvider.GetExecutionData(executionId);
            return Json(new
            {
                projId = exec[0],
                tests = exec[1],
                subscribers = exec[2],
                machine = exec[3],
                browser = exec[4],
                account = exec[5],
                hostConf = exec[6],
                execConf = exec[7],
                frame = exec[8]
            });
        }

        public JsonResult ReRunTestCase(int tcId, string tcName)
        {
            try
            {
                var client = new LinearSoapClient();
                client.ReRunTestAsync(tcId);
                return Json(new { type = "Message", text = "Test case '" + tcName + "\' was restarted"});
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        public ActionResult GetTestSuiteForQuickView(int id, string filter)
        {
            try
            {
                return PartialView("GetTestSuite", _testCycleProvider.GetTestSuite(id, filter));
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

    }
}
