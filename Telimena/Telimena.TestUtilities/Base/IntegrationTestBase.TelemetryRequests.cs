
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Microsoft.Deployment.WindowsInstaller;
using Newtonsoft.Json;
using NUnit.Framework;
using SharedLogic;
using Telimena.TestUtilities.Base.TestAppInteraction;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Messages;
using TelimenaClient.Model;

namespace Telimena.TestUtilities.Base
{
    [TestFixture]
    public abstract partial class IntegrationTestBase 
    {

        protected async Task<TelemetryQueryResponse> CheckTelemetry(TelemetryQueryRequest request)
        {
            HttpResponseMessage response = await this.HttpClient.PostAsJsonAsync(this.BaseUrl.TrimEnd('/') + "/api/v1/telemetry/execute-query", request).ConfigureAwait(false);

            var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

            return JsonConvert.DeserializeObject<TelemetryQueryResponse>(content);
        }

        public async Task SendBasicTelemetry(Guid guid, BasicTelemetryItem telemetry)
        {
            HttpResponseMessage response = await this.HttpClient.PostAsJsonAsync($"{this.BaseUrl.TrimEnd('/')}/api/v1/telemetry/{guid.ToString()}/basic", telemetry).ConfigureAwait(false);
            var text = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            Console.WriteLine(text);
            response.EnsureSuccessStatusCode();
        }
    }
}