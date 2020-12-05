using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotNetLittleHelpers;
using FluentAssertions;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Api.V1;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Utils;
using TelimenaClient;
using JsonSerializer = Microsoft.ApplicationInsights.Extensibility.Implementation.JsonSerializer;
using LogLevel = TelimenaClient.Model.LogLevel;
using TelimenaContextPropertyKeys = TelimenaClient.Model.TelimenaContextPropertyKeys;

namespace Telimena.Tests
{

    [TestFixture]
    public class TelemetryUserPropertiesTests
    {
        private readonly Guid testTelemetryKey = Guid.Parse("dc13cced-30ea-4628-a81d-21d86f37df95");


        class FakeSettingsProvider : IRemoteSettingsProvider
        {
            public async Task<string> GetUserTrackingSettings(Guid telemetryKey)
            {
                await Task.Delay(0);
                return @"{""UserIdentifierMode"":0,""ShareIdentifierWithOtherTelimenaApps"":false}";
            }
        }

        [Test]
        public async Task TestNoTelemetryLost()
        {
            string userName = "Homer";
            List<ITelemetry> sentTelemetry = new List<ITelemetry>();
            var teli = Helpers.GetCrackedTelimena(sentTelemetry, this.testTelemetryKey, userName);

            teli.Track.View("TestView");
            teli.Track.Event("TestEvent");

            Assert.AreEqual(0, sentTelemetry.Count);
            await Task.Delay(20000);

            Assert.AreEqual(1, sentTelemetry.Count(x => (x as PageViewTelemetry)?.Name == "TestView"));
            Assert.AreEqual(1, sentTelemetry.Count(x => (x as EventTelemetry)?.Name == "TestEvent"));
            Assert.AreEqual(1, sentTelemetry.Count(x => (x as EventTelemetry)?.Name == "TelimenaSessionStarted"));

            foreach (var item in sentTelemetry)
            {
                Assert.AreEqual(userName, item.Context.User.Id);
            }

            Assert.AreEqual(3, sentTelemetry.Count, string.Join(", ", sentTelemetry.Select(x=>x.GetType().Name)));


         
        }

        [Test]
        public async Task TestWithImmediateFlushNoTelemetryLost()
        {
            string userName = "Homer";
            List<ITelemetry> sentTelemetry = new List<ITelemetry>();
            var teli = Helpers.GetCrackedTelimena(sentTelemetry, this.testTelemetryKey, userName);

            teli.Track.View("TestView");
            teli.Track.Event("TestEvent");
            teli.Track.SendAllDataNow();

            Assert.AreEqual(1, sentTelemetry.Count(x => (x as PageViewTelemetry)?.Name == "TestView"));
            Assert.AreEqual(1, sentTelemetry.Count(x => (x as EventTelemetry)?.Name == "TestEvent"));
            Assert.AreEqual(1, sentTelemetry.Count(x => (x as EventTelemetry)?.Name == "TelimenaSessionStarted"));

            foreach (var item in sentTelemetry)
            {
                Assert.AreEqual(userName, item.Context.User.Id);
            }

            Assert.AreEqual(3, sentTelemetry.Count, string.Join(", ", sentTelemetry.Select(x => x.GetType().Name)));



        }
    }

    [TestFixture]
    public class AppInsightsDeserializerTests
    {
        private readonly Guid testTelemetryKey = Guid.Parse("dc13cced-30ea-4628-a81d-21d86f37df95");


        [Test]
        public void TestEvent()
        {

            List<ITelemetry> sentTelemetry = new List<ITelemetry>();
            var teli = Helpers.GetCrackedTelimena(sentTelemetry, this.testTelemetryKey, "ASD", true);

            teli.Track.Event("TestEvent", new Dictionary<string, string>()
            {
                {"AKey", $"AValue"},
                {"AKey2", $"AValue2"}
            });
            teli.Track.SendAllDataNow();
            List<TelemetryItem> mapped = DoTheMapping(sentTelemetry);


            Assert.AreEqual(TelemetryItemTypes.Event, mapped.Single().TelemetryItemType);
            Assert.AreEqual("AValue", mapped[0].Properties["AKey"]);
            Assert.AreEqual("AValue2", mapped[0].Properties["AKey2"]);
            Assert.AreEqual(3, mapped[0].Properties.Count);
            Assert.IsNotNull(mapped[0].Properties.SingleOrDefault(x=>x.Value == this.testTelemetryKey.ToString()));
        }

