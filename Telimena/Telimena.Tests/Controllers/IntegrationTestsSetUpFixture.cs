namespace Telimena.Tests
{
    using System.Data.Entity;
    using NUnit.Framework;
    using WebApp.Infrastructure.Database;

    [SetUpFixture]
    internal sealed class IntegrationTestsSetUpFixture
    {
        internal static TelimenaContext Context = new TelimenaContext("name=AutomatedTests");

        [OneTimeSetUp]
        public void Initialize()
        {
            Database.SetInitializer(new DropCreateDatabaseAlwaysAndSeed());
            IntegrationTestsSetUpFixture.Context.Database.Initialize(false);
        }
    }
}