namespace Telimena.WebApp.Core.Interfaces
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Client;
    using DTO;
    using Models;

    //public interface ITelimenaRepository
    //{
    //    Task AddFunction(Function objectToAdd);
    //    Task AddFunctionUsage(FunctionUsage objectToAdd);
    //    Task AddProgram(Program objectToAdd);
    //    Task AddProgramUsage(ProgramUsage objectToAdd);
    //    Task AddUserInfo(ClientAppUser objectToAdd);
    //    Task<Function> GetFunctionOrAddIfNotExists(string functionName, Program program);
    //    Task<FunctionUsage> GetFunctionUsageOrAddIfNotExists(string functionName, Program program, ClientAppUser clientAppUser);
    //    Task<Program> GetProgramOrAddIfNotExists(string programName);
    //    Task<ProgramUsage> GetProgramUsageDataOrAddIfNotExists(Program program, ClientAppUser clientAppUser);
    //    Task<ClientAppUser> GetUserInfoOrAddIfNotExists(string userName);
    //    Task<FunctionUsage> IncrementFunctionUsage(FunctionUsage usage);
    //    Task<ProgramUsage> IncrementProgramUsage(ProgramUsage usage);
    //    Task<ClientAppUser> GetUserInfoOrAddIfNotExists(Client.UserInfo userInfoDto);
    //    Task<Program> GetProgramOrAddIfNotExists(ProgramInfo programDto);
    //    Task<PortalSummaryData> GetPortalSummary();
    //    Task<AllProgramsSummaryData> GetAllProgramsSummaryCounts();
    //    IQueryable<Program> GetPrograms(string developerName = null);
    //}
}
