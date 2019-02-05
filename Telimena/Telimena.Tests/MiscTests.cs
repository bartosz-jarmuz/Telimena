using System;
using System.Diagnostics;
using Newtonsoft.Json;
using NUnit.Framework;
using Telimena.WebApp.Core.DTO;

namespace Telimena.Tests
{
    [TestFixture]
    public class MiscTests
    {
        [Test]
        public void ValidateExpandoObject()
        {
            TelemetryInfoTableHeader header = new TelemetryInfoTableHeader();
            header.Date = new TelemetryInfoHeaderItem() { type = "datetime" };
            header.ComponentName = new TelemetryInfoHeaderItem();


            var serialized = JsonConvert.SerializeObject(header);

        }

        [Test]
        public void Main()
        {
            DoIt();
        }

        public void DoIt()
        {
            try
            {
                Trace.WriteLine("inner try");
                int i = 0;
                Trace.WriteLine(12 / i); // oops
            }
            catch (Exception e)
            {
                Trace.WriteLine("inner catch");
                throw e; // or "throw", or "throw anything"
            }
            finally
            {
                Trace.WriteLine("inner finally");
            }
        }

    }
}