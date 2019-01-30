using System.Web.Mvc;
using System.Web.Routing;

namespace Telimena.WebApp
{
    /// <summary>
    /// Class RouteConfig.
    /// </summary>
    public class RouteConfig
    {
        /// <summary>
        /// Registers the routes.
        /// </summary>
        /// <param name="routes">The routes.</param>
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");
            routes.MapMvcAttributeRoutes();

            routes.MapRoute(name: "Default", url: "{controller}/{action}/{id}"
                , defaults: new {controller = "Home", action = "Index", id = UrlParameter.Optional});

        }
    }
}