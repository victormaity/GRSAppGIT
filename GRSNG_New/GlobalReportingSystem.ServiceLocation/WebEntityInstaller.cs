using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using GlobalReportingSystem.BL.Helper;
using GlobalReportingSystem.BL.Implementation;
using GlobalReportingSystem.Core.Abstract;
using GlobalReportingSystem.Core.Abstract.BL.Helper;
using GlobalReportingSystem.Core.Abstract.ProviderInterfaces;
using GlobalReportingSystem.Data.Concrete;
using GlobalReportingSystem.Data.DB;
using Microsoft.Practices.ServiceLocation;

namespace GlobalReportingSystem.ServiceLocation
{
    public class WebEntityInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<DbContext>().ImplementedBy<GRSDataBaseEntities>().LifestylePerWebRequest());
            container.Register(Component.For(typeof(IRepository<>)).ImplementedBy(typeof(RepositoryBase<>)).LifeStyle.PerWebRequest);

            container.Register(Component.For<ISessionHelper>().ImplementedBy<SessionHelper>().LifestylePerWebRequest());

            container.Register(Component.For(typeof(ISerializer<>)).ImplementedBy(typeof(Serializer<>)));

            container.Register(Component.For(typeof(IManageUserProvider)).ImplementedBy<ManageUserProvider>().LifestylePerWebRequest());

            container.Register(Component.For(typeof(IManageTestCycleProvider)).ImplementedBy<ManageTestCycleProvider>().LifestylePerWebRequest());

            container.Register(Component.For(typeof(IManageReportingProvider)).ImplementedBy<ManageReportingProvider>().LifestylePerWebRequest());

            container.Register(Component.For(typeof(IManageExecutionProvider)).ImplementedBy<ManageExecutionProvider>().LifestylePerWebRequest());

            container.Register(Component.For(typeof(IManageConfigurationProvider)).ImplementedBy<ManageConfigurationProvider>().LifestylePerWebRequest());

            container.Register(Component.For(typeof(IManageLaunchProvider)).ImplementedBy<ManageLaunchProvider>().LifestylePerWebRequest());

            container.Register(Component.For(typeof(IManageHomeProvider)).ImplementedBy<ManageHomeProvider>().LifestylePerWebRequest());

            container.Register(Component.For(typeof(IManageQcProvider)).ImplementedBy<ManageQcProvider>().LifestylePerWebRequest());

            container.Register(Component.For(typeof(IManageExportToExcel)).ImplementedBy<ManageExportToExcelProvider>().LifestylePerWebRequest());

            container.Register(Component.For<IEmailer>().ImplementedBy<Emailer>().LifestylePerWebRequest());

            container.Register(Component.For<IMangaeRelReportProvider>().ImplementedBy<MangaeRelReportProvider>().LifestylePerWebRequest());

            var controllerFactory = new WindsorControllerFactory(container.Kernel);
            ControllerBuilder.Current.SetControllerFactory(controllerFactory);

            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));
        }
    }
}
