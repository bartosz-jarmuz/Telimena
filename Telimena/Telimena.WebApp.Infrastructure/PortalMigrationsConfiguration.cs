using Telimena.WebApp.Infrastructure.Database;

namespace Telimena.WebApp.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    public sealed class PortalMigrationsConfiguration : DbMigrationsConfiguration<Telimena.WebApp.Infrastructure.Database.TelimenaPortalContext>
    {
        public PortalMigrationsConfiguration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "Telimena.WebApp.Infrastructure.Database.TelimenaPortalContext";
        }

        protected override void Seed(Telimena.WebApp.Infrastructure.Database.TelimenaPortalContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.

            //protected override void Seed(TelimenaPortalContext portalContext)
            //{
            TelimenaPortalDbInitializer.SeedUsers(context);
            TelimenaPortalDbInitializer.SeedToolkit(context);
            context.SaveChanges();

            //}
        }
    }
}
