using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;

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