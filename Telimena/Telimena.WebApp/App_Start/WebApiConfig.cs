using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Http.Description;
using System.Web.Http.Routing;
using Microsoft.Web.Http.Routing;
using Microsoft.Web.Http.Versioning.Conventions;
using MvcAuditLogger;
using Newtonsoft.Json;
using Swashbuckle.Application;
using Swashbuckle.Swagger;

namespace Telimena.WebApp
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            // Web API configuration and services
            // Web API routes
            config.Formatters.JsonFormatter.SerializerSettings = new JsonSerializerSettings {NullValueHandling = NullValueHandling.Ignore};
            var constraintResolver = new DefaultInlineConstraintResolver()
            {
                ConstraintMap =
                {
                    ["apiVersion"] = typeof( ApiVersionRouteConstraint )
                }
            };
            config.AddApiVersioning(opt =>
            {
                opt.Conventions.Add(new VersionByNamespaceConvention());
            });
            config.MapHttpAttributeRoutes(constraintResolver);

            var apiExplorer = config.AddVersionedApiExplorer(
         options =>
         {
             options.GroupNameFormat = "'v'VVV";

                    // note: this option is only necessary when versioning by url segment. the SubstitutionFormat
                    // can also be used to control the format of the API version in route templates
                    options.SubstituteApiVersionInUrl = true;
             
         });
            config.EnableSwagger(
                            "{apiVersion}/swagger",
                            swagger =>
                            {
                                // build a swagger document and endpoint for each discovered API version
                                swagger.MultipleApiVersions(
                                    (apiDescription, version) => apiDescription.GetGroupName() == version,
                                    info =>
                                    {
                                        foreach (var group in apiExplorer.ApiDescriptions)
                                        {
                                            var description = "Telimena's (almost) RESTful API. Majority of the endpoints try to adhere to the REST design principles, with the exception of the API version in the URL. " +
                                                              "There are also some RPC-style calls where more natural than REST.";

                                            if (group.IsDeprecated)
                                            {
                                                description += " This API version has been deprecated.";
                                            }

                                            info.Version(group.Name, $"Telimena API {group.ApiVersion}")
                                                .Contact(c => c.Name("Bartosz Jarmuż").Email("bartosz.jarmuz@gmail.com")).Description(description);
                                        }
                                    });

                                // add a custom operation filter which sets default values
                                swagger.OperationFilter<SwaggerDefaultValues>();

                                // integrate xml comments
                                swagger.IncludeXmlComments(XmlCommentsFilePath);
                            })
                         .EnableSwaggerUi(swagger => swagger.EnableDiscoveryUrlSelector());


            config.Routes.MapHttpRoute(name: "DefaultApi", routeTemplate: "api/v{version:apiVersion}/{controller}/{id}", defaults: new { id = RouteParameter.Optional });
            config.Routes.MapHttpRoute(name: "Api2", routeTemplate: "api/{controller}");
            config.Filters.Add(new ApiAuditFilter());
          

        }

        /// <summary>
        /// Get the root content path.
        /// </summary>
        /// <value>The root content path of the application.</value>
        public static string ContentRootPath
        {
            get
            {
                var app = AppDomain.CurrentDomain;

                if (string.IsNullOrEmpty(app.RelativeSearchPath))
                {
                    return app.BaseDirectory;
                }

                return app.RelativeSearchPath;
            }
        }

        static string XmlCommentsFilePath
        {
            get
            {
                var fileName = typeof(WebApiConfig).GetTypeInfo().Assembly.GetName().Name + ".xml";
                return Path.Combine(ContentRootPath, fileName);
            }
        }
    }

    /// <summary>
    /// Represents the Swagger/Swashbuckle operation filter used to provide default values.
    /// </summary>
    /// <remarks>This <see cref="IOperationFilter"/> is only required due to bugs in the <see cref="SwaggerGenerator"/>.
    /// Once they are fixed and published, this class can be removed.</remarks>
    public class SwaggerDefaultValues : IOperationFilter
    {
        /// <summary>
        /// Applies the filter to the specified operation using the given context.
        /// </summary>
        /// <param name="operation">The operation to apply the filter to.</param>
        /// <param name="schemaRegistry">The API schema registry.</param>
        /// <param name="apiDescription">The API description being filtered.</param>
        public void Apply(Operation operation, SchemaRegistry schemaRegistry, ApiDescription apiDescription)
        {
            if (operation.parameters == null)
            {
                return;
            }

            foreach (var parameter in operation.parameters)
            {
                var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.name);

                // REF: https://github.com/domaindrivendev/Swashbuckle/issues/1101
                if (parameter.description == null)
                {
                    parameter.description = description.Documentation;
                }

                // REF: https://github.com/domaindrivendev/Swashbuckle/issues/1089
                // REF: https://github.com/domaindrivendev/Swashbuckle/pull/1090
                if (parameter.@default == null)
                {
                    parameter.@default = description.ParameterDescriptor?.DefaultValue;
                }
            }
        }
    }
}