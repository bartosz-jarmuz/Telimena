using System.Data.Entity.Migrations;

namespace Telimena.WebApp.Infrastructure.Migrations.Telemetry
{
    public sealed class TelemetryMigrationsConfiguration : DbMigrationsConfiguration<Telimena.WebApp.Infrastructure.Database.TelimenaTelemetryContext>
    {
        public TelemetryMigrationsConfiguration()
        {
            this.AutomaticMigrationsEnabled = false;
            this.ContextKey = "Telimena.WebApp.Infrastructure.Database.TelimenaTelemetryContext";
        }

        protected override void Seed(Telimena.WebApp.Infrastructure.Database.TelimenaTelemetryContext context)
        {
            //  This method will be called after migrating to the latest version.

            //  You can use the DbSet<T>.AddOrUpdate() helper extension method 
            //  to avoid creating duplicate seed data.
        }
    }
}
