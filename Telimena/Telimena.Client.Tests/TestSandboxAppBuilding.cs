using NUnit.Framework;
using TelimenaTestSandboxApp;

namespace TelimenaClient.Tests
{
    [TestFixture]
    class TestSandboxAppBuilding
    {
        [Test]
        public void JustCallItEnsureItBuilds()
        {
            TelimenaHammer hammer = null;//no action needed
            Assert.IsNull(hammer);
        }
    }
}
