using System;
using System.Collections.Generic;
using System.Linq;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models;
using TelimenaClient;

namespace Telimena.WebApp.Controllers.Api.V1
{
    /// <summary>
    /// Class TelemetryQueryResponseCreator.
    /// </summary>
    public static class TelemetryQueryResponseCreator
    {
        private static IEnumerable<IEnumerable<ITelemetryAware>> GetCollections(IEnumerable<TelemetryItemTypes> types, Program program)
        {
            foreach (TelemetryItemTypes telemetryItemType in types)
            {
                switch (telemetryItemType)
                {
                    case TelemetryItemTypes.Event:
                        yield return program.Events;
                        break;
                    case TelemetryItemTypes.View:
                        yield return program.Views;
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        /// <summary>
        /// Creates the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <param name="program">The program.</param>
        /// <returns>TelemetryQueryResponse.</returns>
        public static TelemetryQueryResponse Create(TelemetryQueryRequest request, Program program)
        {

            TelemetryQueryResponse queryResult = new TelemetryQueryResponse();

            IEnumerable<IEnumerable<ITelemetryAware>> collections = GetCollections(request.TelemetryItemTypes, program);

            foreach (IEnumerable<ITelemetryAware> collection in collections)
            {
                foreach (ITelemetryAware telemetryAwareComponent in collection.Where(cmp => request.ComponentKeys.Contains("*") || request.ComponentKeys.Contains(cmp.Name)))
                {
                    ProcessComponents(request, queryResult, telemetryAwareComponent);
                }
            }

            return queryResult;
        }

        private static void ProcessComponents(TelemetryQueryRequest request, TelemetryQueryResponse queryResult, ITelemetryAware telemetryAwareComponent)
        {
            if (queryResult.TelemetryAware == null)
            {
                queryResult.TelemetryAware = new List<TelemetryAwareComponentDto>();
            }
            TelemetryAwareComponentDto componentDto = new TelemetryAwareComponentDto(telemetryAwareComponent, request.PropertiesToInclude);
            queryResult.TelemetryAware.Add(componentDto);

            if (request.Granularity >= TelemetryRequestGranularity.Summaries)
            {
                foreach (TelemetrySummary telemetrySummary in telemetryAwareComponent.GetTelemetrySummaries())
                {
                    ProcessSummaries(request, telemetrySummary, componentDto);
                }
            }
        }

        private static void ProcessSummaries(TelemetryQueryRequest request, TelemetrySummary telemetrySummary, TelemetryAwareComponentDto componentDto)
        {
            if (componentDto.Summaries == null)
            {
                componentDto.Summaries = new List<TelemetrySummaryDto>();
            }

            TelemetrySummaryDto summaryDto = new TelemetrySummaryDto(telemetrySummary, request.PropertiesToInclude);
            componentDto.Summaries.Add(summaryDto);

            if (request.Granularity >= TelemetryRequestGranularity.Details)
            {
                foreach (TelemetryDetail telemetryDetail in telemetrySummary.GetTelemetryDetails())
                {
                    ProcessDetails(request, telemetryDetail, summaryDto);
                }
            }

        }

        private static void ProcessDetails(TelemetryQueryRequest request, TelemetryDetail telemetryDetail, TelemetrySummaryDto summaryDto)
        {
            if (summaryDto.Details == null)
            {
                summaryDto.Details = new List<TelemetryDetailDto>();
            }
            TelemetryDetailDto detailDto = new TelemetryDetailDto(telemetryDetail, request.PropertiesToInclude);
            summaryDto.Details.Add(detailDto);

            if (request.Granularity >= TelemetryRequestGranularity.Units)
            {
                if (detailDto.Units == null)
                {
                    detailDto.Units = new List<TelemetryUnitDto>();
                }
                foreach (TelemetryUnit telemetryUnit in telemetryDetail.GetTelemetryUnits())
                {
                    TelemetryUnitDto unit = new TelemetryUnitDto(telemetryUnit, request.PropertiesToInclude);
                    detailDto.Units.Add(unit);
                }
            }
        }
    }
}