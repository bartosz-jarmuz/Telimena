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
        public virtual ICollection<FunctionUsageSummary> Usages { get; set; } = new List<FunctionUsageSummary>();

        public FunctionUsageSummary GetFunctionUsage(int clientAppUserId)
        {
            return this.Usages.FirstOrDefault(x => x.ClientAppUser.Id == clientAppUserId);
        }
    }
}