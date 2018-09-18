using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Telimena.WebApp.Core.Interfaces;

namespace Telimena.WebApp.Infrastructure.Security
{
    public class TelimenaAuthorizeAttribute : AuthorizeAttribute
    {
        protected override bool AuthorizeCore(HttpContextBase httpContext)
        {
            if (!httpContext.User.Identity.IsAuthenticated)
            {
                return false;
            }


            if (httpContext.User.IsInRole(TelimenaRoles.Admin))
            {
                return true;
            }

            if (string.IsNullOrEmpty(this.Roles))
            {
                return true;
            }

            foreach (string role in this.Roles.Split(','))
            {
                if (httpContext.User.IsInRole(role))
                {
                    return true;
                }
            }

            return false;
        }

        protected override void HandleUnauthorizedRequest(AuthorizationContext filterContext)
        {
            if (!filterContext.HttpContext.User.Identity.IsAuthenticated)
            {
                //if not logged, it will work as normal Authorize and redirect to the Login
                base.HandleUnauthorizedRequest(filterContext);
            }
            else
            {
                //logged and wihout the role to access it - redirect to the custom controller action
                filterContext.Result =
                    new RedirectToRouteResult(new RouteValueDictionary(new {controller = "Error", action = "AccessDenied", roles = this.Roles}));
            }
        }
    }
}