namespace Telimena.WebApp.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class Function
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual  Program Program { get; set; }
        public int ProgramId { get; set; }
        public DateTime RegisteredDate { get; set; }
        public virtual ICollection<FunctionUsageSummary> UsageSummaries { get; set; } = new List<FunctionUsageSummary>();

        public FunctionUsageSummary GetFunctionUsageSummary(int clientAppUserId)
        {
            return this.UsageSummaries.FirstOrDefault(x => x.ClientAppUser.Id == clientAppUserId);
        }

        public ICollection<FunctionUsageDetail> GetFunctionUsageDetails(int clientAppUserId)
        {
            var usage = this.GetFunctionUsageSummary(clientAppUserId);
            return usage.UsageDetails;
        }
    }
}