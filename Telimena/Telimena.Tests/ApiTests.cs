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
    public class ApiTests
    {
        [Test]
        public void ValidateControllers()
        {
            var apiControllers = typeof(ProgramsController).Assembly.GetTypes().Where(x => x.IsClass && typeof(ApiController).IsAssignableFrom(x)).ToList();
            var errors = new List<string>();
            foreach (Type controllerType in apiControllers)
            {
                ValidateControllerRouteAttribute(controllerType, errors);
                ValidateRouteHelpers(controllerType, errors);
                ValidateActionAttributes(controllerType, errors);
                ValidateControllerAuthorizeAttribute(controllerType, errors);
            }

            if (errors.Any())
            {
                Assert.Fail($"{errors.Count} errors:\r\n" + String.Join("\r\n", errors));
            }
        }

        private static void ValidateRouteHelpers(Type controllerType, List<string> errors)
        {
            var helpersType = controllerType.GetNestedType("Routes");
            if (helpersType != null)
            {
                foreach (FieldInfo field in helpersType.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy).Where(fi => fi.IsLiteral && !fi.IsInitOnly))
                {
                    var actions = controllerType.GetMembers(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)
                        .Where(x => x.MemberType == MemberTypes.Method && x.Name == field.Name).ToList();
                    if (actions.Count == 0)
                    {
                        errors.Add($"[{controllerType.Name}] contains a route for [{field.Name}], but does not contain a corresponding action");
                    }
                    else if (actions.Count > 1)
                    {
                        errors.Add($"[{controllerType.Name}] contains a route for [{field.Name}], but contains more than 1 action with this name");
                    }
                    else
                    {
                        if ((string) field.GetValue(null) != controllerType.Name + "." + actions[0].Name)
                        {
                            errors.Add($"[{controllerType.Name}] contains a route for [{field.Name}], but contains more than 1 action with this name");
                        }
                    }
                }
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
                else
                {
                    VerifyRouteName(routeAttribute, controllerType, action, errors);
                }
            }
        }

        private static void VerifyRouteName(RouteAttribute routeAttribute, Type controllerType, MemberInfo action, List<string> errors)
        {
            if (string.IsNullOrWhiteSpace(routeAttribute.Name))
            {
                errors.Add($"Action [{action.Name}] on [{controllerType.Name}] has empty name on [{nameof(RouteAttribute)}] with route [{routeAttribute.Template}]");
            }
            else if (routeAttribute.Name != controllerType.Name + "." + action.Name)
            {
                errors.Add($"Action [{action.Name}] on [{controllerType.Name}] has incorrect name on [{nameof(RouteAttribute)}] with route [{routeAttribute.Template}]. " +
                           $"Expected name = [{controllerType.Name + "." + action.Name}]. Actual name: [{routeAttribute.Name}]");
            }
        }
    }
}