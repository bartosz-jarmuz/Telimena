namespace Telimena.WebApp.Core.Models
{
    using System;

    public class TelimenaToolkitData 
    {
        public int Id { get; set; }
        public string Version { get; set; }
        public bool IsBetaVersion { get; set; }
        public DateTime ReleaseDate { get; set; }
    }
}
