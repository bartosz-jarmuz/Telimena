namespace Telimena.WebApi.Controllers
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.IdentityModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web.Http;
    using AutoMapper;
    using Client;
    using WebApp.Core.Interfaces;
    using WebApp.Core.Models;
    using WebApp.Infrastructure.UnitOfWork;
    #endregion

    public class StatisticsController : ApiController
    {
        public StatisticsController(IStatisticsUnitOfWork unitOfWork)
        {
            this.unitOfWork = unitOfWork;
        }

        private readonly IStatisticsUnitOfWork unitOfWork;

        public async Task<Function> GetFunctionOrAddIfNotExists(string functionName, Program program)
        {
            Function func = (await this.unitOfWork.FunctionRepository.FindAsync(x => x.Name == functionName && x.Program.Name == program.Name)).FirstOrDefault();
            if (func == null)
            {
                func = new Function()
                {
                    Name = functionName,
                    Program = program,
                    ProgramId = program.Id
                };
                this.unitOfWork.FunctionRepository.Add(func);
            }

            await this.unitOfWork.CompleteAsync();
            return func;
        }

        [HttpPost]
        public async Task<StatisticsUpdateResponse> UpdateProgramStatistics(StatisticsUpdateRequest updateRequest)
        {
            if (this.IsRequestValid(updateRequest))
            {
                try
                {
                    Program program = await this.unitOfWork.GetProgramOrAddIfNotExists(updateRequest.ProgramInfo);
                    ClientAppUser clientAppUser = await this.unitOfWork.GetUserInfoOrAddIfNotExists(updateRequest.UserInfo);
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
                var usageData = await this.unitOfWork.GetFunctionUsageOrAddIfNotExists(updateRequest.FunctionName, program, clientAppUser);
                return await this.unitOfWork.IncrementFunctionUsage(usageData);
            }
            else
            {
                var usageData = await this.unitOfWork.GetProgramUsageDataOrAddIfNotExists(program, clientAppUser);
                return await this.unitOfWork.IncrementProgramUsage(usageData);
            }
        }
    }
}