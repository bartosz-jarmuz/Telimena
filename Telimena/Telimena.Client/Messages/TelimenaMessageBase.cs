namespace Telimena.Client
{
    using System;

    public abstract class TelimenaResponseBase
    {
        public Exception Error { get; set; }
    }

}