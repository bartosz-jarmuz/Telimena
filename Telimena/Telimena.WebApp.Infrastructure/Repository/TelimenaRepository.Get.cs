//namespace Telimena.WebApp.Infrastructure.Repository
//{
//    using System;
//    using System.Collections.Generic;
//    using System.Data.Entity;
//    using System.Linq;
//    using System.Threading.Tasks;
//    using Core.DTO;
//    using Core.Interfaces;
//    using Core.Models;

//    public partial class TelimenaRepository : ITelimenaRepository
//    {
//        public async Task<PortalSummaryData> GetPortalSummary()
//        {
//            var summary = new PortalSummaryData();

//            summary.TotalUsersCount = await this.DbContext.Users.CountAsync();
//            summary.NewestUser = await this.DbContext.Users.OrderByDescending(x => x.UserNumber).FirstAsync();
//            summary.LastActiveUser = await this.DbContext.Users.OrderByDescending(x => x.LastLoginDate).FirstAsync();
//            summary.UsersActiveInLast24Hrs = await this.DbContext.Users.CountAsync(x => x.LastLoginDate != null && DbFunctions.DiffDays(DateTime.UtcNow, x.LastLoginDate.Value) < 1);
//            return summary;
//        }

//        public async Task<IEnumerable<Program>> GetPrograms(string developerName = null)
//        {
//            if (!string.IsNullOrEmpty(developerName))
//            {
//                return (await this.DbContext.Programs.Where(x => x.Developer != null && x.Developer.Name == developerName).ToListAsync());
//            }
//            else
//            {
//                return await this.DbContext.Programs.ToListAsync();
//            }
//        }
//    }
//}