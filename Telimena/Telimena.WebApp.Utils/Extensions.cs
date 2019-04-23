using System.Net.Http;
using System.Security.Principal;
using System.Web;
using Telimena.WebApp.Core.Interfaces;

namespace Telimena.WebApp.Infrastructure
{
    public static class Extensions
    {
        public static bool SupportsRole(this IPrincipal user, string roleName)
        {
            if (user.IsInRole(TelimenaRoles.Admin))
            {
                return true;
            }

            return user.IsInRole(roleName);
        }

        public static string GetClientIp(this HttpRequestMessage request)
        {
            if (request?.Properties != null && request.Properties.ContainsKey("MS_HttpContext"))
            {
                if (request.Properties["MS_HttpContext"] is HttpContextWrapper ctxWrp)
                {
                    return ctxWrp.Request.UserHostAddress;
                }
                else if (request.Properties["MS_HttpContext"] is HttpContextBase ctx)
                {
                    return ctx.Request.UserHostAddress;
                }
            }

            return null;
        }
    }
}