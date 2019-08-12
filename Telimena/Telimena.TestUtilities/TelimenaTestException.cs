using System;

namespace Telimena.TestUtilities
{
    //just wrap the errors we're throwing in a custom type so that they stand out more easily in logs
    public class TelimenaTestException : Exception
    {
        public TelimenaTestException(string message) : base(message)
        {
        }

        public TelimenaTestException(string message, Exception innerException) : base(message, innerException)
        {
        }

    }
}