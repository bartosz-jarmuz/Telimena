using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.TestFramework;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using TelimenaClient.Model;
using TelimenaClient.Serializer;
using TelimenaTestSandboxApp;

namespace TelimenaClient.Tests
{
    internal static class Helpers
    {
        public static readonly Uri TeliUri = new Uri("http://localhost:7757/");
        
        public static string TestAppDataPath =>
            Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).FullName, "FakeAppData");


        public static Mock<ITelimenaHttpClient> GetMockClient()
        {
            Mock<ITelimenaHttpClient> client = new Mock<ITelimenaHttpClient>();
            client.Setup(x => x.PostAsync(It.IsAny<string>(), It.IsAny<HttpContent>())).Callback((string uri, HttpContent content) =>
            {
                throw new AggregateException(new AssertionException(uri), new AssertionException(content.ReadAsStringAsync().GetAwaiter().GetResult()));
            });
            return client;
        }

        public static void SetupMockHttpClient(ITelimena telimena, Mock<ITelimenaHttpClient> client)
        {
            ((Telimena) telimena).Messenger = new Messenger(((Telimena) telimena)?.Serializer, client.Object);
        }

        public static  IMessenger GetMessenger_InitializeAndAcceptTelemetry(Guid key)
        {
            Mock<ITelimenaHttpClient> client = new Mock<ITelimenaHttpClient>();
            client.Setup(x => x.GetAsync(It.IsRegex("^" + Regex.Escape(ApiRoutes.GetProgramUpdaterName(key))))).Returns((string uri) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();
                response.Content = new StringContent("Updater.exe");
                return Task.FromResult(response);
            });
            client.Setup(x => x.PostAsync(ApiRoutes.Initialize, It.IsAny<HttpContent>())).Returns((string uri, HttpContent requestContent) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();
                TelemetryInitializeResponse telemetryInitializeResponse = new TelemetryInitializeResponse { UserId = Guid.NewGuid() };
                response.Content = new StringContent(JsonConvert.SerializeObject(telemetryInitializeResponse));
                return Task.FromResult(response);
            });
            client.Setup(x => x.PostAsync(It.IsIn(ApiRoutes.PostTelemetryData), It.IsAny<HttpContent>())).Returns(
                (string uri, HttpContent requestContent) => Task.FromResult(new HttpResponseMessage(HttpStatusCode.Accepted)));
            return new Messenger(new TelimenaSerializer(), client.Object);
            ;
        }

        [TestFixture]
        class TestSandboxAppBuilding
        {
            [Test]
            public void JustCallItEnsureItBuilds()
            {
                TelimenaHammer hammer = null;//no action needed
                Assert.IsNull(hammer);
            }
        }
    }
}