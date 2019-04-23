using System.IO;
using System.Linq;

namespace Telimena.PackageTriggerUpdater.CommandLineArguments
{
    internal static class CommandLineArgumentParser
    {
        public static UpdaterStartupSettings GetSettings(string[] args)
        {
            if (args.Length == 0 || args.All(string.IsNullOrEmpty))
            {
                return null;
            }

            UpdaterStartupSettings startupSettings = new UpdaterStartupSettings {InstructionsFile = GetInstructionsArg(args)};

            return startupSettings;
        }

        private static FileInfo GetInstructionsArg(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg.StartsWith("instructions:"))
                {
                    string path = arg.Substring("instructions:".Length);
                    return new FileInfo(path);
                }
            }

            return null;
        }
    }
}