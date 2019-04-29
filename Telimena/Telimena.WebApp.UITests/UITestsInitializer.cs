using NUnit.Framework;
using Telimena.TestUtilities.Base;

namespace Telimena.WebApp.UITests
{
    [SetUpFixture]
    public class TestInitializerInNoNamespace
    {
   

    

        [OneTimeTearDown]
        public void Teardown()
        {
            if (WebsiteTestBase.RemoteDriver.IsValueCreated)
            {
                WebsiteTestBase.RemoteDriver.Value.Dispose();
            }

        }
    }
}