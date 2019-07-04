using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using System.Threading;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Constants;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS;
using GlobalReportingSystem.Web.UI.Attributes;
using GlobalReportingSystem.Web.UI.Harvest;
using GlobalReportingSystem.Web.UI.SikuliService;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
using CustomStatuses = GlobalReportingSystem.Core.Models.GRS.DB;

namespace GlobalReportingSystem.Web.UI.Controllers
{
    public class QcExportController : Controller
    {
        private readonly IManageQcProvider _qcClientProvider;
        private readonly IManageTestCycleProvider _testCycleProvider;
        private readonly ISessionHelper _sessionHelper;

        public QcExportController(IManageQcProvider qcClientProvider, IManageTestCycleProvider testCycleProvider, ISessionHelper sessionHelper)
        {
            _qcClientProvider = qcClientProvider;
            _sessionHelper = sessionHelper;
            _testCycleProvider = testCycleProvider;
        }

        [HttpPost]
        [Authorize]
        public ActionResult QcExport(int cycleId, List<string> tsIds, List<string> tsNames, List<string> tsNamesAndIps)
        {
            try
            {
                var ids = new List<string>();
                if (tsIds != null)
                {
                    ids.AddRange(tsIds);
                }
                if (tsNames != null)
                {
                    ids.AddRange(_testCycleProvider.GetTestSuitesIdsByNames(cycleId, tsNames));
                }
                if (tsNamesAndIps != null)
                {
                    ids.AddRange(_testCycleProvider.GetTestSuitesIdsByIP(cycleId, tsNamesAndIps));
                }
                var testSuitesIds = ids.Select(p => Int32.Parse(p)).ToList();
                //_testCycleProvider.GetSelectedTestSuitIds(Request.Cookies["CurrentCycle"].Value,
                //    Convert.ToBoolean(selectAll), testSuitesIds).ForEach(p => tsIds.Add(Int32.Parse(p)));
                _qcClientProvider.ExportTestResult(User, testSuitesIds);
                return Json(new { type = "Message", text = "Export has been ended." });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    type = "Error",
                    text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message)
                });
            }
        }

    }
}
