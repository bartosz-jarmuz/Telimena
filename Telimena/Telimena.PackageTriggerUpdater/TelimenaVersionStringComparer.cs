using System;
using System.Collections.Generic;

namespace Telimena.PackageTriggerUpdater
{
    /// <summary>
    /// Compares versions
    /// </summary>
    public class TelimenaVersionStringComparer : IComparer<string>
    {
        /// <summary>
        /// Return 1 if version is newer, 0 if equal and -1 if older
        /// </summary>
        /// <param name="firstOne"></param>
        /// <param name="secondOne"></param>
        /// <returns></returns>
        public int Compare(string firstOne, string secondOne)
        {
            if (TelimenaVersionStringComparer.IsNewerVersionThan(firstOne, secondOne))
            {
                return 1;
            }

            return firstOne == secondOne ? 0 : -1;
        }

        /// <summary>
        ///     Checks whether a version string is larger than a comparison one.
        ///     Expects a version in format "1.0.0.0", between 2 and 4 segments
        ///     <para>
        ///         The 'substituteForMissingParts' parameter determines whether string 1.0 should be treated as equivalent to
        ///         1.0.0.0
        ///     </para>
        ///     <para>Default AssemblyVersion class parse returns -1 for each missing component (essentially 1.0 is like 1.0.-1.-1)</para>
        ///     <para>So, consequently, 1.0.0 is larger than 1.0, which would be nonsense</para>
        /// </summary>
        /// <param name="currentVersionString"></param>
        /// <param name="comparisonVersionString"></param>
        /// <param name="substituteForMissingParts">Replaces -1 with 0 in the parsed version objects</param>
        /// <returns></returns>
        public static bool IsNewerVersionThan(string currentVersionString, string comparisonVersionString, bool substituteForMissingParts = true)
        {
            int result = TelimenaVersionStringComparer.CompareVersionStrings(currentVersionString, comparisonVersionString, substituteForMissingParts);
            return result > 0;
        }


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

        /// <summary>
        ///     Returns 1 if first version is larger, -1 if version is smaller and 0 if they are equal.
        ///     Expects a version in format "1.0.0.0", between 2 and 4 segments
        ///     <para>
        ///         The 'substituteForMissingParts' parameter determines whether string 1.0 should be treated as equivalent to
        ///         1.0.0.0
        ///     </para>
        ///     <para>Default AssemblyVersion class parse returns -1 for each missing component (essentially 1.0 is like 1.0.-1.-1)</para>
        ///     <para>So, consequently, 1.0.0 is larger than 1.0, which would be nonsense</para>
        /// </summary>
        /// <param name="currentVersionString"></param>
        /// <param name="comparisonVersionString"></param>
        /// <param name="substituteForMissingParts">Replaces -1 with 0 in the parsed version objects</param>
        /// <returns></returns>
        public static int CompareVersionStrings(string currentVersionString, string comparisonVersionString, bool substituteForMissingParts = true)
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
                throw new ArgumentException($"Error while parsing [{(object)currentVersionString}] as AssemblyVersion.", ex);
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
                throw new ArgumentException($"Error while parsing [{(object)comparisonVersionString}] as AssemblyVersion.", ex);
            }

            return version1.CompareTo(version2);
        }

    }
}