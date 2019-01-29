using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Api.V1;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Utils;
using TelimenaClient;
using TelimenaClient.Telemetry;

namespace Telimena.Tests
{
    

    [TestFixture]
    public class AppInsightsDeserializerTests
    {
        private readonly Guid testTelemetryKey = Guid.Parse("dc13cced-30ea-4628-a81d-21d86f37df95");

        [Test]
        public void TrackEventSendsEventTelemetryWithSpecifiedNameToProvideSimplestWayOfSendingEventTelemetry()
        {

            List<ITelemetry> sentTelemetry = new List<ITelemetry>();
            TelemetryModule telemetryModule = Helpers.GetTelemetryModule(sentTelemetry, this.testTelemetryKey);

            for (int i = 0; i < 250; i++)
            {
                telemetryModule.Event("TestEvent");
                telemetryModule.View("TestView");
            }

            byte[] serialized = JsonSerializer.Serialize(sentTelemetry, true);


            var items = AppInsightsDeserializer.Deserialize(serialized, true).ToList();
            var mapped = AppInsightsTelemetryAdapter.Map(items);

        }

    }
}