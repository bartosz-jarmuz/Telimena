using NUnit.Framework;

namespace TelimenaClient.Tests
{
    [TestFixture]
    class TestSandboxAppBuilding
    {
        [Test]
        public void JustCallItEnsureItBuilds()
        {
            var form = new TelimenaTestSandboxApp.Form1();
        }
    }
}
