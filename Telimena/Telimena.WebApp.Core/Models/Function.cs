using System;
using System.Collections.Generic;
using System.Linq;

namespace Telimena.WebApp.Core.Models
{
    public class Function
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual Program Program { get; set; }
        public int ProgramId { get; set; }
        public DateTime RegisteredDate { get; set; }
        public virtual ICollection<FunctionUsageSummary> UsageSummaries { get; set; } = new List<FunctionUsageSummary>();

        public ICollection<FunctionUsageDetail> GetFunctionUsageDetails(int clientAppUserId)
        {
            FunctionUsageSummary usage = this.GetFunctionUsageSummary(clientAppUserId);
            return usage.UsageDetails;
        }

        public FunctionUsageSummary GetFunctionUsageSummary(int clientAppUserId)
        {
            return this.UsageSummaries.FirstOrDefault(x => x.ClientAppUser.Id == clientAppUserId);
        }
    }
}