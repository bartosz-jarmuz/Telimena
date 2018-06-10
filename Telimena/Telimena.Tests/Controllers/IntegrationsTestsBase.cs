namespace Telimena.Tests
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using NUnit.Framework;
    using WebApp.Infrastructure.Database;

    public class IntegrationsTestsBase
    {
        protected TelimenaContext Context => IntegrationTestsSetUpFixture.Context;

        protected IntegrationsTestsBase()
        {
        }

        [SetUp]
        public void Setup()
        {
        }

        [TearDown]
        public void ResetChangeTracker()
        {
            IEnumerable<DbEntityEntry> changedEntriesCopy = this.Context.ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Added ||
                            e.State == EntityState.Modified ||
                            e.State == EntityState.Deleted
                );
            foreach (DbEntityEntry entity in changedEntriesCopy)
            {
                this.Context.Entry(entity.Entity).State = EntityState.Detached;
            }
        }
    }
}