using System.ComponentModel.DataAnnotations.Schema;

namespace Telimena.WebApp.Core.Models
{
    public class TelimenaPackageInfo : RepositoryFileBase, IRepositoryFile
    {
        private const string PackageFileName = "Telimena.Client.zip";
        protected TelimenaPackageInfo() : base() { }
        public TelimenaPackageInfo(string version, long fileSizeBytes) : base(PackageFileName, fileSizeBytes)
        {
            this.Version = version;
        }
        [ForeignKey(nameof(TelimenaToolkitData))]
        public int Id { get; set; }
        public string Version { get; set; }
        public bool IsBeta { get; set; }
        public bool IntroducesBreakingChanges { get; set; }
        public virtual TelimenaToolkitData TelimenaToolkitData { get; set; }
    }
}