    using System.Data.Entity;
    using NUnit.Framework;
    using Telimena.WebApp;
    using Telimena.WebApp.Infrastructure.Database;

[SetUpFixture]
public class TestInitializerInNoNamespace
{
    [OneTimeSetUp]
    public void Setup()
    {
        AutoMapperConfiguration.Configure();
    }

    [OneTimeTearDown]
    public void Teardown() { /* ... */ }
}

