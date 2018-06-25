using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    }
}
