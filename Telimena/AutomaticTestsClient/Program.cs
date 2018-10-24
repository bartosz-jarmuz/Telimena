using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace AutomaticTestsClient
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length == 0)
            {
                Console.WriteLine($"TestAppVersion: {typeof(Program).Assembly.GetName().Version}");
                Console.WriteLine("This app requires arguments to run");
                Console.ReadKey();
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
