namespace Telimena.WebApp.Core.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Linq;

    public class Program
    {
        public int Id { get; set; }
        [Required]
        public virtual PrimaryAssembly PrimaryAssembly { get; set; }
        public virtual ICollection<ReferencedAssembly> Assemblies { get; set; } = new List<ReferencedAssembly>();
        public virtual ICollection<Function> Functions { get; set; } = new List<Function>();
        public virtual ICollection<ProgramUsage> Usages { get; set; } = new List<ProgramUsage>();
        public DateTime RegisteredDate { get; set; }
        [Required]
        public string Name { get; set; }
        public string Description { get; set; }

        public Developer Developer { get; set; }

        public ProgramUsage GetProgramUsage(int clientAppUserId)
        {
            return this.Usages.FirstOrDefault(x => x.ClientAppUser.Id == clientAppUserId);
        }
    }
}