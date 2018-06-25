namespace Telimena.WebApp.Core.Messages
{
    public class SetLatestVersionRequest
    {
        public int ProgramId { get; set; }
        public string Version { get; set; }
    }
}