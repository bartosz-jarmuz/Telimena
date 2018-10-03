using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetLittleHelpers;

namespace Telimena.WebApp.Core.Models
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

        [StringLength(255)]
        [Index(IsUnique = true)]
        [Required]
        public string InternalName { get; set; }

        public int Id { get; set; }
        public virtual RestrictedAccessCollection<UpdaterPackageInfo> Packages{ get; set; } = new List<UpdaterPackageInfo>();

        public virtual RestrictedAccessCollection<Program> Programs { get; set; } = new List<Program>();

        public virtual DeveloperAccount DeveloperAccount { get; set; }

        public bool IsPublic { get; set; }
    }


}
