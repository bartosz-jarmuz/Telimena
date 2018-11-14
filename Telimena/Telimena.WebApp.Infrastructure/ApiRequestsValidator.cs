using System;
using System.Collections.Generic;
using System.Linq;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.Messages;
using TelimenaClient;

namespace Telimena.WebApp.Infrastructure
{
    public static class ApiRequestsValidator
    {
        public static bool IsRequestValid(TelemetryInitializeRequest request)
        {
            return request != null;
        }

        public static bool IsRequestValid(TelemetryUpdateRequest request)
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

        public static bool IsRequestValid(RegisterProgramRequest request, out List<string> errorMessages)
        {
            errorMessages = new List<string>();
            if (request == null)
            {
                errorMessages.Add("Request is null!");
            }
            else
            {
                if (string.IsNullOrWhiteSpace(request.Name))
                {
                    errorMessages.Add("Program name is missing!");
                }
            }

            return !errorMessages.Any();
        }
    }
}