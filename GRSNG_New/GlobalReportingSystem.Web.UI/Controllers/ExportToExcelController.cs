using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using System.IO;

namespace GlobalReportingSystem.Web.UI.Controllers
{
    public class ExportToExcelController : Controller
    {
        //
        // GET: /ExportToExcel/
        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        private readonly IManageExportToExcel _exportToExcelClientProvider;
        private readonly IManageTestCycleProvider _testCycleProvider;
        private readonly ISessionHelper _sessionHelper;

        public ExportToExcelController(IManageExportToExcel exportToExcelClientProvider, ISessionHelper sessionHelper, IManageTestCycleProvider testCycleProvider)
        {
            _exportToExcelClientProvider = exportToExcelClientProvider;
            _sessionHelper = sessionHelper;
            _testCycleProvider = testCycleProvider;
        }

        [Authorize]
        [HttpPost]
        public ActionResult ExportTestSets(int cycleId, List<string> tsIds, List<string> tsNames, List<string> tsNamesAndIps)
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
                var testSuitesIds = ids.Select(p => Int32.Parse(p)).ToArray();
                //_testCycleProvider.GetSelectedTestSuitIds(Request.Cookies["CurrentCycle"].Value,
                //    Convert.ToBoolean(selectAll), ids).ForEach(p => tsIds.Add(Int32.Parse(p)));

                var responseFile = _exportToExcelClientProvider.ExportTestSetsToExcel(User, testSuitesIds);
                if (responseFile != null)
                {
                    var url = "http://" + (HttpContext.Request.Url.Host) + ":" + ((HttpContext.Request.Url.Port) + (HttpContext.Request.ApplicationPath) + "/Temp/" + responseFile.Split('\\').Last());
                    return Json(new
                    {
                        type = "Text",
                        text = url
                    });
                    //try
                    //{
                    //    byte[] fileData = System.IO.File.ReadAllBytes(responseFile);
                    //    return File(fileData, "application/vnd.ms-excel", responseFile.Split('\\').Last());
                    //}
                    //finally
                    //{
                    //    System.IO.File.Delete(responseFile);
                    //}
                }
                throw new Exception("Report is empty. Please check if there is any data in test cycle");
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
