using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web;
using System.Web.Mvc;
using System.Xml;
using Castle.Core.Internal;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Constants;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS;
using GlobalReportingSystem.Web.UI.com.reuters.ime.apac.bjz.jira;
using GlobalReportingSystem.Core.Models.GRS.DB;
using GlobalReportingSystem.Web.UI.Attributes;
using GlobalReportingSystem.Web.UI.SikuliService;
using Messages = GlobalReportingSystem.Core.Constants.Messages;

namespace GlobalReportingSystem.Web.UI.Controllers
{
    public class ConfigurationController : Controller
    {
        private readonly IManageConfigurationProvider _configurationProvider;

        private readonly ISessionHelper _sessionHelper;

        public ConfigurationController(IManageConfigurationProvider configurationProvider, ISessionHelper sessionHelper)
        {
            _configurationProvider = configurationProvider;
            _sessionHelper = sessionHelper;
        }
        //
        // GET: /Configuration/

        [Authorize]
        public ActionResult Index()
        {
            return View();
        }

        [Authorize]
        public ActionResult TestCycles(string testcycletype = "")
        {
            var releaseInfos = _configurationProvider.GetReleases(User);
            var releaseinfodata = releaseInfos.ReleaseFromDb;
            var releaseFDB = (from x in releaseinfodata
                              select new DropdownList
                              {
                                  Text = x.ReleaseName,
                                  Value = x.ID
                              }).ToList();

            var teamInfos = _configurationProvider.GetTeams(User);
            var teaminfodata = teamInfos.TeamFromDb;
            var teamFDB = (from x in teaminfodata
                           select new DropdownList
                           {
                               Text = x.TeamName,
                               Value = x.ID
                           }).ToList();

            string cycledatafilter = "THISMONTH";
            if (!string.IsNullOrEmpty(testcycletype))
            {
                if (testcycletype.ToUpper() == "ALL") { cycledatafilter = "ALL"; }
                else if (testcycletype.ToUpper() == "ALLACTIVE") { cycledatafilter = "ALLACTIVE"; }
                else if (testcycletype.ToUpper() == "ALLINACTIVE") { cycledatafilter = "ALLINACTIVE"; }
                else if (testcycletype.ToUpper() == "THISYEAR") { cycledatafilter = "THISYEAR"; }
                else if (testcycletype.ToUpper() == "LASTONEYEAR") { cycledatafilter = "LASTONEYEAR"; }
                else if (testcycletype.ToUpper() == "LASTSIXMONTH") { cycledatafilter = "LASTSIXMONTH"; }
                else if (testcycletype.ToUpper() == "THISMONTH") { cycledatafilter = "THISMONTH"; }
                else if (testcycletype.ToUpper() == "LASTONEMONTH") { cycledatafilter = "LASTONEMONTH"; }
                else if (testcycletype.ToUpper() == "LASTONEWEEK") { cycledatafilter = "LASTONEWEEK"; }
                else if (testcycletype.ToUpper() == "YESTERDAY") { cycledatafilter = "YESTERDAY"; }
                else if (testcycletype.ToUpper() == "TODAY") { cycledatafilter = "TODAY"; }
                else { cycledatafilter = "THISMONTH"; }
            }
            else { cycledatafilter = "THISMONTH"; }

            var cycledata = _configurationProvider.GetCycles(User, cycledatafilter);
            var cycleFDB = cycledata.CyclesFromDb;

            CyclesModel model = new CyclesModel()
            {
                CyclesFromDb = cycleFDB,
                ReleaseFromDb = releaseFDB,
                TeamFromDb = teamFDB
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public ActionResult TestCycles(CyclesModel model)
        {
            try
            {
                _configurationProvider.AddCycle(User, model);
            }
            catch (Exception ex)
            {

                ViewBag.ErrorMessage = ex.Message;
            }

            return RedirectToAction("TestCycles");
        }

        [Authorize]
        public ActionResult SetCurrentTestCycle(int id)
        {
            _configurationProvider.SetCurrentTestCycle(User, id);
            return RedirectToAction("TestCycles");
        }

        [HttpPost]
        [Authorize]
        public ActionResult SetTestCycleInnactive(int id)
        {
            try
            {
                _configurationProvider.SetTestCycleInnactive(User, id);
                return Json("");
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }

        }

        [HttpPost]
        [Authorize]
        public JsonResult DeleteTestCycle(int testCycleId)
        {
            string message = _configurationProvider.DeleteTestCycles(User, testCycleId);
            if (message.Contains("Exception occured while deleting test cycle"))
            {
                ResponseDelete response = new ResponseDelete() { IsDeleted = false, IsException = true, Message = message };
                var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                jsonResponseResult.MaxJsonLength = int.MaxValue;
                return jsonResponseResult;
            }
            else
            {
                ResponseDelete response = new ResponseDelete() { IsDeleted = true, IsException = false, Message = message };
                var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                jsonResponseResult.MaxJsonLength = int.MaxValue;
                return jsonResponseResult;
            }
        }

        [Authorize]
        public ActionResult PerformSvnSync()
        {
            string responsemessage = _configurationProvider.PerformSvnSync(User, Server.MapPath("~") + @"\\TestsStructure");
            return Json(new { type = "Message", text = responsemessage });
            //return RedirectToAction("Project", "Configuration");
        }

        [Authorize]
        public ActionResult Configurations()
        {
            return View(_configurationProvider.GetExecutionConfigurations(User));
        }

        [Authorize]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult Configurations(ExecutionConfigurationsModel model)
        {
            _configurationProvider.SetExecutionConfiguration(User, model);
            return RedirectToAction("Configurations");
        }

        [Authorize]
        public ActionResult Frameworks()
        {
            return View(_configurationProvider.GetVwFilesStorage(User));
        }

        [Authorize]
        public ActionResult Environments()
        {
            return View(_configurationProvider.GetExecutionEnvironments(User));
        }

        [Authorize]
        [HttpPost]
        public ActionResult Environments(ExecutionEnvironmentsModel model)
        {
            _configurationProvider.AddHostsConfiguration(User, model);
            return RedirectToAction("Environments");
        }

        [Authorize]
        public ActionResult Accounts()
        {
            return View(_configurationProvider.GetExecutionAccounts(User));
        }

        [Authorize]
        [HttpPost]
        public ActionResult Accounts(ExecutionAccountsModel model)
        {
            _configurationProvider.AddAccountForTestRun(User, model);
            return RedirectToAction("Accounts");
        }

        //[OutputCacheAttribute(VaryByParam = "*", Duration = 0, NoStore = true)] // will disable caching for Index only
        [Authorize]
        public ActionResult Project()
        {
            ModelState.Clear();
            return View(_configurationProvider.GetProjectConfiguration(User));
        }

        [Authorize]
        [HttpPost]
        public ActionResult Project(ProjectConfigurationModel model)
        {
            if (!ModelState.IsValid)
            {
                var btrack = _configurationProvider.GetProjectConfiguration(User);
                model.Bugtrackers = btrack.Bugtrackers;
                return View(model);
            }

            if (model.IsGITDefaultAccount)
            {
                string gpath = model.GITPath;
                gpath = gpath.ToUpper();

                if (gpath.Contains("HTTPS://GIT.SAMI.INT.THOMSONREUTERS.COM"))
                {
                    model.GITUser = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["GITUser"]);
                    model.GITPassword = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["GITPassword"]);
                }
                else if (gpath.Contains("HTTPS://GIT.CLARIVATE.IO"))
                { 
                    model.GITUser = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["GITUserClarivate"]);
                    model.GITPassword = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["GITPasswordClarivate"]);
                }
            }
            else
            {
                model.GITUser = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["GITUserClarivate"]);
                model.GITPassword = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["GITPasswordClarivate"]);
            }

            _configurationProvider.SetUpdatedConfiguration(User, model);
            ViewBag.Message = "Record updated successfully.";
            ViewBag.SelectedTab = model.Selectedtab;
            ModelState.Clear();
            return View(_configurationProvider.GetProjectConfiguration(User));
        }

        [Authorize]
        public ActionResult Machines(string filter = null)
        {
            string filterTypeMessage = string.Empty;
            var model = _configurationProvider.GetExecutionMachnies(User, filter, ref filterTypeMessage);
            ViewBag.FilterType = filterTypeMessage;
            return View(model);
        }


        [Authorize]
        [HttpPost]
        public ActionResult Machines(ExecutionMachniesModel model)
        {
            string errorMessage = string.Empty;
            _configurationProvider.AddClient(User, model, ref errorMessage);
            if (string.IsNullOrEmpty(errorMessage))
            {
                ViewBag.ErrorMessage = errorMessage;
                return RedirectToAction("Machines");
            }
            return RedirectToAction("Machines");
        }

        [Authorize]
        public ActionResult DeleteMachines(int id)
        {
            _configurationProvider.DeleteClient(User, id);
            return RedirectToAction("Machines");
        }

        [Authorize]
        public ActionResult CustomStatuses()
        {
            if (!string.IsNullOrEmpty(TempData["shortMessage"] as string))
            {
                ViewBag.Error = TempData["shortMessage"];
            }
            return View(_configurationProvider.GetCustomStatuses(User));
        }


        [Authorize]
        public ActionResult SikuliObjectRepository()
        {
            return View(_configurationProvider.GetSikuliObjects(User));
        }

        [Authorize]
        public ActionResult Users(string filter = null)
        {
            string filterTypeMessage = string.Empty;
            var model = filter == null
                ? _configurationProvider.GetProjectAndUsers(User)
                : _configurationProvider.GetProjectAndUsers(User, filter);
            ViewBag.UserFilterType = filterTypeMessage;
            return View(model);
        }

        [Authorize]
        [HttpPost]
        public JsonResult UpdateSikuliObject(string category, string name, string description, int id)
        {
            try
            {
                var sikuliClient = new SikuliServiceSoapClient();
                sikuliClient.UpdateSikuliItem(id, _sessionHelper.GetSessionDetails(User).ID, name, description, category);
                return Json(new { type = "Message", text = Messages.SikuliObjectUpdateMessage, userName = _sessionHelper.GetUser(User).UserName });
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        [Authorize]
        public void UploadSikuliObjects()
        {
            foreach (var filename in Request.Files)
            {
                HttpPostedFileBase httpPostedFileBase = Request.Files[filename.ToString()];
                var inputStream = httpPostedFileBase.InputStream;
                var sikuliClient = new SikuliServiceSoapClient();
                sikuliClient.AddSikuliItem(ReadData(inputStream), _sessionHelper.GetSessionDetails(User).ID,
                    httpPostedFileBase.FileName);
            }

        }

        [Authorize]
        public JsonResult DeleteSikuliObjects(int id)
        {
            try
            {
                var sikuliClient = new SikuliServiceSoapClient();
                sikuliClient.RemoveSikuliItem(id);
                return Json(new { type = "Message", text = Messages.SikuliObjectRemoveMessage });
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
            //  sikuliClient.(path, _sessionHelper.GetSessionDetails(User).ID);
        }
        [Authorize]
        public JsonResult AcceptFramework()
        {
            foreach (var filename in Request.Files)
            {
                HttpPostedFileBase httpPostedFileBase = Request.Files[filename.ToString()];
                var path = Server.MapPath(string.Format("~/Temp/{0}", httpPostedFileBase.FileName));
                var inputStream = httpPostedFileBase.InputStream;

                using (var fileStream = System.IO.File.Create(path))
                {
                    inputStream.CopyTo(fileStream);
                }

                //var fileStream = new FileStream(path, FileMode.Create);

                //inputStream.CopyTo(fileStream);
                //fileStream.Close();
                if (path.Split('.').Last().Trim().ToLower().Equals("zip"))
                    _configurationProvider.Sync(path, User, Server.MapPath("~"));
                System.IO.File.Delete(path);
            }
            return Json("");
        }
        [Authorize]
        public ActionResult GetFrameworkById(int id)
        {
            var framework = _configurationProvider.GetFramework(id);
            if (framework != null)
            {
                var fileData = (byte[])framework.FileContent.ToArray();
                string fileName = framework.Name;
                return File(fileData, ".zip", fileName);
            }
            return null;
        }
        [Authorize]
        public ActionResult DeleteAccount(int id)
        {
            _configurationProvider.DeleteAccount(User, id);
            return RedirectToAction("Accounts");
        }

        [Authorize]
        public ActionResult DeleteEnvironment(int id)
        {
            _configurationProvider.DeleteHostsConfiguration(User, id);
            return RedirectToAction("Environments");
        }

        [Authorize]
        public ActionResult DeleteExecutionConfiguration(int id)
        {
            _configurationProvider.DeleteExecutionConfiguration(User, id);
            return RedirectToAction("Configurations");
        }

        [Authorize]
        public ActionResult EditExecutionConfiguration(int id, string name, string fileName, string content)
        {
            _configurationProvider.EditExecutionConfiguration(User, id, name, fileName, content);
            return RedirectToAction("Configurations");
        }

        [Authorize]
        public ActionResult EditExecutionAccount(int id, string name, string login, string password, string comments)
        {
            _configurationProvider.EditExecutionAccount(User, id, name, login, password, comments);
            return RedirectToAction("Configurations");
        }

        [Authorize]
        public ActionResult EditExecutionEnvironment(int id, string name, string url, string content)
        {
            _configurationProvider.EditExecutionEnvironment(User, id, name, url, content);
            return RedirectToAction("Configurations");
        }

        [Authorize]
        public ActionResult RemoveFramework(int id)
        {
            _configurationProvider.DeleteFileStorage(User, id);
            return RedirectToAction("Frameworks");
        }

        [Authorize]
        public ActionResult AssignMachineToProject(int id)
        {
            _configurationProvider.SetMachineToProject(User, id);
            return RedirectToAction("Machines");
        }
        [Authorize]
        public ActionResult AssignUserOnProject(int id)
        {
            _configurationProvider.AssignUserOnProject(User, id);
            return RedirectToAction("Users");
        }
        [Authorize]
        public ActionResult UnassignUserFromProject(int id)
        {
            _configurationProvider.UnassignUser(User, id);
            return RedirectToAction("Users");
        }
        [Authorize]
        public ActionResult ReleaseMachine(int id)
        {
            _configurationProvider.ReleaseMachine(id);
            return RedirectToAction("Machines");
        }
        [Authorize]
        public ActionResult CustomStatusInfo(string statusName)
        {
            if (statusName == Labels.AddNew)
            {
                return PartialView(new Status());
            }
            return PartialView(_configurationProvider.GetCustomStatus(User, statusName));
        }
        [Authorize]
        public JsonResult CheckUnique(string color, string statusName, string statusID, Guid uniqueID)
        {
            string message = string.Empty;
            if (_configurationProvider.CheckUniqueValues(User, color, statusName, statusID, uniqueID, ref message))
            {
                return Json(new { type = "Message", text = "It is ok" });
            }
            else
            {
                return Json(new { type = "Error", text = message });
            }

        }
        [Authorize]
        public ActionResult SaveCustomStatus(Status status)
        {
            try
            {
                string message = string.Empty;
                if (status.UniqueID == Guid.Empty)
                {
                    status.UniqueID = Guid.NewGuid();
                    _configurationProvider.AddCustomStatus(User, status);
                }
                else
                {
                    _configurationProvider.EditCustomStatus(User, status);
                }
                return RedirectToAction("CustomStatuses");

            }
            catch (Exception ex)
            {
                TempData["shortMessage"] = ex.Message;
            }

            return RedirectToAction("CustomStatuses");
        }
        [Authorize]
        public ActionResult UpdateIssues(object sender, EventArgs e)
        {
            string status, value;
            try
            {
                string bugtrackerLogin, bugtrackerPass;
                int projId;
                var listOfPossibleDefects = _configurationProvider.GetListOfPossibleDefects(User, out bugtrackerLogin, out bugtrackerPass, out projId);
                if (!listOfPossibleDefects.Any())
                {
                    status = "Message";
                    value = Messages.NoDefectsToUpdateException;
                    return Json(new { type = status, text = value });
                }
                var jClient = new JiraSoapServiceService();
                var token = jClient.login(bugtrackerLogin, bugtrackerPass);
                var allStatuses = jClient.getStatuses(token).ToList();
                foreach (var defect in listOfPossibleDefects)
                {
                    try
                    {
                        var jiraIssue = jClient.getIssue(token, defect);
                        var newIssue = _configurationProvider.CreateIssue(projId, defect);
                        if (jiraIssue != null)
                        {
                            var theStat = allStatuses.Find(p => p.id == jiraIssue.status);
                            _configurationProvider.SetIssueStatus(true, jiraIssue.summary, newIssue, theStat.name);
                        }
                        else _configurationProvider.SetIssueStatus(true, null, newIssue, null);
                    }
                    catch { }
                }
                status = "Message";
                value = Messages.UpdateIssueMessage;
            }
            catch (Exception ex)
            {
                status = "Error";
                value = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message);
            }

            return Json(new { type = status, text = value });
        }

        private byte[] ReadData(Stream stream)
        {
            var buffer = new byte[16 * 1024];

            using (var ms = new MemoryStream())
            {
                int read;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }

                return ms.ToArray();
            }
        }

        //[Authorize]
        //public JsonResult RefreshSvn(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        var projId = _configurationProvider.GetProject(User);
        //        _configurationProvider.UpdateFrameworkFromSvn(projId, User, Server.MapPath("~"));
        //        return Json("");
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
        //    }
        //}

        [Authorize]
        [HttpPost]
        public ActionResult GetQcPath(string testSet)
        {
            try
            {
                var projId = _configurationProvider.GetProject(User);
                var path = _configurationProvider.GetQcPath(projId, testSet);
                return Json(path);
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }
        [Authorize]
        [HttpPost]
        public ActionResult SetQcPath(string testSetId, string qcPath)
        {
            try
            {
                var isUpdated = _configurationProvider.SetQcPath(testSetId, qcPath);
                return Json(isUpdated);
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        //[Authorize]
        //[HttpPost]
        //public ActionResult SubmitProjectSettings(object sender, EventArgs e)
        //{
        //    try
        //    {
        //        var values = HttpUtility.UrlDecode(Request.Form.ToString());
        //        Dictionary<string, string> request = new Dictionary<string, string>();
        //        var valuesArray = values.Split('&');
        //        foreach(var item in valuesArray)
        //        {
        //            var temp = item.Split('=');
        //            if (temp.Length == 2)
        //            {
        //                request.Add(key: temp[0], value: temp[1]);
        //            }
        //            else
        //            {
        //                request.Add(key: temp[0], value: null);
        //            }
        //        }
        //        var projectId = _configurationProvider.GetProject(User);
        //        var updatedFields = _configurationProvider.SubmitProjectSettings(request, projectId, User);
        //        //return Json(new { type = "Message", text = "Changes were submitted successfully" });
        //        return Json(updatedFields);
        //    }
        //    catch (Exception ex)
        //    {
        //        return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
        //    }
        //}

        [HttpPost]
        [Authorize]
        public ActionResult EditTestCycle(int id, string name, string start, string end, string comment, Int64 releaseid, Int64 teamid)
        {
            try
            {
                var releasedata = _configurationProvider.GetReleaseById(releaseid);
                var teamdata = _configurationProvider.GetTeamById(teamid);
                _configurationProvider.EditTestCycle(User, id, name, start, end, comment, releasedata.ReleaseName, releasedata.ReleaseDate.ToString("MM/dd/yyyy"), teamdata.TeamName);
                return null;
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        #region TEAM
        [HttpGet]
        [Authorize]
        public ActionResult TeamInfos()
        {
            var teamInfos = _configurationProvider.GetTeams(User);
            return View(teamInfos);
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddTeamInfo(string TeamName, string Comment)
        {
            try
            {
                _configurationProvider.AddNewTeam(User, TeamName, Comment);
            }
            catch (Exception ex)
            {
                //ViewBag.ErrorMessage = ex.Message;
            }
            return RedirectToAction("TeamInfos");
        }

        [HttpPost]
        [Authorize]
        public ActionResult UpdateTeamInfo(Int64 TeamInfoId, string TeamName, string Comment)
        {
            try
            {
                _configurationProvider.UpdateTeamInfo(User, TeamInfoId, TeamName, Comment);
                return null;
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        [HttpPost]
        [Authorize]
        public JsonResult DeleteTeamInfo(Int64 TeamInfoId)
        {
            string message = _configurationProvider.DeleteTeam(User, TeamInfoId);
            if (message.Contains("Exception occured while deleting Team Information."))
            {
                ResponseDelete response = new ResponseDelete() { IsDeleted = false, IsException = true, Message = message };
                var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                jsonResponseResult.MaxJsonLength = int.MaxValue;
                return jsonResponseResult;
            }
            else
            {
                ResponseDelete response = new ResponseDelete() { IsDeleted = true, IsException = false, Message = message };
                var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                jsonResponseResult.MaxJsonLength = int.MaxValue;
                return jsonResponseResult;
            }
        }

        #endregion TEAM

        #region RELEASE
        [HttpGet]
        [Authorize]
        public ActionResult ReleaseInfos()
        {
            var releaseInfos = _configurationProvider.GetReleases(User);
            return View(releaseInfos);
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddReleaseInfo(string ReleaseName, string ReleaseDate, string Comment)
        {
            try
            {
                _configurationProvider.AddNewRelease(User, ReleaseName, ReleaseDate, Comment);
            }
            catch (Exception ex)
            {
                //ViewBag.ErrorMessage = ex.Message;
            }
            return RedirectToAction("ReleaseInfos");
        }

        [HttpPost]
        [Authorize]
        public ActionResult UpdateReleaseInfo(Int64 ReleaseInfoId, string ReleaseName, string ReleaseDate, string Comment)
        {
            try
            {
                _configurationProvider.UpdateReleaseInfo(User, ReleaseInfoId, ReleaseName, ReleaseDate, Comment);
                return null;
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }

        [HttpPost]
        [Authorize]
        public JsonResult DeleteReleaseInfo(Int64 ReleaseInfoId)
        {
            string message = _configurationProvider.DeleteRelease(User, ReleaseInfoId);
            if (message.Contains("Exception occured while deleting Team Information."))
            {
                ResponseDelete response = new ResponseDelete() { IsDeleted = false, IsException = true, Message = message };
                var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                jsonResponseResult.MaxJsonLength = int.MaxValue;
                return jsonResponseResult;
            }
            else
            {
                ResponseDelete response = new ResponseDelete() { IsDeleted = true, IsException = false, Message = message };
                var jsonResponseResult = Json(response, JsonRequestBehavior.AllowGet);
                jsonResponseResult.MaxJsonLength = int.MaxValue;
                return jsonResponseResult;
            }
        }

        #endregion RELEASE

        #region Execution Groups
        //[HttpGet]
        //[Authorize]
        //public ActionResult ExecutionGroupInfo()
        //{
        //    List<ExecutionGroupModel> result = _configurationProvider.GetExecutionGroupInfo(User);
        //    ViewBag.GroupList = result;
        //    return View();
        //}

        //[HttpPost]
        //[Authorize]
        //public ActionResult ExecutionGroupInfo(AddExecutionGroupModel inputModel)
        //{
        //    if (ModelState.IsValid)
        //    {
        //        var result = _configurationProvider.AddExecutionGroup(User, inputModel.GroupName);
        //        ViewBag.Message = result;
        //        //return RedirectToAction("ExecutionGroupInfo", "Configuration");
        //        return RedirectToAction("ExecutionGroupInfo");
        //    }
        //    else 
        //    {
        //        return View();
        //    }
        //}

        //[HttpPost]
        //[Authorize]
        //public JsonResult UpdateExecutionGroup(EditExecutionGroupModel inputSheet)
        //{
        //    var result = _configurationProvider.UpdateExecutionGroup(User, inputSheet.Id, inputSheet.GroupName);
        //    var response = Json(result, JsonRequestBehavior.AllowGet);
        //    response.MaxJsonLength = int.MaxValue;
        //    return response;
        //}

        //[HttpPost]
        //[Authorize]
        //public JsonResult DeleteExecutionGroup(Int64 ExecutionGroupId)
        //{
        //    ResultReturnModel result = _configurationProvider.DeleteExecutionGroup(User, ExecutionGroupId);
        //    var response = Json(result, JsonRequestBehavior.AllowGet);
        //    response.MaxJsonLength = int.MaxValue;
        //    return response;
        //}

        //[HttpGet]
        //[Authorize]
        //public ActionResult ExecutionGroup_Clients()
        //{
        //}

        //[HttpPost]
        //[Authorize]
        //public ActionResult ExecutionGroup_Clients()
        //{ 
        //}

        //[HttpPost]
        //[Authorize]
        //public JsonResult UpdateExecutionGroup_Clients()
        //{
        //}

        //[HttpPost]
        //[Authorize]
        //public JsonResult DeleteExecutionGroup_Clients()
        //{ 
        //}
        #endregion Execution Groups

        
    }
}
