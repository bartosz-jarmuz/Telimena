using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telimena.Client.Model
{
    public class UpdateCheckResult
    {
        public bool IsUpdateAvailable => this.PrimaryAssemblyUpdateInfo != null || (this.HelperAssembliesToUpdate != null && this.HelperAssembliesToUpdate.Count != 0);
        public Exception Error { get; set; }
        public AssemblyUpdateInfo PrimaryAssemblyUpdateInfo { get; set; }
        public ICollection<AssemblyUpdateInfo> HelperAssembliesToUpdate { get; set; } = new List<AssemblyUpdateInfo>();
    }
}
