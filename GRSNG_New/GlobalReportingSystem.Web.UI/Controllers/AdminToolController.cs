using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Models.GRS;
using System.Web.Routing;
using GlobalReportingSystem.Core.Abstract.BL.Helper;

namespace GlobalReportingSystem.Web.UI.Controllers
{
    public class AdminToolController : Controller
    {
        private readonly IManageUserProvider _manageUsers;
        private readonly IManageConfigurationProvider _manageConfiguration;
        private readonly ISessionHelper _sessionHelper;

        public AdminToolController(IManageUserProvider manageUserProvider, IManageConfigurationProvider manageConfiguration, ISessionHelper sessionHelper)
        {
            _manageUsers = manageUserProvider;
            _manageConfiguration = manageConfiguration;
            _sessionHelper = sessionHelper;
        }


        [Authorize]
        public ActionResult Index()
        {
            return View();
        }


        [Authorize]
        public ActionResult GlobalNotifications()
        {

            return View();
        }

        [Authorize]
        [HttpPost]
        [ValidateInput(false)]
        public ActionResult GlobalNotifications(AdminToolGlobalNotificationsModel model)
        {
            _manageUsers.SendNotification(model.Message);
            return View();
        }

        [Authorize]
        public ActionResult UserAccesses()
        {
            if (!_sessionHelper.GetSessionDetails(User).User.UserGlobalAdmin)
            {
                ViewBag.Message = "You don't have access to Admin Panel";
                return PartialView("Forbidden");
                //return RedirectToAction("Forbidden", "ErrorHandle", new { projectName = testCycle.Project.DisplayName });
            }
            return View(_manageConfiguration.GetAvailableProjects());
        }

        [Authorize]
        public ActionResult UsersTable(int projectId)
        {
            var model = _manageConfiguration.GetProjectAndUsers(projectId);
            return PartialView(model);
        }

        [Authorize]
        public ActionResult AssignUserOnProject(int projectId, int userId)
        {
            _manageConfiguration.AssignUserOnProject(projectId, userId);
            return RedirectToAction("UsersTable", new { projectId = projectId });
        }
        [Authorize]
        public ActionResult UnassignUserFromProject(int projectId, int userId)
        {
            _manageConfiguration.UnassignUser(projectId, userId);
            return RedirectToAction("UsersTable", new { projectId = projectId });
        }

        //New changes by Sundram============================================

