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
            Telimena telimena = this.GetTelimena();


            try
            {
                switch (this.arguments.Action)
                {
                    case Actions.Initialize:
                        this.HandleInitialize(telimena);
                        break;
                    case Actions.ReportFunctionUsage:
                        this.HandleReportFunctionUsage(telimena);
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

        private Telimena GetTelimena()
        {
            Telimena telimena;
            if (this.arguments.ProgramInfo != null)
            {
             
                telimena = new Telimena(telemetryApiBaseUrl: new Uri(this.arguments.ApiUrl), programInfo: this.arguments.ProgramInfo);
            }
            else
            {
                telimena = new Telimena(telemetryApiBaseUrl: new Uri(this.arguments.ApiUrl));
            }

            return telimena;
        }

        private void HandleInitialize(Telimena telimena)
        {

            RegistrationResponse result = telimena.InitializeBlocking();

            Console.WriteLine(JsonConvert.SerializeObject(result));
        }

        private void HandleReportFunctionUsage(Telimena telimena)
        {
            
            StatisticsUpdateResponse result;
            if (this.arguments.FunctionName != null)
            {
                result = telimena.ReportUsageBlocking(this.arguments.FunctionName);
            }
            else
            {
                result = telimena.ReportUsageBlocking();
            }

            Console.WriteLine(JsonConvert.SerializeObject(result));
        }
    }
}