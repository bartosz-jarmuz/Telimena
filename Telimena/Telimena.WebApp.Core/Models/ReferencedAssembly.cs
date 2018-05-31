namespace Telimena.WebApp.Core.Models
{
    public class ReferencedAssembly : AssemblyData
    {
        public int Id { get; set; }
        public virtual Program Program { get; set; }
        public int ProgramId { get; set; }
    }
}