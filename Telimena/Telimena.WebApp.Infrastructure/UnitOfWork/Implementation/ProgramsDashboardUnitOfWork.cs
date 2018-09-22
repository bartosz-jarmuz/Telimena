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

    public class ProgramsDashboardUnitOfWork : IProgramsDashboardUnitOfWork
    {
        public ProgramsDashboardUnitOfWork(TelimenaContext context)
        {
            this.context = context;
            this.Programs = new ProgramRepository(this.context);
            this.Functions = new FunctionRepository(this.context);
            this.Users = new Repository<TelimenaUser>(this.context);
        }

        private readonly TelimenaContext context;
        public IFunctionRepository Functions { get; }

        public IRepository<TelimenaUser> Users { get; }
        public IProgramRepository Programs { get; }

        public async Task<IEnumerable<ProgramSummary>> GetProgramsSummary(List<Program> programs)
        {
            List<ProgramSummary> returnData = new List<ProgramSummary>();

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

        public async Task<AllProgramsSummaryData> GetAllProgramsSummaryCounts(List<Program> programs)
        {
            IEnumerable<int> programIds = programs.Select(x => x.Id);
            List<Function> functions = programs.SelectMany(x => x.Functions).ToList();
            IEnumerable<int> functionIds = functions.Select(x => x.Id);
            List<ProgramUsageSummary> programUsageSummaries = await this.context.ProgramUsages.Where(usg => programIds.Contains(usg.ProgramId)).ToListAsync();
            List<FunctionUsageSummary> functionUsageSummaries =
                await this.context.FunctionUsages.Where(usg => functionIds.Contains(usg.FunctionId)).ToListAsync();
            List<ClientAppUser> users = programUsageSummaries.DistinctBy(x => x.ClientAppUserId).Select(x => x.ClientAppUser).ToList();
            AllProgramsSummaryData summary = new AllProgramsSummaryData
            {
                TotalProgramsCount = programs.Count()
                , TotalAppUsersCount = users.Count()
                , AppUsersRegisteredLast7DaysCount = users.Count(x => (DateTime.UtcNow - x.RegisteredDate).TotalDays <= 7)
                , TotalFunctionsCount = functions.Count()
            };
            int? value = programUsageSummaries.Sum(x => (int?) x.UsageDetails.Count) ?? 0;
            summary.TotalProgramUsageCount = value ?? 0;
            value = functionUsageSummaries.Sum(x => (int?) x.UsageDetails.Count) ?? 0;
            summary.TotalFunctionsUsageCount = value ?? 0;

            summary.NewestProgram = programs.OrderByDescending(x => x.Id) /*.Include(x=>x.Developer)*/.FirstOrDefault();

            return summary;
        }

        public async Task CompleteAsync()
        {
            await this.context.SaveChangesAsync();
        }
    }
}