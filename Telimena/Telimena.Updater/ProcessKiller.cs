using System.Diagnostics;
using System.IO;

namespace Telimena.Updater
{
    public static class ProcessKiller
    {

        public static void Kill(string exePath)
        {
            var exeName = Path.GetFileNameWithoutExtension(exePath);
            var processes = Process.GetProcessesByName(exeName);
            foreach (Process process in processes)
            {
                process.Kill();
            }
        }
    }
}