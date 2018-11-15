using System;
using System.Threading.Tasks;
using System.Web.Http;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using TelimenaClient;

namespace Telimena.WebApp.Controllers.Api
{
    public class TelemetryController : ApiController
    {
        public TelemetryController(ITelemetryUnitOfWork work)
        {
            this.work = work;
        }

        private readonly ITelemetryUnitOfWork work;

        [HttpPost]
        public Task<TelemetryUpdateResponse> Event(TelemetryUpdateRequest request)
        {
            return TelemetryControllerHelpers.PerformUpdate(this.work, request,
                () => this.Request.GetClientIp(),
                (name, prg) => TelemetryControllerHelpers.GetEventOrAddIfMissing(this.work, name, prg));

        }

        [HttpPost]
        public Task<TelemetryUpdateResponse> View(TelemetryUpdateRequest request)
        {
            return TelemetryControllerHelpers.PerformUpdate(this.work, request, 
                ()=> this.Request.GetClientIp(),  
                (name, prg) => TelemetryControllerHelpers.GetViewOrAddIfMissing(this.work, name, prg));
        }

        [HttpPost]
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

                AssemblyVersionInfo versionInfoInfo = TelemetryControllerHelpers.GetAssemblyVersionInfoOrAddIfMissing(request.ProgramInfo.PrimaryAssembly.AssemblyVersion,
                    request.ProgramInfo.PrimaryAssembly.FileVersion, actionItems.program);

                summary.UpdateTelemetry(versionInfoInfo, ip, request.TelemetryData);

                await this.work.CompleteAsync();
                var response = new TelemetryInitializeResponse()
                {

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