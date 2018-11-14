using System;
using System.IdentityModel;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using TelimenaClient;

namespace Telimena.WebApp.Controllers.Api
{
    #region Using

    #endregion


    public class TelemetryController : ApiController
    {
        public TelemetryController(ITelemetryUnitOfWork work)
        {
            this.work = work;
            this.helper = new TelemetryHelperService(work);
        }

        private readonly ITelemetryUnitOfWork work;
        private readonly TelemetryHelperService helper;

        [HttpPost]
        public async Task<StatisticsUpdateResponse> Event(StatisticsUpdateRequest request)
        {
            if (!ApiRequestsValidator.IsRequestValid(request))
            {
                return new StatisticsUpdateResponse { Exception = new BadRequestException("Request is not valid") };
            }

            try
            {
                Program program = await this.work.Programs.FirstOrDefaultAsync(x => x.Id == request.ProgramId);
                if (program == null)
                {
                    return new StatisticsUpdateResponse {Exception = new InvalidOperationException($"Program [{request.ProgramId}] is null")};
                }

                ClientAppUser clientAppUser = this.work.ClientAppUsers.GetById(request.UserId);
                if (clientAppUser == null)
                {
                    return new StatisticsUpdateResponse {Exception = new InvalidOperationException($"User [{request.UserId}] is null")};
                }

                Event trackedComponent = await this.helper.GetEventOrAddIfNotExists(request.ComponentName, program);

                TelemetrySummary summary = this.GetTelemetry(clientAppUser, trackedComponent);

                program.PrimaryAssembly.AddVersion(request.AssemblyVersion, request.FileVersion);
                AssemblyVersionInfo versionInfoInfo = program.PrimaryAssembly.GetVersion(request.AssemblyVersion, request.FileVersion);

                var ip = this.Request.GetClientIp();
                summary.IncrementUsage(versionInfoInfo, ip, request.TelemetryData);

                await this.work.CompleteAsync();
                return PrepareResponse(request, summary, program, clientAppUser);
            }
            catch (Exception ex)
            {
                return new StatisticsUpdateResponse { Exception = new InvalidOperationException("Error while processing statistics update request", ex) };
            }

        }
        private static StatisticsUpdateResponse PrepareResponse(StatisticsUpdateRequest updateRequest, TelemetrySummary usageSummary, Program program
            , ClientAppUser clientAppUser)
        {

            StatisticsUpdateResponse response = new StatisticsUpdateResponse
            {
                Count = usageSummary.SummaryCount,
                ProgramId = program.Id,
                UserId = clientAppUser.Id,
                ComponentName = updateRequest.ComponentName
            };
            if (usageSummary is ViewTelemetrySummary summary)
            {
                response.ComponentId = summary.ViewId;
            }

            return response;
        }

        private TelemetrySummary GetTelemetry(ClientAppUser clientAppUser, ProgramComponent component)
        {
            var usageSummary = component.GetTelemetrySummary(clientAppUser.Id);
            if (usageSummary == null)
            {
                component.AddTelemetrySummary(clientAppUser.Id);
            }

            return usageSummary;
        }

    }

    public class StatisticsController : ApiController
    {
        public StatisticsController(ITelemetryUnitOfWork work)
        {
            this.work = work;
            this.helper = new TelemetryHelperService(work);
        }

        private readonly ITelemetryUnitOfWork work;
        private readonly TelemetryHelperService helper;

        [HttpPost]
        public async Task<RegistrationResponse> RegisterClient(RegistrationRequest request)
        {
            if (!ApiRequestsValidator.IsRequestValid(request))
            {
                return new RegistrationResponse {Exception = new BadRequestException("Request is not valid")};
            }

            try
            {
                Program program = await this.helper.GetProgramOrAddIfNotExists(request);
                var ip = this.Request.GetClientIp();
                ClientAppUser clientAppUser = await this.helper.GetUserInfoOrAddIfNotExists(request.UserInfo, ip);
                TelemetrySummary usageSummary = await this.GetUsageData(program, clientAppUser);
                if (!request.SkipUsageIncrementation)
                {
                    AssemblyVersionInfo versionInfo = program.PrimaryAssembly.GetVersion(request.ProgramInfo.PrimaryAssembly.Version, request.ProgramInfo.PrimaryAssembly.FileVersion);
                    usageSummary.IncrementUsage(versionInfo, ip);
                }

                await this.work.CompleteAsync();
                return new RegistrationResponse {Count = usageSummary.SummaryCount, ProgramId = program.Id, UserId = clientAppUser.Id};
            }
            catch (Exception ex)
            {
                return new RegistrationResponse {Exception = new InvalidOperationException("Error while processing registration request", ex)};
            }
        }

