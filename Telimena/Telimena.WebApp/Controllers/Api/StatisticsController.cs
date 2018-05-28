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
        public async Task<RegistrationResponse> RegisterClient(RegistrationRequest updateRequest)
        {
            if (!ApiRequestsValidator.IsRequestValid(updateRequest))
            {
                return new RegistrationResponse()
                {
                    Error = new BadRequestException("Request is not valid")
                };
            }

            try
            {
                Program program = await this.helper.GetProgramOrAddIfNotExists(updateRequest.ProgramInfo);
                ClientAppUser clientAppUser = await this.helper.GetUserInfoOrAddIfNotExists(updateRequest.UserInfo);
                UsageData usageData = await this.GetUpdatedUsageData(program, clientAppUser);
                await this.work.CompleteAsync();
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
                Program program = this.work.Programs.Get(updateRequest.ProgramId);
                if (program == null)
                {
                    return new StatisticsUpdateResponse()
                    {
                        Error = new InvalidOperationException($"Program [{updateRequest.ProgramId}] is null")
                    };
                }

                ClientAppUser clientAppUser = this.work.ClientAppUsers.Get(updateRequest.UserId);
                if (clientAppUser == null)
                {
                    return new StatisticsUpdateResponse()
                    {
                        Error = new InvalidOperationException($"User [{updateRequest.UserId}] is null")
                    };
                }

                UsageData usageData = await this.GetUpdatedUsageData(program, clientAppUser, updateRequest.FunctionName);

                await this.work.CompleteAsync();
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
                    Error = new InvalidOperationException("Error while processing statistics update request", ex)
                };
            }
        }

        private async Task<UsageData> GetUpdatedUsageData(Program program, ClientAppUser clientAppUser, string functionName = null)
        {
            UsageData usageData;
            if (!string.IsNullOrEmpty(functionName))
            {
                usageData = await this.GetFunctionUsageData(program, clientAppUser, functionName);
            }
            else
            {
                usageData = this.GetProgramUsageData(program, clientAppUser);
            }

            usageData.IncrementUsage();
            return usageData;
        }

        private async Task<UsageData> GetFunctionUsageData(Program program, ClientAppUser clientAppUser, string functionName)
        {
            UsageData usageData;
            var func = await this.helper.GetFunctionOrAddIfNotExists(functionName, program);
            usageData = this.work.Functions.GetUsage(func, clientAppUser);
            if (usageData == null)
            {
                usageData = new FunctionUsage()
                {
                    Function = func,
                    ClientAppUser = clientAppUser
                };
                this.work.Functions.AddUsage((FunctionUsage) usageData);
            }

            return usageData;
        }

        private UsageData GetProgramUsageData(Program program, ClientAppUser clientAppUser)
        {
            UsageData usageData;
            usageData = this.work.Programs.GetUsage(program, clientAppUser);
            if (usageData == null)
            {
                usageData = new ProgramUsage()
                {
                    Program = program,
                    ClientAppUser = clientAppUser
                };
                this.work.Programs.AddUsage((ProgramUsage) usageData);
            }

            return usageData;
        }

    }
}