namespace Telimena.WebApi.Controllers
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Client;
    using WebApp.Core.Models;
    using WebApp.Infrastructure.UnitOfWork;

    public class StatisticsHelperService
    {
        private readonly IStatisticsUnitOfWork work;

        public StatisticsHelperService(IStatisticsUnitOfWork work)
        {
            this.work = work;
        }

        public async Task<ClientAppUser> GetUserInfoOrAddIfNotExists(UserInfo userDto)
        {
            ClientAppUser user = await this.work.ClientAppUsers.FirstOrDefaultAsync(x => x.UserName == userDto.UserName);
            if (user == null)
            {
                user = Mapper.Map<ClientAppUser>(userDto);
                user.RegisteredDate = DateTime.UtcNow;
                this.work.ClientAppUsers.Add(user);
            }

            return user;
        }

        public async Task<Function> GetFunctionOrAddIfNotExists(string functionName, Program program)
        {
            Function func = await this.work.Functions.FirstOrDefaultAsync(x => x.Name == functionName && x.Program.Name == program.Name);
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

            return func;
        }

        public async Task<Program> GetProgramOrAddIfNotExists(ProgramInfo updateRequestProgramInfo)
        {
            Program program = await this.work.Programs.FirstOrDefaultAsync(x => x.Name == updateRequestProgramInfo.Name);
            if (program == null)
            {
                program = Mapper.Map<Program>(updateRequestProgramInfo);
               // program.MainAssembly = Mapper.Map<ProgramAssembly>(updateRequestProgramInfo.MainAssembly)
                this.work.Programs.Add(program);
            }
            return program;
        }

    }
}