namespace Telimena.WebApp.Core.Models
{
    public class UpdatePackageInfo : RepositoryFileBase, IRepositoryFile
    {
        protected UpdatePackageInfo() : base(){ }
        public UpdatePackageInfo(string fileName, int programId, string version, long fileSizeBytes) : base(fileName, fileSizeBytes)
        {
            this.ProgramId = programId;
            this.Version = version;
        }

        public int Id { get; set; }
        public int ProgramId { get; set; }
        public string Version { get; set; }

        public bool IsBeta { get; set; }


        public bool IsStandalone { get; set; }
    }
}
