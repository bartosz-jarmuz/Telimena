using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using DbIntegrationTestHelpers;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Api.V1.Helpers;
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
        public async Task TestMissingUser()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());

            unit.Programs.Add(new Program("SomeApp", Guid.NewGuid()) {PrimaryAssembly = new ProgramAssembly {Name = "SomeApp.dll", Company = "SomeCompm"}});

            await unit.CompleteAsync();
            Program prg = (await unit.Programs.GetAsync(x => x.Name == "SomeApp")).FirstOrDefault();
            Assert.IsTrue(prg.Id > 0);

            TelemetryUpdateRequest request = new TelemetryUpdateRequest(prg.TelemetryKey, Guid.NewGuid(), null);

            try
            {
                await TelemetryControllerHelpers.InsertData(unit, request, "127.1.1.1");
            }
            catch (Exception ex)
            {
                Assert.AreEqual($"User [{request.UserId}] is null", ex.Message);
            }
        }

        [Test]
        public async Task TestUpdateAction()
        {
            //prepare context
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            await Helpers.SeedInitialPrograms(this.Context, 4, "TestApp", new[] {"Johnny Walker", "Jim Beam", "Eric Cartman"});
            Helpers.AddHelperAssemblies(this.Context, 2, "TestApp");
            Helpers.GetProgramAndUser(this.Context, "TestApp3", "Jim Beam", out Program prg, out ClientAppUser usr);
            Assert.IsTrue(prg.Id > 0 && usr.Id > 0);

            //prepare requests
            TelemetryItem telemetryItem = new TelemetryItem(Helpers.GetName("SomeView"), TelemetryItemTypes.View, new VersionData("1.2.3.4", "2.0.0.0")
                , new Dictionary<string, object> {{"AKey", "AValue"}, {"AKey2", "AValue2"}});

            TelemetryUpdateRequest request = new TelemetryUpdateRequest(prg.TelemetryKey, usr.Guid, new List<string>(){this.serializer.Serialize(telemetryItem)});

            //send
            List<TelemetrySummary> response = await TelemetryControllerHelpers.InsertData(unit, request, "127.1.1.1");

            Helpers.AssertUpdateResponse(response, prg, usr, 1, Helpers.GetName("SomeView"));
            View view = prg.Views.FirstOrDefault(x => x.Name == Helpers.GetName("SomeView"));
            Assert.AreEqual(1, view.TelemetrySummaries.Count);
            ViewTelemetrySummary summary = view.TelemetrySummaries.SingleOrDefault(x => x.ClientAppUser.UserName == Helpers.GetName("Jim Beam"));
            Assert.AreEqual(1, summary.SummaryCount);
            Assert.AreEqual(summary.GetTelemetryDetails().Last().AssemblyVersionId, prg.PrimaryAssembly.GetVersion(telemetryItem.VersionData.Map()).Id);

            //run again
            telemetryItem.Id = Guid.NewGuid();
            request = new TelemetryUpdateRequest(prg.TelemetryKey, usr.Guid, new List<string>() { this.serializer.Serialize(telemetryItem) });
            response = await TelemetryControllerHelpers.InsertData(unit, request, "127.1.1.1");

            Helpers.GetProgramAndUser(this.Context, "TestApp3", "Jim Beam", out prg, out usr);
            Helpers.AssertUpdateResponse(response, prg, usr, 1, Helpers.GetName("SomeView"));
            Assert.AreEqual(2, response.Single().SummaryCount);
            view = prg.Views.FirstOrDefault(x => x.Name == Helpers.GetName("SomeView"));

            Assert.AreEqual(1, view.TelemetrySummaries.Count);

            Assert.AreEqual(2, view.GetTelemetrySummary(response.First().ClientAppUser.Id).SummaryCount);
            Assert.AreEqual(2, view.TelemetrySummaries.Single(x => x.ClientAppUser.UserName == Helpers.GetName("Jim Beam")).SummaryCount);

            Assert.AreEqual(2, view.GetTelemetryDetails(response.First().ClientAppUser.Id).Count);
            Assert.AreEqual(summary.GetTelemetryDetails().Last().AssemblyVersionId, prg.PrimaryAssembly.GetVersion(telemetryItem.VersionData.Map()).Id);
        }

        [Test]
        public async Task TestViewUsages()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            List<KeyValuePair<string, Guid>> apps = await Helpers.SeedInitialPrograms(this.Context, 2, "TestApp", new[] {"Billy Jean", "Jack Black"});

            Helpers.GetProgramAndUser(this.Context, "TestApp", "Billy Jean", out Program prg, out ClientAppUser usr);

            TelemetryItem telemetryItem = new TelemetryItem(Helpers.GetName("Func1"), TelemetryItemTypes.View, new VersionData("1.2.3.4", "2.0.0.0")
                , new Dictionary<string, object> {{"AKey", "AValue"}, {"AKey2", "AValue2"}});

            TelemetryUpdateRequest request = new TelemetryUpdateRequest(apps[0].Value, usr.Guid, new List<string>() { this.serializer.Serialize(telemetryItem) });

            List<TelemetrySummary> result = await TelemetryControllerHelpers.InsertData(unit, request, "127.1.1.1");

            View view = prg.Views.Single();

            int viewId = this.Context.Views.FirstOrDefault(x => x.Name == view.Name).Id;

            Helpers.AssertUpdateResponse(result, prg, usr, 1, Helpers.GetName("Func1"), viewId);

            Assert.AreEqual(1, prg.Views.Count);
            Assert.AreEqual(Helpers.GetName("Func1"), view.Name);
            Assert.AreEqual(1, view.TelemetrySummaries.Count);
            Assert.AreEqual(prg.Id, view.ProgramId);

            TelemetrySummary summary = view.GetTelemetrySummary(this.GetUserByGuid(result.First().ClientAppUser.Guid).Id);
            Assert.AreEqual(usr.Id, summary.ClientAppUserId);

            TelemetryDetail detail = view.GetTelemetryDetails(this.GetUserByGuid(result.First().ClientAppUser.Guid).Id).Single();
            Assert.AreEqual(detail.GetTelemetrySummary().Id, summary.Id);
            Assert.AreEqual(usr.Id, detail.GetTelemetrySummary().ClientAppUserId);
            Assert.AreEqual(2, detail.GetTelemetryUnits().Count());
            Assert.AreEqual("AKey", detail.GetTelemetryUnits().ElementAt(0).Key);
            Assert.AreEqual("AValue", detail.GetTelemetryUnits().ElementAt(0).ValueString);
            Assert.AreEqual("AKey2", detail.GetTelemetryUnits().ElementAt(1).Key);
            Assert.AreEqual("AValue2", detail.GetTelemetryUnits().ElementAt(1).ValueString);


            ClientAppUser otherUser = Helpers.GetUser(this.Context, "Jack Black");
            telemetryItem = new TelemetryItem(Helpers.GetName("Func1"), TelemetryItemTypes.View, new VersionData("1.2.3.4", "2.0.0.0")
                , new Dictionary<string, object> {{"AKey3", "AValue3"}, {"AKey4", "AValue4"}, {"AKey5", "AValue5"}});

            //run again with different user
            request = new TelemetryUpdateRequest(apps[0].Value, otherUser.Guid
                , new List<string> {this.serializer.Serialize(telemetryItem)});
            result = await TelemetryControllerHelpers.InsertData(unit, request, "127.1.1.1");

            Helpers.AssertUpdateResponse(result, prg, otherUser, 1, Helpers.GetName("Func1"), viewId);

            prg = await unit.Programs.FirstOrDefaultAsync(x => x.Id == prg.Id);
            Assert.AreEqual(1, prg.Views.Count);
            view = prg.Views.Single();
            Assert.AreEqual(Helpers.GetName("Func1"), view.Name);
            Assert.AreEqual(2, view.TelemetrySummaries.Count);
            Assert.AreEqual(1, view.GetTelemetrySummary(this.GetUserByGuid(result.First().ClientAppUser.Guid).Id).SummaryCount);
            Assert.AreEqual(1, summary.GetTelemetryDetails().Count());

            Assert.AreNotEqual(otherUser.Guid, usr.Guid);

            TelemetrySummary otherSummary = view.GetTelemetrySummary(this.GetUserByGuid(result.First().ClientAppUser.Guid).Id);
            Assert.AreEqual(otherUser.Id, otherSummary.ClientAppUserId);

            TelemetryDetail otherUserDetail = view.GetTelemetryDetails(this.GetUserByGuid(result.First().ClientAppUser.Guid).Id).Single();
            Assert.AreEqual(otherUser.Id, otherUserDetail.GetTelemetrySummary().ClientAppUserId);
            Assert.AreEqual(3, otherUserDetail.GetTelemetryUnits().Count());
            Assert.AreEqual("AKey3", otherUserDetail.GetTelemetryUnits().ElementAt(0).Key);
            Assert.AreEqual("AValue3", otherUserDetail.GetTelemetryUnits().ElementAt(0).ValueString);
            Assert.AreEqual("AKey5", otherUserDetail.GetTelemetryUnits().ElementAt(2).Key);
            Assert.AreEqual("AValue5", otherUserDetail.GetTelemetryUnits().ElementAt(2).ValueString);

            telemetryItem = new TelemetryItem(Helpers.GetName("Func1"), TelemetryItemTypes.View, new VersionData("1.2.3.4", "2.0.0.0"), null);

            request = new TelemetryUpdateRequest(apps[0].Value, usr.Guid, new List<string>() { this.serializer.Serialize(telemetryItem) });
            //run again with first user
            result = await TelemetryControllerHelpers.InsertData(unit, request, "127.1.1.1");

            view = prg.Views.Single();
            Assert.AreEqual(2, view.TelemetrySummaries.Count);
            Assert.AreEqual(2, view.GetTelemetrySummary(this.GetUserByGuid(result.First().ClientAppUser.Guid).Id).SummaryCount);
            Assert.AreEqual(2, summary.GetTelemetryDetails().Count());


            List<ViewTelemetryDetail> details = view.GetTelemetryDetails(this.GetUserByGuid(result.First().ClientAppUser.Guid).Id).OrderBy(x => x.Timestamp)
                .Cast<ViewTelemetryDetail>().ToList();
            Assert.AreEqual(2, details.Count);
            Assert.IsTrue(details.All(x => x.TelemetrySummary.ClientAppUserId == this.GetUserByGuid(result.First().ClientAppUser.Guid).Id));
            Assert.IsTrue(details.First().Timestamp < details.Last().Timestamp);

            Assert.AreEqual(3, this.Context.ViewTelemetryDetails.Count(x => x.TelemetrySummary.View.Name == telemetryItem.EntryKey));
            Assert.AreEqual(2, this.Context.ViewTelemetryDetails.Count(x => x.TelemetrySummaryId == summary.Id));
        }
    }
}