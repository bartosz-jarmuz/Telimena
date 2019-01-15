using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Telimena.WebApp.Core.DTO;
using TelimenaClient;

namespace Telimena.WebApp.Core.Messages
{

    public class TelemetryQueryRequest
    {
        public Guid TelemetryKey { get; set; }
        public List<TelemetryItemTypes> TelemetryItemTypes { get; set; } = new List<TelemetryItemTypes>();
        public TelemetryRequestGranularity Granularity { get; set; }
        public List<string> ComponentKeys { get; set; } = new List<string>();
        public List<string> PropertiesToInclude { get; set; } = new List<string>();


        public static TelemetryQueryRequest CreateFull(Guid telemetryKey)
        {
            TelemetryQueryRequest request = new TelemetryQueryRequest {TelemetryKey = telemetryKey};
            foreach (TelemetryItemTypes type in Enum.GetValues(typeof(TelemetryItemTypes)).Cast<TelemetryItemTypes>())
            {
                request.TelemetryItemTypes.Add(type);
            }
            request.Granularity = Enum.GetValues(typeof(TelemetryRequestGranularity)).Cast<TelemetryRequestGranularity>().Max();
            request.ComponentKeys.Add("*");
            AddProperties(request, typeof(TelemetryAwareComponentDto), typeof(TelemetrySummaryDto), typeof(TelemetryDetailDto), typeof(TelemetryUnitDto));

            return request;
        }

        private static void AddProperties(TelemetryQueryRequest request ,params Type[] types)
        {
            foreach (Type type in types)
            {
                foreach (PropertyInfo propertyInfo in type.GetProperties())
                {
                    request.PropertiesToInclude.Add(propertyInfo.Name);
                }
            }
        }
    }

    
}
