using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Telimena.WebApp.Core.Models.Telemetry
{
    public class ExceptionInfo
    {
        public long ExceptionId { get; set; }

        public long ExceptionOuterId { get; set; }

        public string TypeName { get; set; }

        public string Message { get; set; }

        public bool HasFullStack { get; set; }

        public string ParsedStack { get; set; }

        public string Sequence { get; set; }

        [Key,Index(IsUnique = true, IsClustered = false)]
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

        public string ProgramVersion { get; set; }
    }
}