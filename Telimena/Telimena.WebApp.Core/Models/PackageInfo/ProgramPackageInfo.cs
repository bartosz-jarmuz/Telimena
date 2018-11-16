namespace Telimena.WebApp.Core.Models
{
    public class ProgramPackageInfo : RepositoryFileBase, IRepositoryFile
    {
        protected ProgramPackageInfo()
        {
        }

        public ProgramPackageInfo(string fileName, int programId, long fileSizeBytes, string supportedToolkitVersion) : base(fileName, fileSizeBytes)
        {
            this.ProgramId = programId;
            this.SupportedToolkitVersion = supportedToolkitVersion;
        }

        public int Id { get; set; }
        public int ProgramId { get; set; }
        public string SupportedToolkitVersion { get; set; }
    }
}