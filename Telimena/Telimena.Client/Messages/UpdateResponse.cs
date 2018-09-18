using System.Collections.Generic;

namespace Telimena.Client
{
    public class UpdateResponse : TelimenaResponseBase
    {
        public IReadOnlyList<UpdatePackageData> UpdatePackages { get; set; } = new List<UpdatePackageData>();
    }
}