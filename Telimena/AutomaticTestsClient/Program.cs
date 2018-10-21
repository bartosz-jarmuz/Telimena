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
            Arguments arguments;
            try
            {
                arguments = JsonConvert.DeserializeObject<Arguments>(Base64Decode(args[0]));
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Error while deserializing [{args[0]}]", ex);
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
