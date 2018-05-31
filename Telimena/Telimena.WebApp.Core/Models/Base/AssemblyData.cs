namespace Telimena.WebApp.Core.Models
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;

    public class PrimaryAssembly : AssemblyData
    {
        [ForeignKey("Program")]
        public int Id { get; set; }
        [Required]
        public virtual Program Program { get; set; }
    }

    public class ReferencedAssembly : AssemblyData
    {
        public int Id { get; set; }
        public virtual Program Program { get; set; }
        public int ProgramId { get; set; }
    }

    public abstract class AssemblyData
    {
        public string Name { get; set; }
        public string Product { get; set; }
        public string Trademark { get; set; }
        public string Description { get; set; }
        public string Copyright { get; set; }
        public string Title { get; set; }
        public string Company { get; set; }
        public string FullName { get; set; }
        public string Version { get; set; }
    }
}