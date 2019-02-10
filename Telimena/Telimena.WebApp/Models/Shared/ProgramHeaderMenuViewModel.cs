using System;

namespace Telimena.WebApp.Models.Shared
{
    /// <summary>
    /// Class ProgramHeaderMenuViewModel.
    /// </summary>
    public class ProgramHeaderMenuViewModel
    {
        /// <summary>
        /// Gets or sets the telemetry key.
        /// </summary>
        /// <value>The telemetry key.</value>
        public Guid? TelemetryKey { get; set; }
        /// <summary>
        /// Gets or sets the name of the program.
        /// </summary>
        /// <value>The name of the program.</value>
        public string ProgramName { get; set; }
    }
}