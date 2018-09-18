using System.Collections.Generic;

namespace Telimena
{
    public class TelimenaVersionStringComparer : IComparer<string>
    {
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