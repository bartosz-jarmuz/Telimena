using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telimena.WebApp.Core.Models.Portal
{
    public class TelimenaToolkitData
    {
        protected TelimenaToolkitData()
        {
        }

        public TelimenaToolkitData(string version)
        {
            this.Version = version;
        }

        public string Version { get; set; }
        public virtual TelimenaPackageInfo TelimenaPackageInfo { get; set; }
        [Key]
        public int Id { get; set; }  

        [Index(IsUnique = true, IsClustered = false)]
        public Guid PublicId { get; set; } = Guid.NewGuid();
    }
}