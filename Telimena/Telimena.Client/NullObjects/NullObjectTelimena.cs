namespace TelimenaClient
{
    internal class NullObjectTelimena : ITelimena
    {
        public NullObjectTelimena(ITelimenaProperties properties)
        {
            this.Properties = properties;
        }

        public IUpdatesModule Update { get; } = new NullObjectUpdatesModule();
        public ITelemetryModule Track { get; } = new NullObjectTelemetryModule();
        public ITelimenaProperties Properties { get; } 
    }
}