using System;
using System.Collections.Generic;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.Messages;
using TelimenaClient;

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

        public static bool IsRequestValid(CreateUpdatePackageRequest request, out List<string> errorMessages)
        {
            bool valid = false;
            errorMessages = new List<string>();
            if (request != null && request.ProgramId > 0)
            {
                return true;
            }
            else
            {
                errorMessages.Add("Required request property is null!");
            }

            return valid;
        }

    }
}