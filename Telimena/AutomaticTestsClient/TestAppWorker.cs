using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using TelimenaClient;

namespace AutomaticTestsClient
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
                        this.HandleUpdates(telimena);
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            Console.WriteLine("Done");
        }

        private ITelimena GetTelimena(Guid argumentsTelemetryKey)
        {
            ITelimena telimena;

            if (this.arguments.ProgramInfo != null)
            {
                TelimenaStartupInfo si = new TelimenaStartupInfo(argumentsTelemetryKey, new Uri(this.arguments.ApiUrl))
                {
                    ProgramInfo = this.arguments.ProgramInfo
                };
                telimena = Telimena.Construct(si);
            }
            else
            {
                telimena = Telimena.Construct(new TelimenaStartupInfo(argumentsTelemetryKey, new Uri(this.arguments.ApiUrl)));
            }

            return telimena;
        }

        private void HandleInitialize(ITelimena telimena)
        {
            Console.WriteLine("Sending Initialize request");
            TelemetryInitializeResponse result = telimena.Telemetry.Blocking.Initialize();
            Console.WriteLine("Received Initialize response");

            Console.WriteLine(JsonConvert.SerializeObject(result));
        }

        private void HandleReportViewUsage(ITelimena telimena)
        {
            Console.WriteLine("Sending View usage report");

            TelemetryItem result;
            Dictionary<string, object> customData = new Dictionary<string, object>();
            customData.Add("Time", DateTime.Now.ToShortTimeString());
            customData.Add("RandomNumber", new Random().Next(0, 10).ToString());
            if (this.arguments.ViewName != null)
            {
                result = telimena.Telemetry.Blocking.View(this.arguments.ViewName, customData);
            }
            else
            {
                result = telimena.Telemetry.Blocking.View("DefaultView", customData);
            }
            Console.WriteLine("Received View usage response..");

            JsonSerializerSettings settings = new JsonSerializerSettings { ContractResolver = new MyJsonContractResolver(), TypeNameHandling = TypeNameHandling.Auto };
            Console.WriteLine(JsonConvert.SerializeObject(result, settings));
        }

        private void HandleUpdates(ITelimena telimena)
        {
            Console.WriteLine("Starting update handling...");

            UpdateCheckResult result = telimena.Updates.Blocking.HandleUpdates(false);
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