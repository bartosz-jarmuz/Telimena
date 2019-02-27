using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using DbIntegrationTestHelpers;
using Moq;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Api.V1;
using Telimena.WebApp.Controllers.Api.V1.Helpers;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Telemetry;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;
using TelimenaClient;
using TelimenaClient.Serializer;

namespace Telimena.Tests
{
    [TestFixture]
    [SuppressMessage("ReSharper", "ConsiderUsingConfigureAwait")]
    public class TelemetryControllerHelpersTests : GlobalContextTestBase
    {
        [Test]
        public async Task TestRegisterApp()
        {
            string methodName = MethodInfo.GetCurrentMethod().Name;
            Mock<HttpRequestContext> requestContext = await Helpers.SetupUserIntoRequestContext(this.PortalContext, "testDevTeam", "testDev@Email", nameof(TestRegisterApp)).ConfigureAwait(false);

            RegisterProgramUnitOfWork unit = new RegisterProgramUnitOfWork(this.TelemetryContext, this.PortalContext);

            RegisterProgramController programsController = new RegisterProgramController(unit) { RequestContext = requestContext.Object };

            RegisterProgramRequest register = new RegisterProgramRequest
            {
                Name = Helpers.GetName("MyTestApp"),
                TelemetryKey = Guid.NewGuid(),
                PrimaryAssemblyFileName = "MyTestApp.dll"
            };
            RegisterProgramResponse response = await programsController.Register(register).ConfigureAwait(false);
            if (response.Exception != null)
            {
                throw response.Exception;
            }

            var prg = this.PortalContext.Programs.Single(x => x.TelemetryKey == response.TelemetryKey);
            var root = this.TelemetryContext.TelemetryRootObjects.Single(x => x.TelemetryKey == prg.TelemetryKey);

            Assert.AreEqual(prg.Id, root.ProgramId);
            Assert.AreEqual(prg.TelemetryKey, root.TelemetryKey);

            Assert.AreEqual(Helpers.GetName("MyTestApp"), prg.Name);
            Assert.AreEqual(Helpers.GetName("testDevTeam"), prg.DeveloperTeam.Name);
            Assert.AreEqual(Helpers.GetName("testDev@Email"), prg.DeveloperTeam.MainEmail);
            Assert.AreEqual("MyTestApp.dll", prg.PrimaryAssembly.Name + prg.PrimaryAssembly.Extension);
        }

        private ClientAppUser GetUserByGuid(int id)
        {
            return this.TelemetryContext.AppUsers.FirstOrDefault(x => x.Id == id);
        }
      

        [Test]
        public async Task TestUpdateAction()
        {
            //prepare context
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.TelemetryContext, this.PortalContext, new AssemblyStreamVersionReader());
            await Helpers.SeedInitialPrograms(this.PortalContext, this.TelemetryContext, 4, "TestApp", new[] { "Johnny Walker", "Jim Beam", "Eric Cartman" });
            Helpers.GetProgramAndUser(unit, "TestApp3", "Jim Beam", out TelemetryRootObject prg, out ClientAppUser usr);
            Assert.IsTrue(prg.ProgramId >0 && usr.Id >0);

            //prepare requests
            TelemetryItem telemetryItem = new TelemetryItem()

            {
                EntryKey = Helpers.GetName("SomeView"),
                TelemetryItemType = TelemetryItemTypes.View,
                Sequence = "aaa:1",
                VersionData = new VersionData("1.2.3.4", "2.0.0.0"),
                Timestamp = new DateTimeOffset(DateTime.UtcNow),
                TelemetryData = new Dictionary<string, string> { { "AKey", "AValue" }, { "AKey2", "AValue2" } },
                UserIdentifier = usr.UserIdentifier
            };

            //send
            List<TelemetrySummary> response = await TelemetryControllerHelpers.InsertData(unit, (new[]{telemetryItem}).ToList(), prg, "127.1.1.1");

            Helpers.AssertUpdateResponse(response, prg, usr, 1, Helpers.GetName("SomeView"));
            View view = prg.Views.FirstOrDefault(x => x.Name == Helpers.GetName("SomeView"));
            Assert.AreEqual(1, view.TelemetrySummaries.Count);
            ViewTelemetrySummary summary = view.TelemetrySummaries.SingleOrDefault(x => x.ClientAppUser.UserIdentifier == Helpers.GetName("Jim Beam"));
            Assert.AreEqual(1, summary.SummaryCount);
            Assert.AreEqual(summary.GetTelemetryDetails().Last().AssemblyVersion, "1.2.3.4");
            Assert.AreEqual(summary.GetTelemetryDetails().Last().FileVersion, "2.0.0.0");

            //run again
            telemetryItem = new TelemetryItem()

