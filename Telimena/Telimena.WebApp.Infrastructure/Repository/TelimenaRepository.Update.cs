namespace Telimena.WebApp.Infrastructure.Repository
{
    using System;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.DTO;
    using Core.Interfaces;
    using Core.Models;

    public partial class TelimenaRepository : ITelimenaRepository
    {
      
        public async Task<AllProgramsSummaryData> GetAllProgramsSummaryCounts()
        {
            var summary = new AllProgramsSummaryData
            {
                TotalProgramsCount = await this.DbContext.Programs.CountAsync(),
                TotalAppUsersCount = await this.DbContext.AppUsers.CountAsync(),
                AppUsersRegisteredLast7DaysCount = await this.DbContext.AppUsers.CountAsync(x => DbFunctions.DiffDays(x.RegisteredDate, DateTime.UtcNow) <= 7),
                TotalFunctionsCount = await this.DbContext.Functions.CountAsync()
            };

            int? value = await this.DbContext.ProgramUsages.SumAsync(x => (int?)x.Count) ?? 0;
            summary.TotalProgramUsageCount = value ?? 0;
            value = await this.DbContext.FunctionUsages.SumAsync(x => (int?)x.Count) ?? 0;
            summary.TotalFunctionsUsageCount = value ?? 0;

            summary.NewestProgram = await this.DbContext.Programs.OrderByDescending(x => x.Id).Include(x => x.Developer).FirstOrDefaultAsync();

            return summary;
        }

        public async Task<FunctionUsage> IncrementFunctionUsage(FunctionUsage usage)
        {
            FunctionUsage entity = this.DbContext.FunctionUsages.SingleOrDefault(x => x.Id == usage.Id);
            entity.Count++;
            entity.LastUsageDateTime = DateTime.Today;
            await this.DbContext.SaveChangesAsync();
            return entity;
        }

        public async Task<ProgramUsage> IncrementProgramUsage(ProgramUsage usage)
        {
            ProgramUsage entity = this.DbContext.ProgramUsages.SingleOrDefault(x => x.Id == usage.Id);
            entity.Count++;
            entity.LastUsageDateTime = DateTime.Today;
            await this.DbContext.SaveChangesAsync();
            return entity;
        }
    }
}