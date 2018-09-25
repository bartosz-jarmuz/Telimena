namespace Telimena.WebApp.Core.Models
{
    public class TelimenaToolkitData
    {
        protected TelimenaToolkitData()
        {
        }

        public TelimenaToolkitData(string version)
        {
            this.Version = version;
        }

        public int Id { get; set; }
        public string Version { get; set; }
        public virtual TelimenaPackageInfo TelimenaPackageInfo { get; set; }

        public bool IntroducesBreakingChanges { get; set; }

    }
}