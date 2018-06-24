namespace Telimena.WebApp.Core.Models
{
    using System;
    using System.Collections.Generic;

    public class ProgramUsageSummary : UsageSummary
    {
        public int ProgramId { get; set; }
        public virtual Program Program { get; set; }

        public virtual ICollection<ProgramUsageDetail> UsageDetails { get; set; } = new List<ProgramUsageDetail>();

        private int _summaryCount;
        public override int SummaryCount
        {
            get
            {
                this._summaryCount = this.UsageDetails?.Count ?? 0;
                return this._summaryCount;
            }
            set => this._summaryCount = value;
        }

        public override void UpdateUsageDetails(DateTime lastUsageDateTime, AssemblyVersion version)
        {
            var usage = new ProgramUsageDetail()
            {
                DateTime = lastUsageDateTime,
                UsageSummary = this,
                AssemblyVersion = version
            };
            this.UsageDetails.Add(usage);
            this.SummaryCount = this.UsageDetails.Count;

        }


    }


}