        [Test]
        public void TestException()
        {

            List<ITelemetry> sentTelemetry = new List<ITelemetry>();
            var teli = Helpers.GetCrackedTelimena(sentTelemetry, this.testTelemetryKey, "ASD", true);

            teli.Track.Exception(new InvalidCastException("A Message", new InvalidOperationException("Inner")));
            teli.Track.SendAllDataNow();
            List<TelemetryItem> mapped = DoTheMapping(sentTelemetry);
            var exTelemetry = sentTelemetry.First() as ExceptionTelemetry;
            Assert.AreEqual(TelemetryItemTypes.Exception, mapped.Single().TelemetryItemType);
            Assert.AreEqual(exTelemetry.Exception.Message, mapped[0].Exceptions[0].Message);
            Assert.AreEqual(exTelemetry.Exception.InnerException.Message, mapped[0].Exceptions[1].Message);
            Assert.AreEqual(exTelemetry.Exception.GetType().FullName, mapped[0].Exceptions[0].TypeName);
            Assert.AreEqual(exTelemetry.Exception.InnerException.GetType().FullName, mapped[0].Exceptions[1].TypeName);

        }

        [Test]
        public void TestLogMessage()
        {

            List<ITelemetry> sentTelemetry = new List<ITelemetry>();
            var teli = Helpers.GetCrackedTelimena(sentTelemetry, this.testTelemetryKey, "ASD", true);

            teli.Track.Log(LogLevel.Warn, "A Message");
            teli.Track.SendAllDataNow();
            List<TelemetryItem> mapped = DoTheMapping(sentTelemetry);

            Assert.AreEqual(TelemetryItemTypes.LogMessage, mapped.Single().TelemetryItemType);
            Assert.AreEqual(LogLevel.Warn.ToString(), mapped.Single().LogLevel.ToString());
            Assert.AreEqual((int)LogLevel.Warn, (int)mapped.Single().LogLevel);
            Assert.AreEqual((sentTelemetry[0] as TraceTelemetry).Message, mapped[0].LogMessage);

        }

        [Test]
        public void TestView()
        {

            List<ITelemetry> sentTelemetry = new List<ITelemetry>();
            var teli = Helpers.GetCrackedTelimena(sentTelemetry, this.testTelemetryKey, "ASD", true);

            teli.Track.View("A View", new Dictionary<string, string>() { { "AKey", $"AValue" } });
            teli.Track.SendAllDataNow();
            List<TelemetryItem> mapped = DoTheMapping(sentTelemetry);

            Assert.AreEqual(TelemetryItemTypes.View, mapped.Single().TelemetryItemType);
            Assert.AreEqual("AValue", mapped[0].Properties["AKey"]);
            Assert.AreEqual(2, mapped[0].Properties.Count);
            Assert.IsNotNull(mapped[0].Properties.SingleOrDefault(x => x.Value == this.testTelemetryKey.ToString()));

        }





        private static List<TelemetryItem> DoTheMapping(List<ITelemetry> sentTelemetry)
        {
            byte[] serialized = JsonSerializer.Serialize(sentTelemetry, false);
            var items = AppInsightsDeserializer.Deserialize(serialized, false).ToList();
            var mapped = TelemetryMapper.Map(items).ToList();
            return mapped;
        }

        [Test]
        public void CheckListOfItems()
        {

            List<ITelemetry> sentTelemetry = new List<ITelemetry>();
            var teli = Helpers.GetCrackedTelimena(sentTelemetry, this.testTelemetryKey, "ASD", true);

            teli.Track.Event("TestEvent", new Dictionary<string, string>() { { "AKey", $"AValue" } });
            teli.Track.View("TestView");
            teli.Track.Log(LogLevel.Warn, "A log message");
            teli.Track.Exception(new Exception("An error that happened"));
            teli.Track.Exception(new Exception("An error that happened with note"), "A note for error");

            teli.Track.SendAllDataNow();
            byte[] serialized = JsonSerializer.Serialize(sentTelemetry, true);


            var items = AppInsightsDeserializer.Deserialize(serialized, true).ToList();
            var mapped = TelemetryMapper.Map(items).ToList();

            for (int index = 0; index < sentTelemetry.Count; index++)
            {
                ITelemetry appInsightsItem = sentTelemetry[index];
                TelemetryItem telimenaItem = mapped[index];

                Assert.AreEqual(appInsightsItem.Timestamp, telimenaItem.Timestamp);
                Assert.AreEqual(appInsightsItem.Sequence, telimenaItem.Sequence);
                Assert.AreEqual(appInsightsItem.Context.User.Id, telimenaItem.UserIdentifier);
                if (telimenaItem.TelemetryItemType != TelemetryItemTypes.LogMessage && telimenaItem.TelemetryItemType != TelemetryItemTypes.Exception)
                {
                    Assert.AreEqual(appInsightsItem.GetPropertyValue<string>("Name"), telimenaItem.EntryKey);
                }
                foreach (KeyValuePair<string, string> keyValuePair in appInsightsItem.GetPropertyValue<ConcurrentDictionary<string, string>>("Properties"))
                {
                    var props = typeof(TelimenaContextPropertyKeys).GetProperties().Select(x => x.Name);
                    if (!props.Contains(keyValuePair.Key))
                    {
                        Assert.AreEqual(keyValuePair.Value, telimenaItem.Properties[keyValuePair.Key]);
                    }
                }

            }
        }

    }
}