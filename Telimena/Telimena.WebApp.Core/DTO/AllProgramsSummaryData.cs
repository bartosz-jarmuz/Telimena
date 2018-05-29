namespace Telimena.WebApp.Core.DTO
{
    using System.Collections.Generic;
    using Models;

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