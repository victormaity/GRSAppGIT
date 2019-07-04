using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Core.Models.Entities;
using GlobalReportingSystem.Core.Models.GRS;
using GlobalReportingSystem.Web.UI.Attributes;

namespace GlobalReportingSystem.Web.UI.Controllers
{
    public class UsersController : Controller
    {
        private readonly IManageUserProvider _userProvider;

        public UsersController(IManageUserProvider userProvider)
        {
            _userProvider = userProvider;
        }
        //
        // GET: /Users/

        public ActionResult Index()
        {
            return User.Identity.IsAuthenticated
                ? (ActionResult)
                    (_userProvider.IsSetProject(User)
                        ? RedirectToAction("Index", "Home")
                        : RedirectToAction("SelectProject", "Users"))
                : View();
        }

        public ActionResult Logout()
        {
            FormsAuthentication.SignOut();
            return RedirectToAction("Index");
        }

        [HttpPost]
       /* [Transaction]*/
        public ActionResult Index(AuthenticationModel model)
        {
            try
            {
                FormsAuthentication.SetAuthCookie(
                    _userProvider.AddUserToDb(model, Request.UserHostAddress, Request.Browser.Browser).ToString(),
                    model.RememberMe);
                return RedirectToAction("SelectProject");
            }
            catch (Exception ex)
            {

                ViewBag.ErrorMessage = ex.Message;
            }
            return View();
        }

        public ActionResult Register()
        {
            return View();  
        }

        [HttpPost]
        public ActionResult Register(AuthenticationModel model)
        {
            try
            {
                _userProvider.RegisterNewUser(model);
                return RedirectToAction("Index", "Users");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }
            return View();  
        }

        public ActionResult RetrievePassword()
        {
            return View();
        }

        
        [HttpPost]
        public ActionResult RetrievePassword(AuthenticationModel model)
        {
            if (!_userProvider.RecoverPassowrd(model.Email))
            {
                ViewBag.ErrorMessage = "Cant find requested email";
            }
            return RedirectToAction("Index");
        }
            
        [Authorize]
        public ActionResult SelectProject()
        {
            return View(_userProvider.SelectProjectsByUser(User));
        }

        [Authorize]
        [HttpPost]
        /*[Transaction]*/
        public ActionResult SelectProject(AuthenticationModel model)
        {
            _userProvider.SaveLastUsedProject(model, User);
            return RedirectToAction("Index", "Home");
        }

        [Authorize]
        public ActionResult Profile()
        {
            return View(_userProvider.GetUserProfile(User));
        }

        [Authorize]
        [HttpPost]
        public ActionResult Profile(UserProfileModel model)
        {
            try
            {
                _userProvider.UpdateUserProfile(User, model);
                ViewBag.Message = "Changes saved!";
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = ex.Message;
            }
            return View(_userProvider.GetUserProfile(User));
        }

        [Authorize]
        public ActionResult NotificationsSettings()
        {
            return View(_userProvider.getUserAccesses(User));
        }
        [Authorize]
        public ActionResult UpdateNotificationsSettings(int projectId)
        {
            try
            {
                _userProvider.updateUserNotifications(User, projectId);
                return Json("");
            }
            catch (Exception ex)
            {
                return Json(new { type = "Error", text = ex.Message.Length > 461 ? (ex.Message.Substring(0, 458) + "...") : (ex.Message) });
            }
        }
    }
}
