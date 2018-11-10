using System;
using System.Collections.Generic;

namespace Telimena.WebApp.Core.Models
{
    public class ProgramUsageSummary : UsageSummary
    {
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

        public int ProgramId { get; set; }
        public virtual Program Program { get; set; }

        public virtual ICollection<ProgramUsageDetail> UsageDetails { get; set; } = new List<ProgramUsageDetail>();

        public override void UpdateUsageDetails(DateTime lastUsageDateTime, string ipAddress, AssemblyVersion versionInfo, string customData)
        {
            ProgramUsageDetail usage = new ProgramUsageDetail
            {
                DateTime = lastUsageDateTime, UsageSummary = this, AssemblyVersion = versionInfo, IpAddress = ipAddress
            };
            if (customData != null)
            {
                usage.CustomUsageData = new ProgramCustomUsageData(customData);
            }
            this.UsageDetails.Add(usage);
            this.SummaryCount = this.UsageDetails.Count;
        }
    }
}