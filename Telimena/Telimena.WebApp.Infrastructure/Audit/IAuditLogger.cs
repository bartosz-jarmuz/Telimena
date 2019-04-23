namespace MvcAuditLogger
{
    public interface IAuditLogger
    {
        /// <summary>
        /// Stores the without request data.
        /// </summary>
        /// <param name="audit">The audit.</param>
        void StoreWithoutRequestData(IAudit audit);
        /// <summary>
        /// Stores the with request data.
        /// </summary>
        /// <param name="audit">The audit.</param>
        void StoreWithRequestData(IAudit audit);
    }
}