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
using TelimenaClient;

namespace Telimena.Client.Tests
{
    #region Using

    #endregion

    [TestFixture, Ignore("Static tests seem problematic ATM")]
    public class TestStaticClient
    {
        private Mock<ITelimenaHttpClient> GetMockClientForStaticClient_FirstRequestPass()
        {
            Mock<ITelimenaHttpClient> client = new Mock<ITelimenaHttpClient>();
            client.Setup(x => x.PostAsync(ApiRoutes.RegisterClient, It.IsAny<HttpContent>())).Returns((string uri, HttpContent requestContent) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();
                RegistrationResponse registrationResponse = new RegistrationResponse {Count = 0, ProgramId = 1, UserId = 2};
                response.Content = new StringContent(JsonConvert.SerializeObject(registrationResponse));
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

        [Test, Ignore("temporarily")]
        public void Test_StaticClient_IsProperFunctionRecorded()
        {
            Mock<ITelimenaHttpClient> client = this.GetMockClientForStaticClient_FirstRequestPass();
            try
            {
                TelimenaClient.Telimena.ReportUsageStatic(client.Object, suppressAllErrors: false).GetAwaiter().GetResult();
                Assert.Fail("Error expected");
            }
            catch (Exception e)
            {
                TelimenaException ex = e as TelimenaException;
                StatisticsUpdateRequest jObj = ex.RequestObjects[1].Value as StatisticsUpdateRequest;
                Assert.AreEqual("Test_StaticClient_IsProperFunctionRecorded", jObj.FunctionName);
            }

            try
            {
                TelimenaClient.Telimena.ReportUsageStatic(suppressAllErrors: false, telemetryApiBaseUrl:new Uri("http://localhost:7757/")).GetAwaiter().GetResult();
                Assert.Fail("Error expected..");
            }
            catch (Exception e)
            {
                TelimenaException ex = e as TelimenaException;
                Assert.IsNotNull(ex, "Exception type: " + e.GetType());
                Assert.IsTrue(ex.Message.Contains("[Test_StaticClient_IsProperFunctionRecorded]"));
            }


            StatisticsUpdateResponse result = TelimenaClient.Telimena.ReportUsageStatic(telemetryApiBaseUrl: new Uri("http://localhost:7757/")).GetAwaiter().GetResult();
            TelimenaException err = result.Exception as TelimenaException;
            Assert.IsTrue(err.Message.Contains("[Test_StaticClient_IsProperFunctionRecorded]"));

            result = TelimenaClient.Telimena.ReportUsageStatic("BOOO", telemetryApiBaseUrl: new Uri("http://localhost:7757/")).GetAwaiter().GetResult();
            err = result.Exception as TelimenaException;
            Assert.IsTrue(err.Message.Contains("[BOOO]"));

            result = TelimenaClient.Telimena.ReportUsageStatic(new ProgramInfo(), telemetryApiBaseUrl: new Uri("http://localhost:7757/")).GetAwaiter().GetResult();
            err = result.Exception as TelimenaException;
            Assert.IsTrue(err.Message.Contains("[Test_StaticClient_IsProperFunctionRecorded]"));

            result = TelimenaClient.Telimena.ReportUsageStatic(telemetryApiBaseUrl: new Uri("http://localhost:666")).GetAwaiter().GetResult();
            err = result.Exception as TelimenaException;
            Assert.IsTrue(err.Message.Contains("[Test_StaticClient_IsProperFunctionRecorded]"));
        }

        [Test]
        public void Test_StaticClient_RegisterRequest()
        {
            Mock<ITelimenaHttpClient> client = Helpers.GetMockClient();
            try
            {
                StatisticsUpdateResponse result = TelimenaClient.Telimena.ReportUsageStatic(client.Object, suppressAllErrors: false).GetAwaiter().GetResult();
                Assert.Fail("Error expected");
            }
            catch (Exception e)
            {
                TelimenaException ex = e as TelimenaException;

                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreEqual("An error occured while posting to [api/Statistics/RegisterClient]", ex.InnerExceptions[0].Message);
                RegistrationRequest jObj = ex.RequestObjects[0].Value as RegistrationRequest;
                Assert.AreEqual(true, jObj.SkipUsageIncrementation);
                Assert.AreEqual(Assembly.GetExecutingAssembly().GetName().Name, jObj.ProgramInfo.Name);
                Assert.AreEqual("Telimena.Client.Tests", jObj.ProgramInfo.PrimaryAssembly.Name);
            }

            client = this.GetMockClientForStaticClient_FirstRequestPass();
            try
            {
                StatisticsUpdateResponse result = TelimenaClient.Telimena.ReportUsageStatic(client.Object, suppressAllErrors: false).GetAwaiter().GetResult();
                Assert.Fail("Error expected");
            }
            catch (Exception e)
            {
                TelimenaException ex = e as TelimenaException;

                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreEqual("An error occured while posting to [api/Statistics/Update]", ex.InnerExceptions[0].Message);
                StatisticsUpdateRequest jObj = ex.RequestObjects[1].Value as StatisticsUpdateRequest;
                Assert.AreEqual(1, jObj.ProgramId);
                Assert.AreEqual(2, jObj.UserId);
                Assert.AreEqual("Test_StaticClient_RegisterRequest", jObj.FunctionName);
                Assert.IsTrue(Version.TryParse(jObj.Version, out _));
            }
        }

        [Test]
        public void Test_StaticClient_WithProgramInfo()
        {
            Mock<ITelimenaHttpClient> client = Helpers.GetMockClient();
            ProgramInfo pi = new ProgramInfo {Name = "An App!", PrimaryAssembly = new AssemblyInfo(this.GetType().Assembly)};
            try
            {
                StatisticsUpdateResponse result = TelimenaClient.Telimena.ReportUsageStatic(client.Object, pi, suppressAllErrors: false).GetAwaiter().GetResult();
                Assert.Fail("Error expected");
            }
            catch (Exception e)
            {
                TelimenaException ex = e as TelimenaException;

                Assert.AreEqual(1, ex.InnerExceptions.Count);
                Assert.AreEqual("An error occured while posting to [api/Statistics/RegisterClient]", ex.InnerExceptions[0].Message);
                RegistrationRequest jObj = ex.RequestObjects[0].Value as RegistrationRequest;
                Assert.AreEqual(true, jObj.SkipUsageIncrementation);

                pi.ThrowIfPublicPropertiesNotEqual(jObj.ProgramInfo, true);
            }
        }
    }
}