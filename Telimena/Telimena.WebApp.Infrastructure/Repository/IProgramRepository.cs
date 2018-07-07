namespace Telimena.WebApp.Infrastructure.Repository
{
    using System.Collections.Generic;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Client;
    using Core.Models;

    public interface IProgramRepository : IRepository<Program>
    {
        //void AddUsage(ProgramUsage objectToAdd);
        Task<IEnumerable<Program>> GetProgramsByDeveloperName(string developerName);
        Task<IEnumerable<Program>> GetProgramsForUserAsync(TelimenaUser user);
        //UsageData GetUsage(Program program, ClientAppUser clientAppUser);
        //Task<List<ProgramUsage>> GetAllUsages(Program program);
        List<Program> GetProgramsVisibleToUser(TelimenaUser user, IPrincipal principal);
        Task<List<Program>> GetProgramsVisibleToUserAsync(TelimenaUser user, IPrincipal principal);
    }
}