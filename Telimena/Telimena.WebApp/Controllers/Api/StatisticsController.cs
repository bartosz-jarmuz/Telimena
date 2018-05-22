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
        }

        private readonly IStatisticsUnitOfWork work;

        [HttpPost]
        public async Task<RegistrationResponse> RegisterClient(RegistrationRequest updateRequest)
        {
            if (ApiRequestsValidator.IsRequestValid(updateRequest))
            {
                try
                {
                    Program program = this.work.Programs.GetProgramOrAddIfNotExists(updateRequest.ProgramInfo);
                    ClientAppUser clientAppUser = this.work.ClientAppUserRepository.GetUserInfoOrAddIfNotExists(updateRequest.UserInfo);
                    UsageData usageData = await this.UpdateUsageData(program, clientAppUser);
                    return new RegistrationResponse()
                    {
                        Count = usageData.Count,
                        ProgramId = program.Id,
                        UserId = clientAppUser.Id,
                    };
                }
                catch (Exception ex)
                {
                    return new RegistrationResponse()
                    {
                        Error = ex
                    };
                }
            }
            else
            {
                return new RegistrationResponse()
                {
                    Error = new BadRequestException("Request is not valid")
                };
            }
        }

        [HttpPost]
        public async Task<StatisticsUpdateResponse> UpdateProgramStatistics(StatisticsUpdateRequest updateRequest)
        {
            if (ApiRequestsValidator.IsRequestValid(updateRequest))
            {
                try
                {
                    Program program = this.work.Programs.Get(updateRequest.ProgramId);
                    ClientAppUser clientAppUser = this.work.ClientAppUserRepository.Get(updateRequest.UserId);
                    UsageData usageData = await this.UpdateUsageData(program, clientAppUser, updateRequest.FunctionName);
                    return new StatisticsUpdateResponse()
                    {
                        Count = usageData.Count,
                        ProgramId = program.Id,
                        UserId = clientAppUser.Id,
                        FunctionName = updateRequest.FunctionName
                    };
                }
                catch (Exception ex)
                {
                    return new StatisticsUpdateResponse()
                    {
                        Error = ex
                    };
                }
            }
            else
            {
                return new StatisticsUpdateResponse()
                {
                    Error = new BadRequestException("Request is not valid")
                };
            }
        }

        private async Task<UsageData> UpdateUsageData(Program program, ClientAppUser clientAppUser, string functionName = null)
        {
            UsageData usageData;
            if (!string.IsNullOrEmpty(functionName))
            {
                usageData = this.work.Functions.GetFunctionUsageOrAddIfNotExists(functionName, program, clientAppUser);
            }
            else
            {
                usageData = this.work.Programs.GetProgramUsageOrAddIfNotExists(program, clientAppUser);
            }

            usageData.IncrementUsage();
            await this.work.CompleteAsync();
            return usageData;
        }
    }

    
}