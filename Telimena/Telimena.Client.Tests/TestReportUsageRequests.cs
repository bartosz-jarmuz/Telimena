// -----------------------------------------------------------------------
//  <copyright file="TestRegistrationRequests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
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

        private IMessenger GetMessenger_FirstRequestPass(Guid key)
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
                TelemetryInitializeResponse telemetryInitializeResponse = new TelemetryInitializeResponse {Count = 0, UserId = Guid.NewGuid()};
                response.Content = new StringContent(JsonConvert.SerializeObject(telemetryInitializeResponse));
                return Task.FromResult(response);
            });
            client.Setup(x => x.PostAsync(It.IsIn(ApiRoutes.ReportEvent, ApiRoutes.ReportView), It.IsAny<HttpContent>())).Callback(
                (string uri, HttpContent requestContent) =>
                {
                    throw new AggregateException(new AssertionException(uri)
                        , new AssertionException(requestContent.ReadAsStringAsync().GetAwaiter().GetResult()));
                });
            return new Messenger(new TelimenaSerializer(), client.Object);
            ;
        }

        [Test]
        public void Test_CustomDataObject()
        {
            ITelimena telimena = Telimena.Construct(new TelimenaStartupInfo(this.testTelemetryKey) {SuppressAllErrors = false});
            ((Telimena) telimena).Messenger = this.GetMessenger_FirstRequestPass(telimena.Properties.TelemetryKey);
            Dictionary<string, object> data = new Dictionary<string, object> {{"AKey", "AValue"}};
            Action act = () => telimena.Telemetry.Async.View("SomeView", data).GetAwaiter().GetResult();
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
                    Assert.AreEqual("SomeView", jObj.ComponentName);

                    Assert.AreEqual(data, jObj.TelemetryData);
                }

                act = () => telimena.Telemetry.Blocking.View("SomeView", new Dictionary<string, object> {{"AKey", "AValue"}});
            }
        }

        [Test]
        public void Test_EmptyGuid()
        {
            ITelimena telimena = Telimena.Construct(new TelimenaStartupInfo(Guid.Empty) {SuppressAllErrors = false});
            ((Telimena)telimena).Messenger = this.GetMessenger_FirstRequestPass(telimena.Properties.TelemetryKey);
            Action act = () => telimena.Telemetry.Async.View("SomeView").GetAwaiter().GetResult();
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

                act = () => telimena.Telemetry.Blocking.Event("SomeView");
            }
        }

        [Test]
        public void Test_NoCustomData()
        {
            ITelimena telimena = Telimena.Construct(new TelimenaStartupInfo(this.testTelemetryKey) {SuppressAllErrors = false});
            ((Telimena)telimena).Messenger = this.GetMessenger_FirstRequestPass(telimena.Properties.TelemetryKey);


            Action act = () => telimena.Telemetry.Async.View("SomeView").GetAwaiter().GetResult();
            for (int i = 0; i < 2; i++)
            {
                try
                {
                    act();
                    Assert.Fail("Error expected");
                }
                catch (Exception e)
                {
                    Assert.AreEqual("Updater.exe", telimena.Properties.LiveProgramInfo.UpdaterName);
                    TelimenaException ex = e as TelimenaException;
                    TelemetryUpdateRequest jObj = ex.RequestObjects[0].Value as TelemetryUpdateRequest;
                    Assert.AreEqual("SomeView", jObj.ComponentName);
                    Assert.AreEqual(null, jObj.TelemetryData);
                }

                act = () => telimena.Telemetry.Blocking.View("SomeView");
            }
        }
    }
}