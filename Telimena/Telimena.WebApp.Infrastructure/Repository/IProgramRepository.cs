namespace Telimena.WebApp.Infrastructure.Repository
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Client;
    using Core.Models;

    public interface IProgramRepository : IRepository<Program>
    {
        void AddUsage(ProgramUsage objectToAdd);
        Task<IEnumerable<Program>> GetProgramsByDeveloperName(string developerName);
        UsageData GetUsage(Program program, ClientAppUser clientAppUser);
        Task<List<ProgramUsage>> GetAllUsages(Program program);
    }
}