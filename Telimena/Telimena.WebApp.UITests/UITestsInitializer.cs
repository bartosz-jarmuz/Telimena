using NUnit.Framework;
using Telimena.TestUtilities.Base;
using Telimena.WebApp.UITests;

[SetUpFixture]
// ReSharper disable once CheckNamespace
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
