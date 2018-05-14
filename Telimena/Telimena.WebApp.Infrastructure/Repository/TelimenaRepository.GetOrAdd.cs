namespace Telimena.WebApp.Infrastructure.Repository
{
    #region Using
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Client;
    using Core.Interfaces;
    using Core.Models;
    using WebApi;
    #endregion

    public partial class TelimenaRepository : ITelimenaRepository
    {
       
        public async Task<Function> GetFunctionOrAddIfNotExists(string functionName, Program program)
        {
            var func = this.DbContext.Functions.FirstOrDefault(x => x.Name == functionName && x.Program.Name == program.Name);
            if (func == null)
            {
                func = new Function()
                {
                    Name = functionName,
                    Program = program,
                    ProgramId = program.Id
                };
                await this.AddFunction(func);
            }

            return func;
        }

        public async Task<FunctionUsage> GetFunctionUsageOrAddIfNotExists(string functionName, Program program, ClientAppUser clientAppUser)
        {
            var function = await this.GetFunctionOrAddIfNotExists(functionName, program);
            var usageData = this.DbContext.FunctionUsages.FirstOrDefault(x => x.Function.Name == function.Name && x.ClientAppUser.UserName == clientAppUser.UserName);
            if (usageData == null)
            {
                usageData = new FunctionUsage()
                {
                    ClientAppUser = clientAppUser,
                    Function = function,
                    FunctionId = function.Id
                };
                await this.AddFunctionUsage(usageData);
            }

            return usageData;
        }

        public async Task<Program> GetProgramOrAddIfNotExists(string programName)
        {
            var program = this.DbContext.Programs.FirstOrDefault(x => x.Name == programName);
            if (program == null)
            {
                program = new Program()
                {
                    Name = programName
                };
                await this.AddProgram(program);
            }

            return program;
        }

        public async Task<Program> GetProgramOrAddIfNotExists(ProgramInfo programDto)
        {
            var program = this.DbContext.Programs.FirstOrDefault(x => x.Name == programDto.Name);
            if (program == null)
            {
                program = Mapper.Map<Program>(programDto);
                await this.AddProgram(program);
            }

            return program;
        }

        public async Task<ClientAppUser> GetUserInfoOrAddIfNotExists(UserInfo userDto)
        {
            var user = this.DbContext.UserInfos.FirstOrDefault(x => x.UserName == userDto.UserName);
            if (user == null)
            {
                user = Mapper.Map<ClientAppUser>(userDto);
                await this.AddUserInfo(user);
            }

            return user;
        }


        public async Task<ProgramUsage> GetProgramUsageDataOrAddIfNotExists(Program program, ClientAppUser clientAppUser)
        {
            var usageData = this.DbContext.ProgramUsages.FirstOrDefault(x => x.Program.Name == program.Name && x.ClientAppUser.UserName == clientAppUser.UserName);
            if (usageData == null)
            {
                usageData = new ProgramUsage()
                {
                    Program = program,
                    ClientAppUser = clientAppUser,
                };
                await this.AddProgramUsage(usageData);
            }

            return usageData;
        }

        public async Task<ClientAppUser> GetUserInfoOrAddIfNotExists(string userName)
        {
            var user = this.DbContext.UserInfos.FirstOrDefault(x => x.UserName == userName);
            if (user == null)
            {
                user = new ClientAppUser()
                {
                    UserName = userName
                };
                await this.AddUserInfo(user);
            }

            return user;
        }
    }
}