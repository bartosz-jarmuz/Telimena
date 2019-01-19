// -----------------------------------------------------------------------
//  <copyright file="ClientTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using DotNetLittleHelpers;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace TelimenaClient.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class TestStaticClient
    {

        private readonly Guid telemetryKey = Guid.Parse("dc13cced-30ea-4628-a81d-21d86f37df95");
        private readonly Guid returnedUserGuid = Guid.Parse("59154c52-b858-429e-8071-30763b2c8811");


        private Mock<ITelimenaHttpClient> GetMockClientForStaticClient_FirstRequestPass()
        {
            Mock<ITelimenaHttpClient> client = new Mock<ITelimenaHttpClient>();
            client.Setup(x => x.PostAsync(ApiRoutes.Initialize, It.IsAny<HttpContent>())).Returns((string uri, HttpContent requestContent) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();
                TelemetryInitializeResponse telemetryInitializeResponse = new TelemetryInitializeResponse { UserId = this.returnedUserGuid };
                response.Content = new StringContent(JsonConvert.SerializeObject(telemetryInitializeResponse));
                return Task.FromResult(response);
            });
            client.Setup(x => x.PostAsync("TODO", It.IsAny<HttpContent>())).Callback(
                (string uri, HttpContent requestContent) =>
                {
                    throw new AggregateException(new AssertionException(uri)
                        , new AssertionException(requestContent.ReadAsStringAsync().GetAwaiter().GetResult()));
                });
            return client;
        }

    

        //[Test]
        //public void Test_StaticClient_RegisterRequest()
        //{
        //    var si = new TelimenaStartupInfo(this.telemetryKey);
        //    si.TelemetryApiBaseUrl = new Uri("http://localhost:667");
        //        TelemetryUpdateResponse result = TelimenaClient.Telimena.Telemetry.Async.Event(si, "Boo").GetAwaiter().GetResult();
        //        Assert.AreEqual("Error occurred while sending update [Boo] telemetry request to [api/v1/telemetry]", result.Exception.Message);
        //    Assert.IsInstanceOf<TelimenaException>(result?.Exception.InnerException);
        //    TelimenaException registrationError = result.Exception.InnerException as TelimenaException;

        //    Assert.AreEqual(1, registrationError.InnerExceptions.Count);
        //    StringAssert.Contains("An error occurred while posting to [api/v1/telemetry/initialize]", registrationError.InnerExceptions[0].Message);
        //    TelemetryInitializeRequest jObj = registrationError.RequestObjects[0].Value as TelemetryInitializeRequest;
        //    Assert.AreEqual(Assembly.GetExecutingAssembly().GetName().Name, jObj.ProgramInfo.Name);
        //    Assert.AreEqual("Telimena.Client.Tests", jObj.ProgramInfo.PrimaryAssembly.Name);Ł

        //}

        //[Test]
        //public void Test_StaticClient_WithProgramInfo()
        //{
        //    ProgramInfo pi = new ProgramInfo {Name = "An App!", PrimaryAssembly = new AssemblyInfo(this.GetType().Assembly)};
        //    var si = new TelimenaStartupInfo(this.telemetryKey);
        //    si.ProgramInfo = pi;
        //    si.TelemetryApiBaseUrl = new Uri("http://localhost:667");
        //        TelemetryUpdateResponse result = TelimenaClient.Telimena.Telemetry.Async.View(si, "Booo").GetAwaiter().GetResult();
        //    Assert.AreEqual("Error occurred while sending update [Booo] telemetry request to [api/v1/telemetry]", result.Exception.Message);
        //    Assert.AreEqual("An App!", (result.Exception as TelimenaException).TelimenaProperties.StaticProgramInfo.Name);
        //}
    }
}