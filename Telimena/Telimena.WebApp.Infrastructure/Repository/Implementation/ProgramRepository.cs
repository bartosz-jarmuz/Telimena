namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using Core.Models;
    using Database;

    public partial class ProgramRepository : Repository<Program>, IProgramRepository
    {
        private TelimenaContext TelimenaContext => this.DbContext as TelimenaContext;

        public override void Add(Program objectToAdd)
        {
            objectToAdd.RegisteredDate = DateTime.UtcNow;
            this.TelimenaContext.Programs.Add(objectToAdd);
        }

        public void AddProgramUsage(ProgramUsage objectToAdd)
        {
            this.TelimenaContext.ProgramUsages.Add(objectToAdd);
        }

        public IEnumerable<Program> GetProgramsByDeveloperName(string developerName)
        {
              return this.TelimenaContext.Programs.Include(x => x.Developer).Where(x => x.Developer != null && x.Developer.Name == developerName);
        }


        public ProgramRepository(DbContext dbContext) : base(dbContext)
        {
        }
    }
}