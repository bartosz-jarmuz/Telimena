namespace Telimena.WebApp.Core.Models
{
    using System;

    public class AssemblyVersion
    {
        public int Id { get; set; }
        public string Version { get; set; }
        public bool IsBeta { get; set; }
        public DateTime BetaReleaseDate { get; set; }
        public DateTime ProductionReleaseDate { get; set; }
        public int AssemblyId { get; set; }
        public AssemblyData Assembly { get; set; }
    }
}