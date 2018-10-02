using Telimena.WebApp.Infrastructure.Database;

namespace Telimena.WebApp.Infrastructure.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    public sealed class Configuration : DbMigrationsConfiguration<TelimenaContext>
    {
        public Configuration()
        {
            this.AutomaticMigrationsEnabled = false;
            this.ContextKey = "Telimena.WebApp.Infrastructure.Database.TelimenaContext";
        }

        protected override void Seed(TelimenaContext context)
        {
            TelimenaDbInitializer.SeedUsers(context);
            TelimenaDbInitializer.SeedToolkit(context);
            context.SaveChanges();
        }


    }
}
