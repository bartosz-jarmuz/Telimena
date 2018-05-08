namespace Telimena.WebApp.Core.Interfaces
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Infrastructure.DTO;
    using Models;

    public interface ITelimenaRepository
    {
        Task AddFunction(Function objectToAdd);
        Task AddFunctionUsage(FunctionUsage objectToAdd);
        Task AddProgram(Program objectToAdd);
        Task AddProgramUsage(ProgramUsage objectToAdd);
        Task AddUserInfo(UserInfo objectToAdd);
        Task<Function> GetFunctionOrAddIfNotExists(string functionName, Program program);
        Task<FunctionUsage> GetFunctionUsageOrAddIfNotExists(string functionName, Program program, UserInfo user);
        Task<Program> GetProgramOrAddIfNotExists(string programName);
        Task<ProgramUsage> GetProgramUsageDataOrAddIfNotExists(Program program, UserInfo user);
        Task<UserInfo> GetUserInfoOrAddIfNotExists(string userName);
        Task<FunctionUsage> IncrementFunctionUsage(FunctionUsage usage);
        Task<ProgramUsage> IncrementProgramUsage(ProgramUsage usage);
        Task<UserInfo> GetUserInfoOrAddIfNotExists(UserInfoDto userInfoDto);
        Task<Program> GetProgramOrAddIfNotExists(ProgramInfoDto programDto);
        Task<IEnumerable<Program>> GetDeveloperPrograms(string developerName);
    }
}
