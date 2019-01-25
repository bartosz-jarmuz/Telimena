//The MIT License(MIT)

//Copyright(c) 2015 Microsoft

//Permission is hereby granted, free of charge, to any person obtaining a copy
//of this software and associated documentation files (the "Software"), to deal
//in the Software without restriction, including without limitation the rights
//to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
//copies of the Software, and to permit persons to whom the Software is
//furnished to do so, subject to the following conditions:

//The above copyright notice and this permission notice shall be included in all
//copies or substantial portions of the Software.

//THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
//IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
//FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
//AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
//LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
//OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
//SOFTWARE.


using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TelimenaClient.Serializer;

namespace TelimenaClient
{
    /// <summary>
    /// A transmitter that will immediately send telemetry over HTTP. 
    /// Telemetry items are being sent when Flush is called, or when the buffer is full (An OnFull "event" is raised) or every 30 seconds.
    /// Based on AppInsights SDK (MIT)
    /// </summary>
    internal class InMemoryTransmitter : IDisposable
    {
        private readonly TelemetryBuffer buffer;
        private readonly ITelimenaSerializer serializer;
        private readonly Messenger messenger;
        private readonly ITelimenaProperties properties;

        /// <summary>
        /// A lock object to serialize the sending calls from Flush, OnFull event and the Runner.  
        /// </summary>
        private readonly object sendingLockObj = new object();
        private AutoResetEvent startRunnerEvent;
        private bool enabled = true;

        /// <summary>
        /// The number of times this object was disposed.
        /// </summary>
        private int disposeCount;

        internal InMemoryTransmitter(TelemetryBuffer buffer, ITelimenaSerializer serializer, Messenger messenger, ITelimenaProperties properties)
        {
            this.buffer = buffer;
            this.serializer = serializer;
            this.messenger = messenger;
            this.properties = properties;
            this.buffer.SetOnFullEvent(this.OnBufferFull);

            // Starting the Runner
            Task.Factory.StartNew(this.Runner, CancellationToken.None, TaskCreationOptions.LongRunning, TaskScheduler.Default)
                .ContinueWith(
                    task =>
                    {
                        //string msg = string.Format(CultureInfo.InvariantCulture, "InMemoryTransmitter: Unhandled exception in Runner: {0}", task.Exception);
                        //todo - log CoreEventSource.Log.LogVerbose(msg);
                    },
                    TaskContinuationOptions.OnlyOnFaulted);
        }

        private TimeSpan SendingInterval { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Flushes the in-memory buffer and sends it.
        /// </summary>
        internal void Flush(TimeSpan timeout)
        {
            SdkInternalOperationsMonitor.Enter();
            try
            {
                this.DequeueAndSend(timeout);
            }
            finally
            {
                SdkInternalOperationsMonitor.Exit();
            }
        }

        /// <summary>
        /// Flushes the in-memory buffer and sends the telemetry items in <see cref="SendingInterval"/> intervals or when 
        /// <see cref="startRunnerEvent" /> is set.
        /// </summary>
        private void Runner()
        {
            SdkInternalOperationsMonitor.Enter();
            try
            {
                using (this.startRunnerEvent = new AutoResetEvent(false))
                {
                    while (this.enabled)
                    {
                        // Pulling all items from the buffer and sending as one transmission.
                        this.DequeueAndSend(timeout: default(TimeSpan)); // when default(TimeSpan) is provided, value is ignored and default timeout of 100 sec is used

                        // Waiting for the flush delay to elapse
                        this.startRunnerEvent.WaitOne(this.SendingInterval);
                    }
                }
            }
            finally
            {
                SdkInternalOperationsMonitor.Exit();
            }
        }

        /// <summary>
        /// Happens when the in-memory buffer is full. Flushes the in-memory buffer and sends the telemetry items.
        /// </summary>
        private void OnBufferFull()
        {
            this.startRunnerEvent.Set();
            //todo - log CoreEventSource.Log.LogVerbose("StartRunnerEvent set as Buffer is full.");
        }

        /// <summary>
        /// Flushes the in-memory buffer and send it.
        /// </summary>
        private void DequeueAndSend(TimeSpan timeout)
        {
            lock (this.sendingLockObj)
            {
                IEnumerable<TelemetryItem> telemetryItems = this.buffer.Dequeue();
                try
                {
                    // send request
                    this.Send(telemetryItems, timeout).Wait();
                }
                catch (Exception e)
                {
                    //todo - log CoreEventSource.Log.FailedToSend(e.Message);
                }
            }
        }

        /// <summary>
        /// Serializes a list of telemetry items and sends them.
        /// </summary>
        private async Task Send(IEnumerable<TelemetryItem> telemetryItems, TimeSpan timeout)
        {
            List <string> data = null;
            
            if (telemetryItems != null)
            {

                foreach (var telemetryItem in telemetryItems)
                {
                    var serialized = this.serializer.Serialize(telemetryItem);
                    if (serialized != null)
                    {
                        data.Add(serialized);
                    }
                }
            }

            if (data == null || data.Count == 0)
            {
                //todo log - CoreEventSource.Log.LogVerbose("No Telemetry Items passed to Enqueue");
                return;
            }

            var request = new TelemetryUpdateRequest(this.properties.TelemetryKey, this.properties.LiveProgramInfo.UserId, data);

            HttpResponseMessage response = await this.messenger.SendPostRequest(ApiRoutes.PostTelemetryData, request).ConfigureAwait(false);
            
            //var transmission = new Transmission(this.endpointAddress, data, JsonSerializer.ContentType, JsonSerializer.CompressionType, timeout);

            //return transmission.SendAsync();
        }

        private void Dispose(bool disposing)
        {
            if (disposing && Interlocked.Increment(ref this.disposeCount) == 1)
            {
                // Stops the runner loop.
                this.enabled = false;

                if (this.startRunnerEvent != null)
                {
                    // Call Set to prevent waiting for the next interval in the runner.
                    try
                    {
                        this.startRunnerEvent.Set();
                    }
                    catch (ObjectDisposedException)
                    {
                        // We need to try catch the Set call in case the auto-reset event wait interval occurs between setting enabled
                        // to false and the call to Set then the auto-reset event will have already been disposed by the runner thread.
                    }
                }

                this.Flush(default(TimeSpan));
            }
        }
    }
}