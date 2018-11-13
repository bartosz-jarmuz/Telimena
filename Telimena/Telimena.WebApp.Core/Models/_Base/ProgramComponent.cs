using System;

namespace Telimena.WebApp.Core.Models
{
    public abstract class ProgramComponent
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual Program Program { get; set; }
        public int ProgramId { get; set; }
        public DateTime FirstReportedDate { get; set; }
    }
}