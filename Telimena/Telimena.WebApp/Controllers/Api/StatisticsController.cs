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

    public class StatisticsController : ApiController
    {
        public StatisticsController(IStatisticsUnitOfWork work)
        {
            this.work = work;
            this.helper = new StatisticsHelperService(work);
        }

        private readonly IStatisticsUnitOfWork work;
        private readonly StatisticsHelperService helper;

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
                UsageSummary usageSummary = await this.GetUsageData(program, clientAppUser);
                if (!request.SkipUsageIncrementation)
                {
                    AssemblyVersion versionInfo = program.PrimaryAssembly.GetVersion(request.ProgramInfo.PrimaryAssembly.Version, request.ProgramInfo.PrimaryAssembly.FileVersion);
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

                UsageSummary usageSummary = await this.GetUsageData(program, clientAppUser, updateRequest.FunctionName);
                program.PrimaryAssembly.AddVersion(updateRequest.Version, updateRequest.FileVersion);
                AssemblyVersion versionInfoInfo = program.PrimaryAssembly.GetVersion(updateRequest.Version, updateRequest.FileVersion);

                var ip = this.Request.GetClientIp();
                usageSummary.IncrementUsage(versionInfoInfo, ip, updateRequest.CustomData);

                await this.work.CompleteAsync();
                return PrepareResponse(updateRequest, usageSummary, program, clientAppUser);
            }
            catch (Exception ex)
            {
                return new StatisticsUpdateResponse {Exception = new InvalidOperationException("Error while processing statistics update request", ex)};
            }
        }

        private static StatisticsUpdateResponse PrepareResponse(StatisticsUpdateRequest updateRequest, UsageSummary usageSummary, Program program
            , ClientAppUser clientAppUser)
        {

            StatisticsUpdateResponse response = new StatisticsUpdateResponse
            {
                Count = usageSummary.SummaryCount, ProgramId = program.Id, UserId = clientAppUser.Id, FunctionName = updateRequest.FunctionName
            };
            if (usageSummary is FunctionUsageSummary summary)
            {
                response.FunctionId = summary.FunctionId;
            }

            return response;
        }

        private async Task<UsageSummary> GetFunctionUsageData(Program program, ClientAppUser clientAppUser, string functionName)
        {
            Function func = await this.helper.GetFunctionOrAddIfNotExists(functionName, program);
            UsageSummary usageSummary = func.GetFunctionUsageSummary(clientAppUser.Id);
            if (usageSummary == null)
            {
                usageSummary = new FunctionUsageSummary {Function = func, ClientAppUser = clientAppUser};
                func.UsageSummaries.Add((FunctionUsageSummary) usageSummary);
            }

            return usageSummary;
        }

        private UsageSummary GetProgramUsageData(Program program, ClientAppUser clientAppUser)
        {
            UsageSummary usageSummary = program.UsageSummaries.FirstOrDefault(x => x.ClientAppUser.Id == clientAppUser.Id);
            if (usageSummary == null)
            {
                usageSummary = new ProgramUsageSummary {Program = program, ClientAppUser = clientAppUser};
                program.UsageSummaries.Add((ProgramUsageSummary) usageSummary);
            }

            return usageSummary;
        }

        private async Task<UsageSummary> GetUsageData(Program program, ClientAppUser clientAppUser, string functionName = null)
        {
            UsageSummary usageSummary;
            if (!string.IsNullOrEmpty(functionName))
            {
                usageSummary = await this.GetFunctionUsageData(program, clientAppUser, functionName);
            }
            else
            {
                usageSummary = this.GetProgramUsageData(program, clientAppUser);
            }

            return usageSummary;
        }
    }
}