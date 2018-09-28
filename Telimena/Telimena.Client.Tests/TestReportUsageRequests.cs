// -----------------------------------------------------------------------
//  <copyright file="TestRegistrationRequests.cs" company="SDL plc">
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
using TelimenaClient;
using TelimenaClient.Serializer;

namespace Telimena.Client.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class TestReportUsageRequests
    {
        private class CustomDataObject
        {
            public int SomeValue { get; set; }

            public Exception TrySerializingThisBadBoy { get; set; }
        }

        private class NonSerializableObject
        {
            public NonSerializableObject(int someValue)
            {
                this.SomeValue = this;
            }

            public NonSerializableObject SomeValue { get; set; }
        }

        private IMessenger GetMessenger_FirstRequestPass()
        {
            Mock<ITelimenaHttpClient> client = new Mock<ITelimenaHttpClient>();
            client.Setup(x => x.PostAsync(ApiRoutes.RegisterClient, It.IsAny<HttpContent>())).Returns((string uri, HttpContent requestContent) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();
                RegistrationResponse registrationResponse = new RegistrationResponse { Count = 0, ProgramId = 1, UserId = 2 };
                response.Content = new StringContent(JsonConvert.SerializeObject(registrationResponse));
                return Task.FromResult(response);
            });
            client.Setup(x => x.PostAsync(ApiRoutes.UpdateProgramStatistics, It.IsAny<HttpContent>())).Callback(
                (string uri, HttpContent requestContent) =>
                {
                    throw new AggregateException(new AssertionException(uri)
                        , new AssertionException(requestContent.ReadAsStringAsync().GetAwaiter().GetResult()));
                });
            return new Messenger(new TelimenaSerializer(), client.Object); ;
        }

        [Test]
        public void Test_NoCustomData()
        {

            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena
            {
                SuppressAllErrors = false
            };
            telimena.Messenger = this.GetMessenger_FirstRequestPass();
               
            try
            {
                telimena.ReportUsage().GetAwaiter().GetResult();
                Assert.Fail("Error expected");
            }
            catch (Exception e)
            {
                TelimenaException ex = e as TelimenaException;
                StatisticsUpdateRequest jObj = ex.RequestObjects[0].Value as StatisticsUpdateRequest;
                Assert.AreEqual("Test_NoCustomData", jObj.FunctionName);
                Assert.AreEqual(null, jObj.CustomData);
            }

        }

        [Test]
        public void Test_NullCustomData()
        {

            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena
            {
                SuppressAllErrors = false
            };
            telimena.Messenger = this.GetMessenger_FirstRequestPass();

            try
            {
                telimena.ReportUsageWithCustomData(null).GetAwaiter().GetResult();
                Assert.Fail("Error expected");
            }
            catch (Exception e)
            {
                TelimenaException ex = e as TelimenaException;
                StatisticsUpdateRequest jObj = ex.RequestObjects[0].Value as StatisticsUpdateRequest;
                Assert.AreEqual("Test_NullCustomData", jObj.FunctionName);
                Assert.AreEqual(null, jObj.CustomData);
            }

        }

        [Test]
        public void Test_CustomDataString()
        {

            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena
            {
                SuppressAllErrors = false
            };
            telimena.Messenger = this.GetMessenger_FirstRequestPass();

            try
            {
                telimena.ReportUsageWithCustomData("AAAAAA").GetAwaiter().GetResult();
                Assert.Fail("Error expected");
            }
            catch (Exception e)
            {
                TelimenaException ex = e as TelimenaException;
                StatisticsUpdateRequest jObj = ex.RequestObjects[0].Value as StatisticsUpdateRequest;
                Assert.AreEqual("AAAAAA", jObj.CustomData);
                Assert.AreEqual("Test_CustomDataString", jObj.FunctionName);
            }
        }

        [Test]
        public void Test_CustomDataObject()
        {

            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena
            {
                SuppressAllErrors = false
            };
            telimena.Messenger = this.GetMessenger_FirstRequestPass();
            var obj = new CustomDataObject();
            obj.SomeValue = 333;
            try
            {
                telimena.ReportUsageWithCustomData(obj).GetAwaiter().GetResult();
                Assert.Fail("Error expected");
            }
            catch (Exception e)
            {
                TelimenaException ex = e as TelimenaException;
                StatisticsUpdateRequest jObj = ex.RequestObjects[0].Value as StatisticsUpdateRequest;
                Assert.AreEqual("Test_CustomDataObject", jObj.FunctionName);

                Assert.AreEqual("{\"SomeValue\":333,\"TrySerializingThisBadBoy\":null}", jObj.CustomData);
            }
        }


        [Test]
        public void Test_InvalidDataObject()
        {

            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena
            {
                SuppressAllErrors = false
            };
            telimena.Messenger = this.GetMessenger_FirstRequestPass();
            var obj = new NonSerializableObject(99);
            try
            {
                telimena.ReportUsageWithCustomData(obj).GetAwaiter().GetResult();
                Assert.Fail("Error expected");
            }
            catch (Exception e)
            {
                ArgumentException ex = e as ArgumentException;
                Assert.AreEqual("Invalid object passed as custom data for telemetry.", ex.Message);
            }
        }
    }
}