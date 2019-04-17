using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TelimenaClient;
using TelimenaClient.Model;
using TelimenaClient.Model.Internal;

namespace SharedLogic
{
    public class TestAppWorker
    {
        public TestAppWorker(Arguments arguments)
        {
            this.arguments = arguments;
        }

        private readonly Arguments arguments;

        public void Work()
        {
            ITelimena telimena = this.GetTelimena(this.arguments.TelemetryKey);

            try
            {
                switch (this.arguments.Action)
                {
                    case Actions.Initialize:
                        this.HandleInitialize(telimena);
                        break;
                    case Actions.ReportViewUsage:
                        this.HandleReportViewUsage(telimena);
                        break;
                    case Actions.HandleUpdates:
                        this.HandleUpdates(telimena, false);
                        break;
                    case Actions.CheckAndInstallUpdates:
                        this.CheckAndInstallUpdates(telimena, false);
                        break;
                    case Actions.HandleUpdatesWithBeta:
                        this.HandleUpdates(telimena, true);
                        break;
                }
                Thread.Sleep(7*1000);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("Done");
        }

        private void CheckAndInstallUpdates(ITelimena telimena, bool takeBeta)
        {
            Console.WriteLine($"Starting {MethodBase.GetCurrentMethod().Name}...");

            UpdateCheckResult result = telimena.Update.CheckForUpdates();
            Console.WriteLine($"Finished update check. Update available: {result.IsUpdateAvailable}...");

            JsonSerializerSettings settings = new JsonSerializerSettings { ContractResolver = new MyJsonContractResolver(), TypeNameHandling = TypeNameHandling.Auto };
            Console.WriteLine(JsonConvert.SerializeObject(result, settings));

            telimena.Update.InstallUpdates(result, takeBeta);


            Console.WriteLine($"Finished {MethodBase.GetCurrentMethod().Name}");
        }

        private ITelimena GetTelimena(Guid argumentsTelemetryKey)
        {
            ITelimena telimena;

            if (this.arguments.ProgramInfo != null)
            {
                TelimenaStartupInfo si = new TelimenaStartupInfo(argumentsTelemetryKey, new Uri(this.arguments.ApiUrl))
                {
                    ProgramInfo = this.arguments.ProgramInfo
                    ,DeliveryInterval = TimeSpan.FromSeconds(3)
                };
                telimena = TelimenaFactory.Construct(si);
            }
            else
            {
                telimena = TelimenaFactory.Construct(new TelimenaStartupInfo(argumentsTelemetryKey, new Uri(this.arguments.ApiUrl))
                {
                    DeliveryInterval = TimeSpan.FromSeconds(3)
                });
            }

            return telimena;
        }

        private void HandleInitialize(ITelimena telimena)
        {
            Console.WriteLine("Sending Initialize request");
            TelemetryInitializeResponse result = (telimena as Telimena).Initialize().GetAwaiter().GetResult();
            Console.WriteLine("Received Initialize response");
            Console.WriteLine(JsonConvert.SerializeObject(result));
        }

        private void HandleReportViewUsage(ITelimena telimena)
        {
            Console.WriteLine("Sending View usage report");

            Dictionary<string, string> customData = new Dictionary<string, string>();
            customData.Add("Time", DateTime.Now.ToShortTimeString());
            customData.Add("RandomNumber", new Random().Next(0, 10).ToString());
            if (this.arguments.ViewName != null)
            {
                 telimena.Track.View(this.arguments.ViewName, customData);
            }
            else
            {
                 telimena.Track.View("DefaultView", customData);
            }
            Console.WriteLine("Received View usage response..");

            JsonSerializerSettings settings = new JsonSerializerSettings { ContractResolver = new MyJsonContractResolver(), TypeNameHandling = TypeNameHandling.Auto };
            //todo - some result - Console.WriteLine(JsonConvert.SerializeObject(result, settings));
        }

        private void HandleUpdates(ITelimena telimena, bool takeBeta)
        {
            Console.WriteLine("Starting update handling...");

            UpdateCheckResult result = telimena.Update.HandleUpdates(takeBeta);
            Console.WriteLine("Finished update handling...");
            JsonSerializerSettings settings = new JsonSerializerSettings {ContractResolver = new MyJsonContractResolver(), TypeNameHandling = TypeNameHandling.Auto};
            Console.WriteLine(JsonConvert.SerializeObject(result, settings));

            Console.WriteLine("Updating done");
        }
    }

    internal class MyJsonContractResolver : DefaultContractResolver
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
}