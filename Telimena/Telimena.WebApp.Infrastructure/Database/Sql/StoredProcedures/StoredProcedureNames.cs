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
    }
}
