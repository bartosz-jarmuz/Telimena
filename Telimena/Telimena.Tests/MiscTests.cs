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
            header.DateTime = new TelemetryInfoHeaderItem() { type = "datetime" };
            header.ComponentName = new TelemetryInfoHeaderItem();


            var serialized = JsonConvert.SerializeObject(header);

        }

    }
}