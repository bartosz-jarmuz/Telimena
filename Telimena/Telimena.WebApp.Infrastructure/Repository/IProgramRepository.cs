namespace Telimena.WebApp.Infrastructure.Repository
{
    using System.Collections.Generic;
    using Client;
    using Core.Models;

    public interface IProgramRepository : IRepository<Program>
    {
        void AddProgramUsage(ProgramUsage objectToAdd);
        IEnumerable<Program> GetProgramsByDeveloperName(string developerName);
        Program GetProgramOrAddIfNotExists(string programName);
        Program GetProgramOrAddIfNotExists(ProgramInfo programDto);
        ProgramUsage GetProgramUsageDataOrAddIfNotExists(Program program, ClientAppUser clientAppUser);
    }
}