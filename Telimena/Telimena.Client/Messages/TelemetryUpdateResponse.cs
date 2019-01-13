using System;
using System.Net.Http;

namespace TelimenaClient
{
    /// <summary>
    /// The telemetry response
    /// </summary>
    public class TelemetryUpdateResponse : TelimenaResponseBase
    {
        /// <summary>
        /// The action result
        /// </summary>
        public HttpResponseMessage Result { get; set; }
    }
}