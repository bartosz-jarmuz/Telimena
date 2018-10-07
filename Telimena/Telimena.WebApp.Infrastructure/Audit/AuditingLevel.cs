namespace MvcAuditLogger
{
    public enum AuditingLevel
    {
        /// <summary>
        ///     No request data is serialized and stored
        /// </summary>
        NoData,

        /// <summary>
        ///     The basic data is stored - cookies, headers, query string
        /// </summary>
        BasicData,

        /// <summary>
        ///     More data is stored
        /// </summary>
        AdvancedData
    }
}