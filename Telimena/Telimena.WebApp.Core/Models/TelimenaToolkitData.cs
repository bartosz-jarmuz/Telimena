namespace Telimena.WebApp.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;
    using DotNetLittleHelpers;
    using Microsoft.AspNet.Identity.EntityFramework;

    public class TelimenaToolkitData 
    {
        public int Id { get; set; }
        public string Version { get; set; }
        public bool IsBetaVersion { get; set; }
        public DateTime ReleaseDate { get; set; }
    }
}
