using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Telimena.Client.Tests
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
