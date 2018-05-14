namespace Telimena.WebApp.Infrastructure.Identity
{
    using System.Security.Principal;
    using Core.Interfaces;

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