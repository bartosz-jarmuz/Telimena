// -----------------------------------------------------------------------
//  <copyright file="TestRegistrationRequests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using TelimenaClient.Serializer;

namespace TelimenaClient.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class TestReportUsageRequests
    {

        private readonly Guid testTelemetryKey = Guid.Parse("dc13cced-30ea-4628-a81d-21d86f37df95");
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
            client.Setup(x => x.GetAsync(It.IsRegex("^"+Regex.Escape(ApiRoutes.GetProgramUpdaterName)))).Returns((string uri) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();
                response.Content = new StringContent("Updater.exe");
                return Task.FromResult(response);
            });
            client.Setup(x => x.PostAsync(ApiRoutes.RegisterClient, It.IsAny<HttpContent>())).Returns((string uri, HttpContent requestContent) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();
                TelemetryInitializeResponse telemetryInitializeResponse = new TelemetryInitializeResponse { Count = 0, ProgramId = 1, UserId = 2 };
                response.Content = new StringContent(JsonConvert.SerializeObject(telemetryInitializeResponse));
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

            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena(this.testTelemetryKey)
            {
                SuppressAllErrors = false
            };
            telimena.Messenger = this.GetMessenger_FirstRequestPass();

                
            Action act = () => telimena.ReportUsageAsync().GetAwaiter().GetResult();
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    act();
                    Assert.Fail("Error expected");
                }
                catch (Exception e)
                {
                    Assert.AreEqual("Updater.exe", telimena.LiveProgramInfo.UpdaterName);
                    TelimenaException ex = e as TelimenaException;
                    TelemetryUpdateRequest jObj = ex.RequestObjects[0].Value as TelemetryUpdateRequest;
                    Assert.AreEqual("Test_NoCustomData", jObj.ComponentName);
                    Assert.AreEqual(null, jObj.TelemetryData);
                }

                act = () => telimena.ReportUsageBlocking();
            }

            

        }

        [Test]
        public void Test_NullCustomData()
        {

            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena(this.testTelemetryKey)
            {
                SuppressAllErrors = false
            };
            telimena.Messenger = this.GetMessenger_FirstRequestPass();

            Action act = () => telimena.ReportUsageWithCustomDataAsync(null).GetAwaiter().GetResult();
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    act();
                    Assert.Fail("Error expected");
                }
                catch (Exception e)
                {
                    TelimenaException ex = e as TelimenaException;
                    TelemetryUpdateRequest jObj = ex.RequestObjects[0].Value as TelemetryUpdateRequest;
                    Assert.AreEqual("Test_NullCustomData", jObj.ComponentName);
                    Assert.AreEqual(null, jObj.TelemetryData);
                }
                act = () => telimena.ReportUsageWithCustomDataBlocking(null);
            }
        }

        [Test]
        public void Test_CustomDataString()
        {

            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena(this.testTelemetryKey)
            {
                SuppressAllErrors = false
            };
            telimena.Messenger = this.GetMessenger_FirstRequestPass();
            Action act = () => telimena.ReportUsageWithCustomDataAsync("AAAAAA").GetAwaiter().GetResult();

            for (int i = 0; i < 2; i++)
            {
                try
                {
                    act();
                    Assert.Fail("Error expected");
                }
                catch (Exception e)
                {
                    TelimenaException ex = e as TelimenaException;
                    TelemetryUpdateRequest jObj = ex.RequestObjects[0].Value as TelemetryUpdateRequest;
                    Assert.AreEqual("AAAAAA", jObj.TelemetryData, e.ToString());
                    Assert.AreEqual("Test_CustomDataString", jObj.ComponentName);
                }
                act = () => telimena.ReportUsageWithCustomDataBlocking("AAAAAA");

            }
        }

        [Test]
        public void Test_CustomDataObject()
        {

            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena(this.testTelemetryKey)
            {
                SuppressAllErrors = false
            };
            telimena.Messenger = this.GetMessenger_FirstRequestPass();
            var obj = new CustomDataObject();
            obj.SomeValue = 333;
            Action act = () => telimena.ReportUsageWithCustomDataAsync(obj).GetAwaiter().GetResult();
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    act();
                    Assert.Fail("Error expected");
                }
                catch (Exception e)
                {
                    TelimenaException ex = e as TelimenaException;
                    TelemetryUpdateRequest jObj = ex.RequestObjects[0].Value as TelemetryUpdateRequest;
                    Assert.AreEqual("Test_CustomDataObject", jObj.ComponentName);

                    Assert.AreEqual("{\"SomeValue\":333,\"TrySerializingThisBadBoy\":null}", jObj.TelemetryData, e.ToString());
                }
                act = () => telimena.ReportUsageWithCustomDataBlocking(obj);

            }
        }


        [Test]
        public void Test_InvalidDataObject()
        {

            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena(this.testTelemetryKey)
            {
                SuppressAllErrors = false
            };
            telimena.Messenger = this.GetMessenger_FirstRequestPass();
            var obj = new NonSerializableObject(99);
            Action act = () => telimena.ReportUsageWithCustomDataAsync(null).GetAwaiter().GetResult();
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    act();
                    Assert.Fail("Error expected");
                }
                catch (Exception e)
                {
                    ArgumentException ex = e as ArgumentException;
                    Assert.AreEqual("Invalid object passed as custom data for telemetry.", ex.Message);
                }

                act = () => telimena.ReportUsageWithCustomDataBlocking(null);
            }
        }


        [Test]
        public void Test_EmptyGuid()
        {

            TelimenaClient.Telimena telimena = new TelimenaClient.Telimena(Guid.Empty)
            {
                SuppressAllErrors = false
            };
            telimena.Messenger = this.GetMessenger_FirstRequestPass();
            Action act = () => telimena.ReportUsageAsync().GetAwaiter().GetResult();
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    act();
                    Assert.Fail("Error expected");
                }
                catch (Exception e)
                {
                    ArgumentException ex = e.InnerException.InnerException as ArgumentException;
                    Assert.AreEqual("Telemetry key is an empty guid.\r\nParameter name: TelemetryKey", ex.Message);
                }

                act = () => telimena.ReportUsageBlocking();
            }
        }
    }
}