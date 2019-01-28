using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TelimenaClient
{
    /// <summary>
    /// A single telemetry record of any type
    /// </summary>
    public class TelemetryItem
    {
        /// <summary>
        /// Type of telemetry item
        /// </summary>
        public TelemetryItemTypes TelemetryItemType { get; set; }

        /// <summary>
        /// 
        /// </summary>
        [Obsolete("For serialization")]
        public TelemetryItem()
        {
        }

        /// <summary>
        /// Creates a new instance
        /// </summary>
        /// <param name="entryKey"></param>
        /// <param name="telemetryItemType"></param>
        /// <param name="versionData"></param>
        /// <param name="telemetryData"></param>
        public TelemetryItem(string entryKey, TelemetryItemTypes telemetryItemType, VersionData versionData, Dictionary<string, object> telemetryData)
        {
            this.TelemetryItemType = telemetryItemType;
            this.Id = Guid.NewGuid();
            this.Timestamp = DateTimeOffset.Now;
            this.EntryKey = entryKey;
            this.VersionData = versionData;
            this.TelemetryData = telemetryData;
        }

        /// <summary>
        /// Unique identifier
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// When an event occurred
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The name of the telemetry object, such as event (e.g. 'UserLoggedIn') or view ('AdvancedSettingsPanel')
        /// </summary>
        public string EntryKey { get; set; }

        /// <summary>
        /// Holds info about the version components (file, assembly)
        /// </summary>
        public VersionData VersionData { get; set; }

        /// <summary>
        /// Custom telemetry data
        /// </summary>
        public Dictionary<string, object> TelemetryData { get; set; }
    }
}
