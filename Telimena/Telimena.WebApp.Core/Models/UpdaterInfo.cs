namespace Telimena.WebApp.Core.Models
{
    public class UpdaterInfo : RepositoryFileBase, IRepositoryFile
    {
        public const string UpdaterFileName = "Updater.zip";
        protected UpdaterInfo() : base(){ }
        public UpdaterInfo(string version, long fileSizeBytes) : base(UpdaterFileName, fileSizeBytes)
        {
            this.Version = version;
        }

        public int Id { get; set; }
        public string Version { get; set; }

    }
}
