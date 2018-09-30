namespace Telimena.WebApp.Core.Messages
{
    public class UploadUpdaterRequest
    {
        public int ProgramId { get; set; }
        public string UpdaterInternalName { get; set; }

        public string MinimumCompatibleToolkitVersion { get; set; } = "0.0.0.0";
    }
}