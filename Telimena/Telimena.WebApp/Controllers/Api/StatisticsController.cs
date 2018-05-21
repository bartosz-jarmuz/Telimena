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
        public StatisticsController(IStatisticsUnitOfWork work)
        {
            this.work = work;
        }

        private readonly IStatisticsUnitOfWork work;

        public async Task<Function> GetFunctionOrAddIfNotExists(string functionName, Program program)
        {
            Function func = (await this.work.Functions.FindAsync(x => x.Name == functionName && x.Program.Name == program.Name)).FirstOrDefault();
            if (func == null)
            {
                func = new Function()
                {
                    Name = functionName,
                    Program = program,
                    ProgramId = program.Id
                };
                this.work.Functions.Add(func);
            }

            await this.work.CompleteAsync();
            return func;
        }

        [HttpPost]
        public async Task<StatisticsUpdateResponse> UpdateProgramStatistics(StatisticsUpdateRequest updateRequest)
        {
            if (this.IsRequestValid(updateRequest))
            {
                try
                {
                    Program program = this.work.Programs.GetProgramOrAddIfNotExists(updateRequest.ProgramInfo);
                    ClientAppUser clientAppUser = this.work.ClientAppUserRepository.GetUserInfoOrAddIfNotExists(updateRequest.UserInfo);
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
            UsageData usageData;
            if (!string.IsNullOrEmpty(updateRequest.FunctionName))
            {
                usageData = this.work.Functions.GetFunctionUsageOrAddIfNotExists(updateRequest.FunctionName, program, clientAppUser);
            }
            else
            {
                usageData = this.work.Programs.GetProgramUsageDataOrAddIfNotExists(program, clientAppUser);
            }
            usageData.IncrementUsage();
            await this.work.CompleteAsync();
            return usageData;
        }
    }
}