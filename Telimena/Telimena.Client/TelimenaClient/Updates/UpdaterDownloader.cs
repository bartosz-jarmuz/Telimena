using System.Threading.Tasks;

namespace Telimena.Client
{
    public class UpdaterDownloader
    {
        internal UpdaterDownloader(ProgramInfo programInfo, IMessenger messenger)
        {
        }

        public Task EnsureUpdaterIsAvailable()
        {
            return Task.CompletedTask;
        }
    }
}