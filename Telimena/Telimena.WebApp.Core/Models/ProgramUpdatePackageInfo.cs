namespace Telimena.WebApp.Core.Models
{
    public class ProgramUpdatePackageInfo : RepositoryFileBase, IRepositoryFile
    {
        protected ProgramUpdatePackageInfo() : base(){ }
        public ProgramUpdatePackageInfo(string fileName, int programId, string version, long fileSizeBytes, string supportedToolkitVersion) : base(fileName, fileSizeBytes)
        {
            this.ProgramId = programId;
            this.Version = version;
            this.SupportedToolkitVersion = supportedToolkitVersion;
        }

        public int Id { get; set; }
        public int ProgramId { get; set; }
        public string Version { get; set; }
        public string SupportedToolkitVersion { get; set; }

        public bool IsBeta { get; set; }


        public bool IsStandalone { get; set; }
    }
}
