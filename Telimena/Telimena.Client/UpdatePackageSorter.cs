using System.Collections.Generic;

namespace Telimena.Client
{
    internal static class UpdatePackageSorter
    {
        public static List<UpdatePackageData> Sort(IEnumerable<UpdatePackageData> packages)
        {
            if (packages == null)
            {
                return null;
            }
            return new List<UpdatePackageData>(packages);
        }
    }
}