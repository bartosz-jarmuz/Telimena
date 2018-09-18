using System;

namespace Telimena.Client
{
    public abstract class TelimenaResponseBase
    {
        public Exception Exception { get; set; }
    }
}