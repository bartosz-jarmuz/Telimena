using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;

namespace Telimena.WebApp.Infrastructure.Repository.Implementation
{
    #region Using

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

        public override void Remove(Program program)
        {
            this.TelimenaContext.FunctionUsageDetails.RemoveRange(this.TelimenaContext.FunctionUsageDetails.Where(x => x.UsageSummary.Function.ProgramId == program.Id));
            this.TelimenaContext.FunctionUsages.RemoveRange(this.TelimenaContext.FunctionUsages.Where(x => x.Function.ProgramId == program.Id));
            this.TelimenaContext.Functions.RemoveRange(this.TelimenaContext.Functions.Where(x => x.ProgramId == program.Id));

            this.TelimenaContext.ProgramUsageDetails.RemoveRange(this.TelimenaContext.ProgramUsageDetails.Where(x => x.UsageSummary.ProgramId == program.Id));
            this.TelimenaContext.ProgramUsages.RemoveRange(this.TelimenaContext.ProgramUsages.Where(x => x.ProgramId == program.Id));

            this.TelimenaContext.Versions.RemoveRange(this.TelimenaContext.Versions.Where(x => x.ProgramAssembly.ProgramId == program.Id));
            this.TelimenaContext.ProgramAssemblies.RemoveRange(this.TelimenaContext.ProgramAssemblies.Where(x => x.ProgramId == program.Id));

            this.TelimenaContext.ProgramPackages.RemoveRange(this.TelimenaContext.ProgramPackages.Where(x => x.ProgramId == program.Id));

            this.TelimenaContext.Programs.Remove(program);

        }

        public async Task<IEnumerable<Program>> GetProgramsByDeveloperName(string developerName)
        {
            return await this.TelimenaContext.Programs.Include(x => x.DeveloperAccount)
                .Where(x => x.DeveloperAccount != null && x.DeveloperAccount.Name == developerName).ToListAsync();
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

        public void AddUsage(ProgramUsageSummary objectToAdd)
        {
            this.TelimenaContext.ProgramUsages.Add(objectToAdd);
        }

        public Task<List<ProgramUsageSummary>> GetAllUsages(Program program)
        {
            return this.TelimenaContext.ProgramUsages.Where(x => x.Program.Id == program.Id).ToListAsync();
        }

        public UsageSummary GetUsage(Program program, ClientAppUser clientAppUser)
        {
            return this.TelimenaContext.ProgramUsages.FirstOrDefault(x => x.Program.Id == program.Id && x.ClientAppUser.Id == clientAppUser.Id);
        }

        private IQueryable<Program> GetProgramsVisibleToUserImpl(TelimenaUser user, IPrincipal principal)
        {
            if (principal != null && principal.IsInRole(TelimenaRoles.Admin))
            {
                return this.TelimenaContext.Programs;
            }

            return this.TelimenaContext.Programs.Where(x => x.DeveloperAccount != null && x.DeveloperAccount.MainUserId == user.Id);
        }
    }
}