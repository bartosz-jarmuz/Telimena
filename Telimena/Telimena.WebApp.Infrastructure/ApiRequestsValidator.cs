using System;
using System.Collections.Generic;

namespace Telimena.WebApp.Infrastructure
{
    using Client;
    using Core.Messages;

    public static class ApiRequestsValidator
    {
        public static bool IsRequestValid(RegistrationRequest request)
        {
            return request != null;
        }

        public static bool IsRequestValid(StatisticsUpdateRequest request)
        {
            return request != null;
        }

        public static bool IsRequestValid(SetLatestVersionRequest request)
        {
            return request != null;
        }


        public static bool IsRequestValid(CreateUpdatePackageRequest request, out List<string> errorMessages)
        {
            errorMessages = new List<string>();
            if (request != null && request.ProgramId > 0 && !string.IsNullOrEmpty(request.PackageVersion) )
            {
                if (Version.TryParse(request.PackageVersion, out Version _))
                {
                    return true;
                }
                else
                {
                    errorMessages.Add($"String [{request.PackageVersion}] is not a valid version format!");
                }
            }
            else
            {
                errorMessages.Add($"Required request property is null!");
            }

            return false;
        }
    }
}
