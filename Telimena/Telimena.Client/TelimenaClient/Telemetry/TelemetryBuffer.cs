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
using System.Linq;
using System.Text;

namespace TelimenaClient 
{
    /// <summary>
    /// Accumulates <see cref="TelemetryItem"/> items for efficient transmission. Based on Application insights solution (MIT)
    /// </summary>
    internal class TelemetryBuffer
    {
        /// <summary>
        /// Delegate that is raised when the buffer is full.
        /// </summary>
        private Action onFull;

        private const int DefaultCapacity = 500;
        private const int DefaultBacklogSize = 1000000;
        private readonly object lockObj = new object();
        private int capacity = DefaultCapacity;
        private int backlogSize = DefaultBacklogSize;
        private int minimumBacklogSize = 1001;
        private List<TelemetryItem> items;
        private bool itemDroppedMessageLogged = false;

        internal TelemetryBuffer()
        {
            this.items = new List<TelemetryItem>(this.Capacity);
        }

        /// <summary>
        /// Gets or sets the maximum number of telemetry items that can be buffered before transmission.
        /// </summary>        
        private int Capacity
        {
            get => this.capacity;

            set
            {
                if (value < 1)
                {
                    this.capacity = DefaultCapacity;
                    return;
                }

                if (value > this.backlogSize)
                {
                    this.capacity = this.backlogSize;
                    return;
                }

                this.capacity = value;
            }
        }

        /// <summary>
        /// Gets or sets the maximum number of telemetry items that can be in the backlog to send. Items will be dropped
        /// once this limit is hit.
        /// </summary>        
        private int BacklogSize
        {
            get => this.backlogSize;

            set
            {
                if (value < this.minimumBacklogSize)
                {
                    this.backlogSize = this.minimumBacklogSize;
                    return;
                }

                if (value < this.capacity)
                {
                    this.backlogSize = this.capacity;
                    return;
                }

                this.backlogSize = value;
            }
        }

        public void SetOnFullEvent(Action onBufferFull)
        {
            this.onFull = onBufferFull;
        }

        public void Enqueue(TelemetryItem item)
        {
            if (item == null)
            {
                //todo - log CoreEventSource.Log.LogVerbose("item is null in TelemetryBuffer.Enqueue");
                return;
            }

            lock (this.lockObj)
            {
                if (this.items.Count >= this.BacklogSize)
                {
                    if (!this.itemDroppedMessageLogged)
                    {
                        //todo - log CoreEventSource.Log.ItemDroppedAsMaximumUnsentBacklogSizeReached(this.BacklogSize);
                        this.itemDroppedMessageLogged = true;
                    }

                    return;
                }

                this.items.Add(item);
                if (this.items.Count >= this.Capacity)
                {
                     this.onFull?.Invoke();
                }
            }
        }

        public virtual IEnumerable<TelemetryItem> Dequeue()
        {
            List<TelemetryItem> telemetryToFlush = null;

            if (this.items.Count > 0)
            {
                lock (this.lockObj)
                {
                    if (this.items.Count > 0)
                    {
                        telemetryToFlush = this.items;
                        this.items = new List<TelemetryItem>(this.Capacity);
                        this.itemDroppedMessageLogged = false;
                    }
                }
            }

            return telemetryToFlush;
        }
    }
}
