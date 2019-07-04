using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Caching;
using System.Security.Cryptography;
using System.Web;
using System.Web.Mvc;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Constants;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS;
using GlobalReportingSystem.Web.UI.Attributes;
using GlobalReportingSystem.Web.UI.AnalysisService;
using GlobalReportingSystem.Web.UI.Harvest;
using GlobalReportingSystem.Web.UI.SikuliService;
using CustomStatuses = GlobalReportingSystem.Core.Models.GRS.DB;

namespace GlobalReportingSystem.Web.UI.Controllers
{
    public class TestCycleController : Controller
    {
        /*  private IEnumerable<TestSuit> _testSuits;*/

        private readonly IManageTestCycleProvider _testCycleProvider;
        private readonly IManageReportingProvider _reportingProvider;
        private readonly ISessionHelper _sessionHelper;
        private readonly IManageUserProvider _userProvider;

        public TestCycleController(IManageTestCycleProvider testCycleProvider, ISessionHelper sessionHelper, IManageUserProvider userProvider, IManageReportingProvider reportingProvider)
        {
            _reportingProvider = reportingProvider;
            _testCycleProvider = testCycleProvider;
            _sessionHelper = sessionHelper;
            _userProvider = userProvider;
        }

        [Authorize]
        public ActionResult Index(int id)
        {
            if (_testCycleProvider.IsTestCycleExist(id))
            {
                var testCycle = _testCycleProvider.GetTestCycle(id);
                                
                if (!_testCycleProvider.IsUserHasAccess(_sessionHelper.GetSessionDetails(User).User.ID, testCycle.Project.ID))
                {
                    ViewBag.Message = String.Concat("Please, contact administrator to get access to project ", testCycle.Project.DisplayName);
                    return PartialView("Forbidden");
                    //return RedirectToAction("Forbidden", "ErrorHandle", new { projectName = testCycle.Project.DisplayName });
                }
                //if (testCycle == null)
                //{

                //    return HttpNotFound(); error handler need to be added
                //}
                //

                var currentCycle = new HttpCookie("CurrentCycle")
                {
                    Expires = DateTime.Now.AddDays(3),
                    Value = id.ToString()
                };
                Response.Cookies.Add(currentCycle);
                //MemoryCache.Default.Remove("CurrentCycle");
                //CacheItemPolicy policy = new CacheItemPolicy();
                //policy.AbsoluteExpiration = DateTimeOffset.Now.AddHours(12);
                //MemoryCache.Default.Add("CurrentCycle", id, policy);
                /* Session["CurrentCycle"] = id;*/
                _testCycleProvider.SwitchProject(User, testCycle.Project);
                return View(testCycle);
            }
            return null;
            //return HttpNotFound(); error handler need to be added
            
        }
        [HttpPost]
        [Authorize]
        public ActionResult GetTestSuits()
        {
            var cycleId = Convert.ToInt32(Request.Cookies["CurrentCycle"].Value);
            var testCycle = _testCycleProvider.GetTestCycle((int)cycleId);
            var ts = testCycle.TestSuits.GroupBy(p => p.TSName).OrderBy(p => p.Key);
            string folderColor = "green";
            var items = new List<JsTreeItemModel>();
            foreach(var t in ts)
            {
                if (t.All(p => p.TestCases == null || p.TestCases != null && p.TestCases.Count == 0))
                {
                    folderColor = "grey";
                }
                else if (t.Any(p => p.FailedTestCases.HasValue && p.FailedTestCases > 0))
                {
                    folderColor = "red";
                }
                else
                {
                    folderColor = "green";
                }
                items.Add(new JsTreeItemModel
                {
                    id = t.Key,
                    text = t.Key.Length > 39 ? t.Key.Substring(0, 39) + "..." : t.Key,
                    children = true,
                    icon = String.Concat("glyphicon glyphicon-folder-close ", folderColor),
                    li_attr = new HtmlAttribute { title = String.Concat(t.Key, "( Passed: ", t.Sum(p => p.PassedTestCases), ", Failed: ", t.Sum(p => p.FailedTestCases), ")") }
                });
            }
            return Json(items);
        }
        [HttpPost]
        [Authorize]
        public ActionResult GetTestSuitsByKey(int cycleId, string key)
        {
            try
            {
                var ts = _testCycleProvider.GetTestSuitsByKey(key, cycleId);
                var folderColor = "green";
                var items = new List<JsTreeItemModel>();
                foreach (var t in ts)
                {
                    if (t.Fail == 0 && t.Pass == 0)
                    {
                        folderColor = "grey";
                    }
                    else if(t.Fail > 0)
                    {
                        folderColor = "red";
                    }
                    else
                    {
                        folderColor = "green";
                    }
                    items.Add(new JsTreeItemModel
                    {
                        id = String.Concat(key, "/", t.Item),
                        text = t.Item,
                        children = true,
                        icon = String.Concat("glyphicon glyphicon-folder-close ", folderColor),
                        li_attr = new HtmlAttribute { title = String.Concat("Passed: ", t.Pass, ", Failed: ", t.Fail) }
                    });
                }
                return Json(items);
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

        [HttpPost]
        [Authorize]
        public ActionResult GetTestSuitsByKeyAndIp(int cycleId, string ip, string tsname)
        {
            try
            {
                var ts = _testCycleProvider.GetTestSuitsByKeyAndIp(ip, cycleId, tsname);
                var fileColor = "green";
                var items = new List<JsTreeItemModel>();
                foreach (var t in ts)
                {
                    if (t.Fail == 0 && t.Pass == 0)
                    {
                        fileColor = "grey";
                    }
                    else if (t.Fail > 0)
                    {
                        fileColor = "red";
                    }
                    else
                    {
                        fileColor = "green";
                    }
                    items.Add(new JsTreeItemModel
                    {
                        id = String.Concat(t.Id),
                        text = t.Item.Split(';')[0],
                        children = true,
                        icon = String.Concat("glyphicon glyphicon-file ", fileColor),
                        li_attr = new HtmlAttribute { title = String.Concat("Passed: ", t.Pass, ", Failed: ", t.Fail) }
                    });
                }
                return Json(items);
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

        [HttpPost]
        [Authorize]
        public ActionResult GetTestSuitDetails(string id)
        {
            try
            {
                var item = _testCycleProvider.GetTestSuitDetails(id);
                //string ret = "<ul>";
                //var fileColor = "green";
                //int i = 0;
                //foreach (var item in items)
                //{
                //    if (item.Fail == 0 && item.Pass == 0)
                //    {
                //        fileColor = "grey";
                //    }
                //    else if (item.Fail > 0)
                //    {
                //        fileColor = "red";
                //    }
                //    else
                //    {
                //        fileColor = "green";
                //    }
                //    ret += "<li id='" + item.Id + "' class='jstree-closed' data-jstree='{\"icon\":\"" + fileColor + " glyphicon glyphicon-file\"}'>" + item.Item.Split(';')[0] + "</li>";
                //    i++;
                //}
                //ret += "</ul>";
                var ret = new List<JsTreeItemModel>();
                foreach (var t in item.Item.Split(';').Skip(1))
                {

                    ret.Add(new JsTreeItemModel
                    {
                        text = t.Length > 40 ? t.Substring(0,37) + "..." : t,
                        state = new State { disabled = true},
                        children = false,
                        icon = "glyphicon glyphicon-minus tsdetails",
                        li_attr = t.Length > 40 ? new HtmlAttribute{title = t} : null
                    });
                }
                return Json(ret);
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

        [HttpPost]
        [Authorize]
        public JsonResult GetJiraIssue(string id)
        {
            try
            {
                return Json(_reportingProvider.GetJiraIssue(id));
                //return Json(_testCycleProvider.GetTestSuitsByKey(key, Convert.ToInt32(MemoryCache.Default.Get("CurrentCycle")))/*(int)Session["CurrentCycle"]*/);
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

        [HttpGet]
        [Authorize]
        public ActionResult GetJiraHtmlIssue(string jiraid)
        {
            try
            {
                var cont = _reportingProvider.GetJiraIssue(jiraid);
                var ret = @"<strong>Summary</strong>: <p>" + cont.Summary + "</p>" +
                          "<br /><strong>Status</strong>: <p>" + cont.Status + "</p>" +
                          @"<div align='right'><a href='http://jira.bjz.apac.ime.reuters.com/browse/" + jiraid +"' target='_blank'>See in Jira</a></div>";
                return Content(ret);
                //return Json(_testCycleProvider.GetTestSuitsByKey(key, Convert.ToInt32(MemoryCache.Default.Get("CurrentCycle")))/*(int)Session["CurrentCycle"]*/);
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

        //[HttpPost]
        //[Authorize]
        //public ActionResult GetTestSuitsByKey(string key)
        //{
        //    try
        //    {
        //        return Json(_testCycleProvider.GetTestSuitsByKey(key, Convert.ToInt32(Request.Cookies["CurrentCycle"].Value)));
        //        //return Json(_testCycleProvider.GetTestSuitsByKey(key, Convert.ToInt32(MemoryCache.Default.Get("CurrentCycle")))/*(int)Session["CurrentCycle"]*/);
        //    }
        //    catch (Exception ex)
        //    {
        //        return
        //            Json(
        //                new
        //                {
        //                    type = "Error",
        //                    text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message)
        //                });
        //    }

        //}

        //[HttpPost]
        //[Authorize]
        //public ActionResult GetTestSuitsByKeyAndIp(string ip, string tsname)
        //{
        //    return Json(_testCycleProvider.GetTestSuitsByKeyAndIp(ip, Convert.ToInt32(Request.Cookies["CurrentCycle"].Value), tsname));
        //    //return Json(_testCycleProvider.GetTestSuitsByKeyAndIp(ip, Convert.ToInt32(MemoryCache.Default.Get("CurrentCycle"))/*(int)Session["CurrentCycle"]*/, tsname));
        //}

        [HttpPost]
        [Authorize]
        public ActionResult GetTsIssues(int id)
        {
            var testSuite = _testCycleProvider.GetTestSuite(id, null);
            var testCycleId = int.Parse(Request.Cookies["CurrentCycle"].Value);
            var testCycle = _testCycleProvider.GetTestCycle(testCycleId);
            var testCycleDuplicates = testCycle.TestSuits.ToList();
            var listduplicates = new List<CycleDuplicates>();
            foreach (var duplicates in testCycleDuplicates)
            {
                var groupedDups = duplicates.TestCases.GroupBy(p => p.TCName).Where(p => p.Count() >= 2);
                if (groupedDups.Count() >= 1)
                    listduplicates.Add(new CycleDuplicates
                        {
                            TestSuiteName = duplicates.TSName,
                            TestSuiteId = duplicates.ID,
                            CountOfDuplicates = groupedDups.Sum(p => p.Count())
                        });

            }
            var model = new TestSuiteIssuesModel()
                {
                    Id = id,
                    Duplicates = testSuite.TestCases.GroupBy(p => p.TCName).Where(p => p.Count() >= 2).ToList(),
                    CycleDuplicates = listduplicates

                };

            return PartialView(model);
        }

        [HttpPost]
        [Authorize]
        public ActionResult RemoveAllDuplicatesForTestCycle()
        {
            var id = int.Parse(Request.Cookies["CurrentCycle"].Value);

            _testCycleProvider.RemoveAllDuplicatesForTestCycle(id);
            return null;
        }

        [HttpPost]
        [Authorize]
        public ActionResult RemoveAllDuplicates(int id)
        {
            _testCycleProvider.RemoveAllDuplicates(id);
            return null;
        }

        [HttpPost]
        [Authorize]
        public ActionResult RemoveDuplicate(int id, string name)
        {
            _testCycleProvider.RemoveDuplicate(id, name);
            return null;
        }

        [HttpPost]
        [Authorize]
        public ActionResult GetTsLegend(int id)
        {
            try
            {
                return PartialView(_testCycleProvider.GetTsLegend(id, null));
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult GetTestSuite(int id, string filter)
        {
            try
            {
                return PartialView(_testCycleProvider.GetTestSuite(id, filter));
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        [HttpPost]
        [Authorize]
        public string GetTestSuiteName(int id)
        {
            return _testCycleProvider.GetTestSuiteName(id);
        }

        [HttpPost]
        [Authorize]
        public ActionResult GetSubSteps(int id)
        {
            try
            {
                return PartialView(_testCycleProvider.GetSubSteps(id));
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult GetSelectedTestSuitIds(int cycleId, List<string> tsIds, List<string> tsNames, List<string> tsNamesAndIps)
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
                return Json(new { type = "", text = ids });
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult RemoveTestSuits(int cycleId, List<string> tsIds, List<string> tsNames, List<string> tsNamesAndIps)
        {
            //try
            //{
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
                _testCycleProvider.DeleteTestSuites(ids);
                return Json(new { type = "", text = "" });
            //}
            //catch (Exception ex)
            //{
            //    return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            //}
        }


        [HttpPost]
        [Authorize]
        public ActionResult RemoveTestCase(string tcId)
        {
            try
            {
                _testCycleProvider.DeleteTestCase(Convert.ToInt32(tcId));
                return Json(new { type = "", text = "" });
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult GetTestSteps(int id)
        {
            try
            {
                return PartialView(_testCycleProvider.GetTestCases(id));
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult GetTestStepsForQuickView(int id)
        {
            try
            {
                return PartialView("GetTestStepsForQuickView", _testCycleProvider.GetTestCases(id));
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        [HttpPost]
        [Authorize]
        /* [Transaction]*/
        public ActionResult EditSubStep(int id, string state, string analyzedStatus, string defects)
        {
            try
            {
                var userDetails = _userProvider.GetUserProfile(User);
                _testCycleProvider.UpdateSubStep(id, state, analyzedStatus, defects, userDetails.Id);
                
                _testCycleProvider.UpdateTestSetWithUserByStep(id, userDetails.Id);
                return null;
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        [HttpPost]
        [Authorize]
        public PartialViewResult ManualUpload()
        {
            return PartialView("ManualUpload");
        }

        [Authorize]
        public void ManualUploadTestCycle()
        {
            foreach (var filename in Request.Files)
            {
                HttpPostedFileBase httpPostedFileBase = Request.Files[filename.ToString()];
                var path = Server.MapPath(string.Format("~/Temp/{0}", httpPostedFileBase.FileName));
                var inputStream = httpPostedFileBase.InputStream;

                var fileStream = new FileStream(path, FileMode.Create);

                inputStream.CopyTo(fileStream);
                fileStream.Close();
                if (path.Split('.').Last().Trim().ToLower().Equals("zip"))
                {
                    var serviceConnectorClient = new ServiceConnectorSoapClient();
                    serviceConnectorClient.AcceptReport(System.IO.File.ReadAllBytes(path), 0);
                }
                System.IO.File.Delete(path);
            }
        }
        [HttpPost]
        [Authorize]
        public ActionResult StartAutoAnalyze(List<string> suiteIds, int type)
        {
            try
            {
                var userDetails = _userProvider.GetUserProfile(User);

                var tsIdsList = new ArrayOfInt();
                tsIdsList.AddRange(suiteIds.Select(p => Int32.Parse(p)));
                var client = new AnalysisServiceSoapClient();
                client.PerformAutoMigration(tsIdsList, type);

                _testCycleProvider.UpdateTestSetsWithUser(tsIdsList, userDetails.Id);

                return null;
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult DeleteOldDefects(int stepId, string defectId)
        {
            try
            {
                _testCycleProvider.DeleteOldDefects(stepId, defectId);
                return null;
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult MigrateTestSuites(List<int> testSuitesIds, int targetTestCycle)
        {
            try
            {
                _testCycleProvider.MigrateTestSuite(testSuitesIds, targetTestCycle);
                return null;
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult MergeTestSuites(List<int> testSuitesIdsToMerge)
        {
            try
            {
                _testCycleProvider.MergeTestSuite(testSuitesIdsToMerge);
                return null;
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }
        [HttpPost]
        [Authorize]
        public ActionResult UpdateSameSubSteps(int subStepId, bool allSubsteps, bool checkedStep)
        {
            try
            {
                _testCycleProvider.UpdateSubStepsWithSameError(subStepId, allSubsteps, checkedStep);
                return null;
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult CheckIfTesetsBelongsSameSet(List<int> testSuits)
        {
            try
            {
                return Json(new { type = "Message", text = _testCycleProvider.CheckIfTesetsBelongsSameSet(testSuits)});
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }
    }
}
