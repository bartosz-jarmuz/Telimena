using System;
using System.Collections.Generic;
using System.IdentityModel;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using DotNetLittleHelpers;
using Hangfire;
using Newtonsoft.Json;
using Telimena.WebApp.Controllers.Api.V1.Helpers;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.AppInsightsTelemetryModel;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Telemetry;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using Telimena.WebApp.Utils;
using JsonSerializer = Microsoft.ApplicationInsights.Extensibility.Implementation.JsonSerializer;

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

            TelemetryRootObject prg = await this.work.GetMonitoredProgram(request.TelemetryKey).ConfigureAwait(false);
            if (prg == null)
            {
                return this.BadRequest($"Program with key [{request.TelemetryKey}] not found");
            }


             TelemetryQueryResponse queryResult = TelemetryQueryResponseCreator.Create(request, prg);

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
                if (!ApiRequestsValidator.IsRequestValid(request))
                {
                    return new TelemetryInitializeResponse() { Exception = new BadRequestException("Request is not valid") };
                }

                TelemetryRootObject program = await work.GetMonitoredProgram(request.TelemetryKey).ConfigureAwait(false);
                if (program == null)
                {
                    {
                       return new TelemetryInitializeResponse { Exception = new InvalidOperationException($"Program [{request.TelemetryKey}] is null") };
                    }
                }

                string ip = this.Request.GetClientIp();
                ClientAppUser clientAppUser = await TelemetryControllerHelpers.GetUserOrAddIfMissing(this.work, request.UserInfo, ip).ConfigureAwait(false);

                await this.work.CompleteAsync().ConfigureAwait(false);
                TelemetryInitializeResponse response = new TelemetryInitializeResponse {UserId = clientAppUser.PublicId};
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
        /// <param name="program"></param>
        /// <param name="ip"></param>
        /// <param name="items"></param>
        /// <returns></returns>
        [NonAction]
        public Task InsertDataInternal(IEnumerable<TelemetryItem> items, TelemetryRootObject program, string ip)
        {
            return TelemetryControllerHelpers.InsertData(this.work, items.ToList(), program, ip);
        }

        /// <summary>
        ///     Report a program view access
        /// </summary>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("{telemetryKey}", Name = Routes.Post)]
        public async Task<IHttpActionResult> Post(Guid telemetryKey)
        {
            TelemetryRootObject program = await this.work.GetMonitoredProgram(telemetryKey).ConfigureAwait(false);
            if (program == null)
            {
                throw new InvalidOperationException($"Program with telemetry key [{telemetryKey}] does not exist");
            }

            IEnumerable<AppInsightsTelemetry> appInsightsTelemetries = AppInsightsDeserializer.Deserialize(await this.Request.Content.ReadAsByteArrayAsync().ConfigureAwait(false), true);

            IEnumerable<TelemetryItem> telemetryItems = AppInsightsTelemetryMapper.Map(appInsightsTelemetries);

            string ip = this.Request.GetClientIp();

            await this.InsertDataInternal(telemetryItems, program, ip).ConfigureAwait(false);

            return await Task.FromResult(this.StatusCode(HttpStatusCode.Accepted)).ConfigureAwait(false);

        
        }







    }
}