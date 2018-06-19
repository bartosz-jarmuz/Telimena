namespace Telimena.WebApi.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Client;
    using DotNetLittleHelpers;
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
                this.work.Programs.Add(program);
            }
            this.EnsureVersionIsRegistered(program, updateRequestProgramInfo);
            return program;
        }

        /// <summary>
        /// Verifies that the version of the program is added to the list of versions
        /// </summary>
        /// <param name="program"></param>
        /// <param name="updateRequestProgramInfo"></param>
        /// <returns></returns>
        public void EnsureVersionIsRegistered(Program program, ProgramInfo updateRequestProgramInfo)
        {
            if (program.PrimaryAssembly.Versions.AnyAndNotNull())
            {
                if (!program.PrimaryAssembly.Versions.Any(x => x.Version == updateRequestProgramInfo.PrimaryAssembly.Version))
                {
                    program.PrimaryAssembly.Versions.Add(new AssemblyVersion()
                    {
                        Version = updateRequestProgramInfo.PrimaryAssembly.Version
                    });
                }
            }
            else
            {
                program.PrimaryAssembly.Versions.Add(new AssemblyVersion()
                {
                    Version = updateRequestProgramInfo.PrimaryAssembly.Version
                });

            }
        }
    }
}