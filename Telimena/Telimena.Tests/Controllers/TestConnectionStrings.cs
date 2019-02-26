namespace Telimena.Tests
{
    public static class TestConnectionStrings
    {
        public static string PortalDb { get; } = @"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=IntegrationTestsPortalDb;Integrated Security=SSPI;MultipleActiveResultSets=true;";
        public static string TelemetryDb { get; } = @"Data Source=(LocalDb)\MSSQLLocalDB;Initial Catalog=IntegrationTestsTelemetryDb;Integrated Security=SSPI;MultipleActiveResultSets=true;";

    }
}