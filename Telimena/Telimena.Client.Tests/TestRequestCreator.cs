using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using FluentAssertions;
using Newtonsoft.Json;
using NUnit.Framework;
using Telimena.WebApp.Core.Models;
using TelimenaClient.Serializer;

namespace TelimenaClient.Tests
{
    [TestFixture]
    [SuppressMessage("ReSharper", "ConsiderUsingConfigureAwait")]
    public class TestRequestCreator
    {
        Guid telemetryKey = Guid.Parse("cb451750-d3a1-4917-8f5f-1f978e86d064");
        Guid userId = Guid.Parse("abcc5364-af27-4b6f-ab8b-d4f000727798");

        [Test]
        public async Task ProcessItems_CreateRequest_EndToEndTest()
        {
            ProgramInfo program = new ProgramInfo()
            {
                Name = "TestRequestCreator",
                PrimaryAssembly = new AssemblyInfo(this.GetType().Assembly)
                
            };

            ITelimena telimena = Telimena.Construct(new TelimenaStartupInfo(this.telemetryKey) { SuppressAllErrors = false });
            ((Telimena)telimena).Messenger = Helpers.GetMessenger_InitializeAndAcceptTelemetry(telimena.Properties.TelemetryKey);

            TestLocator locator = GetLocatorWithCleanFolder(program);

            TelemetryItem item1 = new TelemetryItem("MainView", TelemetryItemTypes.View, new VersionData("1.0", null),null);
            TelemetryItem item2 = new TelemetryItem("MainView", TelemetryItemTypes.View, new VersionData("1.0", null), new Dictionary<string, object>()
            {
                {"TS", new DateTime(2001,01,02,12,14,00, DateTimeKind.Local) },
                {"TSOffset", new DateTimeOffset(2001,01,02,12,14,00, TimeSpan.Zero) }
            });

            TelemetryItem item3 = new TelemetryItem("UserLogin", TelemetryItemTypes.Event, new VersionData("2.0", "3.2.2"), new Dictionary<string, object>()
            {
                {"Dec", 23.2M }
            });


            await TestItemsProcessing(locator, new []{item1, item2, item3});

            TelemetryRequestCreator requestCreator = new TelemetryRequestCreator(locator.TelemetryStorageDirectory);

            var  requestTuple = await requestCreator.Create(this.telemetryKey, this.userId);

            VerifyRequest(requestTuple, item1, item2, item3);

            var sender = new TelemetryRequestSender((Telimena) telimena);
            var response = await sender.SendRequests(requestTuple.Item1, requestTuple.Item2);

            Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);
            foreach (var fileInfo in requestTuple.Item2)
            {
                Assert.IsFalse(File.Exists(fileInfo.FullName));
            }
        }

        private static void VerifyRequest(Tuple<TelemetryUpdateRequest, List<FileInfo>> requestTuple, TelemetryItem item1, TelemetryItem item2, TelemetryItem item3)
        {
            TelemetryUpdateRequest request = requestTuple.Item1;
            var files = requestTuple.Item2;

            Assert.AreEqual(files.Count, request.SerializedTelemetryUnits.Count);

            TelimenaSerializer serializer = new TelimenaSerializer();

            string serialized = serializer.Serialize(request);
            //deserialilzation done in the API with Json.Net
            TelemetryUpdateRequest deserialized = JsonConvert.DeserializeObject<TelemetryUpdateRequest>(serialized);
            Assert.AreEqual(3, deserialized.SerializedTelemetryUnits.Count);

            List<TelemetryItem> list = new List<TelemetryItem>();
            list.Add(serializer.Deserialize<TelemetryItem>(deserialized.SerializedTelemetryUnits[0]));
            list.Add(serializer.Deserialize<TelemetryItem>(deserialized.SerializedTelemetryUnits[1]));
            list.Add(serializer.Deserialize<TelemetryItem>(deserialized.SerializedTelemetryUnits[2]));
            TelemetryItem deserializedItem1 = list.Single(x => x.Id == item1.Id);

            deserializedItem1.ShouldBeEquivalentTo(item1);
            Assert.IsInstanceOf<Guid>(deserializedItem1.Id);
            Assert.IsInstanceOf<DateTimeOffset>(deserializedItem1.Timestamp);

            list.Single(x => x.Id == item2.Id).ShouldBeEquivalentTo(item2);
            list.Single(x => x.Id == item3.Id).ShouldBeEquivalentTo(item3);

            foreach (var fileInfo in files)
            {
                Assert.IsTrue(list.Single(x => $"{x.Id}.json" == fileInfo.Name) != null);
                Assert.IsTrue(File.Exists(fileInfo.FullName));

            }
        }

        private static async Task TestItemsProcessing(TestLocator locator, TelemetryItem[] items)
        {
            Assert.AreEqual(0, locator.TelemetryStorageDirectory.GetFiles().Length);
            TelemetryProcessingPipeline pipeline = TelemetryModule.BuildProcessingPipeline(locator);
            for (int index = 0; index < items.Length; index++)
            {
                TelemetryItem telemetryItem = items[index];
                await pipeline.Process(telemetryItem);
                if (index == 0)
                {
                    FileInfo file = locator.TelemetryStorageDirectory.GetFiles().Single();
                    Assert.AreEqual(telemetryItem.Id + ".json", file.Name);
                    var serializer = new TelimenaSerializer();
                    var deserialized = serializer.Deserialize<TelemetryItem>(File.ReadAllText(file.FullName));
                    telemetryItem.ShouldBeEquivalentTo(deserialized);
                }
                if (index == 1)
                {
                    Assert.IsNotNull(locator.TelemetryStorageDirectory.GetFiles().Single(x=>x.Name == telemetryItem.Id + ".json"));
                }
            }
            Assert.AreEqual(items.Length, locator.TelemetryStorageDirectory.GetFiles().Length);

        }

        private static TestLocator GetLocatorWithCleanFolder(ProgramInfo program)
        {
            TestLocator locator = new TestLocator(program);
            foreach (FileInfo enumerateFile in locator.TelemetryStorageDirectory.EnumerateFiles())
            {
                enumerateFile.Delete();
            }

            return locator;
        }

    }
}
