namespace Telimena.Client
{
    using System;

    public abstract class TelimenaResponseBase
    {
        public bool IsMessageSuccessful { get;  set; } = true;
        public Exception Exception { get; set; }
    }
}