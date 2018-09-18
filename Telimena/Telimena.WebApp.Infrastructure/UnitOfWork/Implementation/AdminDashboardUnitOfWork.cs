using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using DotNetLittleHelpers;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository.Implementation;

namespace Telimena.WebApp.Infrastructure.Repository
{
    #region Using

    #endregion

    public class AdminDashboardUnitOfWork : IAdminDashboardUnitOfWork
    {
        public AdminDashboardUnitOfWork(TelimenaContext context)
        {
            this.context = context;
            this.Programs = new ProgramRepository(this.context);
            this.Functions = new FunctionRepository(this.context);
        }

        private readonly TelimenaContext context;
        public IFunctionRepository Functions { get; }

        public IProgramRepository Programs { get; }

        public async Task<IEnumerable<ProgramSummary>> GetProgramsSummary()
        {
            List<ProgramSummary> returnData = new List<ProgramSummary>();
            IEnumerable<Program> programs = await this.Programs.GetAsync();

            foreach (Program program in programs)
            {
                List<Function> functions = await this.Functions.FindAsync(x => x.ProgramId == program.Id);
                ProgramSummary summary = new ProgramSummary
                {
                    ProgramName = program.Name
                    , DeveloperName = program.DeveloperAccount?.Name ?? "N/A"
                    , LatestVersion = program.PrimaryAssembly.LatestVersion.Version
                    , AssociatedToolkitVersion = program.PrimaryAssembly.LatestVersion.ToolkitData?.Version
                    , ProgramId = program.Id
                    , RegisteredDate = program.RegisteredDate
                    , LastUsage = program.UsageSummaries.MaxOrNull(x => x.LastUsageDateTime)
                    , UsersCount = program.UsageSummaries.Count
                    , TodayUsageCount =
                        program.UsageSummaries.Where(x => (DateTime.UtcNow - x.LastUsageDateTime).TotalHours <= 24).Sum(smr =>
                            smr.UsageDetails.Count(detail => (DateTime.UtcNow - detail.DateTime).TotalHours <= 24))
                    , TotalUsageCount = program.UsageSummaries.Sum(x => x.SummaryCount)
                    , FunctionsCount = functions.Count
                    , TotalFunctionsUsageCount = functions.Sum(f => f.UsageSummaries.Sum(s => s.SummaryCount))
                    , TotalTodayFunctionsUsageCount = functions.Sum(f =>
                        f.UsageSummaries.Where(x => (DateTime.UtcNow - x.LastUsageDateTime).TotalHours <= 24).Sum(smr =>
                            smr.UsageDetails.Count(detail => (DateTime.UtcNow - detail.DateTime).TotalHours <= 24)))
                };
                returnData.Add(summary);
            }

            return returnData;
        }

        public async Task<PortalSummaryData> GetPortalSummary()
        {
            PortalSummaryData summary = new PortalSummaryData
            {
                TotalUsersCount = await this.context.Users.CountAsync()
                , NewestUser = await this.context.Users.OrderByDescending(x => x.UserNumber).FirstAsync()
                , LastActiveUser = await this.context.Users.OrderByDescending(x => x.LastLoginDate).FirstAsync()
                , UsersActiveInLast24Hrs = await this.context.Users.CountAsync(x =>
                    x.LastLoginDate != null && DbFunctions.DiffDays(DateTime.UtcNow, x.LastLoginDate.Value) < 1)
            };
            return summary;
        }

        public async Task<AllProgramsSummaryData> GetAllProgramsSummaryCounts()
        {
            AllProgramsSummaryData summary = new AllProgramsSummaryData
            {
                TotalProgramsCount = await this.context.Programs.CountAsync()
                , TotalAppUsersCount = await this.context.AppUsers.CountAsync()
                , AppUsersRegisteredLast7DaysCount =
                    await this.context.AppUsers.CountAsync(x => DbFunctions.DiffDays(x.RegisteredDate, DateTime.UtcNow) <= 7)
                , TotalFunctionsCount = await this.context.Functions.CountAsync()
            };
            int? value = await this.context.ProgramUsages.SumAsync(x => (int?) x.UsageDetails.Count) ?? 0;
            summary.TotalProgramUsageCount = value ?? 0;
            value = await this.context.FunctionUsages.SumAsync(x => (int?) x.UsageDetails.Count) ?? 0;
            summary.TotalFunctionsUsageCount = value ?? 0;

            summary.NewestProgram = await this.context.Programs.OrderByDescending(x => x.Id) /*.Include(x=>x.Developer)*/.FirstOrDefaultAsync();

            return summary;
        }

        public async Task CompleteAsync()
        {
            await this.context.SaveChangesAsync();
        }
    }
}