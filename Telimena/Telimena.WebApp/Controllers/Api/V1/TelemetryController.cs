using System;
using System.Threading.Tasks;
using System.Web.Http;
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
    public class TelemetryController : ApiController
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
        [HttpPost]
        [AllowAnonymous]
        [Route("event")]
        public Task<TelemetryUpdateResponse> Event(TelemetryUpdateRequest request)
        {
            return TelemetryControllerHelpers.InsertData(this.work, request,
                () => this.Request.GetClientIp(),
                (name, prg) => TelemetryControllerHelpers.GetEventOrAddIfMissing(this.work, name, prg));

        }

        /// <summary>
        /// Report a program view access
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [Route("view")]
        public Task<TelemetryUpdateResponse> View(TelemetryUpdateRequest request)
        {
            return TelemetryControllerHelpers.InsertData(this.work, request, 
                ()=> this.Request.GetClientIp(),  
                (name, prg) => TelemetryControllerHelpers.GetViewOrAddIfMissing(this.work, name, prg));
        }

        /// <summary>
        /// Initialization of telemetry
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [AllowAnonymous]
        [HttpPost]
        [ Route("initialize")]
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