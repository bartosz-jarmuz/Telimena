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
            Telimena telimena = this.GetTelimena(this.arguments.TelemetryKey);


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

        private Telimena GetTelimena(Guid argumentsTelemetryKey)
        {
            Telimena telimena;

            if (this.arguments.ProgramInfo != null)
            {
                TelimenaStartupInfo si = new TelimenaStartupInfo(argumentsTelemetryKey, new Uri(this.arguments.ApiUrl))
                {
                    ProgramInfo = this.arguments.ProgramInfo
                };
                telimena = new Telimena(si);
            }
            else
            {
                telimena = new Telimena(new TelimenaStartupInfo(argumentsTelemetryKey, new Uri(this.arguments.ApiUrl)));
            }

            return telimena;
        }

        private void HandleInitialize(Telimena telimena)
        {
            TelemetryInitializeResponse result = telimena.Blocking.Initialize();

            Console.WriteLine(JsonConvert.SerializeObject(result));
        }

        private void HandleReportViewUsage(Telimena telimena)
        {
            TelemetryUpdateResponse result;
            Dictionary<string, string> customData = new Dictionary<string, string>();
            customData.Add("Time", DateTime.Now.ToShortTimeString());
            customData.Add("RandomNumber", new Random().Next(0, 10).ToString());
            if (this.arguments.ViewName != null)
            {
                result = telimena.Blocking.ReportViewAccessed(this.arguments.ViewName, customData);
            }
            else
            {
                result = telimena.Blocking.ReportViewAccessed("DefaultView", customData);
            }

            Console.WriteLine(JsonConvert.SerializeObject(result));
        }

        private void HandleUpdates(Telimena telimena)
        {
            Console.WriteLine("Starting update handling...");

            UpdateCheckResult result = telimena.Blocking.HandleUpdates(false);
            Console.WriteLine("Finished update handling");
            JsonSerializerSettings settings = new JsonSerializerSettings {ContractResolver = new MyJsonContractResolver()};

            Console.WriteLine(JsonConvert.SerializeObject(result));

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