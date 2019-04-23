using System;
using System.Data.Entity;
using NUnit.Framework;
using Telimena.WebApp.Infrastructure.Database;

namespace Telimena.Tests
{
    public abstract class GlobalContextTestBase
    {
        protected virtual Action SeedAction { get; }

        private static readonly TelimenaTelemetryContext context = DbContextService.CreateInstance<TelimenaTelemetryContext>(@"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=TESTS-TelimenaTelemetryDb;Integrated Security=SSPI;MultipleActiveResultSets=True");
        private static readonly TelimenaPortalContext portalContext = DbContextService.CreateInstance<TelimenaPortalContext>(@"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=TESTS-TelimenaPortalDb;Integrated Security=SSPI;MultipleActiveResultSets=True");

        public TelimenaTelemetryContext TelemetryContext => context;
        public TelimenaPortalContext PortalContext => portalContext;

        [OneTimeTearDown]
        public virtual void OneTimeTearDown()
        {
            DbContextService.ResetChangeTracker((DbContext)this.TelemetryContext);
            DbContextService.ResetChangeTracker((DbContext)this.PortalContext);
        }

        [OneTimeSetUp]
        public virtual void OneTimeSetUp()
        {
            Database.SetInitializer<TelimenaTelemetryContext>((IDatabaseInitializer<TelimenaTelemetryContext>)new DropCreateDatabaseAlwaysWithConnectionClose());
            this.TelemetryContext.Database.Initialize(true);

            Database.SetInitializer<TelimenaPortalContext>((IDatabaseInitializer<TelimenaPortalContext>)new DropCreateDatabaseAlwaysWithConnectionClose());
            this.PortalContext.Database.Initialize(true);
            TelimenaPortalDbInitializer.SeedUsers(this.PortalContext);
            this.SeedAction?.Invoke();

        }


    }
}