using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
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
    public class AppInsightsDeserializerTests
    {
        private readonly Guid testTelemetryKey = Guid.Parse("dc13cced-30ea-4628-a81d-21d86f37df95");


        [Test]
        public void TestEvent()
        {

            List<ITelemetry> sentTelemetry = new List<ITelemetry>();
            TelemetryModule telemetryModule = Helpers.GetTelemetryModule(sentTelemetry, this.testTelemetryKey);

            telemetryModule.Event("TestEvent", new Dictionary<string, string>()
            {
                {"AKey", $"AValue"},
                {"AKey2", $"AValue2"}
            });

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
            TelemetryModule telemetryModule = Helpers.GetTelemetryModule(sentTelemetry, this.testTelemetryKey);

            telemetryModule.Exception(new InvalidCastException("A Message", new InvalidOperationException("Inner")));

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
            TelemetryModule telemetryModule = Helpers.GetTelemetryModule(sentTelemetry, this.testTelemetryKey);

            telemetryModule.Log(LogLevel.Warn, "A Message");

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
            TelemetryModule telemetryModule = Helpers.GetTelemetryModule(sentTelemetry, this.testTelemetryKey);

            telemetryModule.View("A View", new Dictionary<string, string>() { { "AKey", $"AValue" } });

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
            var mapped = AppInsightsTelemetryMapper.Map(items).ToList();
            return mapped;
        }

        [Test]
        public void CheckListOfItems()
        {

            List<ITelemetry> sentTelemetry = new List<ITelemetry>();
            TelemetryModule telemetryModule = Helpers.GetTelemetryModule(sentTelemetry, this.testTelemetryKey);

            telemetryModule.Event("TestEvent", new Dictionary<string, string>() { { "AKey", $"AValue" } });
            telemetryModule.View("TestView");
            telemetryModule.Log(LogLevel.Warn, "A log message");
            telemetryModule.Exception(new Exception("An error that happened"));
            telemetryModule.Exception(new Exception("An error that happened with note"), "A note for error");

            byte[] serialized = JsonSerializer.Serialize(sentTelemetry, true);


            var items = AppInsightsDeserializer.Deserialize(serialized, true).ToList();
            var mapped = AppInsightsTelemetryMapper.Map(items).ToList();

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