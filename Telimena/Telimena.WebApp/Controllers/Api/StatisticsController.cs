namespace Telimena.WebApi.Controllers
{
    #region Using
    using System;
    using System.IdentityModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Client;
    using WebApp.Core.Models;
    using WebApp.Infrastructure;
    using WebApp.Infrastructure.UnitOfWork;
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
                return new RegistrationResponse()
                {
                    Error = new BadRequestException("Request is not valid")
                };
            }

            try
            {
                Program program = await this.helper.GetProgramOrAddIfNotExists(request.ProgramInfo);
                ClientAppUser clientAppUser = await this.helper.GetUserInfoOrAddIfNotExists(request.UserInfo);
                UsageSummary usageSummary = await this.GetUsageData(program, clientAppUser);
                var version = program.PrimaryAssembly.GetVersion(request.ProgramInfo.PrimaryAssembly.Version);
                usageSummary.IncrementUsage(version);
                await this.work.CompleteAsync();

                return new RegistrationResponse()
                {
                    Count = usageSummary.SummaryCount,
                    ProgramId = program.ProgramId,
                    UserId = clientAppUser.Id,
                };
            }
            catch (Exception ex)
            {
                return new RegistrationResponse()
                {
                    Error = new InvalidOperationException("Error while processing registration request", ex)
                };
            }
        }

        [HttpPost]
        public async Task<StatisticsUpdateResponse> UpdateProgramStatistics(StatisticsUpdateRequest updateRequest)
        {
            if (!ApiRequestsValidator.IsRequestValid(updateRequest))
            {
                return new StatisticsUpdateResponse()
                {
                    Error = new BadRequestException("Request is not valid")
                };
            }

            try
            {
                Program program = this.work.Programs.GetById(updateRequest.ProgramId);
                if (program == null)
                {
                    return new StatisticsUpdateResponse()
                    {
                        Error = new InvalidOperationException($"Program [{updateRequest.ProgramId}] is null")
                    };
                }

                ClientAppUser clientAppUser = this.work.ClientAppUsers.GetById(updateRequest.UserId);
                if (clientAppUser == null)
                {
                    return new StatisticsUpdateResponse()
                    {
                        Error = new InvalidOperationException($"User [{updateRequest.UserId}] is null")
                    };
                }

                UsageSummary usageSummary = await this.GetUsageData(program, clientAppUser, updateRequest.FunctionName);
                this.helper.EnsureVersionIsRegistered(program.PrimaryAssembly, updateRequest.Version);
                var versionInfo = program.PrimaryAssembly.GetVersion(updateRequest.Version);

                usageSummary.IncrementUsage(versionInfo);

                await this.work.CompleteAsync();
                return new StatisticsUpdateResponse()
                {
                    Count = usageSummary.SummaryCount,
                    ProgramId = program.ProgramId,
                    UserId = clientAppUser.Id,
                    FunctionName = updateRequest.FunctionName
                };
            }
            catch (Exception ex)
            {
                return new StatisticsUpdateResponse()
                {
                    Error = new InvalidOperationException("Error while processing statistics update request", ex)
                };
            }
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

        private async Task<UsageSummary> GetFunctionUsageData(Program program, ClientAppUser clientAppUser, string functionName)
        {
            var func = await this.helper.GetFunctionOrAddIfNotExists(functionName, program);
            UsageSummary usageSummary = func.GetFunctionUsageSummary(clientAppUser.Id);
            if (usageSummary == null)
            {
                usageSummary = new FunctionUsageSummary()
                {
                    Function = func,
                    ClientAppUser = clientAppUser
                };
                func.UsageSummaries.Add((FunctionUsageSummary) usageSummary);
            }

            return usageSummary;
        }

        private UsageSummary GetProgramUsageData(Program program, ClientAppUser clientAppUser)
        {
            UsageSummary usageSummary = program.UsageSummaries.FirstOrDefault(x => x.ClientAppUser.Id == clientAppUser.Id);
            if (usageSummary == null)
            {
                usageSummary = new ProgramUsageSummary()
                {
                    Program = program,
                    ClientAppUser = clientAppUser
                };
                program.UsageSummaries.Add((ProgramUsageSummary)usageSummary);
            }

            return usageSummary;
        }

    }
}