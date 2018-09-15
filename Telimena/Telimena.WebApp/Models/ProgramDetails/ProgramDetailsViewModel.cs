using System.Collections.Generic;

namespace Telimena.WebApp.Models.ProgramDetails
{
    using Core.Models;

    public class ProgramDetailsViewModel
    {
        public string ProgramName { get; set; }
        public int ProgramId { get; set; }

        public ICollection<ProgramUpdatePackageInfo> UpdatePackages { get; set; } = new List<ProgramUpdatePackageInfo>(); //todo replace with VM

        public string ProgramDownloadUrl { get; set; }
        public ProgramPackageInfo ProgramPackageInfo { get; set; }
    }
}