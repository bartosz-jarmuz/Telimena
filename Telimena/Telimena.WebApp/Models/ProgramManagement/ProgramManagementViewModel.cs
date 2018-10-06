using System.Collections.Generic;
using System.Web.Mvc;
using Telimena.WebApp.Core.Models;

namespace Telimena.WebApp.Models.ProgramDetails
{
    public class ProgramManagementViewModel
    {
        public string ProgramName { get; set; }
        public int ProgramId { get; set; }

        public ICollection<ProgramUpdatePackageInfo> UpdatePackages { get; set; } = new List<ProgramUpdatePackageInfo>(); //todo replace with VM

        public string ProgramDownloadUrl { get; set; }
        public ProgramPackageInfo ProgramPackageInfo { get; set; }

        public int SelectedUpdaterId { get;set; }

        public List<SelectListItem> UpdatersSelectList { get; set; }
    }
}