using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Telimena.WebApp.Models.ProgramDetails
{
    using Core.Models;

    public class ProgramDetailsViewModel
    {
        public string ProgramName { get; set; }
        public int ProgramId { get; set; }

        public ICollection<UpdatePackage> UpdatePackages { get; set; } = new List<UpdatePackage>(); //todo replace with VM
    }
}