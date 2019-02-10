using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DbIntegrationTestHelpers;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Api.V1;
using Telimena.WebApp.Controllers.Api.V1.Helpers;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Models;
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
    public class TelemetryControllerHelpersTests : IntegrationTestsContextSharedGlobally<TelimenaContext>
    {
        protected override Action SeedAction => () => TelimenaDbInitializer.SeedUsers(this.Context);

        private readonly TelimenaSerializer serializer = new TelimenaSerializer();

        private ClientAppUser GetUserByGuid(Guid id)
        {
            return this.Context.AppUsers.FirstOrDefault(x => x.Guid == id);
        }


        [Test]
        public async Task TestUpdateAction()
        {
            //prepare context
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            await Helpers.SeedInitialPrograms(this.Context, 4, "TestApp", new[] { "Johnny Walker", "Jim Beam", "Eric Cartman" });
            Helpers.AddHelperAssemblies(this.Context, 2, "TestApp");
            Helpers.GetProgramAndUser(this.Context, "TestApp3", "Jim Beam", out Program prg, out ClientAppUser usr);
            Assert.IsTrue(prg.Id > 0 && usr.Id > 0);

            //prepare requests
            TelemetryItem telemetryItem = new TelemetryItem()

            {
                EntryKey = Helpers.GetName("SomeView"),
                TelemetryItemType = TelemetryItemTypes.View,
                Sequence = "aaa:1",
                VersionData = new VersionData("1.2.3.4", "2.0.0.0"),
                Timestamp = new DateTimeOffset(DateTime.UtcNow),
                TelemetryData = new Dictionary<string, string> { { "AKey", "AValue" }, { "AKey2", "AValue2" } },
                UserId = usr.UserId
            };

            //send
            List<TelemetrySummary> response = await TelemetryControllerHelpers.InsertData(unit, (new[]{telemetryItem}).ToList(), prg, "127.1.1.1");

            Helpers.AssertUpdateResponse(response, prg, usr, 1, Helpers.GetName("SomeView"));
            View view = prg.Views.FirstOrDefault(x => x.Name == Helpers.GetName("SomeView"));
            Assert.AreEqual(1, view.TelemetrySummaries.Count);
            ViewTelemetrySummary summary = view.TelemetrySummaries.SingleOrDefault(x => x.ClientAppUser.UserId == Helpers.GetName("Jim Beam"));
            Assert.AreEqual(1, summary.SummaryCount);
            Assert.AreEqual(summary.GetTelemetryDetails().Last().AssemblyVersionId, prg.PrimaryAssembly.GetVersion(telemetryItem.VersionData).Id);

            //run again
            telemetryItem = new TelemetryItem()

            {
                EntryKey = Helpers.GetName("SomeView"),
                TelemetryItemType = TelemetryItemTypes.View,
                Sequence = "aaa:1",
                VersionData = new VersionData("1.2.3.4", "2.0.0.0"),
                Timestamp = new DateTimeOffset(DateTime.UtcNow),
                TelemetryData = new Dictionary<string, string> { { "AKey", "AValue" }, { "AKey2", "AValue2" } },
                UserId = usr.UserId
            };
            response = await TelemetryControllerHelpers.InsertData(unit, (new[] { telemetryItem }).ToList(), prg, "127.1.1.1");

            Helpers.GetProgramAndUser(this.Context, "TestApp3", "Jim Beam", out prg, out usr);
            Helpers.AssertUpdateResponse(response, prg, usr, 1, Helpers.GetName("SomeView"));
            Assert.AreEqual(2, response.Single().SummaryCount);
            view = prg.Views.FirstOrDefault(x => x.Name == Helpers.GetName("SomeView"));

            Assert.AreEqual(1, view.TelemetrySummaries.Count);

            Assert.AreEqual(2, view.GetTelemetrySummary(response.First().ClientAppUser.Id).SummaryCount);
            Assert.AreEqual(2, view.TelemetrySummaries.Single(x => x.ClientAppUser.UserId == Helpers.GetName("Jim Beam")).SummaryCount);

            Assert.AreEqual(2, view.GetTelemetryDetails(response.First().ClientAppUser.Id).Count);
            Assert.AreEqual(summary.GetTelemetryDetails().Last().AssemblyVersionId, prg.PrimaryAssembly.GetVersion(telemetryItem.VersionData).Id);
        }

        [Test]
        public async Task TestEvents_VariousUsers()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            List<KeyValuePair<string, Guid>> apps = await Helpers.SeedInitialPrograms(this.Context, 2, "TestApp", new[] { "Billy Jean", "Jack Black" });

            Helpers.GetProgramAndUser(this.Context, "TestApp", "Billy Jean", out Program prg, out ClientAppUser usr);

            TelemetryItem telemetryItem = new TelemetryItem()

            {
                EntryKey = Helpers.GetName("Func1"),
                TelemetryItemType = TelemetryItemTypes.Event,
                Sequence = "aaa:1",
                VersionData = new VersionData("1.2.3.4", "2.0.0.0"),
                Timestamp = new DateTimeOffset(DateTime.UtcNow),
                TelemetryData = new Dictionary<string, string> { { "AKey", "AValue" }, { "AKey2", "AValue2" } },
                UserId = usr.UserId
            };

            List<TelemetrySummary> result = await TelemetryControllerHelpers.InsertData(unit, (new[] { telemetryItem }).ToList(), prg, "127.1.1.1");

            Event @event = prg.Events.Single();

            int eventId = this.Context.Events.FirstOrDefault(x => x.Name == @event.Name).Id;

            Helpers.AssertUpdateResponse(result, prg, usr, 1, Helpers.GetName("Func1"), eventId);

            Assert.AreEqual(1, prg.Events.Count);
            Assert.AreEqual(Helpers.GetName("Func1"), @event.Name);
            Assert.AreEqual(1, @event.TelemetrySummaries.Count);
            Assert.AreEqual(prg.Id, @event.ProgramId);

            TelemetrySummary summary = @event.GetTelemetrySummary(this.GetUserByGuid(result.First().ClientAppUser.Guid).Id);
            Assert.AreEqual(usr.Id, summary.ClientAppUserId);

            TelemetryDetail detail = @event.GetTelemetryDetails(this.GetUserByGuid(result.First().ClientAppUser.Guid).Id).Single();
            Assert.AreEqual(detail.GetTelemetrySummary().Id, summary.Id);
            Assert.AreEqual(usr.Id, detail.GetTelemetrySummary().ClientAppUserId);
            Assert.AreEqual(usr.UserId, detail.UserId);
            Assert.AreEqual(2, detail.GetTelemetryUnits().Count());
            Assert.AreEqual("AKey", detail.GetTelemetryUnits().ElementAt(0).Key);
            Assert.AreEqual("AValue", detail.GetTelemetryUnits().ElementAt(0).ValueString);
            Assert.AreEqual("AKey2", detail.GetTelemetryUnits().ElementAt(1).Key);
            Assert.AreEqual("AValue2", detail.GetTelemetryUnits().ElementAt(1).ValueString);


            ClientAppUser otherUser = Helpers.GetUser(this.Context, "Jack Black");

             telemetryItem = new TelemetryItem()

            {
                EntryKey = Helpers.GetName("Func1"),
                TelemetryItemType = TelemetryItemTypes.Event,
                Sequence = "aaa:1",
                VersionData = new VersionData("1.2.3.4", "2.0.0.0"),
                Timestamp = new DateTimeOffset(DateTime.UtcNow),
                TelemetryData = new Dictionary<string, string> { { "AKey3", "AValue3" }, { "AKey4", "AValue4" }, { "AKey5", "AValue5" } },
                UserId = otherUser.UserId
            };

            result = await TelemetryControllerHelpers.InsertData(unit, (new[] { telemetryItem }).ToList(), prg, "127.1.1.1");

            Helpers.AssertUpdateResponse(result, prg, otherUser, 1, Helpers.GetName("Func1"), eventId);

            prg = await unit.Programs.FirstOrDefaultAsync(x => x.Id == prg.Id);
            Assert.AreEqual(1, prg.Events.Count);
            @event = prg.Events.Single();
            Assert.AreEqual(Helpers.GetName("Func1"), @event.Name);
            Assert.AreEqual(2, @event.TelemetrySummaries.Count);
            Assert.AreEqual(1, @event.GetTelemetrySummary(this.GetUserByGuid(result.First().ClientAppUser.Guid).Id).SummaryCount);
            Assert.AreEqual(1, summary.GetTelemetryDetails().Count());

            Assert.AreNotEqual(otherUser.Guid, usr.Guid);

            TelemetrySummary otherSummary = @event.GetTelemetrySummary(this.GetUserByGuid(result.First().ClientAppUser.Guid).Id);
            Assert.AreEqual(otherUser.Id, otherSummary.ClientAppUserId);

            TelemetryDetail otherUserDetail = @event.GetTelemetryDetails(this.GetUserByGuid(result.First().ClientAppUser.Guid).Id).Single();
            Assert.AreEqual(otherUser.Id, otherUserDetail.GetTelemetrySummary().ClientAppUserId);
            Assert.AreEqual(otherUser.UserId, otherUserDetail.UserId);
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
                UserId = usr.UserId
            };

            //run again with first user
            result = await TelemetryControllerHelpers.InsertData(unit, (new[] { telemetryItem }).ToList(), prg, "127.1.1.1");

            @event = prg.Events.Single();
            Assert.AreEqual(2, @event.TelemetrySummaries.Count);
            Assert.AreEqual(2, @event.GetTelemetrySummary(this.GetUserByGuid(result.First().ClientAppUser.Guid).Id).SummaryCount);
            Assert.AreEqual(2, summary.GetTelemetryDetails().Count());


            List<EventTelemetryDetail> details = @event.GetTelemetryDetails(this.GetUserByGuid(result.First().ClientAppUser.Guid).Id).OrderBy(x => x.Timestamp)
                .Cast<EventTelemetryDetail>().ToList();
            Assert.AreEqual(2, details.Count);
            Assert.IsTrue(details.All(x => x.TelemetrySummary.ClientAppUserId == this.GetUserByGuid(result.First().ClientAppUser.Guid).Id));
            Assert.IsTrue(details.First().Timestamp < details.Last().Timestamp);

            Assert.AreEqual(3, this.Context.EventTelemetryDetails.Count(x => x.TelemetrySummary.Event.Name == telemetryItem.EntryKey));
            Assert.AreEqual(2, this.Context.EventTelemetryDetails.Count(x => x.TelemetrySummaryId == summary.Id));
        }
    }
}