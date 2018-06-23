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
                    ProgramId = program.ProgramId
                };
                this.work.Functions.Add(func);
            }

            return func;
        }

        public async Task<Program> GetProgramOrAddIfNotExists(ProgramInfo requestProgramInfo)
        {
            Program program = await this.work.Programs.FirstOrDefaultAsync(x => x.Name == requestProgramInfo.Name);
            if (program == null)
            {
                program = Mapper.Map<Program>(requestProgramInfo);
                this.work.Programs.Add(program);
            }
            this.EnsureVersionIsRegistered(program.PrimaryAssembly, requestProgramInfo.PrimaryAssembly.Version);

            if (requestProgramInfo.HelperAssemblies.AnyAndNotNull())
            {
                foreach (AssemblyInfo helperAssembly in requestProgramInfo.HelperAssemblies)
                {
                    ProgramAssembly existingAssembly = program.ProgramAssemblies.FirstOrDefault(x => x.Name == helperAssembly.Name);
                    if (existingAssembly == null)
                    {
                        existingAssembly = Mapper.Map<ProgramAssembly>(helperAssembly);
                        program.ProgramAssemblies.Add(existingAssembly);
                    }

                    this.EnsureVersionIsRegistered(existingAssembly, helperAssembly.Version);
                }
            }

            return program;
        }

        /// <summary>
        /// Verifies that the version of the program is added to the list of versions
        /// </summary>
        /// <param name="programAssembly"></param>
        /// <param name="version"></param>
        /// <returns></returns>
        public void EnsureVersionIsRegistered(ProgramAssembly programAssembly, string version)
        {
            if (programAssembly.Versions.AnyAndNotNull())
            {
                if (programAssembly.Versions.All(x => x.Version != version))
                {
                    programAssembly.Versions.Add(new AssemblyVersion()
                    {
                        Version = version
                    });
                }
            }
            else
            {
                programAssembly.Versions.Add(new AssemblyVersion()
                {
                    Version = version
                });

            }
        }
    }
}