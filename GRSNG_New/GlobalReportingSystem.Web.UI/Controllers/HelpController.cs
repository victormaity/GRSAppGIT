using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Web.UI.Attributes;

namespace GlobalReportingSystem.Web.UI.Controllers
{
    public class HelpController : Controller
    {

        public ActionResult Index()
        {
            return View();
        }
    }
}