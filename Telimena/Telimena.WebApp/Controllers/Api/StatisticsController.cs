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
    using WebApp.Infrastructure.DTO;
    using UserInfo = WebApp.Core.Models.UserInfo;
    #endregion

    public class StatisticsController : ApiController
    {
        public StatisticsController(ITelimenaRepository repository)
        {
            this.repository = repository;
        }

        private readonly ITelimenaRepository repository;

        public async Task<StatisticsUpdateResponse> UpdateProgramStatistics(StatisticsUpdateRequest updateRequest)
        {
            if (this.IsRequestValid(updateRequest))
            {
                try
                {

                    Program program = await this.repository.GetProgramOrAddIfNotExists(Mapper.Map<ProgramInfoDto>(updateRequest.ProgramInfo));
                    UserInfo user = await this.repository.GetUserInfoOrAddIfNotExists(Mapper.Map<UserInfoDto>(updateRequest.UserInfo));
                    var usageData = await this.UpdateUsageData(updateRequest, program, user);
                    return new StatisticsUpdateResponse()
                    {
                        Count = usageData.Count
                    };
                }
                catch (Exception ex)
                {
                    return new StatisticsUpdateResponse()
                    {
                        IsMessageSuccessful = false,
                        Exception = ex
                    };
                }
            }
            else
            {
                return new StatisticsUpdateResponse()
                {
                    IsMessageSuccessful = false,
                    Exception = new BadRequestException("Request is not valid")
                };
            }
        }

        private bool IsRequestValid(StatisticsUpdateRequest updateRequest)
        {
            return updateRequest != null;
        }

        private async Task<UsageData> UpdateUsageData(StatisticsUpdateRequest updateRequest, Program program, UserInfo user)
        {
            if (!string.IsNullOrEmpty(updateRequest.FunctionName))
            {
                var usageData = await this.repository.GetFunctionUsageOrAddIfNotExists(updateRequest.FunctionName, program, user);
                return await this.repository.IncrementFunctionUsage(usageData);
            }
            else
            {
                var usageData = await this.repository.GetProgramUsageDataOrAddIfNotExists(program, user);
                return await this.repository.IncrementProgramUsage(usageData);
            }
        }
    }
}