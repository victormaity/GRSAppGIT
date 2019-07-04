using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS;
using GlobalReportingSystem.Web.UI.Attributes;
using Counts = GlobalReportingSystem.Core.Models.GRS.DB.Counts;
using CustomStatuses = GlobalReportingSystem.Core.Models.GRS.DB;

namespace GlobalReportingSystem.Web.UI.Controllers
{
    public class ReportingController : Controller
    {

        private readonly IManageReportingProvider _reportingProvider;
        private readonly IManageTestCycleProvider _testCycleProvider;

        public ReportingController(IManageReportingProvider reportingProvider, IManageTestCycleProvider testCycleProvider)
        {
            _reportingProvider = reportingProvider;
            _testCycleProvider = testCycleProvider;
        }
        //
        // GET: /Reporting/
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult CumulativeMain()
        {
            try
            {
                return View(_reportingProvider.GetCumulativeStatistics(User));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View(new List<CyclesModel>());
            }
        }
        [Authorize]
        public ActionResult AnalysisMain()
        {
            try
            {
                return View(_reportingProvider.GetAnalysisStatistics(User));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View(new List<UsersAndCyclesModel>());
            }
        }

        [Authorize]
        public ActionResult AnalysisTableByCycle(int cycleId)
        {
            try
            {
                return PartialView(_reportingProvider.getAnalysisDataByCycle(User, cycleId));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return PartialView(new List<TestCase>());
            }
        }

        [Authorize]
        public ActionResult AnalysisTableByUser(int userId, DateTime startDate, DateTime? endDate)
        {
            try
            {
                if (endDate.HasValue && DateTime.Compare(startDate, (DateTime)endDate) >= 0)
                {
                    var tmp = startDate;
                    startDate = (DateTime)endDate;
                    endDate = tmp;
                }
                return PartialView(_reportingProvider.getAnalysisDataByUser(userId, startDate, endDate));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return PartialView(new List<TestCase>());
            }
        }

        [Authorize]
        public ActionResult CumulativeTable(int cycleId)
        {
            try
            {
                return PartialView(_reportingProvider.AddReport(User, cycleId));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return PartialView(new List<ReportCommulativeModel>());
            }
        }

        [HttpPost]
        [Authorize]
        public void AddTestSuiteComment(string name, int pk, string value)
        {
            _testCycleProvider.AddCommentToTestSuite(pk, value);
        }

        [Authorize]
        public ActionResult BugListMain()
        {
            try
            {
                return View(_reportingProvider.GetCumulativeStatistics(User));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View(new List<CyclesModel>());
            }
        }

        [Authorize]
        public ActionResult BugListTable(int cycleId)
        {
            try
            {
                return PartialView(_reportingProvider.GetAllBugList(cycleId));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return PartialView(new List<BugListModel>());
            }
        }

        [Authorize]
        public ActionResult BugStatisticsMain()
        {
            try
            {
                return View(_reportingProvider.GetCumulativeStatistics(User));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View(new List<CyclesModel>());
            }
        }

        [Authorize]
        public ActionResult BugListTestCasesDetailsTable(string bugId, int cycleId)
        {
            try
            {
                return PartialView(_reportingProvider.GetFailedTestCases(bugId, cycleId));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return PartialView(new List<BugListModel>());
            }
        }

        [Authorize]
        public ActionResult QuickSummaryMain()
        {
            try
            {
                return View(_reportingProvider.GetCumulativeStatistics(User));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return View(new List<CyclesModel>());
            }
        }

        [Authorize]
        public ActionResult QuickSummaryTable(int cycleId)
        {
            try
            {
                return PartialView(_reportingProvider.GetTestSuits(User, cycleId));
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
                return PartialView(new List<TestSuit>());
            }
        }
    }
}
