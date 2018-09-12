using System.Collections.Generic;
using DotNetLittleHelpers;

namespace Telimena.WebApp.Core.Models
{
    using System;

    public class TelimenaToolkitData 
    {
        protected TelimenaToolkitData() : base() { }
        public TelimenaToolkitData(string version)
        {
            this.Version = version;
        }
        public int Id { get; set; }
        public string Version { get; set; }
        public TelimenaPackageInfo TelimenaPackageInfo { get; set; }

    }
}
