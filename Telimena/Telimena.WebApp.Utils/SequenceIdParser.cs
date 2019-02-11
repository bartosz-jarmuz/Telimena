using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telimena.WebApp.Utils
{
    public static class SequenceIdParser
    {
        public static string GetPrefix(string sequenceId)
        {
            if (sequenceId.IndexOf(':') < 0)
            {
                return sequenceId;
            }
            return sequenceId.Remove(sequenceId.LastIndexOf(':'));
        }

        public static int GetOrder(string sequenceId)
        {
            if (sequenceId.IndexOf(':') < 0)
            {
                return 0;
            }
            var number = sequenceId.Substring(sequenceId.LastIndexOf(':')+1);

            return Int32.Parse(number);
        }
    }
}
