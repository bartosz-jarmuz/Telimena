using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using DotNetLittleHelpers;

namespace Telimena.WebApp.Core.Models.Portal
{
    public class Updater
    {
        [Obsolete("EF required")]
        protected Updater() { }

        public Updater(string fileName, string internalName)
        {
            this.FileName = fileName;
            this.InternalName = internalName;
        }

        [Required]
        public string FileName { get; set; }

        public string Description { get; set; }

        [StringLength(255)]
        [Index(IsUnique = true)]
        [Required]
        public string InternalName { get; set; }

        [Key]
        public int Id { get; set; }  

        [Index(IsUnique = true, IsClustered = false)]
        public Guid PublicId { get; set; } = Guid.NewGuid();
        public virtual RestrictedAccessList<UpdaterPackageInfo> Packages{ get; set; } = new RestrictedAccessList<UpdaterPackageInfo>();

        public virtual RestrictedAccessList<Program> Programs { get; set; } = new RestrictedAccessList<Program>();

        public virtual DeveloperTeam DeveloperTeam { get; set; }

        public bool IsPublic { get; set; }
    }


}
