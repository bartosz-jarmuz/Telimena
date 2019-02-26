using System.Data.Entity;

namespace Telimena.WebApp.Infrastructure.Database
{
    public class TelimenaTelemetryDbInitializer : DropCreateDatabaseIfModelChanges<TelimenaTelemetryContext>
    {

    }
}