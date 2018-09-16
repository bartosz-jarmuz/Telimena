namespace Telimena.Client
{
    using System.Collections.Generic;

 
    public class UpdateResponse : TelimenaResponseBase
    {
        public IReadOnlyList<UpdatePackageData> UpdatePackages { get; set; } = new List<UpdatePackageData>();
        public IReadOnlyList<UpdatePackageData> UpdatePackagesIncludingBeta { get; set; } = new List<UpdatePackageData>();

    }


}