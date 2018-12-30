using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Web.Http;
using System.Web.Routing;
using Castle.Core.Internal;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Api.V1;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Security;

namespace Telimena.Tests
{
    [TestFixture]
    public class ApitTests
    {
        [Test]
        public void ValidateControllers()
        {
            var apiControllers = typeof(ProgramsController).Assembly.GetTypes().Where(x => x.IsClass && typeof(ApiController).IsAssignableFrom(x)).ToList();
            var errors = new List<string>();
            foreach (Type controllerType in apiControllers)
            {
                ValidateControllerRouteAttribute(controllerType, errors);
                ValidateActionAttributes(controllerType, errors);
                ValidateControllerAuthorizeAttribute(controllerType, errors);
            }

            if (errors.Any())
            {
                Assert.Fail($"{errors.Count} errors:\r\n" + String.Join("\r\n", errors));
            }
        }

        private static void ValidateControllerRouteAttribute(Type controllerType, List<string> errors)
        {
            var attribute = controllerType.GetAttribute<RoutePrefixAttribute>();
            if (attribute == null)
            {
                errors.Add($"[{controllerType.Name}] missing {nameof(RoutePrefixAttribute)}");
            }
            else
            {
                if (!attribute.Prefix.Contains("/v{version:apiVersion}/"))
                {
                    errors.Add($"[{controllerType.Name}] missing [/v{{version:apiVersion}}/] part of {nameof(RoutePrefixAttribute)}");
                }
            }
        }

        private static void ValidateControllerAuthorizeAttribute(Type controllerType, List<string> errors)
        {
            var attribute = controllerType.GetAttribute<TelimenaApiAuthorizeAttribute>();
            if (attribute == null)
            {
                errors.Add($"[{controllerType.Name}] missing {nameof(TelimenaApiAuthorizeAttribute)}");
            }
        }

        private static void ValidateActionAttributes(Type controllerType, List<string> errors)
        {
            var actions = controllerType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly ).Where(x=>x.MemberType == MemberTypes.Method);
            foreach (MemberInfo action in actions)
            {
                var noAction = action.GetAttribute<NonActionAttribute>();
                if (noAction != null)
                {
                    continue;
                    
                }
                var actionAttributes = action.GetAttributes<Attribute>().Where(x => x is HttpGetAttribute || 
                                                                              x is HttpPostAttribute ||
                                                                              x is HttpDeleteAttribute ||
                                                                              x is HttpPutAttribute || 
                                                                              x is HttpPatchAttribute).ToList();
                if (!actionAttributes.Any() )
                {
                    errors.Add($"Action [{action.Name}] on [{controllerType.Name}] missing an action attribute (HttpGet, HttpPost etc etc). If the attribute is not an action, add a NonAction attribute");
                }

                var routeAttribute = action.GetAttribute<RouteAttribute>();
                if (routeAttribute == null)
                {
                    errors.Add($"Action [{action.Name}] on [{controllerType.Name}] missing [{nameof(RouteAttribute)}]. Explicit routes are required for all actions.");
                }
            }
        
        }
    }
}