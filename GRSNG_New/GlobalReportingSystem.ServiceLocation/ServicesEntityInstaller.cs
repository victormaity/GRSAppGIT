using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.Facilities.WcfIntegration;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
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
    public class ServicesEntityInstaller : IWindsorInstaller
    {
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<DbContext>()
                .ImplementedBy<GRSDataBaseEntities>()
                .LifeStyle.Transient);
            container.Register(
            Component.For(typeof (IRepository<>)).ImplementedBy(typeof (RepositoryBase<>)).LifeStyle.Transient);
            container.AddFacility<WcfFacility>();

            container.Register(Component.For<IEmailer>().ImplementedBy<Emailer>());

            ServiceLocator.SetLocatorProvider(() => new WindsorServiceLocator(container));
        }
    }
}
