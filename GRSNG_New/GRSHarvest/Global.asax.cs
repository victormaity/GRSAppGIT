using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using GlobalReportingSystem.ServiceLocation;

namespace GRSHarvest
{
    public class Global : System.Web.HttpApplication
    {
        /// The Windsor conteiner
        /// </summary>
        private static IWindsorContainer container;

        /// <summary>
        /// The start application
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void Application_Start(object sender, EventArgs e)
        {
            container = new WindsorContainer();
            container.Install(new ServicesEntityInstaller());

       }

        /// <summary>
        /// The end application
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        protected void Application_End(object sender, EventArgs e)
        {
            if (container != null)
            {
                container.Dispose();
            }
        }
    }
}