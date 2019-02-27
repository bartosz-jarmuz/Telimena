using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Telimena.WebApp.Core.DTO.MappableToClient;

namespace Telimena.WebApp.Core.Models.Telemetry
{
    public class LogMessage
    {
        public string Message { get; set; }
        public string ProgramVersion{ get; set; }

        public LogLevel Level { get; set; }

        public string Sequence { get; set; }

        [Key, Index(IsUnique = true, IsClustered = false)]
        public Guid Id { get; set; } = Guid.NewGuid();

        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Index(IsUnique = true, IsClustered = true)]
        public int ClusterId { get; set; }

        public int ProgramId { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public string UserName { get; set; }

        /// <summary>
        /// When an event occurred
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

    }
}