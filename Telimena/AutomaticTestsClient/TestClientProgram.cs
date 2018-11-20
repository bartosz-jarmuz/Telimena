using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Newtonsoft.Json;
using TelimenaClient;

namespace AutomaticTestsClient
{
    class TestClientProgram
    {

        public static string GetFileVersion(Type type)
        {
            return FileVersionInfo.GetVersionInfo(type.Assembly.Location).FileVersion;
        }

        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                MessageBox.Show($"AssemblyVersion: {TelimenaVersionReader.Read(typeof(TestClientProgram), VersionTypes.AssemblyVersion)}\r\n" + 
                                $"FileVersion: {TelimenaVersionReader.Read(typeof(TestClientProgram), VersionTypes.FileVersion)}\r\n" + 
                                $"Telimena Version: {TelimenaVersionReader.Read(typeof(Telimena), VersionTypes.AssemblyVersion)}\r\n" + 
                                $"Telimena File Version: {TelimenaVersionReader.Read(typeof(Telimena), VersionTypes.FileVersion)}", "AutomaticTestsClient - This app requires arguments to run");
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
