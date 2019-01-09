namespace TelimenaClient
{
    /// <summary>
    /// A DTO for passing the telemetry unit around in the processing pipeline
    /// </summary>
    public class TelemetryPipelineTransportUnit
    {
        /// <summary>
        /// New instance
        /// </summary>
        /// <param name="item"></param>
        public TelemetryPipelineTransportUnit(TelemetryItem item)
        {
            this.TelemetryItem = item;
        }

        /// <summary>
        /// The entry in its original form
        /// </summary>
        public TelemetryItem TelemetryItem { get; set; }

        /// <summary>
        /// The serialized entry 
        /// </summary>
        public string SerializedEntry { get; set; }


    }
}