namespace TelimenaClient
{
    /// <summary>
    /// Combines all updating related functions
    /// </summary>
    public interface IUpdatesModule : IFluentInterface
    {
        /// <summary>
        /// Asynchronous updating methods. All calls should be awaited
        /// </summary>
        IAsyncUpdatesHandler Async { get; }

        /// <summary>
        /// Synchronous updating methods
        /// </summary>
        IBlockingUpdatesHandler Blocking{ get; }
    }
}