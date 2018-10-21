using System;
using Newtonsoft.Json;
using TelimenaClient;

namespace AutomaticTestsClient
{
    public class TestAppWorker
    {
        private readonly Arguments arguments;

        public TestAppWorker(Arguments arguments)
        {
            this.arguments = arguments;
        }

        public void Work()
        {
            Telimena telimena = new Telimena(telemetryApiBaseUrl: new Uri(this.arguments.ApiUrl));
            try
            {
                switch (this.arguments.Action)
                {
                    case Actions.Initialize:
                        HandleInitialize(telimena);
                        break;
                    case Actions.ReportFunctionUsage:
                        telimena.ReportUsageBlocking();
                        break;
                    case Actions.HandleUpdates:
                        telimena.HandleUpdatesBlocking(false);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("Done");
        }

        private static void HandleInitialize(Telimena telimena)
        {

            var result = telimena.InitializeBlocking();

            Console.WriteLine(JsonConvert.SerializeObject(result));
        }
    }
}