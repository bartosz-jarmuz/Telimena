using System.Diagnostics;
using System.IO;

namespace TelimenaClient
{
    internal static class StartInfoCreator
    {
        public static ProcessStartInfo CreateStartInfo(FileInfo instructionsFile, FileInfo updaterFile)
        {
            return new ProcessStartInfo {FileName = updaterFile.FullName, Arguments = $"\"instructions:{instructionsFile.FullName}\""};
        }
    }
}