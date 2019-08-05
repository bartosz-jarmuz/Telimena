using System;

namespace Telimena.WebApp.Models.ProgramStatistics
{
    /// <summary>
    /// Class SequenceHistoryViewModel.
    /// </summary>
    public class SequenceHistoryViewModel
    {
        /// <summary>
        /// Gets or sets the name of the program.
        /// </summary>
        /// <value>The name of the program.</value>
        public string ProgramName { get; set; }
        /// <summary>
        /// Gets or sets the telemetry key.
        /// </summary>
        /// <value>The telemetry key.</value>
        public Guid TelemetryKey { get; set; }

        /// <summary>
        /// Gets or sets the sequence identifier.
        /// </summary>
        /// <value>The sequence identifier.</value>
        public string SequenceId { get; set; }
    }
}