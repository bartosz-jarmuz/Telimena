namespace Telimena.Client
{
    public class UpdateRequest
    {
        public int ProgramId { get; set; }

        public int UserId { get; set; }

        public string ProgramVersion { get; set; }

        public string ToolkitVersion { get; set; }

        public bool AcceptBeta { get; set; }
    }
}