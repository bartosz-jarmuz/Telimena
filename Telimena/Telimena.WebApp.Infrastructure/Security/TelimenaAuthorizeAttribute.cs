namespace Telimena.WebApp.Infrastructure.Security
{
    using System.Web.Mvc;
    using System.Web.Routing;
    using Core.Interfaces;

    public class TelimenaAuthorizeAttribute : AuthorizeAttribute
    {
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
                filterContext.Result = new RedirectToRouteResult(new RouteValueDictionary(new { controller = "Error", action = "AccessDenied", roles = this.Roles }));

            }
        }

        protected override bool AuthorizeCore(System.Web.HttpContextBase httpContext)
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



       
    }
}
