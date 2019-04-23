using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using TelimenaClient.Model;

namespace TelimenaClient
{
    /// <summary>
    /// Class Extensions.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// Determines whether [is directory writable] [the specified throw if fails].
        /// </summary>
        /// <param name="dir">The dir.</param>
        /// <param name="throwIfFails">if set to <c>true</c> [throw if fails].</param>
        /// <returns><c>true</c> if [is directory writable] [the specified throw if fails]; otherwise, <c>false</c>.</returns>
        public static bool IsDirectoryWritable(this DirectoryInfo dir, bool throwIfFails = false)
        {
            return dir.FullName.IsDirectoryWritable(throwIfFails);
        }

        /// <summary>
        /// Determines whether [is directory writable] [the specified throw if fails].
        /// </summary>
        /// <param name="dirPath">The dir path.</param>
        /// <param name="throwIfFails">if set to <c>true</c> [throw if fails].</param>
        /// <returns><c>true</c> if [is directory writable] [the specified throw if fails]; otherwise, <c>false</c>.</returns>
        public static bool IsDirectoryWritable(this string dirPath, bool throwIfFails = false)
        {
            try
            {
                using (FileStream fs = File.Create(
                    Path.Combine(
                        dirPath,
                        Path.GetRandomFileName()
                    ),
                    1,
                    FileOptions.DeleteOnClose)
                )
                { }
                return true;
            }
            catch
            {
                if (throwIfFails)
                    throw;
                else
                    return false;
            }
        }

        /// <summary>
        /// Returns 1 if first version is larger, -1 if version is smaller and 0 if they are equal.
        /// Expects a version in format "1.0.0.0", between 2 and 4 segments
        /// <para>
        /// The 'substituteForMissingParts' parameter determines whether string 1.0 should be treated as equivalent to
        /// 1.0.0.0
        /// </para><para>Default AssemblyVersion class parse returns -1 for each missing component (essentially 1.0 is like 1.0.-1.-1)</para><para>So, consequently, 1.0.0 is larger than 1.0, which would be nonsense</para>
        /// </summary>
        /// <param name="currentVersionString">The current version string.</param>
        /// <param name="comparisonVersionString">The comparison version string.</param>
        /// <param name="substituteForMissingParts">Replaces -1 with 0 in the parsed version objects</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="ArgumentException">
        /// Error while parsing [{(object) currentVersionString}
        /// or
        /// Error while parsing [{(object) comparisonVersionString}
        /// </exception>
        public static int CompareVersionStrings(this string currentVersionString, string comparisonVersionString, bool substituteForMissingParts = true)
        {
            Version version1;
            Version version2;
            try
            {
                version1 = Version.Parse(currentVersionString);
                if (substituteForMissingParts)
                {
                    version1 = FixZerosInVersionString(version1, currentVersionString);
                }
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Error while parsing [{(object) currentVersionString}] as AssemblyVersion.", ex);
            }

            try
            {
                version2 = Version.Parse(comparisonVersionString);
                if (substituteForMissingParts)
                {
                    version2 = FixZerosInVersionString(version2, comparisonVersionString);
                }
            }
            catch (ArgumentException ex)
            {
                throw new ArgumentException($"Error while parsing [{(object) comparisonVersionString}] as AssemblyVersion.", ex);
            }

            return version1.CompareTo(version2);
        }

        /// <summary>
        /// Gets the maximum version.
        /// </summary>
        /// <param name="packages">The packages.</param>
        /// <returns>System.String.</returns>
        public static string GetMaxVersion(this IEnumerable<UpdatePackageData> packages)
        {
            return packages.OrderByDescending(x => x.Version, new TelimenaVersionStringComparer()).FirstOrDefault()?.Version;
        }

        private static readonly string[] SizeSuffixes = new string[9]
        {
            "bytes",
            "KB",
            "MB",
            "GB",
            "TB",
            "PB",
            "EB",
            "ZB",
            "YB"
        };

        /// <summary>
        /// Gets the file / folder size string in largest unit (KB, MB, GB etc)
        /// </summary>
        /// <param name="value">The value.</param>
        /// <param name="decimalPlaces">The decimal places.</param>
        /// <returns>System.String.</returns>
        public static string GetSizeString(this long value, int decimalPlaces = 1)
        {
            if (value < 0L)
                return "-" + (-value).GetSizeString(1);
            int index = 0;
            Decimal d;
            for (d = (Decimal)value; Math.Round(d, decimalPlaces) >= new Decimal(1000) && index < 5; ++index)
                d /= new Decimal(1024);
            return string.Format((IFormatProvider)CultureInfo.InvariantCulture, "{0:n" + (object)decimalPlaces + "} {1}", (object)d, (object)SizeSuffixes[index]);
        }

        /// <summary>
        /// Checks whether a version string is larger than a comparison one.
        /// Expects a version in format "1.0.0.0", between 2 and 4 segments
        /// <para>
        /// The 'substituteForMissingParts' parameter determines whether string 1.0 should be treated as equivalent to
        /// 1.0.0.0
        /// </para><para>Default AssemblyVersion class parse returns -1 for each missing component (essentially 1.0 is like 1.0.-1.-1)</para><para>So, consequently, 1.0.0 is larger than 1.0, which would be nonsense</para>
        /// </summary>
        /// <param name="currentVersionString">The current version string.</param>
        /// <param name="comparisonVersionString">The comparison version string.</param>
        /// <param name="substituteForMissingParts">Replaces -1 with 0 in the parsed version objects</param>
        /// <returns><c>true</c> if [is newer version than] [the specified comparison version string]; otherwise, <c>false</c>.</returns>
        public static bool IsNewerVersionThan(this string currentVersionString, string comparisonVersionString, bool substituteForMissingParts = true)
        {
            int result = currentVersionString.CompareVersionStrings(comparisonVersionString, substituteForMissingParts);
            return result > 0;
        }

        /// <summary>
        /// Fixes the zeros in version string.
        /// </summary>
        /// <param name="version">The version.</param>
        /// <param name="versionString">The version string.</param>
        /// <returns>Version.</returns>
        private static Version FixZerosInVersionString(Version version, string versionString)
        {
            if (version.Minor == -1)
            {
                versionString += ".0";
                version = Version.Parse(versionString);
            }

            if (version.Build == -1)
            {
                versionString += ".0";
                version = Version.Parse(versionString);
            }

            if (version.Revision == -1)
            {
                versionString += ".0";
                version = Version.Parse(versionString);
            }

            return version;
        }
    }
}