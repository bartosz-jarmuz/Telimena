namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using AutoMapper;
    using Client;
    using Core.Models;
    using Database;
    #endregion

    public partial class ProgramRepository : Repository<Program>, IProgramRepository
    {
        public ProgramRepository(DbContext dbContext) : base(dbContext)
        {
        }

        private TelimenaContext TelimenaContext => this.DbContext as TelimenaContext;

        public override void Add(Program objectToAdd)
        {
            objectToAdd.RegisteredDate = DateTime.UtcNow;
            this.TelimenaContext.Programs.Add(objectToAdd);
        }

        public void AddUsage(ProgramUsage objectToAdd)
        {
            this.TelimenaContext.ProgramUsages.Add(objectToAdd);
        }

        public async Task<IEnumerable<Program>> GetProgramsByDeveloperName(string developerName)
        {
            return await this.TelimenaContext.Programs.Include(x => x.Developer).Where(x => x.Developer != null && x.Developer.Name == developerName).ToListAsync();
        }

        public UsageData GetUsage(Program program, ClientAppUser clientAppUser)
        {
            return this.TelimenaContext.ProgramUsages.FirstOrDefault(x => x.Program.Id == program.Id && x.ClientAppUser.Id == clientAppUser.Id);
        }

        public Task<List<ProgramUsage>> GetAllUsages(Program program)
        {
            return this.TelimenaContext.ProgramUsages.Where(x => x.Program.Id == program.Id).ToListAsync();
        }
    }
}