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
                TelemetryInitializeResponse telemetryInitializeResponse = new TelemetryInitializeResponse {Count = 0,  UserId = this.returnedUserGuid };
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

    

        [Test]
        public void Test_StaticClient_RegisterRequest()
        {
            //try
            //{
                TelemetryUpdateResponse result = TelimenaClient.Telimena.Telemetry.Async.Event(new TelimenaStartupInfo(this.telemetryKey), "Boo").GetAwaiter().GetResult();
                Assert.AreEqual("Error occurred while sending update [Boo] telemetry request to [api/v1/Telemetry/Event]", result.Exception.Message);
                
          //      Assert.Fail("Error expected");
     //       }
            //catch (Exception e)
            //{
            //    TelimenaException ex = e as TelimenaException;

            //    Assert.AreEqual(1, ex.InnerExceptions.Count);
            //    Assert.IsTrue(ex.InnerExceptions[0].Message.Contains("An error occured while posting to [api/Telemetry/Initialize]"));
            //    TelemetryInitializeRequest jObj = ex.RequestObjects[0].Value as TelemetryInitializeRequest;
            //    Assert.AreEqual(true, jObj.SkipUsageIncrementation);
            //    Assert.AreEqual(Assembly.GetExecutingAssembly().GetName().Name, jObj.ProgramInfo.Name);
            //    Assert.AreEqual("Telimena.Client.Tests", jObj.ProgramInfo.PrimaryAssembly.Name);
            //}

            //client = this.GetMockClientForStaticClient_FirstRequestPass();
            //try
            //{
            //    TelemetryUpdateResponse result = TelimenaClient.Telimena.ReportUsageStatic(this.telemetryKey, client.Object, suppressAllErrors: false).GetAwaiter().GetResult();
            //    Assert.Fail("Error expected");
            //}
            //catch (Exception e)
            //{
            //    TelimenaException ex = e as TelimenaException;

            //    Assert.AreEqual(1, ex.InnerExceptions.Count);
            //    Assert.IsTrue(ex.InnerExceptions[0].Message.Contains("An error occured while posting to [api/Statistics/Update]"));
            //    TelemetryUpdateRequest jObj = ex.RequestObjects[1].Value as TelemetryUpdateRequest;
            //    Assert.AreEqual(this.telemetryKey, jObj.TelemetryKey);
            //    Assert.AreEqual(this.returnedUserGuid, jObj.UserId);
            //    Assert.AreEqual("Test_StaticClient_RegisterRequest", jObj.ComponentName);
            //    Assert.IsTrue(Version.TryParse(jObj.VersionData.AssemblyVersion, out _));
            //}
        }

        [Test]
        public void Test_StaticClient_WithProgramInfo()
        {
       //     Mock<ITelimenaHttpClient> client = Helpers.GetMockClient();
            ProgramInfo pi = new ProgramInfo {Name = "An App!", PrimaryAssembly = new AssemblyInfo(this.GetType().Assembly)};
            var si = new TelimenaStartupInfo(this.telemetryKey);
            si.ProgramInfo = pi;
            //try
            //{
                TelemetryUpdateResponse result = TelimenaClient.Telimena.Telemetry.Async.View(si, "Booo").GetAwaiter().GetResult();
            Assert.AreEqual("Error occurred while sending update [Booo] telemetry request to [api/v1/Telemetry/View]", result.Exception.Message);
            Assert.AreEqual("An App!", (result.Exception as TelimenaException).TelimenaProperties.StaticProgramInfo.Name);

            //   Assert.Fail("Error expected");
            //}
            //catch (Exception e)
            //{
            //    TelimenaException ex = e as TelimenaException;

            //    Assert.AreEqual(1, ex.InnerExceptions.Count);
            //    Assert.AreEqual("An error occured while posting to [api/Telemetry/Initialize]. Base URL []", ex.InnerExceptions[0].Message);

            //    TelemetryInitializeRequest jObj = ex.RequestObjects[0].Value as TelemetryInitializeRequest;
            //    Assert.AreEqual(true, jObj.SkipUsageIncrementation);

            //    pi.ThrowIfPublicPropertiesNotEqual(jObj.ProgramInfo, true);
            //}
        }
    }
}