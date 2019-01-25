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
using System.Runtime.Remoting.Messaging;

namespace TelimenaClient
{
    /// <summary>
    /// Helps to define whether thread is performing SDK internal operation at the moment.
    /// Taken from AppInsights SDK (MIT)
    /// </summary>
    public static class SdkInternalOperationsMonitor
    {
        private const string InternalOperationsMonitorSlotName = "Telimena.Telemetry.InternalSdkOperation";

        private static readonly object syncObj = new object();

        /// <summary>
        /// Determines whether the current thread executing the internal operation.
        /// </summary>
        /// <returns>true if the current thread executing the internal operation; otherwise, false.</returns>
        public static bool IsEntered()
        {
            object data = null;
            try
            {
                data = CallContext.LogicalGetData(InternalOperationsMonitorSlotName);
            }
            catch (Exception)
            {
                // CallContext may fail in partially trusted environment
            }

            return data != null;
        }

        /// <summary>
        /// Marks the thread as executing the internal operation.
        /// </summary>
        public static void Enter()
        {
            try
            {
                CallContext.LogicalSetData(InternalOperationsMonitorSlotName, syncObj);
            }
            catch (Exception)
            {
                // CallContext may fail in partially trusted environment
            }
        }

        /// <summary>
        /// Unmarks the thread as executing the internal operation.
        /// </summary>
        public static void Exit()
        {
            try
            {
                CallContext.FreeNamedDataSlot(InternalOperationsMonitorSlotName);
            }
            catch (Exception)
            {
                // CallContext may fail in partially trusted environment
            }
        }
    }
}