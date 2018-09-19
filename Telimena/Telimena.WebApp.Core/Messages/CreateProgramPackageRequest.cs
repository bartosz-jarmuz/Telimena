namespace Telimena.WebApp.Core.Messages
{
    public class CreateProgramPackageRequest
    {
        public int ProgramId { get; set; }
        public string ToolkitVersionUsed { get; set; }
    }
}