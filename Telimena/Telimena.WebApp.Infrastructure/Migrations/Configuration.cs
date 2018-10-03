using Telimena.WebApp.Infrastructure.Database;

namespace Telimena.WebApp.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    public sealed class Configuration : DbMigrationsConfiguration<Telimena.WebApp.Infrastructure.Database.TelimenaContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Telimena.WebApp.Infrastructure.Database.TelimenaContext context)
        {
            TelimenaDbInitializer.SeedUsers(context);
            TelimenaDbInitializer.SeedToolkit(context);
            context.SaveChanges();
        }
    }
}
