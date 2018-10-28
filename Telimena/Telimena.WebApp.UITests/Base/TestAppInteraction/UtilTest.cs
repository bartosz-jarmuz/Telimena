using AutomaticTestsClient;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Telimena.WebApp.UITests.Base.TestAppInteraction
{
    [TestFixture]
    public class UtilTest
    {

        [Test]
        public void TestArgsParsing()
        {
            var args = new Arguments() { ApiUrl = "http://localhost:7757", Action = Actions.Initialize };
            string result = JsonConvert.SerializeObject(args);

            var restoredArgs = JsonConvert.DeserializeObject<Arguments>(result);

            Assert.IsNotNull(restoredArgs);
        }

    }
}