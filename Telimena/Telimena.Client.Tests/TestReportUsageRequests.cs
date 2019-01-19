// -----------------------------------------------------------------------
//  <copyright file="TestRegistrationRequests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using FluentAssertions;
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

        

        private DirectoryInfo GetTelemetryStorage()
        {
            Telimena telimena = (Telimena) Telimena.Construct(new TelimenaStartupInfo(this.testTelemetryKey) { SuppressAllErrors = false });
            return telimena.Locator.TelemetryStorageDirectory;

        }


        [Test]
        public void Test_CustomDataObject()
        {

            TelemetryItem item;
            VersionData version = null;
            var si = new TelimenaStartupInfo(this.testTelemetryKey) { SuppressAllErrors = false };
            for (int i = 0; i < 2; i++)
            {
                if (i == 0)
                {
                    ITelimena telimena = Telimena.Construct(si);
                    item = telimena.Telemetry.Async.Event("Something Happened", new Dictionary<string, object>(){{"AKey", "AValue"}}).GetAwaiter().GetResult();
                    version = telimena.Properties.ProgramVersion;
                }
                else
                {
                    item = Telimena.Telemetry.Async.Event(si, "Something Happened", new Dictionary<string, object>() { { "AKey", "AValue" } }).GetAwaiter().GetResult();
                }
                Assert.That(item.TelemetryData.Single().Key == "AKey");
                Assert.That((string)item.TelemetryData.Single().Value == "AValue");

                AssertItem(item, version, "Something Happened", TelemetryItemTypes.Event);
            }
        }

        private void AssertItem(TelemetryItem item, VersionData version, string key, TelemetryItemTypes type)
        {
            Assert.IsTrue(item.Id != Guid.Empty);
            Assert.AreEqual(key, item.EntryKey);
            Assert.That(item.Timestamp, Is.EqualTo(DateTimeOffset.UtcNow).Within(TimeSpan.FromSeconds(1)));
            Assert.That(item.TelemetryItemType, Is.EqualTo(type));
            item.VersionData.ShouldBeEquivalentTo(version);
            Assert.IsFalse(string.IsNullOrEmpty(item.VersionData.AssemblyVersion));
            var file = Path.Combine(this.GetTelemetryStorage().FullName, item.Id.ToString() + ".json");
            Assert.IsTrue(File.Exists(file));
            var deserialized = new TelimenaSerializer().Deserialize<TelemetryItem>(File.ReadAllText(file));
            item.ShouldBeEquivalentTo(deserialized);
        }

        [Test]
        public void Test_NoCustomData()
        {
            TelemetryItem item;
            VersionData version = null;
            var si = new TelimenaStartupInfo(this.testTelemetryKey) {SuppressAllErrors = false};
            for (int i = 0; i < 2; i++)
            {
                if (i == 0)
                {
                    ITelimena telimena = Telimena.Construct(si);
                    item = telimena.Telemetry.Async.View("SomeView").GetAwaiter().GetResult();
                    version = telimena.Properties.ProgramVersion;
                }
                else
                {
                    item = Telimena.Telemetry.Async.View(si,"SomeView").GetAwaiter().GetResult();
                }

                Assert.IsNull(item.TelemetryData);
                AssertItem(item, version, "SomeView", TelemetryItemTypes.View);

            }


        }


        //[Test]
        //public void Test_CustomDataObject()
        //{
        //    ITelimena telimena = Telimena.Construct(new TelimenaStartupInfo(this.testTelemetryKey) {SuppressAllErrors = false});
        //    ((Telimena) telimena).Messenger = this.GetMessenger_InitializeAndAcceptTelemetry(telimena.Properties.TelemetryKey);
        //    Dictionary<string, object> data = new Dictionary<string, object> {{"AKey", "AValue"}};
        //    Action act = () => telimena.Telemetry.Async.View("SomeView", data).GetAwaiter().GetResult();
        //    for (int i = 0; i < 2; i++)
        //    {
        //        try
        //        {
        //            act();
        //            Assert.Fail("Error expected");
        //        }
        //        catch (Exception e)
        //        {
        //            TelimenaException ex = e as TelimenaException;
        //            TelemetryUpdateRequest jObj = ex.RequestObjects[0].Value as TelemetryUpdateRequest;

        //            //Assert.AreEqual(data, jObj.TelemetryData);
        //        }

        //        act = () => telimena.Telemetry.Blocking.View("SomeView", new Dictionary<string, object> {{"AKey", "AValue"}});
        //    }
        //}

        //[Test]
        //public void Test_EmptyGuid()
        //{
        //    ITelimena telimena = Telimena.Construct(new TelimenaStartupInfo(Guid.Empty) {SuppressAllErrors = false});
        //    ((Telimena)telimena).Messenger = this.GetMessenger_InitializeAndAcceptTelemetry(telimena.Properties.TelemetryKey);
        //    Action act = () => telimena.Telemetry.Async.View("SomeView").GetAwaiter().GetResult();
        //    for (int i = 0; i < 2; i++)
        //    {
        //        try
        //        {
        //            act();
        //            Assert.Fail("Error expected");
        //        }
        //        catch (Exception e)
        //        {
        //            ArgumentException ex = e.InnerException.InnerException as ArgumentException;
        //            Assert.AreEqual("Telemetry key is an empty guid.\r\nParameter name: TelemetryKey", ex.Message);
        //        }

        //        act = () => telimena.Telemetry.Blocking.Event("SomeView");
        //    }
        //}

        //[Test]
        //public void Test_NoCustomData()
        //{
        //    ITelimena telimena = Telimena.Construct(new TelimenaStartupInfo(this.testTelemetryKey) {SuppressAllErrors = false});
        //    ((Telimena)telimena).Messenger = this.GetMessenger_InitializeAndAcceptTelemetry(telimena.Properties.TelemetryKey);


        //    Action act = () => telimena.Telemetry.Async.View("SomeView").GetAwaiter().GetResult();
        //    for (int i = 0; i < 2; i++)
        //    {
        //        try
        //        {
        //            act();
        //            Assert.Fail("Error expected");
        //        }
        //        catch (Exception e)
        //        {
        //            Assert.AreEqual("Updater.exe", telimena.Properties.LiveProgramInfo.UpdaterName);
        //            TelimenaException ex = e as TelimenaException;
        //            TelemetryUpdateRequest jObj = ex.RequestObjects[0].Value as TelemetryUpdateRequest;
        //            //Assert.AreEqual("SomeView", jObj.ComponentName);
        //            //Assert.AreEqual(null, jObj.TelemetryData);
        //        }

        //        act = () => telimena.Telemetry.Blocking.View("SomeView");
        //    }
        //}
    }
}