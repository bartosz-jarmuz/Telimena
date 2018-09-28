using System.Collections.Generic;

namespace Telimena.ToolkitClient
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
            if (firstOne.IsNewerVersionThan(secondOne))
            {
                return 1;
            }

            return firstOne == secondOne ? 0 : -1;
        }
    }
}