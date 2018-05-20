namespace Telimena.WebApp.Infrastructure.Repository
{
    using System.Collections.Generic;
    using Core.Models;

    public interface IProgramRepository : IRepository<Program>
    {
        void AddProgramUsage(ProgramUsage objectToAdd);
        IEnumerable<Program> GetProgramsByDeveloperName(string developerName);
    }
}