            {
                EntryKey = Helpers.GetName("SomeView"),
                TelemetryItemType = TelemetryItemTypes.View,
                Sequence = "aaa:1",
                VersionData = new VersionData("1.2.3.4", "2.0.0.0"),
                Timestamp = new DateTimeOffset(DateTime.UtcNow),
                TelemetryData = new Dictionary<string, string> { { "AKey", "AValue" }, { "AKey2", "AValue2" } },
                UserIdentifier = usr.UserIdentifier
            };
            response = await TelemetryControllerHelpers.InsertData(unit, (new[] { telemetryItem }).ToList(), prg, "127.1.1.1");

            Helpers.GetProgramAndUser(unit, "TestApp3", "Jim Beam", out prg, out usr);
            Helpers.AssertUpdateResponse(response, prg, usr, 1, Helpers.GetName("SomeView"));
            Assert.AreEqual(2, response.Single().SummaryCount);
            view = prg.Views.FirstOrDefault(x => x.Name == Helpers.GetName("SomeView"));

            Assert.AreEqual(1, view.TelemetrySummaries.Count);

            Assert.AreEqual(2, view.GetTelemetrySummary(response.First().ClientAppUser.Id).SummaryCount);
            Assert.AreEqual(2, view.TelemetrySummaries.Single(x => x.ClientAppUser.UserIdentifier == Helpers.GetName("Jim Beam")).SummaryCount);

            Assert.AreEqual(2, view.GetTelemetryDetails(response.First().ClientAppUser.Id).Count);
            Assert.AreEqual(summary.GetTelemetryDetails().Last().AssemblyVersion, telemetryItem.VersionData.AssemblyVersion);
            Assert.AreEqual(summary.GetTelemetryDetails().Last().FileVersion, telemetryItem.VersionData.FileVersion);
        }

