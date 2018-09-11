namespace Telimena.WebApp.Core.Models
{
    public class UpdaterPackageInfo : RepositoryFileBase, IRepositoryFile
    {
        public const string UpdaterFileName = "Updater.zip";
        protected UpdaterPackageInfo() : base(){ }
        public UpdaterPackageInfo(string version, long fileSizeBytes) : base(UpdaterFileName, fileSizeBytes)
        {
            this.Version = version;
        }

        public int Id { get; set; }
        public string Version { get; set; }

        public virtual TelimenaToolkitData ToolkitData { get; set; }
    }
}
