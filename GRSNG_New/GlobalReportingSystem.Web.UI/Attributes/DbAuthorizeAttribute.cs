using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
using Microsoft.Practices.ServiceLocation;

namespace GlobalReportingSystem.Web.UI.Attributes
{
    public class DbAuthorize : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            base.OnActionExecuting(filterContext);

            if (filterContext.Controller.ControllerContext.HttpContext.User.Identity.Name==""||!ServiceLocator.Current.GetInstance<ISessionHelper>().IsSessionExist(filterContext.Controller.ControllerContext.HttpContext.User))
            {
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary
                {
                    {"Controller", "Users"},
                    {"Action", "Index"}
                });

                FormsAuthentication.SignOut();
            }
        }


    }
}
