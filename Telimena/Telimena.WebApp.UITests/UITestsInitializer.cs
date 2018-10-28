using NUnit.Framework;
using Telimena.WebApp.UITests.Base;

namespace Telimena.WebApp.UITests
{
    [SetUpFixture]
    public class TestInitializerInNoNamespace
    {
   

    

        [OneTimeTearDown]
        public void Teardown()
        {
            if (UiTestBase.RemoteDriver.IsValueCreated)
            {
                UiTestBase.RemoteDriver.Value.Dispose();
            }

        }
    }
}