using System;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
using Newtonsoft.Json;
using TelimenaClient;

namespace PackageTriggerUpdaterTestApp
{
    class PackageTriggerUpdaterTestProgram
    {

        public static string GetFileVersion(Type type)
        {
            return FileVersionInfo.GetVersionInfo(type.Assembly.Location).FileVersion;
        }

        public static void PerformUpdate()
        {
            Console.WriteLine("PackageTriggerUpdaterTestApp - app launched without arguments - acting as self-update package");
            //we only really care whether the updater has launched the update package - it's up to the package to perform the update.
            //use case - SDL Trados Studio (and similar software) which has plugins. The plugin is compiled to an .sdlplugin file which after executing
            //will present an installation wizard - it will guide the user through the update process, so the only thing that Telimena needs to do is to download this update package and execute it.
            MessageBox.Show("Updater executed");
            Console.WriteLine("Finding and killing the other instance of this app");

            var currentProcess = Process.GetCurrentProcess();
            bool killed = false;
            var otherProcesses = Process.GetProcesses().Where(x => x.ProcessName == typeof(PackageTriggerUpdaterTestProgram).Assembly.GetName().Name);
            foreach (Process otherProcess in otherProcesses)
            {
                if (otherProcess.Id != currentProcess.Id)
                {
                    killed = true;
                    otherProcess.Kill();
                }
            }

            MessageBox.Show($"Killed other processes: {killed}");
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"Starting {typeof(PackageTriggerUpdaterTestProgram).Assembly.GetName().Name}");

            var msg = $"AssemblyVersion: {TelimenaVersionReader.Read(typeof(PackageTriggerUpdaterTestProgram), VersionTypes.AssemblyVersion)}\r\n" +
                      $"FileVersion: {TelimenaVersionReader.Read(typeof(PackageTriggerUpdaterTestProgram), VersionTypes.FileVersion)}\r\n" +
                      $"Telimena Version: {TelimenaVersionReader.Read(typeof(Telimena), VersionTypes.AssemblyVersion)}\r\n" +
                      $"Telimena File Version: {TelimenaVersionReader.Read(typeof(Telimena), VersionTypes.FileVersion)}";
            Console.WriteLine(msg);

            if (args.Length == 0)
            {
                PerformUpdate();
                return;
            }

            PackageUpdateTesterArguments arguments;
            Console.WriteLine("Loading Arguments...");
            string decoded = "";
            try
            {
                decoded = Base64Decode(args[0]);
                arguments = JsonConvert.DeserializeObject<PackageUpdateTesterArguments>(decoded);
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error while deserializing [{args[0]}], decoded: {decoded}", ex);
            }

            Console.WriteLine($"Args: {decoded}");
            Console.WriteLine("Arguments loaded OK");
#if DEBUG
            if (arguments.ApiUrl == null)
            {
                arguments.ApiUrl = "http://localhost:7757";
            }
#endif
            Work(arguments);
        }

        public static void Work(PackageUpdateTesterArguments arguments)
        {
            var teli = new Telimena(arguments.TelemetryKey);

            teli.HandleUpdatesBlocking(false);
        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }



    }
}
