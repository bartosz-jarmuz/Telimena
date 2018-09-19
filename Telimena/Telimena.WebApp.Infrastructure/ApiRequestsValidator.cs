using System;
using System.Collections.Generic;
using DotNetLittleHelpers;
using Telimena.Client;
using Telimena.WebApp.Core.Messages;

namespace Telimena.WebApp.Infrastructure
{
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
            bool valid = false;
            errorMessages = new List<string>();
            if (request != null && request.ProgramId > 0 && !string.IsNullOrEmpty(request.PackageVersion) && !string.IsNullOrEmpty(request.ToolkitVersionUsed))
            {
                valid = true;
                if (!Version.TryParse(request.PackageVersion, out Version _) && Version.TryParse(request.PackageVersion, out Version _))
                {
                    errorMessages.Add($"String [{request.PackageVersion}] is not a valid version format!");
                    valid = false;
                }
                if (!Version.TryParse(request.PackageVersion, out Version _) && Version.TryParse(request.PackageVersion, out Version _))
                {
                    errorMessages.Add($"String [{request.ToolkitVersionUsed}] is not a valid version format!");
                    valid = false;
                }

                return valid;
            }
            else
            {
                errorMessages.Add("Required request property is null!");
            }

            return valid;
        }

        public static bool IsRequestValid(CreateProgramPackageRequest request, out List<string> errorMessages)
        {
            errorMessages = new List<string>();
            if (request != null && request.ProgramId > 0 && !string.IsNullOrEmpty(request.ToolkitVersionUsed))
            {
                if (Version.TryParse(request.ToolkitVersionUsed, out Version _))
                {
                    return true;
                }

                errorMessages.Add($"String [{request.ToolkitVersionUsed}] is not a valid version format!");
            }
            else
            {
                errorMessages.Add("Required request property is null!");
            }

            return false;
        }
    }
}