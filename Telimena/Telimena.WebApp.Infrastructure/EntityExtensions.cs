using System;
using System.Collections.Generic;
using System.Linq;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using TelimenaClient;

namespace Telimena.WebApp.Infrastructure
{
    public static class EntityExtensions
    {
        public static IEnumerable<TelemetryDetail> GetTelemetryDetails(this Program program, TelimenaContext context, TelemetryItemTypes type)
        {
            switch (type)
            {
                case TelemetryItemTypes.View:
                    return context.ViewTelemetryDetails.Where(x => x.TelemetrySummary.View.ProgramId == program.Id);
                case TelemetryItemTypes.Event:
                    return context.EventTelemetryDetails.Where(x => x.TelemetrySummary.Event.ProgramId == program.Id);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }

        
    }
}