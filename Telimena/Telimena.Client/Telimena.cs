using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telimena.Client
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Reflection;

    public interface ITelimena
    {
        Task ReportUsage(string functionName);
        UpdateResponse CheckForUpdates();
        RegistrationResponse RegisterClient();
        Task Initialize();
    }

    public class Telimena : ITelimena
    {
        public Telimena()
        {
            var assembly = Assembly.GetEntryAssembly();
            this.ProgramInfo = new ProgramInfo()
            {
                MainAssembly = assembly,
                Name = assembly.GetName().Name,
                Version = assembly.GetName().Version.ToString()
            };
            this.UserInfo = new UserInfo()
            {
                UserName = Environment.UserName,
                MachineName = Environment.MachineName
            };

            this.TelimenaVersion = Assembly.GetExecutingAssembly().GetName().Version.ToString();

            this.HttpClient = new HttpClient()
            {
                BaseAddress = new Uri(this.DefaultApiAddress)
            };
        }

        protected UserInfo UserInfo { get; }
        protected ProgramInfo ProgramInfo { get; }

        public bool SuppressAllErrors { get; set; } = true;
  
        protected string TelimenaVersion { get;  }

        private readonly string DefaultApiAddress = "";

        private HttpClient HttpClient { get; } 

        private async Task<string> SendGetRequest(string requestUri)
        {
            try
            {
                HttpResponseMessage response = await this.HttpClient.GetAsync(requestUri);
                 string content = await response.Content.ReadAsStringAsync();
                return content;
            }
            catch (Exception)
            {
                if (!this.SuppressAllErrors)
                {
                    throw;
                }

                return "";
            }
        }


        public Task ReportUsage(string functionName)
        {
            throw new System.NotImplementedException();
        }

        public UpdateResponse CheckForUpdates()
        {
            throw new System.NotImplementedException();
        }

        public RegistrationResponse RegisterClient()
        {
            return null;
        }

        public Task Initialize()
        {
            return Task.FromResult(true);
        }
    }
}
