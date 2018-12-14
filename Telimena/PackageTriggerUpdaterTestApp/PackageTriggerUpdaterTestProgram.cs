using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TelimenaClient;

namespace PackageTriggerUpdaterTestApp
{
    internal class PackageTriggerUpdaterTestProgram
    {
        private class MyJsonContractResolver : DefaultContractResolver
        {
            protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
            {
                IList<JsonProperty> list = base.CreateProperties(type, memberSerialization);

                foreach (JsonProperty prop in list)
                {
                    prop.Ignored = false; // Don't ignore any property
                }

                return list;
            }
        }

        public static string Base64Decode(string base64EncodedData)
        {
            byte[] base64EncodedBytes = Convert.FromBase64String(base64EncodedData);
            return Encoding.UTF8.GetString(base64EncodedBytes);
        }

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
            MessageBox.Show("Updater executed", "Updater executed");
            Console.WriteLine("Finding and killing the other instance of this app");

            Process currentProcess = Process.GetCurrentProcess();
            bool killed = false;
            IEnumerable<Process> otherProcesses =
                Process.GetProcesses().Where(x => x.ProcessName == typeof(PackageTriggerUpdaterTestProgram).Assembly.GetName().Name);
            foreach (Process otherProcess in otherProcesses)
            {
                if (otherProcess.Id != currentProcess.Id)
                {
                    killed = true;
                    otherProcess.Kill();
                }
            }

            MessageBox.Show($"Killed other processes: {killed}", "Updater finished");
        }

        public static void Work(PackageUpdateTesterArguments arguments)
        {
            Console.WriteLine("Starting update handling...");

            Telimena teli = new Telimena(new TelimenaStartupInfo(arguments.TelemetryKey, new Uri(arguments.ApiUrl)));
            Console.WriteLine("Telimena created... Handling updates");
            UpdateCheckResult result = teli.Blocking.HandleUpdates(false);
            Console.WriteLine("Finished update handling");
            JsonSerializerSettings settings = new JsonSerializerSettings {ContractResolver = new MyJsonContractResolver()};
            Console.WriteLine(JsonConvert.SerializeObject(result, settings));

            Console.WriteLine("All done");
        }

        private static void Main(string[] args)
        {
            Console.WriteLine($"Starting {typeof(PackageTriggerUpdaterTestProgram).Assembly.GetName().Name}");

            string msg = $"AssemblyVersion: {TelimenaVersionReader.Read(typeof(PackageTriggerUpdaterTestProgram), VersionTypes.AssemblyVersion)}\r\n" +
                         $"FileVersion: {TelimenaVersionReader.Read(typeof(PackageTriggerUpdaterTestProgram), VersionTypes.FileVersion)}\r\n" +
                         $"Telimena Assembly Version: {TelimenaVersionReader.Read(typeof(Telimena), VersionTypes.AssemblyVersion)}\r\n" +
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

            Work(arguments);

            int key = Console.Read();
            while (key != 1)
            {
                key = Console.Read();
            }
        }
    }
}