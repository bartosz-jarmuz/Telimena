using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telimena.WebApp.Core.DTO
{
    using Models;

    public class PortalSummaryData
    {
        public int TotalUsersCount { get; set; }
        public int UsersActiveInLast24Hrs { get; set; }
        public TelimenaUser NewestUser { get; set; }
        public TelimenaUser LastActiveUser { get; set; }
    }

    public class AllProgramsSummaryData
    {
        public int TotalProgramsCount { get; set; }
        public int TotalFunctionsCount { get; set; }
        public int TotalProgramUsageCount { get; set; }
        public int TotalFunctionsUsageCount { get; set; }
        public Program NewestProgram { get; set; }
        public Program LastActiveProgram { get; set; }
        public IEnumerable<Program> ProgramsActiveInLast24Hrs { get; set; }
        public int TotalAppUsersCount { get; set; }
        public int AppUsersRegisteredLast7DaysCount { get; set; }
    }
}