        [HttpPost]
        public async Task<StatisticsUpdateResponse> Update(StatisticsUpdateRequest updateRequest)
        {
            if (!ApiRequestsValidator.IsRequestValid(updateRequest))
            {
                return new StatisticsUpdateResponse {Exception = new BadRequestException("Request is not valid")};
            }

            try
            {
                Program program = await this.work.Programs.FirstOrDefaultAsync(x=>x.Id == updateRequest.ProgramId);
                if (program == null)
                {
                    return new StatisticsUpdateResponse {Exception = new InvalidOperationException($"Program [{updateRequest.ProgramId}] is null")};
                }

                ClientAppUser clientAppUser = this.work.ClientAppUsers.GetById(updateRequest.UserId);
                if (clientAppUser == null)
                {
                    return new StatisticsUpdateResponse {Exception = new InvalidOperationException($"User [{updateRequest.UserId}] is null")};
                }

                var usageSummary = await this.GetUsageData(program, clientAppUser, updateRequest.ComponentName);
                program.PrimaryAssembly.AddVersion(updateRequest.AssemblyVersion, updateRequest.FileVersion);
                AssemblyVersionInfo versionInfoInfo = program.PrimaryAssembly.GetVersion(updateRequest.AssemblyVersion, updateRequest.FileVersion);

                var ip = this.Request.GetClientIp();
                usageSummary.IncrementUsage(versionInfoInfo, ip, updateRequest.TelemetryData);

                await this.work.CompleteAsync();
                return PrepareResponse(updateRequest, (usageSummary as TelemetrySummary), program, clientAppUser);
            }
            catch (Exception ex)
            {
#if DEBUG
                throw;
#endif
                return new StatisticsUpdateResponse {Exception = new InvalidOperationException("Error while processing statistics update request", ex)};
            }
        }

        private static StatisticsUpdateResponse PrepareResponse(StatisticsUpdateRequest updateRequest, TelemetrySummary usageSummary, Program program
            , ClientAppUser clientAppUser)
        {

            StatisticsUpdateResponse response = new StatisticsUpdateResponse
            {
                Count = usageSummary.SummaryCount, ProgramId = program.Id, UserId = clientAppUser.Id, ComponentName = updateRequest.ComponentName
            };
            if (usageSummary is ViewTelemetrySummary summary)
            {
                response.ComponentId = summary.ViewId;
            }

            return response;
        }



        private async Task<TelemetrySummary> GetViewUsageData(Program program, ClientAppUser clientAppUser, string viewName)
        {
            View view = await this.helper.GetViewOrAddIfNotExists(viewName, program);
            var  usageSummary = view.GetTelemetrySummary(clientAppUser.Id);
            if (usageSummary == null)
            {
                usageSummary = view.AddTelemetrySummary(clientAppUser.Id);
            }

            return usageSummary;
        }

        private TelemetrySummary GetProgramUsageData(Program program, ClientAppUser clientAppUser)
        {
            var usageSummary = program.UsageSummaries.FirstOrDefault(x => x.ClientAppUser.Id == clientAppUser.Id);
            if (usageSummary == null)
            {
                usageSummary = new ProgramTelemetrySummary() {Program = program, ClientAppUser = clientAppUser};
                program.UsageSummaries.Add((ProgramTelemetrySummary) usageSummary);
            }

            return usageSummary ;
        }

        private async Task<TelemetrySummary> GetUsageData(Program program, ClientAppUser clientAppUser, string viewName = null)
        {
            TelemetrySummary usageSummary;
            if (!string.IsNullOrEmpty(viewName))
            {
                usageSummary = await this.GetViewUsageData(program, clientAppUser, viewName);
            }
            else
            {
                return this.GetProgramUsageData(program, clientAppUser);
            }

            return usageSummary;
        }
    }
}