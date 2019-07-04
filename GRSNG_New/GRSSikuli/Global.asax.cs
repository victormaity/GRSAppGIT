using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using Castle.Facilities.WcfIntegration;
using System.Web.Http;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using GlobalReportingSystem.ServiceLocation;

namespace GRSSikuli
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801
    public class MvcApplication : System.Web.HttpApplication
    {
        /// The Windsor conteiner
        /// </summary>
        private static IWindsorContainer container;

        protected void Application_Start()
        {
            container = new WindsorContainer();
            container.Install(new ServicesEntityInstaller());
            container.Register(Component.For<ISikuliRetrieveService>().ImplementedBy<SikuliRetrieveService>().AsWcfService(new DefaultServiceModel().
            Hosted()).IsDefault().LifestylePerWebRequest());
            RouteTable.Routes.MapHttpRoute(
    name: "DefaultApi",
    routeTemplate: "api/{controller}/{id}",
    defaults: new { id = System.Web.Http.RouteParameter.Optional }
    );
        }

        protected void Application_End(object sender, EventArgs e)
        {
            if (container != null)
            {
                container.Dispose();
            }
        }
    }
}