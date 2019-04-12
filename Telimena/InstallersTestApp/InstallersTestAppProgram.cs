using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using AutomaticTestsClient;
using Newtonsoft.Json;
using TelimenaClient;
using Console = System.Console;

namespace InstallersTestApp
{
    class InstallersTestAppProgram
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Starting {typeof(InstallersTestAppProgram).Assembly.GetName().Name}");

            var msg = $"AssemblyVersion: {TelimenaVersionReader.Read(typeof(InstallersTestAppProgram), VersionTypes.AssemblyVersion)}\r\n" +
                      $"FileVersion: {TelimenaVersionReader.Read(typeof(InstallersTestAppProgram), VersionTypes.FileVersion)}\r\n" +
                      $"Telimena Version: {TelimenaVersionReader.Read(typeof(Telimena), VersionTypes.AssemblyVersion)}\r\n" +
                      $"Telimena File Version: {TelimenaVersionReader.Read(typeof(Telimena), VersionTypes.FileVersion)}";
            Console.WriteLine(msg);
            if (args.Length == 0)
            {
                MessageBox.Show(msg, "InstallersTestApp - This app requires arguments to run");
                return;
            }


            Arguments arguments;
            Console.WriteLine("Loading Arguments...");
            string decoded = "";
            try
            {
                decoded = Base64Decode(args[0]);
                arguments = JsonConvert.DeserializeObject<Arguments>(decoded);
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
            new TestAppWorker(arguments).Work();

        }

        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }
    }
}
