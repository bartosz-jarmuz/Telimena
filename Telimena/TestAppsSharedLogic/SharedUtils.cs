using System;
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

        public static void ShowArgsRequiredMessage(string[] args, Type type, string appName)
        {
            var msg = $"AssemblyVersion: {TelimenaVersionReader.Read(type, VersionTypes.AssemblyVersion)}\r\n" +
                      $"FileVersion: {TelimenaVersionReader.Read(type, VersionTypes.FileVersion)}\r\n" +
                      $"Telimena Version: {TelimenaVersionReader.Read(typeof(Telimena), VersionTypes.AssemblyVersion)}\r\n" +
                      $"Telimena File Version: {TelimenaVersionReader.Read(typeof(Telimena), VersionTypes.FileVersion)}";
            Console.WriteLine(msg);
            if (args.Length == 0)
            {
                MessageBox.Show(msg, $"{appName} - This app requires arguments to run");
                return;
            }
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