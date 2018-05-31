namespace Telimena.WebApp.Core.Models
{
    using System;
    using System.Collections.Generic;

    public class Function
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual  Program Program { get; set; }
        public int ProgramId { get; set; }
        public DateTime RegisteredDate { get; set; }
        public virtual ICollection<FunctionUsage> Usages { get; set; } = new List<FunctionUsage>();
    }
}