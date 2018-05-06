namespace Telimena.WebApp.Core.Models
{
    using System.Collections.Generic;

    public class Program
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public virtual Developer Developer { get; set; }
        public int? DeveloperId { get; set; }
        public virtual ICollection<ProgramAssembly> Assemblies { get; set; }
    }
}