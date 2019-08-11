using System.Web;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using DataTables.AspNet.Mvc5;
using Hangfire;
using log4net;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Security;
using MvcAuditLogger;
using Ninject;
using Ninject.Extensions.Conventions;
using Ninject.Modules;
using Ninject.Web.Common;
using Ninject.Web.Common.WebHost;
using Ninject.Web.Mvc.FilterBindingSyntax;
using Ninject.Web.WebApi;
using Telimena.WebApp;
using Telimena.WebApp.Controllers.Api;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Repository.Implementation;
using GlobalConfiguration = System.Web.Http.GlobalConfiguration;


namespace Telimena.WebApi
{
    /// <summary>
    /// Class MvcApplication.
    /// </summary>
    /// <seealso cref="Ninject.Web.Common.WebHost.NinjectHttpApplication" />
    public class MvcApplication : NinjectHttpApplication
    {
        /// <summary>
        /// Creates the kernel that will manage your application.
        /// </summary>
        /// <returns>The created kernel.</returns>
        protected override IKernel CreateKernel()
        {
            StandardKernel kernel = new StandardKernel();
            kernel.Load(new ServiceModule());
            kernel.Bind(x => x.FromAssembliesMatching("Telimena.*.dll").SelectAllClasses().Excluding(typeof(TelimenaUserManager)).BindDefaultInterface());
            kernel.BindFilter<AuditFilter>(FilterScope.Action, null).WhenActionMethodHas<AuditAttribute>();
            kernel.Bind<IAuditLogger>().To<DebugAuditLogger>();
            GlobalConfiguration.Configuration.DependencyResolver = new NinjectDependencyResolver(kernel);
            Hangfire.GlobalConfiguration.Configuration.UseNinjectActivator(kernel);
            return kernel;
        }
        /// <summary>
        /// Called when the application is started.
        /// </summary>
        protected override void OnApplicationStarted()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);
            AutoMapperConfiguration.Configure();
            Configuration.RegisterDataTables();
        }

    }

    internal class ServiceModule : NinjectModule
    {
        public override void Load()
        {
            this.Bind<TelimenaPortalContext>().ToSelf().InRequestScope();
            this.Bind<TelimenaTelemetryContext>().ToSelf().InRequestScope();
            this.Bind<IFileSaver>().To<AzureFileSaver>();
            this.Bind<IFileRemover>().To<AzureFileRemover>();
            this.Bind<IFileRetriever>().To<AzureFileRetriever>();
            this.Bind<ILog>().ToMethod(context => LogManager.GetLogger(context.Request.Target.Member.ReflectedType));
            this.Bind<IAuthenticationManager>().ToMethod(c => HttpContext.Current.GetOwinContext().Authentication).InRequestScope();
            this.Bind<ITelimenaUserManager>().ToMethod(c => HttpContext.Current.GetOwinContext().GetUserManager<TelimenaUserManager>()).InRequestScope();
        }
    }
}