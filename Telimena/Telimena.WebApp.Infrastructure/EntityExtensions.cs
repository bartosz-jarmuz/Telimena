using System;
using System.Collections.Generic;
using System.Linq;
using Telimena.WebApp.Core;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;

namespace Telimena.WebApp.Infrastructure
{
    public static class EntityExtensions
    {
        public static IEnumerable<TelemetryDetail> GetTelemetryDetails(this Program program, TelimenaContext context, TelemetryTypes type)
        {
            switch (type)
            {
                case TelemetryTypes.View:
                    return context.ViewTelemetryDetails.Where(x => x.TelemetrySummary.View.ProgramId == program.Id);
                case TelemetryTypes.Event:
                    return context.EventTelemetryDetails.Where(x => x.TelemetrySummary.Event.ProgramId == program.Id);
                case TelemetryTypes.Program:
                    return context.ProgramTelemetryDetails.Where(x => x.TelemetrySummary.ProgramId == program.Id);
                default:
                    throw new ArgumentOutOfRangeException(nameof(type), type, null);
            }
        }
    }
}