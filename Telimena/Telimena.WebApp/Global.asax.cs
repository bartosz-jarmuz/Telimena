using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace Telimena.WebApi
{
    using log4net;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin.Security;
    using Ninject;
    using Ninject.Modules;
    using Ninject.Web.Common;
    using Ninject.Web.Common.WebHost;
    using Ninject.Web.WebApi;
    using WebApp;
    using WebApp.Core.Interfaces;
    using WebApp.Infrastructure.Database;
    using WebApp.Infrastructure.Identity;
    using WebApp.Infrastructure.Repository;
    using WebApp.Infrastructure.UnitOfWork;
    using WebApp.Infrastructure.UnitOfWork.Implementation;

    public class MvcApplication : NinjectHttpApplication
    {
        protected override void OnApplicationStarted()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AutoMapperConfiguration.Configure();
            DataTables.AspNet.Mvc5.Configuration.RegisterDataTables();
        }

        protected override IKernel CreateKernel()
        {
            var kernel = new StandardKernel();
            kernel.Load(new ServiceModule());
            GlobalConfiguration.Configuration.DependencyResolver = new NinjectDependencyResolver(kernel);
            return kernel;
        }
    }

    internal class ServiceModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<IStatisticsUnitOfWork>().To<StatisticsUnitOfWork>();
            this.Bind<IAdminDashboardUnitOfWork>().To<AdminDashboardUnitOfWork>();
            this.Bind<IAccountUnitOfWork>().To<AccountUnitOfWork>();
            this.Bind<ILog>().ToMethod(context =>
                LogManager.GetLogger(context.Request.Target.Member.ReflectedType));
            this.Bind<IAuthenticationManager>().ToMethod(c => HttpContext.Current.GetOwinContext().Authentication).InRequestScope();
            this.Bind<ITelimenaUserManager>().ToMethod(c => HttpContext.Current.GetOwinContext().GetUserManager<TelimenaUserManager>()).InRequestScope();
            this.Bind<TelimenaContext>().ToSelf().InRequestScope();
        }
    }

}
