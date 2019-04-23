using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using NUnit.Framework;
using Telimena.WebApp.Utils;

namespace Telimena.Tests
{
    [TestFixture]
    public class SequenceIdParserTests
    {
        [Test]
        public void Test_EndToEnd()
        {
            SequencePropertyInitializer initializer = new SequencePropertyInitializer();

            EventTelemetry tele1 = new EventTelemetry("Event1");
            initializer.Initialize(tele1);
            EventTelemetry tele2 = new EventTelemetry("Event2");
            initializer.Initialize(tele2);
            EventTelemetry tele3 = new EventTelemetry("Event3");
            initializer.Initialize(tele3);

            Assert.AreEqual(1, SequenceIdParser.GetOrder(tele1.Sequence));
            Assert.AreEqual(2, SequenceIdParser.GetOrder(tele2.Sequence));
            Assert.AreEqual(3, SequenceIdParser.GetOrder(tele3.Sequence));

            var prefix = SequenceIdParser.GetPrefix(tele1.Sequence);
            var prefixAgain = SequenceIdParser.GetPrefix(prefix);
            Assert.IsTrue(!string.IsNullOrEmpty(prefix));

            Assert.AreEqual(prefix, prefixAgain);

            Assert.AreEqual(prefix, SequenceIdParser.GetPrefix(tele2.Sequence));
            Assert.AreEqual(prefix, SequenceIdParser.GetPrefix(tele3.Sequence));
        }
    }
}