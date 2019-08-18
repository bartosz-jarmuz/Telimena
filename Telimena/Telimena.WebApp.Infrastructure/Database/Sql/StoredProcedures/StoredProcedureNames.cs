using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming

namespace Telimena.WebApp.Infrastructure.Database.Sql.StoredProcedures
{
    public static class StoredProcedureNames
    {
        public static string p_GetVersionUsage { get; } = nameof(p_GetVersionUsage);
        public static string p_GetDailySummaryCounts { get; } = nameof(p_GetDailySummaryCounts);
        public static string p_GetDailyUsersCounts { get; } = nameof(p_GetDailyUsersCounts);
        public static string p_GetUserActivitySummary { get; } = nameof(p_GetUserActivitySummary);
        public static string p_GetProgramUsagesSummary { get; } = nameof(p_GetProgramUsagesSummary);
        public static string p_GetProgramSummaryCounts { get; } = nameof(p_GetProgramSummaryCounts);
    }
}
