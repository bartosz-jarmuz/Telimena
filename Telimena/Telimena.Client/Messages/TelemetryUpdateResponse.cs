using System;
using System.Net;
using System.Net.Http;

namespace TelimenaClient
{
    /// <summary>
    /// The telemetry response
    /// </summary>
    public class TelemetryUpdateResponse : TelimenaResponseBase
    {
        /// <summary>
        /// Do not use
        /// </summary>
        [Obsolete("For serialization")]
        protected TelemetryUpdateResponse()
        {

        }

        /// <summary>
        /// New instance
        /// </summary>
        /// <param name="httpResponse"></param>
        public TelemetryUpdateResponse(HttpResponseMessage httpResponse)
        {
            this.HttpResponse = httpResponse;
            this.StatusCode = httpResponse.StatusCode;
            this.ReasonPhrase = httpResponse.ReasonPhrase;
        }

        /// <summary>
        /// New instance 
        /// </summary>
        /// <param name="exception"></param>
        public TelemetryUpdateResponse(Exception exception)
        {
            this.Exception = exception;
        }

        /// <summary>
        /// Reason 
        /// </summary>
        public string ReasonPhrase { get; set; }

        /// <summary>
        /// Status code
        /// </summary>
        public HttpStatusCode StatusCode { get; set; }

        /// <summary>
        /// The action result
        /// </summary>
        public HttpResponseMessage HttpResponse { get; }

        /// <summary>
        /// Gets or sets the content.
        /// </summary>
        /// <value>The content.</value>
        public string Content { get; set; }
    }
}