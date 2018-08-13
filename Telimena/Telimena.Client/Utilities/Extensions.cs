using System;
using System.Collections.Generic;
using System.Linq;
using Telimena.Client;

namespace Telimena
{
    internal static class Extensions
    {

        public static string GetMaxVersion(this IEnumerable<UpdatePackageData> packages)
        {
            return packages.OrderBy(x => x.Version, new VersionStringComparer()).FirstOrDefault()?.Version;
        }

        public static bool IsNewerVersionThan(this string currentVersionString, string comparisonVersionString)
        {
            Version version1;
            try
            {
                version1 = Version.Parse(currentVersionString);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Error while parsing [{(object) currentVersionString}] as Version.", (Exception)ex);
            }
            try
            {
                Version.Parse(comparisonVersionString);
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Error while parsing [{(object) comparisonVersionString}] as Version.", (Exception)ex);
            }
            Version version2 = Version.Parse(comparisonVersionString);
            return version1 > version2;
        }
    }
}