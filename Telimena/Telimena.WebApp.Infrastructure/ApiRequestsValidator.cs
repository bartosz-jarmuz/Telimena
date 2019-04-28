using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Telimena.Portal.Utils;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Messages;

namespace Telimena.WebApp.Infrastructure
{
    public static class ApiRequestsValidator
    {
        public static bool IsRequestValid(TelemetryInitializeRequest request)
        {
            return request != null;
        }

        public static bool IsRequestValid(CreateUpdatePackageRequest request, out List<string> errorMessages)
        {
            bool valid = false;
            errorMessages = new List<string>();
            if (request != null && request.TelemetryKey != Guid.Empty)
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

                if (!request.Name.IsUrlFriendly())
                {
                    var properName = request.Name.MakeUrlFriendly();
                    var properNameHint = "";
                    if (properName != null)
                    {
                        properNameHint = $" Proposed name: {properName}";
                    }
                    errorMessages.Add(
                        $"Program name should only contain letters and numbers or hyphens. Also it needs to begin and end with a letter or digit.{properNameHint}");
                }
                if (request.TelemetryKey == Guid.Empty)
                {
                    errorMessages.Add("Invalid telemetry key!");
                }
                if (string.IsNullOrWhiteSpace(request.PrimaryAssemblyFileName))
                {
                    errorMessages.Add("Primary assembly name is empty!");
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(Path.GetExtension(request.PrimaryAssemblyFileName)))
                    {
                        errorMessages.Add("Primary assembly name is missing extension!");
                    }
                }
            }

            return !errorMessages.Any();
        }
    }
}