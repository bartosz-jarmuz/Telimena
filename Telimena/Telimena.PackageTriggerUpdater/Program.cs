using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telimena.PackageTriggerUpdater.CommandLineArguments;

namespace Telimena.PackageTriggerUpdater
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine("This app has to be run with proper arguments.");
                Console.ReadKey();
                return;
            }
            UpdaterStartupSettings settings = CommandLineArgumentParser.GetSettings(args);
            UpdateInstructions instructions = UpdateInstructionsReader.Read(settings.InstructionsFile);
            Console.WriteLine($"Read update instructions from {settings.InstructionsFile}");
            var package = instructions.PackagePaths.FirstOrDefault();
            if (package != null && File.Exists(package))
            {
                Console.WriteLine($"Launching package {package}.");
                Process.Start(package);
            }
            else
            {
                Console.WriteLine($"Failed to find package from {settings.InstructionsFile}");
            }
        }
    }
}
