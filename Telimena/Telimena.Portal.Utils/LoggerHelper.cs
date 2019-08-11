using System;
using System.Collections.Generic;
using System.Text;
using System.Web.Http.Controllers;

namespace Telimena.Portal.Utils
{
    public static class LoggerHelper
    {
        public static string FormatMessage(HttpRequestContext requestContext, string message)
        {
            string user = "Unspecified";
            try
            {

                user = requestContext.Principal.Identity.IsAuthenticated
                    ? requestContext.Principal.Identity.Name
                    : "Anonymous";
            }
            catch
            {
                // logs formatting should never fail
            }

            return $"[{user}] - {message}";
        }

     
    }
}
