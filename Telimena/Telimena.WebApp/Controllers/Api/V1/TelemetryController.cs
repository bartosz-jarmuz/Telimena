using System;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using DotNetLittleHelpers;
using Hangfire;
using Telimena.WebApp.Controllers.Api.V1.Helpers;
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

        
    }
}