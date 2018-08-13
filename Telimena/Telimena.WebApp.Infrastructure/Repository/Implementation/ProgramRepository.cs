namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Security.Principal;
    using System.Threading.Tasks;
    using Core.Interfaces;
    using Core.Models;
    using Database;

    #endregion

    internal class ProgramRepository : Repository<Program>, IProgramRepository
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

        public void AddUsage(ProgramUsageSummary objectToAdd)
        {
            this.TelimenaContext.ProgramUsages.Add(objectToAdd);
        }

        public async Task<IEnumerable<Program>> GetProgramsByDeveloperName(string developerName)
        {
            return await this.TelimenaContext.Programs.Include(x => x.DeveloperAccount).Where(x => x.DeveloperAccount != null && x.DeveloperAccount.Name == developerName).ToListAsync();
        }

        public async Task<IEnumerable<Program>> GetProgramsForUserAsync(TelimenaUser user)
        {
             return await this.TelimenaContext.Programs.Where(x => x.DeveloperAccount != null && x.DeveloperAccount.MainUserId == user.Id).ToListAsync();
        }
        public List<Program> GetProgramsVisibleToUser(TelimenaUser user, IPrincipal principal)
        {
            return this.GetProgramsVisibleToUserImpl(user, principal).ToList();
        }
        public Task<List<Program>> GetProgramsVisibleToUserAsync(TelimenaUser user, IPrincipal principal)
        {
            return this.GetProgramsVisibleToUserImpl(user, principal).ToListAsync();
        }


        private IQueryable<Program> GetProgramsVisibleToUserImpl(TelimenaUser user, IPrincipal principal)
        {
            if (principal != null && principal.IsInRole(TelimenaRoles.Admin))
            {
                return this.TelimenaContext.Programs;
            }
            else
            {
                return this.TelimenaContext.Programs.Where(x => x.DeveloperAccount != null && x.DeveloperAccount.MainUserId == user.Id);
            }
        }

        public UsageSummary GetUsage(Program program, ClientAppUser clientAppUser)
        {
            return this.TelimenaContext.ProgramUsages.FirstOrDefault(x => x.Program.Id == program.Id && x.ClientAppUser.Id == clientAppUser.Id);
        }

        public Task<List<ProgramUsageSummary>> GetAllUsages(Program program)
        {
            return this.TelimenaContext.ProgramUsages.Where(x => x.Program.Id == program.Id).ToListAsync();
        }
    }
}