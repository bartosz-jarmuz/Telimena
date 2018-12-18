namespace TelimenaClient
{
    /// <inheritdoc />
    public class UpdatesModule : IUpdatesModule
    {
        /// <summary>
        /// New instance
        /// </summary>
        /// <param name="telimena"></param>
        public UpdatesModule(Telimena telimena)
        {
            this.Async = new Telimena.AsyncUpdatesHandler(telimena);
            this.Blocking = new Telimena.BlockingUpdatesHandler(telimena);
        }

        /// <inheritdoc />
        public IAsyncUpdatesHandler Async { get; }

        /// <inheritdoc />
        public IBlockingUpdatesHandler Blocking { get; }
    }
}