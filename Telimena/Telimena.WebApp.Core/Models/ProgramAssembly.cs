namespace Telimena.WebApp.Core.Models
{
    public class ProgramAssembly
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int ProgramId { get; set; }
        public Program Program { get; set; }

    }
}