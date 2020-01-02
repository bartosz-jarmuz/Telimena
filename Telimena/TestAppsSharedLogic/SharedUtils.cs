using System;
using System.IO;
using System.Windows.Forms;
using Newtonsoft.Json;
using TelimenaClient;

namespace SharedLogic
{
    public static class SharedUtils
    {
        public static string Base64Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        public static bool PrintVersionsAndCheckArgs(string[] args, Type type)
        {

            var lines = new string[]
            {
                TelimenaVersionReader.Read(type, VersionTypes.AssemblyVersion)
                , TelimenaVersionReader.Read(type, VersionTypes.FileVersion)
                , TelimenaVersionReader.Read(typeof(Telimena), VersionTypes.AssemblyVersion)
                , TelimenaVersionReader.Read(typeof(Telimena), VersionTypes.FileVersion)
            };

           
            Console.WriteLine(string.Join("\r\n", lines));
            if (args.Length == 0)
            {
                File.WriteAllLines("Versions.txt", lines);
                Console.WriteLine("App executed with zero args. Quitting.");
                return false;
            }

            return true;
        }

        public static Arguments LoadArguments(string[] args)
        {
            Arguments arguments;
            Console.WriteLine("Loading Arguments...");
            string decoded = "";
            try
            {
                decoded = SharedUtils.Base64Decode(args[0]);
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
            return arguments;
        }
    }
}