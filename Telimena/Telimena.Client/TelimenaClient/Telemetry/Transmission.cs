using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace TelimenaClient
{
    /// <summary>
    /// Implements an asynchronous transmission of data to an HTTP POST endpoint.
    /// </summary>
    public class Transmission
    {
        private const string ContentEncodingHeader = "Content-Encoding";

        private static readonly TimeSpan DefaultTimeout = TimeSpan.FromSeconds(100);
        private static HttpClient client = new HttpClient() { Timeout = System.Threading.Timeout.InfiniteTimeSpan };

        private int isSending;

        /// <summary>
        /// Initializes a new instance of the <see cref="Transmission"/> class.
        /// </summary>
        public Transmission(Uri address, byte[] content, string contentType, string contentEncoding, TimeSpan timeout = default(TimeSpan))
        {
            this.EndpointAddress = address ?? throw new ArgumentNullException(nameof(address));
            this.Content = content ?? throw new ArgumentNullException(nameof(content));
            this.ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            this.ContentEncoding = contentEncoding;
            this.Timeout = timeout == default(TimeSpan) ? DefaultTimeout : timeout;
            this.TelemetryItems = null;
        }

        ///// <summary>
        ///// Initializes a new instance of the <see cref="Transmission"/> class.
        ///// </summary>
        //public Transmission(Uri address, ICollection<TelemetryItem> telemetryItems, TimeSpan timeout = default(TimeSpan))
        //    : this(address, JsonSerializer.Serialize(telemetryItems, true), JsonSerializer.ContentType, JsonSerializer.CompressionType, timeout)
        //{
        //    this.TelemetryItems = telemetryItems;
        //}

        ///// <summary>
        ///// Initializes a new instance of the <see cref="Transmission"/> class. This overload is for Test purposes. 
        ///// </summary>
        //internal Transmission(Uri address, IEnumerable<TelemetryItem> telemetryItems, string contentType, string contentEncoding, TimeSpan timeout = default(TimeSpan))
        //    : this(address, JsonSerializer.Serialize(telemetryItems), contentType, contentEncoding, timeout)
        //{
        //}

        /// <summary>
        /// Initializes a new instance of the <see cref="Transmission"/> class. This overload is for Test purposes. 
        /// </summary>
        internal Transmission(Uri address, byte[] content, HttpClient passedClient, string contentType, string contentEncoding, TimeSpan timeout = default(TimeSpan))
            : this(address, content, contentType, contentEncoding, timeout)
        {
            client = passedClient;
        }


        /// <summary>
        /// Gets the Address of the endpoint to which transmission will be sent.
        /// </summary>
        public Uri EndpointAddress
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the content of the transmission.
        /// </summary>
        public byte[] Content
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the content's type of the transmission.
        /// </summary>
        public string ContentType
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the encoding method of the transmission.
        /// </summary>
        public string ContentEncoding
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets a timeout value for the transmission.
        /// </summary>
        public TimeSpan Timeout
        {
            get;
            internal set;
        }

        /// <summary>
        /// Gets the number of telemetry items in the transmission.
        /// </summary>
        public ICollection<TelemetryItem> TelemetryItems
        {
            get; private set;
        }

        /// <summary>
        /// Executes the request that the current transmission represents.
        /// </summary>
        /// <returns>The task to await.</returns>
        public virtual async Task<TelemetryUpdateResponse> SendAsync()
        {
            if (Interlocked.CompareExchange(ref this.isSending, 1, 0) != 0)
            {
                throw new InvalidOperationException("SendAsync is already in progress.");
            }

            try
            {
                using (MemoryStream contentStream = new MemoryStream(this.Content))
                {
                    HttpRequestMessage request = this.CreateRequestMessage(this.EndpointAddress, contentStream);
                    TelemetryUpdateResponse wrapper = null;

                    try
                    {
                        using (var ct = new CancellationTokenSource(this.Timeout))
                        {
                            // HttpClient.SendAsync throws HttpRequestException only on the following scenarios:
                            // "The request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout."
                            // i.e for Server errors (500 status code), no exception is thrown. Hence this method should read the response and status code,
                            // and return correct HttpWebResponseWrapper to give any Retry policies a chance to retry as needed.

                            using (var response = await client.SendAsync(request, ct.Token).ConfigureAwait(false))
                            {
                                if (response != null)
                                {
                                    wrapper = new TelemetryUpdateResponse(response)
                                    {
                                        StatusCode = response.StatusCode,
                                        ReasonPhrase = response.ReasonPhrase // maybe not required?
                                    };

                                    if (response.StatusCode == HttpStatusCode.PartialContent)
                                    {
                                        if (response.Content != null)
                                        {
                                            // Read the entire response body only on PartialContent for perf reasons.
                                            // This cannot be avoided as response tells which items are to be resubmitted.
                                            wrapper.Content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                                        }
                                    }

                                   
                                }
                            }
                        }
                    }
                    catch (TaskCanceledException e)
                    {
                        wrapper = new TelemetryUpdateResponse(e)
                        {
                            StatusCode = HttpStatusCode.RequestTimeout
                        };
                    }

                    return wrapper;
                }
            }
            finally
            {
                Interlocked.Exchange(ref this.isSending, 0);
            }
        }

        /// <summary>
        /// Creates an http request for sending a transmission.
        /// </summary>
        /// <param name="address">The address of the web request.</param>
        /// <param name="contentStream">The stream to write to.</param>
        /// <returns>The request. An object of type HttpRequestMessage.</returns>
        protected virtual HttpRequestMessage CreateRequestMessage(Uri address, Stream contentStream)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, address);
            request.Content = new StreamContent(contentStream);
            if (!string.IsNullOrEmpty(this.ContentType))
            {
                request.Content.Headers.ContentType = new MediaTypeHeaderValue(this.ContentType);
            }

            if (!string.IsNullOrEmpty(this.ContentEncoding))
            {
                request.Content.Headers.Add(ContentEncodingHeader, this.ContentEncoding);
            }

            return request;
        }

        
    }
}