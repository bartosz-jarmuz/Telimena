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
            client.Setup(x => x.PostAsync(ApiRoutes.UpdateProgramStatistics, It.IsAny<HttpContent>())).Callback(
                (string uri, HttpContent requestContent) =>
                {
                    throw new AggregateException(new AssertionException(uri)
                        , new AssertionException(requestContent.ReadAsStringAsync().GetAwaiter().GetResult()));
                });
            return client;
        }

        [Test]
        public void Test_StaticClient_IsProperViewRecorded()
        {
            Mock<ITelimenaHttpClient> client = this.GetMockClientForStaticClient_FirstRequestPass();
            try
            {
                TelimenaClient.Telimena.ReportUsageStatic( this.telemetryKey , client.Object, suppressAllErrors: false).GetAwaiter().GetResult();
                Assert.Fail("Error expected");
            }
            catch (Exception e)
            {
                TelimenaException ex = e as TelimenaException;
                TelemetryUpdateRequest jObj = ex.RequestObjects[1].Value as TelemetryUpdateRequest;
                Assert.AreEqual("Test_StaticClient_IsProperViewRecorded", jObj.ComponentName);
            }

            try
            {
                TelimenaClient.Telimena.ReportUsageStatic(this.telemetryKey, suppressAllErrors: false, telemetryApiBaseUrl:new Uri("http://localhost:666/")).GetAwaiter().GetResult();
                Assert.Fail("Error expected..");
            }
            catch (Exception e)
            {
                TelimenaException ex = e as TelimenaException;
                Assert.IsNotNull(ex, "Exception type: " + e.GetType());
                Assert.IsTrue(ex.Message.Contains("[Test_StaticClient_IsProperViewRecorded]"));
            }


            TelemetryUpdateResponse result = TelimenaClient.Telimena.ReportUsageStatic(this.telemetryKey, telemetryApiBaseUrl: new Uri("http://localhost:666/")).GetAwaiter().GetResult();
            TelimenaException err = result.Exception as TelimenaException;
            Assert.IsTrue(err.Message.Contains("[Test_StaticClient_IsProperViewRecorded]"));

            result = TelimenaClient.Telimena.ReportUsageStatic(this.telemetryKey, "BOOO", telemetryApiBaseUrl: new Uri("http://localhost:666/")).GetAwaiter().GetResult();
            err = result.Exception as TelimenaException;
            Assert.IsTrue(err.Message.Contains("[BOOO]"));

            result = TelimenaClient.Telimena.ReportUsageStatic(this.telemetryKey, new ProgramInfo(), telemetryApiBaseUrl: new Uri("http://localhost:666/")).GetAwaiter().GetResult();
            err = result.Exception as TelimenaException;
            Assert.IsTrue(err.Message.Contains("[Test_StaticClient_IsProperViewRecorded]"));

            result = TelimenaClient.Telimena.ReportUsageStatic(this.telemetryKey, telemetryApiBaseUrl: new Uri("http://localhost:666")).GetAwaiter().GetResult();
            err = result.Exception as TelimenaException;
            Assert.IsTrue(err.Message.Contains("[Test_StaticClient_IsProperViewRecorded]"));
        }

        [Test]
        public void Test_StaticClient_RegisterRequest()
        {
            Mock<ITelimenaHttpClient> client = Helpers.GetMockClient();
            try
            {
                TelemetryUpdateResponse result = TelimenaClient.Telimena.ReportUsageStatic(this.telemetryKey, client.Object, suppressAllErrors: false).GetAwaiter().GetResult();
                Assert.Fail("Error expected");
            }
            catch (Exception e)
            {
                TelimenaException ex = e as TelimenaException;

                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreEqual("An error occured while posting to [api/Telemetry/Initialize]", ex.InnerExceptions[0].Message);
                TelemetryInitializeRequest jObj = ex.RequestObjects[0].Value as TelemetryInitializeRequest;
                Assert.AreEqual(true, jObj.SkipUsageIncrementation);
                Assert.AreEqual(Assembly.GetExecutingAssembly().GetName().Name, jObj.ProgramInfo.Name);
                Assert.AreEqual("Telimena.Client.Tests", jObj.ProgramInfo.PrimaryAssembly.Name);
            }

            client = this.GetMockClientForStaticClient_FirstRequestPass();
            try
            {
                TelemetryUpdateResponse result = TelimenaClient.Telimena.ReportUsageStatic(this.telemetryKey, client.Object, suppressAllErrors: false).GetAwaiter().GetResult();
                Assert.Fail("Error expected");
            }
            catch (Exception e)
            {
                TelimenaException ex = e as TelimenaException;

                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreEqual("An error occured while posting to [api/Statistics/Update]", ex.InnerExceptions[0].Message);
                TelemetryUpdateRequest jObj = ex.RequestObjects[1].Value as TelemetryUpdateRequest;
                Assert.AreEqual(this.telemetryKey, jObj.TelemetryKey);
                Assert.AreEqual(this.returnedUserGuid, jObj.UserId);
                Assert.AreEqual("Test_StaticClient_RegisterRequest", jObj.ComponentName);
                Assert.IsTrue(Version.TryParse(jObj.VersionData.AssemblyVersion, out _));
            }
        }

        [Test]
        public void Test_StaticClient_WithProgramInfo()
        {
            Mock<ITelimenaHttpClient> client = Helpers.GetMockClient();
            ProgramInfo pi = new ProgramInfo {Name = "An App!", PrimaryAssembly = new AssemblyInfo(this.GetType().Assembly)};
            try
            {
                TelemetryUpdateResponse result = TelimenaClient.Telimena.ReportUsageStatic(this.telemetryKey, client.Object, pi, suppressAllErrors: false).GetAwaiter().GetResult();
                Assert.Fail("Error expected");
            }
            catch (Exception e)
            {
                TelimenaException ex = e as TelimenaException;

                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreEqual("An error occured while posting to [api/Telemetry/Initialize]", ex.InnerExceptions[0].Message);
                TelemetryInitializeRequest jObj = ex.RequestObjects[0].Value as TelemetryInitializeRequest;
                Assert.AreEqual(true, jObj.SkipUsageIncrementation);

                pi.ThrowIfPublicPropertiesNotEqual(jObj.ProgramInfo, true);
            }
        }
    }
}