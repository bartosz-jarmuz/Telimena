using System;
using System.Linq;
using NUnit.Framework;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Utils;

namespace Telimena.Tests
{
    [TestFixture]
    public class BasicTelemetryMappingTests
    {
        private readonly Guid testTelemetryKey = Guid.Parse("dc13cced-30ea-4628-a81d-21d86f37df95");

        [Test]
        public void TestDefault()
        {
            var basicItem = new BasicTelemetryItem();
            var item = TelemetryMapper.Map(this.testTelemetryKey, basicItem);

            Assert.IsNotNull(item.EntryKey);
            Assert.IsNotNull(item.UserIdentifier);
            Assert.AreEqual("0.0.0.0", item.VersionData.FileVersion);
            Assert.AreEqual("0.0.0.0", item.VersionData.AssemblyVersion);
            Assert.AreNotEqual(Guid.Empty, item.Id);
            Assert.AreEqual(this.testTelemetryKey, item.TelemetryKey);
            Assert.AreEqual(TelemetryItemTypes.Event, item.TelemetryItemType);
            Assert.AreNotEqual(default(DateTimeOffset), item.Timestamp);
            Assert.IsTrue(!string.IsNullOrEmpty(basicItem.EventName));
            Assert.AreEqual(basicItem.EventName, item.EntryKey);
        }

        [Test]
        public void TestLogMessage()
        {
            var basicItem = new BasicTelemetryItem()
            {
                LogMessage = "Boo",
                TelemetryItemType = TelemetryItemTypes.LogMessage,
                EventName = "LogMsg",
                Timestamp = new DateTimeOffset(1988,02,28,0,0,0,new TimeSpan(0)),
                ProgramVersion = "3.4",
                UserIdentifier = "JimBeam"
            };
            var item = TelemetryMapper.Map(this.testTelemetryKey, basicItem);

            Assert.AreEqual(TelemetryItemTypes.LogMessage, item.TelemetryItemType);
            Assert.AreEqual("Boo", item.LogMessage);
            Assert.AreEqual(new DateTimeOffset(1988, 02, 28, 0, 0, 0, new TimeSpan(0)), item.Timestamp);
            Assert.AreEqual("JimBeam", item.UserIdentifier);
            Assert.AreEqual("3.4", item.VersionData.FileVersion);
            Assert.AreEqual("3.4", item.VersionData.AssemblyVersion);
            Assert.IsTrue(!string.IsNullOrEmpty(basicItem.EventName));
            Assert.AreEqual(basicItem.EventName, item.EntryKey);
        }

        [Test]
        public void TestException()
        {
            var basicItem = new BasicTelemetryItem()
            {
                TelemetryItemType = TelemetryItemTypes.Exception,
                Timestamp = new DateTimeOffset(1988, 02, 28, 0, 0, 0, new TimeSpan(0)),
                ErrorMessage = "An error message"
            };
            var item = TelemetryMapper.Map(this.testTelemetryKey, basicItem);

            Assert.AreEqual(TelemetryItemTypes.Exception, item.TelemetryItemType);
            Assert.AreEqual("An error message", item.Exceptions.Single().Message);
        }
    }
}