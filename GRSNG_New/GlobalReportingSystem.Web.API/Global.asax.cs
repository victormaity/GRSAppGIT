using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using GlobalReportingSystem.ServiceLocation;

namespace GlobalReportingSystem.Web.API
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        private static IWindsorContainer container;

        protected void Application_Start()
        {
            container = new WindsorContainer();

            // EntityFramework Initialization
            container.Install(new ApiEntityInstaller());
            container.Register(Classes.FromThisAssembly().BasedOn<IHttpController>().LifestylePerWebRequest());
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            GlobalConfiguration.Configuration.Formatters.XmlFormatter.SupportedMediaTypes.Clear();
        }

        /// <summary>
        /// Application end event.
        /// </summary>
        protected void Application_End()
        {
            container.Dispose();
        }

    }
}