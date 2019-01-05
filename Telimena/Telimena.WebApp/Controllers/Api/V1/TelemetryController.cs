using System;
using System.Diagnostics;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Results;
using Hangfire;
using Telimena.WebApp.Controllers.Api.V1.Helpers;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Security;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using TelimenaClient;

namespace Telimena.WebApp.Controllers.Api.V1
{
    /// <summary>
    /// Controls the telemetry process
    /// </summary>
    [TelimenaApiAuthorize(Roles = TelimenaRoles.Viewer)]
    [RoutePrefix("api/v{version:apiVersion}/telemetry")]
    public partial class TelemetryController : ApiController
    {
        /// <summary>
        /// New instance
        /// </summary>
        /// <param name="work"></param>
        public TelemetryController(ITelemetryUnitOfWork work)
        {
            this.work = work;
        }

        private readonly ITelemetryUnitOfWork work;

        /// <summary>
        /// Report new event occurrence
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost, Route("event", Name = Routes.Event)]
        public async Task<IHttpActionResult> Event(TelemetryUpdateRequest request)
        {
            if (request.DebugMode)
            {
                return await this.ReportEvent(request);
            }
            else
            {
                BackgroundJob.Enqueue(() => this.ReportEvent(request));
                return await Task.FromResult(this.StatusCode(HttpStatusCode.Accepted));
            }
        }

        /// <summary>
        /// Enqueued request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [NonAction]
        public async Task<IHttpActionResult> ReportEvent(TelemetryUpdateRequest request)
        {
            var ip = this.Request.GetClientIp();

            var result = await TelemetryControllerHelpers.InsertData(this.work, request, ip
                , (name, prg) => TelemetryControllerHelpers.GetEventOrAddIfMissing(this.work, name, prg));
            if (result.Exception == null)
            {
                return this.Ok(result);
            }
            else
            {
                return this.InternalServerError(result.Exception);
            }
        }


        /// <summary>
        /// Report a program view access
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost, Route("view", Name = Routes.View)]
        public async Task<IHttpActionResult> View(TelemetryUpdateRequest request)
        {
            if (request.DebugMode)
            {
                return await this.ReportView(request);
            }
            else
            {
                BackgroundJob.Enqueue(() => this.ReportView(request));
                return await Task.FromResult(this.StatusCode(HttpStatusCode.Accepted));
            }

        }

        /// <summary>
        /// Enqueued request
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [NonAction]
        public async Task<IHttpActionResult> ReportView(TelemetryUpdateRequest request)
        {
            var ip = this.Request.GetClientIp();

            var result = await TelemetryControllerHelpers.InsertData(this.work, request, ip
                , (name, prg) => TelemetryControllerHelpers.GetViewOrAddIfMissing(this.work, name, prg));
            if (result.Exception == null)
            {
                return this.Ok(result);
            }
            else
            {
                return this.InternalServerError(result.Exception);
            }
        }

        /// <summary>
        /// Initialization of telemetry
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost, Route("initialize", Name = Routes.Initialize)]
        public async Task<TelemetryInitializeResponse> Initialize(TelemetryInitializeRequest request)
        {
            try
            {
                (bool isRequestValid, TelemetryInitializeResponse response, Program program) actionItems = await TelemetryControllerHelpers.GetTelemetryInitializeActionItems(this.work, request);
                if (!actionItems.isRequestValid)
                {
                    return actionItems.response;
                }

                string ip = this.Request.GetClientIp();
                ClientAppUser clientAppUser = await TelemetryControllerHelpers.GetUserOrAddIfMissing(this.work, request.UserInfo, ip);

                TelemetrySummary summary = TelemetryControllerHelpers.GetTelemetrySummary(clientAppUser, actionItems.program);

                TelemetryControllerHelpers.SetPrimaryAssembly(actionItems.program, request);

                await TelemetryControllerHelpers.RecordVersions(this.work, actionItems.program, request);

                AssemblyVersionInfo versionInfoInfo = TelemetryControllerHelpers.GetAssemblyVersionInfoOrAddIfMissing(request.ProgramInfo.PrimaryAssembly.VersionData, actionItems.program);

                summary.UpdateTelemetry(versionInfoInfo, ip, request.TelemetryData);

                await this.work.CompleteAsync();
                var response = new TelemetryInitializeResponse()
                {
                    UserId = clientAppUser.Guid,
                    Count = summary.SummaryCount
                };
                return response;
            }
            catch (Exception ex)
            {
                return new TelemetryInitializeResponse()
                {
                    Exception = new InvalidOperationException("Error while processing telemetry initialize request", ex)
                };
            }
        }

    }
}