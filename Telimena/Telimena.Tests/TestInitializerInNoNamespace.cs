using NUnit.Framework;
using Telimena.WebApp;

[SetUpFixture]
public class TestInitializerInNoNamespace
{
    [OneTimeSetUp]
    public void Setup()
    {
        AutoMapperConfiguration.Configure();
    }

    [OneTimeTearDown]
    public void Teardown()
    {
        /* ... */
    }
}