using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Telimena.Updater
{
    internal static class CommandLineArgumentParser
    {
        public static UpdaterSettings GetSettings(string[] args)
        {
            if (args.Length == 0 || args.All(string.IsNullOrEmpty))
            {
                return null;
            }

            UpdaterSettings settings = new UpdaterSettings()
            {
                InstructionsFile = GetInstructionsArg(args)
            };

            return settings;
        }


        private static FileInfo GetInstructionsArg(string[] args)
        {
            foreach (string arg in args)
            {
                if (arg.StartsWith("instructions:"))
                {
                    var path = arg.Substring("instructions:".Length);
                    return new FileInfo(path);
                }
            }

            return null;
        }
    }
}