        [Test]
        public async Task TestEvents_VariousUsers()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.TelemetryContext,this.PortalContext, new AssemblyStreamVersionReader());
            List<KeyValuePair<string, Guid>> apps = await Helpers.SeedInitialPrograms(this.PortalContext, this.TelemetryContext, 2, "TestApp", new[] { "Billy Jean", "Jack Black" });

            Helpers.GetProgramAndUser(unit, "TestApp", "Billy Jean", out TelemetryRootObject prg, out ClientAppUser usr);

            TelemetryItem telemetryItem = new TelemetryItem()

            {
                EntryKey = Helpers.GetName("Func1"),
                TelemetryItemType = TelemetryItemTypes.Event,
                Sequence = "aaa:1",
                VersionData = new VersionData("1.2.3.4", "2.0.0.0"),
                Timestamp = new DateTimeOffset(DateTime.UtcNow),
                TelemetryData = new Dictionary<string, string> { { "AKey", "AValue" }, { "AKey2", "AValue2" } },
                UserIdentifier = usr.UserIdentifier
            };

            List<TelemetrySummary> result = await TelemetryControllerHelpers.InsertData(unit, (new[] { telemetryItem }).ToList(), prg, "127.1.1.1");

            Event @event = prg.Events.Single();

            Guid eventId = this.TelemetryContext.Events.FirstOrDefault(x => x.Name == @event.Name).Id;

            Helpers.AssertUpdateResponse(result, prg, usr, 1, Helpers.GetName("Func1"), eventId);

            Assert.AreEqual(1, prg.Events.Count);
            Assert.AreEqual(Helpers.GetName("Func1"), @event.Name);
            Assert.AreEqual(1, @event.TelemetrySummaries.Count);
            Assert.AreEqual(prg.ProgramId, @event.ProgramId);

            TelemetrySummary summary = @event.GetTelemetrySummary(this.GetUserByGuid(result.First().ClientAppUser.Id).Id);
            Assert.AreEqual(usr.Id, summary.ClientAppUserId);

            TelemetryDetail detail = @event.GetTelemetryDetails(this.GetUserByGuid(result.First().ClientAppUser.Id).Id).Single();
            Assert.AreEqual(detail.GetTelemetrySummary().Id, summary.Id);
            Assert.AreEqual(usr.Id, detail.GetTelemetrySummary().ClientAppUserId);
            Assert.AreEqual(usr.UserIdentifier, detail.UserIdentifier);
            Assert.AreEqual(2, detail.GetTelemetryUnits().Count());
            Assert.AreEqual("AKey", detail.GetTelemetryUnits().ElementAt(0).Key);
            Assert.AreEqual("AValue", detail.GetTelemetryUnits().ElementAt(0).ValueString);
            Assert.AreEqual("AKey2", detail.GetTelemetryUnits().ElementAt(1).Key);
            Assert.AreEqual("AValue2", detail.GetTelemetryUnits().ElementAt(1).ValueString);


            ClientAppUser otherUser = Helpers.GetUser(unit, "Jack Black");

             telemetryItem = new TelemetryItem()

            {
                EntryKey = Helpers.GetName("Func1"),
                TelemetryItemType = TelemetryItemTypes.Event,
                Sequence = "aaa:1",
                VersionData = new VersionData("1.2.3.4", "2.0.0.0"),
                Timestamp = new DateTimeOffset(DateTime.UtcNow),
                TelemetryData = new Dictionary<string, string> { { "AKey3", "AValue3" }, { "AKey4", "AValue4" }, { "AKey5", "AValue5" } },
                UserIdentifier = otherUser.UserIdentifier
            };

            result = await TelemetryControllerHelpers.InsertData(unit, (new[] { telemetryItem }).ToList(), prg, "127.1.1.1");

            Helpers.AssertUpdateResponse(result, prg, otherUser, 1, Helpers.GetName("Func1"), eventId);

            prg = await unit.GetMonitoredProgram(prg.TelemetryKey);
            Assert.AreEqual(1, prg.Events.Count);
            @event = prg.Events.Single();
            Assert.AreEqual(Helpers.GetName("Func1"), @event.Name);
            Assert.AreEqual(2, @event.TelemetrySummaries.Count);
            Assert.AreEqual(1, @event.GetTelemetrySummary(this.GetUserByGuid(result.First().ClientAppUser.Id).Id).SummaryCount);
            Assert.AreEqual(1, summary.GetTelemetryDetails().Count());

            Assert.AreNotEqual(otherUser.Id, usr.Id);

            TelemetrySummary otherSummary = @event.GetTelemetrySummary(this.GetUserByGuid(result.First().ClientAppUser.Id).Id);
            Assert.AreEqual(otherUser.Id, otherSummary.ClientAppUserId);

            TelemetryDetail otherUserDetail = @event.GetTelemetryDetails(this.GetUserByGuid(result.First().ClientAppUser.Id).Id).Single();
            Assert.AreEqual(otherUser.Id, otherUserDetail.GetTelemetrySummary().ClientAppUserId);
            Assert.AreEqual(otherUser.UserIdentifier, otherUserDetail.UserIdentifier);
            Assert.AreEqual(3, otherUserDetail.GetTelemetryUnits().Count());
            Assert.AreEqual("AKey3", otherUserDetail.GetTelemetryUnits().ElementAt(0).Key);
            Assert.AreEqual("AValue3", otherUserDetail.GetTelemetryUnits().ElementAt(0).ValueString);
            Assert.AreEqual("AKey5", otherUserDetail.GetTelemetryUnits().ElementAt(2).Key);
            Assert.AreEqual("AValue5", otherUserDetail.GetTelemetryUnits().ElementAt(2).ValueString);

            telemetryItem = new TelemetryItem()

            {
                EntryKey = Helpers.GetName("Func1"),
                TelemetryItemType = TelemetryItemTypes.Event,
                Sequence = "aaa:1",
                VersionData = new VersionData("1.2.3.4", "2.0.0.0"),
                Timestamp = new DateTimeOffset(DateTime.UtcNow),
                TelemetryData = new Dictionary<string, string> { { "AKey", "AValue" }, { "AKey2", "AValue2" } },
                UserIdentifier = usr.UserIdentifier
            };

            //run again with first user
            result = await TelemetryControllerHelpers.InsertData(unit, (new[] { telemetryItem }).ToList(), prg, "127.1.1.1");

            @event = prg.Events.Single();
            Assert.AreEqual(2, @event.TelemetrySummaries.Count);
            Assert.AreEqual(2, @event.GetTelemetrySummary(this.GetUserByGuid(result.First().ClientAppUser.Id).Id).SummaryCount);
            Assert.AreEqual(2, summary.GetTelemetryDetails().Count());


            List<EventTelemetryDetail> details = @event.GetTelemetryDetails(this.GetUserByGuid(result.First().ClientAppUser.Id).Id).OrderBy(x => x.Timestamp)
                .Cast<EventTelemetryDetail>().ToList();
            Assert.AreEqual(2, details.Count);
            Assert.IsTrue(details.All(x => x.TelemetrySummary.ClientAppUserId == this.GetUserByGuid(result.First().ClientAppUser.Id).Id));
            Assert.IsTrue(details.First().Timestamp < details.Last().Timestamp);

            Assert.AreEqual(3, this.TelemetryContext.EventTelemetryDetails.Count(x => x.TelemetrySummary.Event.Name == telemetryItem.EntryKey));
            Assert.AreEqual(2, this.TelemetryContext.EventTelemetryDetails.Count(x => x.TelemetrySummaryId == summary.Id));
        }
    }
}