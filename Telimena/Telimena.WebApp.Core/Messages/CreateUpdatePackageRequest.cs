namespace Telimena.WebApp.Core.Messages
{
    public class CreateUpdatePackageRequest
    {
        public int ProgramId { get; set; }
        public string PackageVersion { get; set; }
        public bool IsBeta { get; set; }
    }
}