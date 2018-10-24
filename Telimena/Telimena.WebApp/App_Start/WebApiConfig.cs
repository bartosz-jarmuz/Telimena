using System.Web.Http;
using MvcAuditLogger;
using Newtonsoft.Json;

namespace Telimena.WebApp
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Web API routes
            config.MapHttpAttributeRoutes();
            config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};
            config.Routes.MapHttpRoute(name: "DefaultApi", routeTemplate: "api/{controller}/{action}/{id}", defaults: new {id = RouteParameter.Optional});
            config.Filters.Add(new ApiAuditFilter());

        }
    }
}