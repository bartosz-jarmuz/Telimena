namespace Telimena.WebApp.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<Telimena.WebApp.Infrastructure.Database.TelimenaPortalContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
            ContextKey = "Telimena.WebApp.Infrastructure.Database.TelimenaPortalContext";
        }

        protected override void Seed(Telimena.WebApp.Infrastructure.Database.TelimenaPortalContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
        }
    }
}
