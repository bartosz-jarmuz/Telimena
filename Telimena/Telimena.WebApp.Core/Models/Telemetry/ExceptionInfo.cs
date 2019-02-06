using System;
using System.ComponentModel.DataAnnotations;

namespace Telimena.WebApp.Core.Models
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

        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        public int ProgramId { get; set; }

        /// <summary>
        /// Gets or sets the user identifier.
        /// </summary>
        /// <value>The user identifier.</value>
        public string UserId { get; set; }

        /// <summary>
        /// When an event occurred
        /// </summary>
        public DateTimeOffset Timestamp { get; set; }

    }
}