namespace Telimena.WebApi.Controllers
{
    #region Using
    using System;
    using System.IdentityModel;
    using System.Threading.Tasks;
    using System.Web.Http;
    using AutoMapper;
    using Client;
    using WebApp.Core.Interfaces;
    using WebApp.Core.Models;
    #endregion

    public class StatisticsController : ApiController
    {
        public StatisticsController(ITelimenaRepository repository)
        {
            this.repository = repository;
        }

        private readonly ITelimenaRepository repository;

        [HttpPost]
        public async Task<StatisticsUpdateResponse> UpdateProgramStatistics(StatisticsUpdateRequest updateRequest)
        {
            if (this.IsRequestValid(updateRequest))
            {
                try
                {
                    Program program = await this.repository.GetProgramOrAddIfNotExists(updateRequest.ProgramInfo);
                    ClientAppUser clientAppUser = await this.repository.GetUserInfoOrAddIfNotExists(updateRequest.UserInfo);
                    var usageData = await this.UpdateUsageData(updateRequest, program, clientAppUser);
                    return new StatisticsUpdateResponse()
                    {
                        Count = usageData.Count
                    };
                }
                catch (Exception ex)
                {
                    return new StatisticsUpdateResponse(ex);
                }
            }
            else
            {
                return new StatisticsUpdateResponse(new BadRequestException("Request is not valid"));
            }
        }

        private bool IsRequestValid(StatisticsUpdateRequest updateRequest)
        {
            return updateRequest != null;
        }

        private async Task<UsageData> UpdateUsageData(StatisticsUpdateRequest updateRequest, Program program, ClientAppUser clientAppUser)
        {
            if (!string.IsNullOrEmpty(updateRequest.FunctionName))
            {
                var usageData = await this.repository.GetFunctionUsageOrAddIfNotExists(updateRequest.FunctionName, program, clientAppUser);
                return await this.repository.IncrementFunctionUsage(usageData);
            }
            else
            {
                var usageData = await this.repository.GetProgramUsageDataOrAddIfNotExists(program, clientAppUser);
                return await this.repository.IncrementProgramUsage(usageData);
            }
        }
    }
}