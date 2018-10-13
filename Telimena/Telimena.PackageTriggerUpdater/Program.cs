using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Telimena.PackageTriggerUpdater.CommandLineArguments;

namespace Telimena.PackageTriggerUpdater
{
    partial class Program
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
          
                        var worker = new PackageUpdaterWorker();
            worker.TriggerUpdate(settings, instructions);
        }
    }
}
