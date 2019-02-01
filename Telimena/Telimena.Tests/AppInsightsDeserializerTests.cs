using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using DotNetLittleHelpers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility.Implementation;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Api.V1;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Utils;
using TelimenaClient;
using TelimenaClient.Model;

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

                telemetryModule.Event("TestEvent", new Dictionary<string, string>(){{"AKey", $"AValue"}});
                telemetryModule.View("TestView");

            byte[] serialized = JsonSerializer.Serialize(sentTelemetry, true);


            var items = AppInsightsDeserializer.Deserialize(serialized, true).ToList();
            var mapped = AppInsightsTelemetryMapper.Map(items).ToList();

            for (int index = 0; index < sentTelemetry.Count; index++)
            {
                ITelemetry appInsightsItem = sentTelemetry[index];
                TelemetryItem telimenaItem = mapped[index];

                Assert.AreEqual(appInsightsItem.Timestamp, telimenaItem.Timestamp);
                Assert.AreEqual(appInsightsItem.Sequence, telimenaItem.Sequence);
                Assert.AreEqual(appInsightsItem.Context.User.Id, telimenaItem.UserId);
                Assert.AreEqual(appInsightsItem.GetPropertyValue<string>("Name"), telimenaItem.EntryKey);
                foreach (KeyValuePair<string, string> keyValuePair in appInsightsItem.GetPropertyValue<ConcurrentDictionary<string, string>>("Properties"))
                {
                    var props = typeof(TelimenaContextPropertyKeys).GetProperties().Select(x => x.Name);
                    if (!props.Contains(keyValuePair.Key))
                    {
                        Assert.AreEqual(keyValuePair.Value, telimenaItem.TelemetryData[keyValuePair.Key]);
                    }
                }
            }
        }

    }
}