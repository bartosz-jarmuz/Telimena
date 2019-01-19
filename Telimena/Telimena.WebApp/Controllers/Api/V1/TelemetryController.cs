using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using DotNetLittleHelpers;
using Hangfire;
using Telimena.WebApp.Controllers.Api.V1.Helpers;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using TelimenaClient;

namespace Telimena.WebApp.Controllers.Api.V1
{
    /// <summary>
    ///     Controls the telemetry process
    /// </summary>
    [TelimenaApiAuthorize(Roles = TelimenaRoles.Developer)]
    [RoutePrefix("api/v{version:apiVersion}/telemetry")]
    public partial class TelemetryController : ApiController
    {
        /// <summary>
        ///     New instance
        /// </summary>
        /// <param name="work"></param>
        public TelemetryController(ITelemetryUnitOfWork work)
        {
            this.work = work;
        }

        private readonly ITelemetryUnitOfWork work;

        /// <summary>
        ///     Executes the query to get telemetry data as specified in the request object parameters
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous] //todo - this should not be anon
        [HttpPost,Route("execute-query", Name = Routes.ExecuteQuery)]
        public async Task<IHttpActionResult> ExecuteQuery(TelemetryQueryRequest request)
        {
            if (request.TelemetryKey == Guid.Empty)
            {
                return this.BadRequest("Empty telemetry key");
            }


            Program prg = await this.work.Programs.FirstOrDefaultAsync(x => x.TelemetryKey == request.TelemetryKey);
            if (prg == null)
            {
                return this.BadRequest($"Program with key [{request.TelemetryKey}] not found");
            }

            TelemetryQueryResponse queryResult = new TelemetryQueryResponse();

            LoadData(request, prg, queryResult);

            return this.Ok(queryResult);
        }

        /// <summary>
        ///     Initialization of telemetry
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost, Route("initialize", Name = Routes.Initialize)]
        public async Task<TelemetryInitializeResponse> Initialize(TelemetryInitializeRequest request)
        {
            try
            {
                (bool isRequestValid, TelemetryInitializeResponse response, Program program) actionItems =
                    await TelemetryControllerHelpers.GetTelemetryInitializeActionItems(this.work, request);
                if (!actionItems.isRequestValid)
                {
                    return actionItems.response;
                }

                string ip = this.Request.GetClientIp();
                ClientAppUser clientAppUser = await TelemetryControllerHelpers.GetUserOrAddIfMissing(this.work, request.UserInfo, ip);

                TelemetryControllerHelpers.SetPrimaryAssembly(actionItems.program, request);

                await TelemetryControllerHelpers.RecordVersions(this.work, actionItems.program, request);

                TelemetryControllerHelpers.GetAssemblyVersionInfoOrAddIfMissing(request.ProgramInfo.PrimaryAssembly.VersionData, actionItems.program);


                await this.work.CompleteAsync();
                TelemetryInitializeResponse response = new TelemetryInitializeResponse {UserId = clientAppUser.Guid};
                return response;
            }
            catch (Exception ex)
            {
                return new TelemetryInitializeResponse {Exception = new InvalidOperationException("Error while processing telemetry initialize request", ex)};
            }
        }

        /// <summary>
        ///     For Hangfire to support the job creation
        /// </summary>
        /// <param name="request"></param>
        /// <param name="ip"></param>
        /// <returns></returns>
        [NonAction]
        public Task InsertDataInternal(TelemetryUpdateRequest request, string ip)
        {
            
            return TelemetryControllerHelpers.InsertData(this.work, request, ip);
        }

        /// <summary>
        ///     Report a program view access
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("", Name = Routes.Post)]
        public async Task<IHttpActionResult> Post(TelemetryUpdateRequest request)
        {
            if (request.TelemetryKey == Guid.Empty)
            {
                return this.BadRequest("Empty telemetry key");
            }

            if (request.SerializedTelemetryUnits.IsNullOrEmpty())
            {
                return this.BadRequest("Missing telemetry units");
            }

            string ip = this.Request.GetClientIp();
            BackgroundJob.Enqueue(() => this.InsertDataInternal(request, ip));
            return await Task.FromResult(this.StatusCode(HttpStatusCode.Accepted));
        }

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


        private static void LoadData(TelemetryQueryRequest request, Program program, TelemetryQueryResponse queryResult)
        {
            IEnumerable<IEnumerable<ITelemetryAware>> collections = GetCollections(request.TelemetryItemTypes, program);

            foreach (IEnumerable<ITelemetryAware> collection in collections)
            {
                foreach (ITelemetryAware telemetryAwareComponent in collection.Where(cmp => request.ComponentKeys.Contains("*") || request.ComponentKeys.Contains(cmp.Name)))
                {
                    ProcessComponents(request, queryResult, telemetryAwareComponent);
                }
            }
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