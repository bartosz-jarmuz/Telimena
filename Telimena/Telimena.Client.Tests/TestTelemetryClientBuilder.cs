using System;
using NUnit.Framework;

namespace TelimenaClient.Tests
{
    [TestFixture]
    public class TestTelemetryClientBuilder
    {
        private readonly Guid TestTelemetryKey = Guid.Parse("dc13cced-30ea-4628-a81d-21d86f37df95");

        [Test]
        public void Test__NoInstrumentationKey_ClientCanBeBuiltOk()
        {
            var si = new TelimenaStartupInfo(this.TestTelemetryKey, Helpers.TeliUri);
            var props = new TelimenaProperties(si);
            var builder = new TelemetryClientBuilder(props);
            var client = builder.GetClient();
            Assert.AreEqual("",client.InstrumentationKey);
            var channel = client.TelemetryConfiguration.TelemetryChannel as TelimenaInMemoryChannel;
            Assert.IsNotNull(channel);
            var transmitter = channel.transmitter as TelimenaInMemoryTransmitter;
            Assert.IsNotNull(transmitter);
            Assert.That(()=> transmitter.DeliverySettings.AppInsightsEndpointEnabled == false);
            Assert.That(()=> transmitter.DeliverySettings.TelimenaTelemetryEndpoint.ToString(), Is.EqualTo(Telimena.DefaultApiUri+ "api/v1/telemetry"));
            Assert.That(() => transmitter.SendingInterval == TimeSpan.FromSeconds(30));

        }

        [Test]
        public void Test__InstrumentationKey_DeliverySettingsProvided()
        {
            var si = new TelimenaStartupInfo(this.TestTelemetryKey, Helpers.TeliUri);
            si.DeliveryInterval = TimeSpan.FromHours(667);
            si.InstrumentationKey = "yo, MSFT-o";
            var props = new TelimenaProperties(si);
            var builder = new TelemetryClientBuilder(props);
            var client = builder.GetClient();

            Assert.AreEqual("yo, MSFT-o", client.TelemetryConfiguration.InstrumentationKey);
            Assert.AreEqual("yo, MSFT-o", client.InstrumentationKey);
            var channel = client.TelemetryConfiguration.TelemetryChannel as TelimenaInMemoryChannel;
            Assert.IsNotNull(channel);
            var transmitter = channel.transmitter as TelimenaInMemoryTransmitter;
            Assert.IsNotNull(transmitter);
            Assert.That(() => transmitter.SendingInterval == TimeSpan.FromHours(667));
            Assert.That(() => transmitter.DeliverySettings.AppInsightsEndpointEnabled == true);
        }

    }
}