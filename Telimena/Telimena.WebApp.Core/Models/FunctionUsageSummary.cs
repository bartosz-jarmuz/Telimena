namespace Telimena.WebApp.Core.Models
{
    using System;
    using System.Collections.Generic;

    public class FunctionUsageSummary : UsageSummary
    {
        public int FunctionId { get; set; }
        public virtual Function Function { get; set; }
        public virtual ICollection<FunctionUsageDetail> UsageDetails { get; set; } = new List<FunctionUsageDetail>();

        #region Overrides of UsageSummary
        public override void UpdateUsageDetails(DateTime lastUsageDateTime, AssemblyVersion version)
        {
            var usage = new FunctionUsageDetail()
            {
                DateTime = lastUsageDateTime,
                UsageSummary = this,
                AssemblyVersion = version
            };
            this.UsageDetails.Add(usage);
        }

        #endregion
    }
    public class FunctionUsageDetail : UsageDetail
    {
        public virtual FunctionUsageSummary UsageSummary { get; set; }
    }

}