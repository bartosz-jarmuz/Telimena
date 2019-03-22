using System.Data.Entity;
using Telimena.WebApp.Infrastructure.Migrations;
using Telimena.WebApp.Infrastructure.Migrations.Telemetry;

namespace Telimena.WebApp.Infrastructure.Database
{
    //public class TelimenaTelemetryDbInitializer : DropCreateDatabaseIfModelChanges<TelimenaTelemetryContext>
    public class TelimenaTelemetryDbInitializer : MigrateDatabaseToLatestVersion<TelimenaTelemetryContext, TelemetryMigrationsConfiguration>
    {

    }
}