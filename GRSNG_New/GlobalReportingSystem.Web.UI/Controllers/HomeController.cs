using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Web.UI.Attributes;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
using GlobalReportingSystem.Core.Models.GRS;

namespace GlobalReportingSystem.Web.UI.Controllers
{
    public class HomeController : Controller
    {
        private readonly IManageHomeProvider _homeProvider;
        private readonly ISessionHelper _sessionHelper;

        public HomeController(IManageHomeProvider homeProvider, ISessionHelper sessionHelper)
        {
            _homeProvider = homeProvider;
            _sessionHelper = sessionHelper;
        }

        [Authorize]
        [DbAuthorize]
        public ActionResult Index(int deliveryItems = 10)
        {
            return View(_homeProvider.GetHomePageStats(User, deliveryItems));
        }
        
        #region Execution Report

        [Authorize]
        [HttpGet]
        public ActionResult ExecutionReport(string duration = "5DAYS", string release = "ALL", string team = "ALL", string startdate = "", string enddate = "", string isBytestCycle = "false", int cycleid = 0)
        {
            return View(_homeProvider.GetExecutionReport(User, duration, release, team, startdate, enddate, cycleid));
        }
        
        [Authorize]
        [HttpPost]
        public JsonResult GetCountAndVMInfo(string suitIds)
        {
            var model = _homeProvider.GetSuitTestCaseExecutionCountAndClientInfo(suitIds);
            var response = Json(model, JsonRequestBehavior.AllowGet);
            response.MaxJsonLength = int.MaxValue;
            return response;
        }

        [Authorize]
        [HttpPost]
        public JsonResult GetSuitWithCasesInfo(string suitIds)
        {
            var model = _homeProvider.GetTestSuitWithTestCasesDetails(suitIds);
            var response = Json(model, JsonRequestBehavior.AllowGet);
            response.MaxJsonLength = int.MaxValue;
            return response;
        }

        #endregion Execution Report

    }
}
