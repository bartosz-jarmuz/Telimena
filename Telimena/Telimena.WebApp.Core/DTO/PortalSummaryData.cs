using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Core.DTO
{
    public class PortalSummaryData
    {
        public int TotalUsersCount { get; set; }
        public int UsersActiveInLast24Hrs { get; set; }
        public TelimenaUser NewestUser { get; set; }
        public TelimenaUser LastActiveUser { get; set; }
    }
}