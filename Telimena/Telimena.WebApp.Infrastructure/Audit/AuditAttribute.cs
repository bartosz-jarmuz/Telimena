using System;

namespace MvcAuditLogger
{
    #region Using

    #endregion

    public class AuditAttribute : Attribute
    {
        /// <summary>Initializes a new instance of the <see cref="T:System.Web.Mvc.ActionFilterAttribute" /> class.</summary>
        public AuditAttribute()
        {
        }

        /// <summary>Initializes a new instance of the <see cref="T:System.Web.Mvc.ActionFilterAttribute" /> class.</summary>
        public AuditAttribute(AuditingLevel level)
        {
            this.Level = level;
        }

        /// <summary>
        ///     Gets or sets the auditing level.
        /// </summary>
        /// <value>The auditing level.</value>
        public AuditingLevel Level { get; set; }
    }
}