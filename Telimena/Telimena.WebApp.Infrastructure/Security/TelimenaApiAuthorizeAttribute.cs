using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;

namespace Telimena.WebApp.Infrastructure.Security
{
    public class TelimenaApiAuthorizeAttribute : System.Web.Http.AuthorizeAttribute
    {
        protected override void HandleUnauthorizedRequest(HttpActionContext actionContext)
        {
            actionContext.Response = new System.Net.Http.HttpResponseMessage(System.Net.HttpStatusCode.Unauthorized);
        }

        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            if (!actionContext.RequestContext.Principal.Identity.IsAuthenticated)
            {
                return false;
            }


            if (actionContext.RequestContext.Principal.IsInRole(Telimena.WebApp.Core.Interfaces.TelimenaRoles.Admin))
            {
                return true;
            }

            if (string.IsNullOrEmpty(this.Roles))
            {
                return true;
            }

            foreach (string role in this.Roles.Split(','))
            {
                if (actionContext.RequestContext.Principal.IsInRole(role))
                {
                    return true;
                }
            }

            return false;
        }

        public override void OnAuthorization(HttpActionContext actionContext)
        {
            base.OnAuthorization(actionContext);
        }

        public override Task OnAuthorizationAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            return base.OnAuthorizationAsync(actionContext, cancellationToken);
        }
    }
}