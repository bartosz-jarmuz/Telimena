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
        /// Report a program view access
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost, Route("", Name = Routes.Post)]
        public async Task<IHttpActionResult> Post(TelemetryUpdateRequest request)
        {
            var ip = this.Request.GetClientIp();

            if (request.DebugMode)
            {
                TelemetryUpdateResponse result = await TelemetryControllerHelpers.InsertData(this.work, request, ip);
                if (result.Exception == null)
                {
                    return this.Ok(result);
                }
                else
                {
                    return this.InternalServerError(result.Exception);
                }
            }
            else
            {
                BackgroundJob.Enqueue(() => TelemetryControllerHelpers.InsertData(this.work, request, ip));
                return await Task.FromResult(this.StatusCode(HttpStatusCode.Accepted));
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

                TelemetryControllerHelpers.SetPrimaryAssembly(actionItems.program, request);

                await TelemetryControllerHelpers.RecordVersions(this.work, actionItems.program, request);

                TelemetryControllerHelpers.GetAssemblyVersionInfoOrAddIfMissing(request.ProgramInfo.PrimaryAssembly.VersionData, actionItems.program);


                await this.work.CompleteAsync();
                TelemetryInitializeResponse response = new TelemetryInitializeResponse()
                {
                    UserId = clientAppUser.Guid,
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