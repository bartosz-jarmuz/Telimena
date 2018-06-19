namespace Telimena.WebApp.Core.Models
{
    using System;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class AssemblyVersion
    {
        public int Id { get; set; }
        public string Version { get; set; }
        public bool? IsBeta { get; set; }
        public DateTime? BetaReleaseDate { get; set; }
        public DateTime? ProductionReleaseDate { get; set; }
       
    }
}