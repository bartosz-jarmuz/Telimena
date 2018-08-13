// -----------------------------------------------------------------------
//  <copyright file="ClientTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Threading.Tasks;
using DotNetLittleHelpers;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;

namespace Telimena.Client.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class TestStaticClient
    {
        private Mock<ITelimenaHttpClient> GetMockClientForStaticClient_FirstRequestPass()
        {
            Mock<ITelimenaHttpClient> client = new Mock<ITelimenaHttpClient>();
            client.Setup(x => x.PostAsync("api/Statistics/RegisterClient", It.IsAny<HttpContent>())).Returns((string uri, HttpContent requestContent) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();
                RegistrationResponse registrationResponse = new RegistrationResponse
                {
                    Count = 0,
                    ProgramId = 1,
                    UserId = 2
                };
                response.Content = new StringContent(JsonConvert.SerializeObject(registrationResponse));
                return Task.FromResult(response);
            });
            client.Setup(x => x.PostAsync("api/Statistics/UpdateProgramStatistics", It.IsAny<HttpContent>())).Callback(
                (string uri, HttpContent requestContent) =>
                {
                    throw new AggregateException(
                        new AssertionException(uri),
                        new AssertionException(requestContent.ReadAsStringAsync().GetAwaiter().GetResult()));
                });
            return client;
        }

        [Test]
        public void Test_StaticClient_RegisterRequest()
        {
            Mock<ITelimenaHttpClient> client = Helpers.GetMockClient();
            try
            {
                StatisticsUpdateResponse result = Client.Telimena.SendUsageReport(client.Object, suppressAllErrors: false).GetAwaiter().GetResult();
                Assert.Fail("Error expected");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(2, ex.InnerExceptions.Count);
                Assert.AreEqual("api/Statistics/RegisterClient", ex.InnerExceptions[0].Message);
                RegistrationRequest jObj = JsonConvert.DeserializeObject<RegistrationRequest>(ex.InnerExceptions[1].Message);
                Assert.AreEqual(true, jObj.SkipUsageIncrementation);
                Assert.AreEqual("Telimena.Client", jObj.ProgramInfo.Name);
                Assert.AreEqual("Telimena.Client", jObj.ProgramInfo.PrimaryAssembly.Name);
            }

            client = this.GetMockClientForStaticClient_FirstRequestPass();
            try
            {
                StatisticsUpdateResponse result = Client.Telimena.SendUsageReport(client.Object, suppressAllErrors: false).GetAwaiter().GetResult();
                Assert.Fail("Error expected");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(2, ex.InnerExceptions.Count);
                Assert.AreEqual("api/Statistics/UpdateProgramStatistics", ex.InnerExceptions[0].Message);
                StatisticsUpdateRequest jObj = JsonConvert.DeserializeObject<StatisticsUpdateRequest>(ex.InnerExceptions[1].Message);
                Assert.AreEqual(1, jObj.ProgramId);
                Assert.AreEqual(2, jObj.UserId);
                Assert.AreEqual("Test_StaticClient_RegisterRequest", jObj.FunctionName);
                Assert.AreEqual("1.0.0.0", jObj.Version);
            }
        }


        [Test]
        public void Test_StaticClient_WithProgramInfo()
        {
            Mock<ITelimenaHttpClient> client = Helpers.GetMockClient();
            ProgramInfo pi = new ProgramInfo
            {
                Name = "An App!",
                PrimaryAssembly = new AssemblyInfo(this.GetType().Assembly)
            };
            try
            {
                StatisticsUpdateResponse result = Client.Telimena.SendUsageReport(client.Object, pi, suppressAllErrors: false).GetAwaiter().GetResult();
                Assert.Fail("Error expected");
            }
            catch (AggregateException ex)
            {
                Assert.AreEqual(2, ex.InnerExceptions.Count);
                Assert.AreEqual("api/Statistics/RegisterClient", ex.InnerExceptions[0].Message);
                RegistrationRequest jObj = JsonConvert.DeserializeObject<RegistrationRequest>(ex.InnerExceptions[1].Message);
                Assert.AreEqual(true, jObj.SkipUsageIncrementation);
                pi.ThrowIfPublicPropertiesNotEqual(jObj.ProgramInfo, true);
            }
        }
    }
}