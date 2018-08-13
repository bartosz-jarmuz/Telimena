namespace Telimena.WebApp.Infrastructure.Repository
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.DTO;
    using Core.Models;
    using Database;
    using DotNetLittleHelpers;
    using Implementation;
    #endregion

    public class AdminDashboardUnitOfWork : IAdminDashboardUnitOfWork
    {
        private readonly TelimenaContext context;

        public AdminDashboardUnitOfWork(TelimenaContext context)
        {
            this.context = context;
            this.Programs = new ProgramRepository(this.context);
            this.Functions = new FunctionRepository(this.context);
        }

        public IProgramRepository Programs { get; }
        public IFunctionRepository Functions { get; }


        public async Task<IEnumerable<ProgramSummary>> GetProgramsSummary()
        {
            List<ProgramSummary> returnData = new List<ProgramSummary>();
            IEnumerable<Program> programs = await this.Programs.GetAsync();
            
            foreach (Program program in programs)
            {
                List<Function> functions = await this.Functions.FindAsync(x => x.ProgramId == program.Id);
                var smrs = program.UsageSummaries.Where(x => (DateTime.UtcNow - x.LastUsageDateTime).Hours <= 24).Sum(smr => smr.UsageDetails.Count(detail=> (DateTime.UtcNow - detail.DateTime).Hours <= 24));
                ProgramSummary summary = new ProgramSummary()
                {
                    ProgramName = program.Name,
                    DeveloperName = program.DeveloperAccount?.Name??"N/A",
                    LatestVersion = program.PrimaryAssembly.LatestVersion.Version,
                    ProgramId = program.Id,
                    RegisteredDate = program.RegisteredDate,
                    LastUsage = program.UsageSummaries.MaxOrNull(x => x.LastUsageDateTime),
                    UsersCount = program.UsageSummaries.Count,
                    TodayUsageCount = program.UsageSummaries.Where(x => (DateTime.UtcNow - x.LastUsageDateTime).TotalHours <= 24).Sum(smr => smr.UsageDetails.Count(detail => (DateTime.UtcNow - detail.DateTime).TotalHours <= 24)),
                    TotalUsageCount = program.UsageSummaries.Sum(x=>x.SummaryCount),
                    FunctionsCount = functions.Count,
                    TotalFunctionsUsageCount = functions.Sum(f=>f.UsageSummaries.Sum(s=>s.SummaryCount)),
                    TotalTodayFunctionsUsageCount = functions.Sum(f=>f.UsageSummaries.Where(x => (DateTime.UtcNow - x.LastUsageDateTime).TotalHours <= 24).Sum(smr => smr.UsageDetails.Count(detail => (DateTime.UtcNow - detail.DateTime).TotalHours <= 24)))

                };
                returnData.Add(summary);
            }
            return returnData;
        }

        public async Task<PortalSummaryData> GetPortalSummary()
        {
            var summary = new PortalSummaryData
            {
                TotalUsersCount = await this.context.Users.CountAsync(),
                NewestUser = await this.context.Users.OrderByDescending(x => x.UserNumber).FirstAsync(),
                LastActiveUser = await this.context.Users.OrderByDescending(x => x.LastLoginDate).FirstAsync(),
                UsersActiveInLast24Hrs = await this.context.Users.CountAsync(x => x.LastLoginDate != null && DbFunctions.DiffDays(DateTime.UtcNow, x.LastLoginDate.Value) < 1)
            };
            return summary;
        }

        public async Task<AllProgramsSummaryData> GetAllProgramsSummaryCounts()
        {
            var summary = new AllProgramsSummaryData
            {
                TotalProgramsCount = await this.context.Programs.CountAsync(),
                TotalAppUsersCount = await this.context.AppUsers.CountAsync(),
                AppUsersRegisteredLast7DaysCount = await this.context.AppUsers.CountAsync(x => DbFunctions.DiffDays(x.RegisteredDate, DateTime.UtcNow) <= 7),
                TotalFunctionsCount = await this.context.Functions.CountAsync()
            };
            int? value = await this.context.ProgramUsages.SumAsync(x => (int?)x.UsageDetails.Count)??0;
            summary.TotalProgramUsageCount = value ?? 0;
            value = await this.context.FunctionUsages.SumAsync(x => (int?)x.UsageDetails.Count) ?? 0;
            summary.TotalFunctionsUsageCount = value ?? 0;

            summary.NewestProgram = await this.context.Programs.OrderByDescending(x => x.Id)/*.Include(x=>x.Developer)*/.FirstOrDefaultAsync();

            return summary;
        }

        public async Task CompleteAsync()
        {
            await this.context.SaveChangesAsync();
        }
    }
}