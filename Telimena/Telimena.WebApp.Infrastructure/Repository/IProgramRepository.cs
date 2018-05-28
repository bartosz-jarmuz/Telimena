namespace Telimena.WebApp.Infrastructure.Repository
{
    using System.Collections.Generic;
    using Client;
    using Core.Models;

    public interface IProgramRepository : IRepository<Program>
    {
        void AddUsage(ProgramUsage objectToAdd);
        IEnumerable<Program> GetProgramsByDeveloperName(string developerName);
        UsageData GetUsage(Program program, ClientAppUser clientAppUser);
    }
}