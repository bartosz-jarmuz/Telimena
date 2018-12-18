using System.Threading.Tasks;

namespace TelimenaClient
{
    #region Using

    #endregion

    public partial class Telimena
    {
        internal class BlockingUpdatesHandler : IBlockingUpdatesHandler
        {
            /// <summary>
            ///     New instance
            /// </summary>
            /// <param name="telimena"></param>
            public BlockingUpdatesHandler(Telimena telimena)
            {
                this.telimena = telimena;
            }

            private readonly ITelimena telimena;

            /// <inheritdoc />
            public UpdateCheckResult CheckForUpdates()
            {
                return Task.Run(() => this.telimena.Updates.Async.CheckForUpdates()).GetAwaiter().GetResult();
            }

            /// <inheritdoc />
            public UpdateCheckResult HandleUpdates(bool acceptBeta)
            {
                return Task.Run(() => this.telimena.Updates.Async.HandleUpdates(acceptBeta)).GetAwaiter().GetResult();
            }

        }
    }
}