using System.Security.Principal;
using Telimena.WebApp.Core.Interfaces;

namespace Telimena.WebApp.Infrastructure.Identity
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
    }
}