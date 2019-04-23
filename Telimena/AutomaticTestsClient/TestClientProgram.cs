using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using TelimenaClient;

namespace SharedLogic
{
    class TestClientProgram
    {

        public static string GetFileVersion(Type type)
        {
            return FileVersionInfo.GetVersionInfo(type.Assembly.Location).FileVersion;
        }

        static void Main(string[] args)
        {
            Console.WriteLine($"Starting {typeof(TestClientProgram).Assembly.GetName().Name}");

            var msg = $"AssemblyVersion: {TelimenaVersionReader.Read(typeof(TestClientProgram), VersionTypes.AssemblyVersion)}\r\n" +
                      $"FileVersion: {TelimenaVersionReader.Read(typeof(TestClientProgram), VersionTypes.FileVersion)}\r\n" +
                      $"Telimena Version: {TelimenaVersionReader.Read(typeof(Telimena), VersionTypes.AssemblyVersion)}\r\n" +
                      $"Telimena File Version: {TelimenaVersionReader.Read(typeof(Telimena), VersionTypes.FileVersion)}";
            Console.WriteLine(msg);
            if (args.Length == 0)
            {
                MessageBox.Show(msg, "AutomaticTestsClient - This app requires arguments to run");
                return;
            }


            var arguments = SharedUtils.LoadArguments(args);
            new TestAppWorker(arguments).Work();
        }

    }
}