        //Sundram 
        //Cleanup project with old record
        [HttpGet]
        [Authorize]
        public ActionResult DeleteOldRecord()
        {
            bool canshow = _manageConfiguration.CheckIsAdminUsingPage(User);
            ViewBag.CanShow = canshow;
            if (canshow)
            {
                return View(_manageConfiguration.GetProjectListForDeleteOldRecord());
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        [Authorize]
        public JsonResult DeleteOldRecord(int projectId, DateTime beforeDate)
        {
            bool canshow = _manageConfiguration.CheckIsAdminUsingPage(User);
            ViewBag.CanShow = canshow;
            if (canshow)
            {
                var deleteStatus = _manageConfiguration.DeleteOldRecordByAdmin(User, projectId, beforeDate);
                return Json(new { type = "Message", text = deleteStatus });
            }
            else
            {
                return Json(new { type = "Message", text = "You are not autorize to delete record." });
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult AdminRights()
        {
            bool canshow = _manageConfiguration.CheckIsAdminUsingPage(User);
            ViewBag.CanShow = canshow;
            if (canshow)
            {
                return View(_manageConfiguration.GetUserListData(User));
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        [Authorize]
        public JsonResult AssignUserAdminRight(int userId)
        {
            var returnmessage = _manageConfiguration.AssignUserAdminRight(User, userId);
            var afterUpdateStatus = _manageUsers.GetUserById(User, userId);
            string newStatus = afterUpdateStatus.UserAdmin ? "USER ADMIN" : "";
            return Json(new { type = "Message", status = newStatus, text = returnmessage });
        }

        [HttpPost]
        [Authorize]
        public ActionResult UnAssignUserAdminRight(int userId)
        {
            var returnmessage = _manageConfiguration.UnAssignUserAdminRight(User, userId);
            var afterUpdateStatus = _manageUsers.GetUserById(User, userId);
            string newStatus = afterUpdateStatus.UserAdmin ? "USER ADMIN" : "";
            return Json(new { type = "Message", status = newStatus, text = returnmessage });
        }

        [HttpPost]
        [Authorize]
        public ActionResult AssignGlobalAdminRight(int userId)
        {
            var returnmessage = _manageConfiguration.AssignGlobalAdminRight(User, userId);
            var afterUpdateStatus = _manageUsers.GetUserById(User, userId);
            string newStatus = afterUpdateStatus.UserGlobalAdmin ? "GLOBAL ADMIN" : "";
            return Json(new { type = "Message", status = newStatus, text = returnmessage });
        }

        [HttpPost]
        [Authorize]
        public ActionResult UnAssignGlobalAdminRight(int userId)
        {
            var returnmessage = _manageConfiguration.UnAssignGlobalAdminRight(User, userId);
            var afterUpdateStatus = _manageUsers.GetUserById(User, userId);
            string newStatus = afterUpdateStatus.UserGlobalAdmin ? "GLOBAL ADMIN" : "";
            return Json(new { type = "Message", status = newStatus, text = returnmessage });
        }

        [HttpPost]
        [Authorize]
        public ActionResult UnBlockUser(int userId)
        {
            var returnmessage = _manageConfiguration.UnBlockUser(User, userId);
            var afterUpdateStatus = _manageUsers.GetUserById(User, userId);
            string newStatus = (afterUpdateStatus.UserBlocked == true) ? "BLOCKED" : "";
            return Json(new { type = "Message", status = newStatus, text = returnmessage });
        }

        [HttpPost]
        [Authorize]
        public ActionResult BlockUser(int userId)
        {
            var returnmessage = _manageConfiguration.BlockUser(User, userId);
            var afterUpdateStatus = _manageUsers.GetUserById(User, userId);
            string newStatus = (afterUpdateStatus.UserBlocked == true) ? "BLOCKED" : "";
            return Json(new { type = "Message", status = newStatus, text = returnmessage });
        }

        [HttpGet]
        [Authorize]
        public ActionResult AddNewProject()
        {
            bool canshow = _manageConfiguration.CheckIsAdminUsingPage(User);
            ViewBag.CanShow = canshow;
            if (canshow)
            {
                var projectTypeList = _manageConfiguration.GetProjectTypes();
                var ProjectListMapped = (from x in projectTypeList
                                         select new PList
                                         {
                                             Value = x.ID,
                                             Text = x.ProjectTypeName
                                         }).ToList();

                AdminToolAddNewProject model = new AdminToolAddNewProject()
                {
                    ProjectTypeList = ProjectListMapped
                };

                if (TempData["DataMessage"] != null)
                {
                    string message = string.Empty;
                    message = TempData["DataMessage"] as string;
                    ViewBag.Message = message;
                }
                return View(model);
            }
            return View();
        }

        [HttpPost]
        [Authorize]
        public ActionResult AddNewProject(AdminToolAddNewProject addProject)
        {
            var sessiondata = _sessionHelper.GetSessionDetails(User);
            if (sessiondata.User.UserGlobalAdmin)
            {
                bool canshow = _manageConfiguration.CheckIsAdminUsingPage(User);
                ViewBag.CanShow = canshow;

                var projectTypeList = _manageConfiguration.GetProjectTypes();
                var ProjectListMapped = (from x in projectTypeList
                                         select new PList
                                         {
                                             Value = x.ID,
                                             Text = x.ProjectTypeName
                                         }).ToList();
                addProject.ProjectTypeList = ProjectListMapped;

                if (!ModelState.IsValid)
                {
                    return View(addProject);
                }
                string gpath = addProject.GITPath;
                gpath = gpath.ToUpper();
                if (gpath.Contains("HTTPS://GIT.SAMI.INT.THOMSONREUTERS.COM"))
                {
                    addProject.GITUSER = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["GITUser"]);
                    addProject.GITPASSWORD = Convert.ToString(System.Configuration.ConfigurationManager.AppSettings["GITPassword"]);
                }
                var message = _manageConfiguration.AddNewProject(User, addProject);
                
                //if (message.ErrorId == 0)
                //{
                    ModelState.Clear();
                    TempData["DataMessage"] = message.ErrorMessage;
                    return RedirectToAction("AddNewProject", "AdminTool");
                //}
                //else
                //{
                //    return View(addProject);
                //}
            }
            else
            {
                ModelState.Clear();
                ViewBag.CanShow = false;
                return View();
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult CreateNewUser()
        {
            bool canshow = _manageConfiguration.CheckIsAdminUsingPage(User);
            ViewBag.CanShow = canshow;
            if (canshow)
            {
                var projectList = _manageConfiguration.GetProjectListForDeleteOldRecord();
                var ProjectListMapped = (from x in projectList
                                         select new PList
                                         {
                                             Value = x.ID,
                                             Text = x.DisplayName
                                         }).ToList();

                AdminToolCreateNewUserModel model = new AdminToolCreateNewUserModel()
                {
                    ProjectLists = ProjectListMapped
                };

                if (TempData["DataMessage"] != null)
                {
                    string message = string.Empty;
                    message = TempData["DataMessage"] as string;
                    ViewBag.Message = message;
                }
                return View(model);
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        [Authorize]
        public ActionResult CreateNewUser(AdminToolCreateNewUserModel usermodel)
        {
            var sessiondata = _sessionHelper.GetSessionDetails(User);
            if (sessiondata.User.UserGlobalAdmin)
            {
                bool canshow = _manageConfiguration.CheckIsAdminUsingPage(User);
                ViewBag.CanShow = canshow;
                var projectList = _manageConfiguration.GetProjectListForDeleteOldRecord();
                var ProjectListMapped = (from x in projectList
                                         select new PList
                                         {
                                             Value = x.ID,
                                             Text = x.DisplayName
                                         }).ToList();
                usermodel.ProjectLists = ProjectListMapped;

                if (!ModelState.IsValid)
                {
                    return View(usermodel);
                }

                var messagetoshare = _manageConfiguration.AddNewUser(User, usermodel.UserName, usermodel.UserFullName, usermodel.UserEmail, usermodel.IsAdmin, usermodel.IsGlobalAdmin, usermodel.ProjectID);
                if (messagetoshare.ErrorId == 0)
                {
                    ModelState.Clear();
                    TempData["DataMessage"] = messagetoshare.ErrorMessage;
                    return RedirectToAction("CreateNewUser", "AdminTool");
                }
                else
                {
                    return View(usermodel);
                }
            }
            else
            {
                ViewBag.CanShow = false;
                ModelState.Clear();
                return View();
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult UnTrackedFiles()
        {
            bool canshow = _manageConfiguration.CheckIsAdminUsingPage(User);
            ViewBag.CanShow = canshow;
            return View();
        }

        [HttpPost]
        [Authorize]
        public JsonResult DeleteUploadedTempFiles()
        {
            var sessiondata = _sessionHelper.GetSessionDetails(User);
            if (sessiondata.User.UserGlobalAdmin)
            {
                var message = _manageConfiguration.DeleteHarvestTempUploadedFile(User);
                return Json(new { type = "Message", text = message });
            }
            else
            {
                return Json(new { type = "Message", text = "You are not autorize to delete record." });
            }
        }

        [HttpPost]
        [Authorize]
        public JsonResult DeleteWebUITempFiles()
        {
            var sessiondata = _sessionHelper.GetSessionDetails(User);
            if (sessiondata.User.UserGlobalAdmin)
            {
                var message = _manageConfiguration.DeleteWEBUITempFiles(User);
                return Json(new { type = "Message", text = message });
            }
            else
            {
                return Json(new { type = "Message", text = "You are not autorize to delete record." });
            }
        }

        [HttpPost]
        [Authorize]
        public JsonResult DeleteChartFiles()
        {
            var sessiondata = _sessionHelper.GetSessionDetails(User);
            if (sessiondata.User.UserGlobalAdmin)
            {
                var message = _manageConfiguration.DeleteChartFiles(User);
                return Json(new { type = "Message", text = message });
            }
            else
            {
                return Json(new { type = "Message", text = "You are not autorize to delete record." });
            }
        }

        [HttpGet]
        [Authorize]
        public ActionResult ProjectTestSuits()
        {
            bool canshow = _manageConfiguration.CheckIsAdminUsingPage(User);
            ViewBag.CanShow = canshow;
            if (canshow)
            {
                var projectList = _manageConfiguration.GetProjectListForDeleteOldRecord();
                var ProjectListMapped = (from x in projectList
                                         select new PList
                                         {
                                             Value = x.ID,
                                             Text = x.DisplayName
                                         }).ToList();


                return View(ProjectListMapped);
            }
            else
            {
                return View();
            }
        }

        [HttpPost]
        [Authorize]
        public JsonResult GetProjectTestSuits(int projectId, DateTime datebefore)
        {
            Int64 recordCount = 0;
            try
            {
                var sessiondata = _sessionHelper.GetSessionDetails(User);
                if (sessiondata.User.UserGlobalAdmin)
                {
                    var tdata = _manageConfiguration.GetTestSuitList(User, projectId, datebefore);
                    recordCount = tdata.TestsuitManageModel.Count();
                    if (recordCount > 0)
                    {
                        return Json(new { type = "Message", recordCount = recordCount, text = "", suits = tdata.TestsuitManageModel, pname = tdata.ProjectDisplayName });
                    }
                    else
                    {
                        return Json(new { type = "Message", recordCount = recordCount, text = "Testsuit not found.", suits = tdata.TestsuitManageModel, pname = tdata.ProjectDisplayName });
                    }
                }
                else
                {
                    return Json(new { type = "Message", recordCount = recordCount, text = "You are not autorize to delete record.", pname = "" });
                }
            }
            catch (Exception ex)
            {
                recordCount = 0;
                return Json(new { type = "Error", recordCount = recordCount, text = ex.Message, pname = "" });
            }

        }

        [HttpPost]
        [Authorize]
        public JsonResult DeleteTestsuitsByIds(Int64[] TestSuitIDArr)
        {
            try
            {
                var sessiondata = _sessionHelper.GetSessionDetails(User);
                if (sessiondata.User.UserGlobalAdmin)
                {
                    string messageToDisplay = _manageConfiguration.DeleteSuitSuitsByIds(User, new List<Int64>(TestSuitIDArr));
                    return Json(new { type = "Message", text = messageToDisplay });
                }
                else
                {
                    return Json(new { type = "Message", text = "You are not authorized to delete Testsuit(s)." });
                }
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message });
            }
        }

        //[HttpGet]
        //[Authorize]
        //public ActionResult ManageTrackingRowsAndLogs()
        //{
        //    bool canshow = _manageConfiguration.CheckIsAdminUsingPage(User);
        //    ViewBag.CanShow = canshow;
        //    return View();
        //}

        //[HttpPost]
        //[Authorize]
        //public JsonResult GetTrackingRowsData(string MachineIP)
        //{
        //    var sessiondata = _sessionHelper.GetSessionDetails(User);
        //    if (sessiondata.User.UserGlobalAdmin)
        //    {
        //        var reltrackdata = _manageConfiguration.GetRelExecutionTrackingData(User, MachineIP);
        //        Int64 recordCount = reltrackdata.Count();
        //        if (recordCount > 0)
        //        {
        //            return Json(new { type = "Message", atogetdata = true, recordCount = recordCount, text = "", relExecution = reltrackdata });
        //        }
        //        else
        //        {
        //            return Json(new { type = "Message", atogetdata = true, recordCount = recordCount, text = "RelExecution tracking row not found.", relExecution = reltrackdata });
        //        }
        //    }
        //    else
        //    {
        //        return Json(new { type = "Message", atogetdata = false, text = "You are not authorized to Tracking Data." });
        //    }
        //}

        //[HttpPost]
        //[Authorize]
        //public JsonResult DeleteTrackingRowsAndLogs(Int64[] RelExeTrackRowIds)
        //{
        //    var sessiondata = _sessionHelper.GetSessionDetails(User);
        //    if (sessiondata.User.UserGlobalAdmin)
        //    {
        //        try
        //        {
        //            bool isdeleted = _manageConfiguration.DeleteRelExecutionTrackingDataAndLogs(User, RelExeTrackRowIds);
        //            if (isdeleted == true)
        //            {
        //                return Json(new { type = "Message", atogetdata = true, deleted = true, text = "RelExecution Tracking row deleted successfully." });
        //            }
        //            else
        //            {
        //                return Json(new { type = "Message", atogetdata = true, deleted = false, text = "RelExecution Tracking row deletion failed." });
        //            }
        //        }
        //        catch (Exception ex)
        //        {
        //            return Json(new { type = "Error", atogetdata = true, deleted = false, text = ex.Message });
        //        }
        //    }
        //    else
        //    {
        //        return Json(new { type = "Message", atogetdata = false, deleted = false, text = "You are not authorized to delete Tracking Data." });
        //    }
        //}

        //==================================================================
    }
}
