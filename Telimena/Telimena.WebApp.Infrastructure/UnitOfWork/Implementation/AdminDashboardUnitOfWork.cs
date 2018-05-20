namespace Telimena.WebApp.Infrastructure.Repository
{
    #region Using
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Core.DTO;
    using Core.Interfaces;
    using Core.Models;
    using Database;
    #endregion

    public class AdminDashboardUnitOfWork : IAdminDashboardUnitOfWork
    {
        private readonly TelimenaContext telimenaContext;

        public AdminDashboardUnitOfWork(TelimenaContext telimenaContext)
        {
            this.telimenaContext = telimenaContext;
        }

        public async Task<PortalSummaryData> GetPortalSummary()
        {

            var summary = new PortalSummaryData
            {
                TotalUsersCount = await this.telimenaContext.Users.CountAsync(),
                NewestUser = await this.telimenaContext.Users.OrderByDescending(x => x.UserNumber).FirstAsync(),
                LastActiveUser = await this.telimenaContext.Users.OrderByDescending(x => x.LastLoginDate).FirstAsync(),
                UsersActiveInLast24Hrs = await this.telimenaContext.Users.CountAsync(x => x.LastLoginDate != null && DbFunctions.DiffDays(DateTime.UtcNow, x.LastLoginDate.Value) < 1)
            };

            return summary;
        }

        public async Task<AllProgramsSummaryData> GetAllProgramsSummaryCounts()
        {
            var summary = new AllProgramsSummaryData
            {
                TotalProgramsCount = await this.telimenaContext.Programs.CountAsync(),
                TotalAppUsersCount = await this.telimenaContext.AppUsers.CountAsync(),
                AppUsersRegisteredLast7DaysCount = await this.telimenaContext.AppUsers.CountAsync(x => DbFunctions.DiffDays(x.RegisteredDate, DateTime.UtcNow) <= 7),
                TotalFunctionsCount = await this.telimenaContext.Functions.CountAsync()
            };

            int? value = await this.telimenaContext.ProgramUsages.SumAsync(x => (int?)x.Count)??0;
            summary.TotalProgramUsageCount = value ?? 0;
            value = await this.telimenaContext.FunctionUsages.SumAsync(x => (int?)x.Count) ?? 0;
            summary.TotalFunctionsUsageCount = value ?? 0;

            summary.NewestProgram = await this.telimenaContext.Programs.OrderByDescending(x => x.Id).Include(x=>x.Developer).FirstOrDefaultAsync();

            return summary;
        }

        
    }
}