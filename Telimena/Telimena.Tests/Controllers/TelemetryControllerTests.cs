// -----------------------------------------------------------------------
//  <copyright file="TelemetryControllerTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using DbIntegrationTestHelpers;
using DotNetLittleHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Api;
using Telimena.WebApp.Controllers.Api.V1;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;
using TelimenaClient;
using TelimenaClient.Serializer;
using Assert = NUnit.Framework.Assert;

namespace Telimena.Tests
{
    #region Using

    #endregion



    [TestFixture]
    public class TelemetryControllerTests : IntegrationTestsContextSharedGlobally<TelimenaContext>
    {
        protected override Action SeedAction => () => TelimenaDbInitializer.SeedUsers(this.Context);

        private TelimenaSerializer serializer = new TelimenaSerializer();

        [Test]
        public async Task TestViewUsages()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            TelemetryController sut = new TelemetryController(unit);
            List<KeyValuePair<string, Guid>> apps = await  Helpers.SeedInitialPrograms(this.Context, 2, "TestApp", new []{"Billy Jean", "Jack Black"});

            Helpers.GetProgramAndUser(this.Context, "TestApp", "Billy Jean", out Program prg, out ClientAppUser usr);

            TelemetryItem telemetryItem = new TelemetryItem(Helpers.GetName("Func1"), TelemetryItemTypes.View, new VersionData("1.2.3.4", "2.0.0.0"), new Dictionary<string, object>()
            {
                { "AKey", "AValue"},
                { "AKey2", "AValue2"},
            });

            TelemetryUpdateRequest request = new TelemetryUpdateRequest(apps[0].Value) { DebugMode = true, UserId = usr.Guid, SerializedTelemetryUnits = new List<string>(){ this.serializer.Serialize(telemetryItem) } };

            IHttpActionResult result = await sut.View(request);
            if (result is ExceptionResult ex)
            {
                Assert.Fail(ex.Exception.ToString());
            }
            Assert.IsInstanceOf<OkNegotiatedContentResult<TelemetryUpdateResponse>>(result);

            TelemetryUpdateResponse response = (result as OkNegotiatedContentResult<TelemetryUpdateResponse>).Content;
            View view = prg.Views.Single();

            int viewId = this.Context.Views.FirstOrDefault(x => x.Name == view.Name).Id;

            Helpers.AssertUpdateResponse(response, prg, usr, 1, Helpers.GetName("Func1"), viewId);


            Assert.AreEqual(1, prg.Views.Count);
            Assert.AreEqual(Helpers.GetName("Func1"), view.Name);
            Assert.AreEqual(response.ComponentId, view.Id);
            Assert.AreEqual(1, view.TelemetrySummaries.Count);
            Assert.AreEqual(prg.Id, view.ProgramId);

            TelemetrySummary summary = view.GetTelemetrySummary(this.GetUserByGuid(response.UserId).Id);
            Assert.AreEqual(usr.Id, summary.ClientAppUserId);
            
            TelemetryDetail detail = view.GetTelemetryDetails(this.GetUserByGuid(response.UserId).Id).Single();
            Assert.AreEqual(detail.GetTelemetrySummary().Id, summary.Id);
            Assert.AreEqual(usr.Id, detail.GetTelemetrySummary().ClientAppUserId);
            Assert.AreEqual(2, detail.GetTelemetryUnits().Count());
            Assert.AreEqual("AKey", detail.GetTelemetryUnits().ElementAt(0).Key);
            Assert.AreEqual("AValue", detail.GetTelemetryUnits().ElementAt(0).ValueString);
            Assert.AreEqual("AKey2", detail.GetTelemetryUnits().ElementAt(1).Key);
            Assert.AreEqual("AValue2", detail.GetTelemetryUnits().ElementAt(1).ValueString);
          

            ClientAppUser otherUser = Helpers.GetUser(this.Context, "Jack Black");

            telemetryItem.TelemetryData = new Dictionary<string, object>()
                {
                    { "AKey", "AValue"},
                    { "AKey2", "AValue2"},
                    { "AKey5", "AValue5"},

                };
            //run again with different user
            request = new TelemetryUpdateRequest(apps[0].Value) {
                SerializedTelemetryUnits = new List<string>() { this.serializer.Serialize(telemetryItem) },
                DebugMode = true,
                UserId = otherUser.Guid,
            };

            response = (await sut.View(request) as OkNegotiatedContentResult<TelemetryUpdateResponse>).Content;

            Assert.AreEqual(1, response.Count);
            prg = await unit.Programs.FirstOrDefaultAsync(x => x.Id == prg.Id);
            Assert.AreEqual(1, prg.Views.Count);
            view = prg.Views.Single();
            Assert.AreEqual(Helpers.GetName("Func1"), view.Name);
            Assert.AreEqual(2, view.TelemetrySummaries.Count);
            Assert.AreEqual(1, view.GetTelemetrySummary(this.GetUserByGuid(response.UserId).Id).SummaryCount);
            Assert.AreEqual(1, summary.GetTelemetryDetails().Count());

            Assert.AreNotEqual(otherUser.Guid, usr.Guid);

            TelemetrySummary otherSummary = view.GetTelemetrySummary(this.GetUserByGuid(response.UserId).Id);
            Assert.AreEqual(otherUser.Id, otherSummary.ClientAppUserId);

            TelemetryDetail otherUserDetail = view.GetTelemetryDetails(this.GetUserByGuid(response.UserId).Id).Single();
            Assert.AreEqual(otherUser.Id, otherUserDetail.GetTelemetrySummary().ClientAppUserId);
            Assert.AreEqual(3, otherUserDetail.GetTelemetryUnits().Count());
            Assert.AreEqual("AKey3", otherUserDetail.GetTelemetryUnits().ElementAt(0).Key);
            Assert.AreEqual("AValue3", otherUserDetail.GetTelemetryUnits().ElementAt(0).ValueString);
            Assert.AreEqual("AKey5", otherUserDetail.GetTelemetryUnits().ElementAt(2).Key);
            Assert.AreEqual("AValue5", otherUserDetail.GetTelemetryUnits().ElementAt(2).ValueString);

            telemetryItem.TelemetryData = null;
            request = new TelemetryUpdateRequest(apps[0].Value) { DebugMode = true, UserId = usr.Guid, SerializedTelemetryUnits = new List<string>() { this.serializer.Serialize(telemetryItem) }};
            //run again with first user
            response = (await sut.View(request) as OkNegotiatedContentResult<TelemetryUpdateResponse>).Content;
            view = prg.Views.Single();
            Assert.AreEqual(2, view.TelemetrySummaries.Count);
            Assert.AreEqual(2, view.GetTelemetrySummary(this.GetUserByGuid(response.UserId).Id).SummaryCount);
            Assert.AreEqual(2, summary.GetTelemetryDetails().Count());
      

            List<ViewTelemetryDetail> details = view.GetTelemetryDetails(this.GetUserByGuid(response.UserId).Id).OrderBy(x => x.Id).Cast<ViewTelemetryDetail>().ToList();
            Assert.AreEqual(2, details.Count);
            Assert.IsTrue(details.All(x => x.TelemetrySummary.ClientAppUserId == this.GetUserByGuid(response.UserId).Id));
            Assert.IsTrue(details.First().Timestamp < details.Last().Timestamp);

            Assert.AreEqual(3, this.Context.ViewTelemetryDetails.Count(x=>x.TelemetrySummary.View.Name == telemetryItem.EntryKey));
            Assert.AreEqual(2, this.Context.ViewTelemetryDetails.Count(x => x.TelemetrySummaryId == summary.Id));
        }

        [Test]
        public async Task TestMissingProgram()
        {
            TelemetryInitializeRequest request = new TelemetryInitializeRequest(Guid.NewGuid()) { };

            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());

            TelemetryController sut = new TelemetryController(unit);
            TelemetryInitializeResponse response = await sut.Initialize(request);
            Assert.IsTrue(response.Exception.Message.Contains($"Program [{request.TelemetryKey}] is null"));
        }

        [Test]
        public async Task TestMissingUser()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());

            unit.Programs.Add(new Program("SomeApp", Guid.NewGuid()) { PrimaryAssembly = new ProgramAssembly { Name = "SomeApp.dll", Company = "SomeCompm" } });

            await unit.CompleteAsync();
            Program prg = (await unit.Programs.GetAsync(x => x.Name == "SomeApp")).FirstOrDefault();
            Assert.IsTrue(prg.Id > 0);

            TelemetryUpdateRequest request = new TelemetryUpdateRequest(prg.TelemetryKey) { DebugMode = true, UserId = Guid.NewGuid() };

            TelemetryController sut = new TelemetryController(unit);


            ExceptionResult response = (await sut.View(request) as ExceptionResult);
            
            Assert.AreEqual($"User [{request.UserId}] is null", response.Exception.Message);
        }

        [Test]
        public async Task TestReferencedAssemblies_Add()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            TelemetryController sut = new TelemetryController(unit);
            List<KeyValuePair<string, Guid>> apps = await Helpers.SeedInitialPrograms(this.Context, 4, "TestApp", new []{ "Johnny Walker"});
            Helpers.AddHelperAssemblies(this.Context, 2, "TestApp");
            Helpers.GetProgramAndUser(this.Context, "TestApp", "Johnny Walker", out Program prg, out ClientAppUser usr);

            Assert.AreEqual(3, prg.ProgramAssemblies.Count);
            Assert.AreEqual(Helpers.GetName("TestApp") + ".dll", prg.PrimaryAssembly.Name + prg.PrimaryAssembly.Extension);
            Assert.IsNotNull(prg.ProgramAssemblies.Single(x => x.Name == Helpers.GetName("TestApp")));
            Assert.IsNotNull(prg.ProgramAssemblies.Single(x => x.Name == "HelperAss0_" + Helpers.GetName("TestApp")));
            Assert.IsNotNull(prg.ProgramAssemblies.Single(x => x.Name == "HelperAss1_" + Helpers.GetName("TestApp")));
        }

        [Test]
        public async Task TestReferencedAssembliesAddRemove()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            TelemetryController sut = new TelemetryController(unit);
            List<KeyValuePair<string, Guid>> apps = await Helpers.SeedInitialPrograms(this.Context, 4, "TestApp", new[] { "NewGuy" });
            Helpers.GetProgramAndUser(this.Context, "TestApp", "NewGuy", out Program prg, out ClientAppUser usr);

            prg.ProgramAssemblies.Add(new ProgramAssembly { Name = "Helper1" });
            prg.ProgramAssemblies.Add(new ProgramAssembly { Name = "Helper2" });
            this.Context.SaveChanges();

            Helpers.GetProgramAndUser(this.Context, "TestApp", "NewGuy", out prg, out usr);

            Assert.AreEqual(3, prg.ProgramAssemblies.Count);

            this.Context.Programs.Remove(prg);

            this.Context.SaveChanges();

            Assert.AreEqual(0, this.Context.ProgramAssemblies.Count(x => x.Program.Name == prg.Name));
            Assert.AreEqual(0, this.Context.Programs.Count(x => x.Name == prg.Name));
        }




        [Test]
        public async Task TestRegistration_SameAppEachTime()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            TelemetryController sut = new TelemetryController(unit);
            UserInfo userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));

            List<KeyValuePair<string, Guid>> apps = await Helpers.SeedInitialPrograms(this.Context, 4, "TestProg", new[] { "NewGuy" });

            TelemetryInitializeRequest request = new TelemetryInitializeRequest(apps[0].Value)
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")),
                TelimenaVersion = "1.0.0.0",
                UserInfo = userInfo
            };
            request.ProgramInfo.HelperAssemblies = new List<AssemblyInfo>
            {
                new AssemblyInfo {Name = "Helper_" + Helpers.GetName("TestProg") + ".dll", VersionData = new VersionData("0.0.0.1", "2.0.0.0")}
            };

            TelemetryInitializeResponse response = await sut.Initialize(request);
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program prg, out ClientAppUser usr);
            Helpers.AssertRegistrationResponse(response, prg, usr, 2);
            Assert.AreEqual(1, prg.PrimaryAssembly.Versions.Count);
            Assert.AreEqual(2, prg.ProgramAssemblies.Count);
            ProgramAssembly helper = prg.ProgramAssemblies.Single(x => x.Name == "Helper_" + Helpers.GetName("TestProg") + ".dll");
            Assert.AreEqual("0.0.0.1", helper.GetLatestVersion().AssemblyVersion);
            Assert.AreEqual(1, helper.Versions.Count);

            Assert.AreEqual("1.2.3.4", prg.PrimaryAssembly.GetLatestVersion().AssemblyVersion);
            Assert.AreEqual("1.2.3.4", prg.GetLatestVersion().AssemblyVersion);
            Assert.AreEqual(unit.Versions.Single(x => x.ProgramAssemblyId == prg.PrimaryAssembly.Id).Id, prg.PrimaryAssembly.GetLatestVersion().Id);
            int firstVersionId = prg.PrimaryAssembly.GetLatestVersion().Id;

            //second time
            response = await  sut.Initialize(request);
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out prg, out usr);
            Helpers.AssertRegistrationResponse(response, prg, usr, 3);
            Assert.AreEqual(1, prg.PrimaryAssembly.Versions.Count);
            Assert.AreEqual("1.2.3.4", prg.PrimaryAssembly.GetLatestVersion().AssemblyVersion);
            Assert.AreEqual(unit.Versions.Single(x => x.ProgramAssemblyId == prg.PrimaryAssembly.Id).Id, prg.PrimaryAssembly.GetLatestVersion().Id);


            //third time - different version
            request = new TelemetryInitializeRequest(apps[0].Value)
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg"), version: new VersionData("2.0.0.0", "3.0.0.0")),
                TelimenaVersion = "1.0.0.0",
                UserInfo = userInfo
            };
            response = await sut.Initialize(request);
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out prg, out usr);
            Helpers.AssertRegistrationResponse(response, prg, usr, 4);
            Assert.AreEqual("2.0.0.0", prg.PrimaryAssembly.GetLatestVersion().AssemblyVersion);
            //Assert.AreEqual("1.2.3.4", prg.PrimaryAssembly.GetLatestVersion().Version);
            Assert.AreEqual(unit.Versions.Single(x => x.ProgramAssemblyId == prg.PrimaryAssembly.Id && x.AssemblyVersion == "2.0.0.0").Id
                , prg.PrimaryAssembly.GetLatestVersion().Id);


            int latestId = prg.PrimaryAssembly.GetLatestVersion().Id;

            Assert.AreEqual(2, prg.PrimaryAssembly.Versions.Count);

            //fourth time - use first version again
            request = new TelemetryInitializeRequest(apps[0].Value)
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")),
                TelimenaVersion = "1.0.0.0",
                UserInfo = userInfo
            };
            response = await sut.Initialize(request);
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out prg, out usr);
            Helpers.AssertRegistrationResponse(response, prg, usr, 5);
            Assert.AreEqual(latestId, prg.PrimaryAssembly.GetLatestVersion().Id);
            Assert.AreEqual(latestId, prg.GetLatestVersion().Id);
            Assert.AreEqual(firstVersionId, prg.PrimaryAssembly.GetVersion(request.ProgramInfo.PrimaryAssembly.VersionData.Map()).Id);

            Assert.AreEqual(2, prg.PrimaryAssembly.Versions.Count);
        }

        [Test]
        public async Task TestRegistration_SameAppEachTime_ValidateHelperAssemblies()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            TelemetryController sut = new TelemetryController(unit);
            List<KeyValuePair<string, Guid>> apps = await Helpers.SeedInitialPrograms(this.Context, 1, "TestProg", new string[0] );
            UserInfo userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));

            TelemetryInitializeRequest request = new TelemetryInitializeRequest(apps[0].Value)
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")),
                TelimenaVersion = "1.0.0.0",
                UserInfo = userInfo
            };
            request.ProgramInfo.HelperAssemblies = new List<AssemblyInfo>
            {
                new AssemblyInfo {Name = "Helper_" + Helpers.GetName("TestProg") + ".dll", VersionData = new VersionData("0.0.0.1", "2.0.0.0")}
            };
            TelemetryInitializeResponse response = await sut.Initialize(request);
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program prg, out ClientAppUser usr);
            Helpers.AssertRegistrationResponse(response, prg, usr, 1);
            Assert.AreEqual(2, prg.ProgramAssemblies.Count);
            ProgramAssembly helper = prg.ProgramAssemblies.Single(x => x.Name == "Helper_" + Helpers.GetName("TestProg") + ".dll");
            Assert.AreEqual("0.0.0.1", helper.GetLatestVersion().AssemblyVersion);
            Assert.AreEqual(1, helper.Versions.Count);


            //second time
            request.ProgramInfo.HelperAssemblies = new List<AssemblyInfo>
            {
                new AssemblyInfo {Name = "Helper2_" + Helpers.GetName("TestProg") + ".dll", VersionData = new VersionData("0.0.2.2", "2.0.0.0")}
            };
            response = await sut.Initialize(request);
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out prg, out usr);
            Helpers.AssertRegistrationResponse(response, prg, usr, 2);
            Assert.AreEqual(3, prg.ProgramAssemblies.Count);
            helper = prg.ProgramAssemblies.Single(x => x.Name == "Helper_" + Helpers.GetName("TestProg") + ".dll");
            ProgramAssembly helper2 = prg.ProgramAssemblies.Single(x => x.Name == "Helper2_" + Helpers.GetName("TestProg") + ".dll");
            Assert.AreEqual("0.0.0.1", helper.GetLatestVersion().AssemblyVersion);
            Assert.AreEqual(1, helper.Versions.Count);
            Assert.AreEqual("0.0.2.2", helper2.GetLatestVersion().AssemblyVersion);
            Assert.AreEqual(1, helper2.Versions.Count);


            //third time - different version
            request = new TelemetryInitializeRequest(apps[0].Value)
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg"), version: new VersionData("2.0.0.0", "3.0.0.0")),
                TelimenaVersion = "1.0.0.0",
                UserInfo = userInfo
            };
            request.ProgramInfo.HelperAssemblies = new List<AssemblyInfo>
            {
                new AssemblyInfo {Name = "Helper_" + Helpers.GetName("TestProg") + ".dll", VersionData = new VersionData("0.3.0.1", "2.0.0.0")} //newer version of helper!
            };
            response = await sut.Initialize(request);
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out prg, out usr);
            Helpers.AssertRegistrationResponse(response, prg, usr, 3);

            Assert.AreEqual(3, prg.ProgramAssemblies.Count);
            helper = prg.ProgramAssemblies.Single(x => x.Name == "Helper_" + Helpers.GetName("TestProg") + ".dll");
            helper2 = prg.ProgramAssemblies.Single(x => x.Name == "Helper2_" + Helpers.GetName("TestProg") + ".dll");
            Assert.AreEqual("0.3.0.1", helper.GetLatestVersion().AssemblyVersion);
            Assert.AreEqual(2, helper.Versions.Count);
            Assert.AreEqual("0.0.2.2", helper2.GetLatestVersion().AssemblyVersion);
            Assert.AreEqual(1, helper2.Versions.Count);
            Assert.AreEqual("2.0.0.0", prg.GetLatestVersion().AssemblyVersion);


            //fourth time - do not register helpers anymore
            request = new TelemetryInitializeRequest(apps[0].Value)
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")),
                TelimenaVersion = "1.0.0.0",
                UserInfo = userInfo
            };
            response =  await sut.Initialize(request);
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out prg, out usr);
            Helpers.AssertRegistrationResponse(response, prg, usr, 4);
            Assert.AreEqual(3, prg.ProgramAssemblies.Count);

            helper = prg.ProgramAssemblies.Single(x => x.Name == "Helper_" + Helpers.GetName("TestProg") + ".dll");
            helper2 = prg.ProgramAssemblies.Single(x => x.Name == "Helper2_" + Helpers.GetName("TestProg") + ".dll");
            Assert.AreEqual("0.3.0.1", helper.GetLatestVersion().AssemblyVersion);
            Assert.AreEqual(2, helper.Versions.Count);
            Assert.AreEqual("0.0.2.2", helper2.GetLatestVersion().AssemblyVersion);
            Assert.AreEqual(1, helper2.Versions.Count);
            Assert.AreEqual("2.0.0.0", prg.GetLatestVersion().AssemblyVersion);
        }

        [Test]
        public async Task TestRegistration_SameAppTwoUsers_DifferentVersions()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            TelemetryController sut = new TelemetryController(unit);
            List<KeyValuePair<string, Guid>> apps = await Helpers.SeedInitialPrograms(this.Context, 1, "TestProg", new string[0]);

            UserInfo oldGuyUserInfo = Helpers.GetUserInfo(Helpers.GetName("OldGuy"));
            UserInfo newGuyUserInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));

            TelemetryInitializeRequest oldGuyRequest = new TelemetryInitializeRequest(apps[0].Value)
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")),
                TelimenaVersion = "1.0.0.0",
                UserInfo = oldGuyUserInfo
            };
            TelemetryInitializeRequest newGuyRequest = new TelemetryInitializeRequest(apps[0].Value)
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg"), version: new VersionData("6.0", "7.0")),
                TelimenaVersion = "1.0.0.0",
                UserInfo = newGuyUserInfo
            };

            TelemetryInitializeResponse response = await sut.Initialize(oldGuyRequest);
            await sut.Initialize(newGuyRequest);
            await sut.Initialize(oldGuyRequest);
            await sut.Initialize(newGuyRequest);
            await sut.Initialize(oldGuyRequest);
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program prg, out ClientAppUser usr);

            Assert.AreEqual(2, prg.PrimaryAssembly.Versions.Count);
            Assert.AreEqual(1, prg.ProgramAssemblies.Count);
            Assert.IsTrue(prg.PrimaryAssembly.Versions.Any(x => x.AssemblyVersion == "1.2.3.4"));
            Assert.IsTrue(prg.PrimaryAssembly.Versions.Any(x => x.AssemblyVersion == "6.0"));
        }

        private void SetIp(ApiController controller, string ip)
        {
            Mock<HttpRequestBase> request = new Mock<HttpRequestBase>();
            // Not working - IsAjaxRequest() is static extension method and cannot be mocked
            // request.Setup(x => x.IsAjaxRequest()).Returns(true /* or false */);
            // use this
            request.SetupGet(x => x.UserHostAddress).Returns(ip);

            //var ctx = new Mock<HttpContextWrapper>();
            //ctx.SetupGet(x => x.Request).Returns(request.Object);

            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.SetupAllProperties();
            mockContext.Setup(c => c.Request).Returns(request.Object);

            TestingUtilities.SetReuqest(controller, HttpMethod.Post, new Dictionary<string, object>() { { "MS_HttpContext", mockContext.Object} });

        }

        [Test]
        public async Task TestIp_Ok()
        {
            TelemetryUnitOfWork unit;
            TelemetryController sut;
            TelemetryInitializeRequest request = null;
            UserInfo userInfo;
            List<KeyValuePair<string, Guid>> apps = await Helpers.SeedInitialPrograms(this.Context, 1, "TestProg", new string[0]);

            for (int i = 0; i <= 2; i++)
            {

                unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
                sut = new TelemetryController(unit);

                this.SetIp(sut, "1.2.3.4");
                userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));

                request = new TelemetryInitializeRequest(apps[0].Value)
                {
                    ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")),
                    TelimenaVersion = "1.0.0.0",
                    UserInfo = userInfo
                };

                TelemetryInitializeResponse response = await sut.Initialize(request);
                Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program prg, out ClientAppUser usr);

                Assert.AreEqual("1.2.3.4", usr.IpAddresses.Single());

            }

            unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            sut = new TelemetryController(unit);

             this.SetIp(sut, "2.2.3.4");
            userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));


            await sut.Initialize(request);
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program _, out ClientAppUser usr2);

            Assert.AreEqual("1.2.3.4", usr2.IpAddresses[0]);
            Assert.AreEqual("2.2.3.4", usr2.IpAddresses[1]);

        }

        [Test]
        public async Task TestRegistration_SameUserTwoApps_HappyPath()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            TelemetryController sut = new TelemetryController(unit);
            UserInfo userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));
            List<KeyValuePair<string, Guid>> apps = await Helpers.SeedInitialPrograms(this.Context, 1, "TestProg", new string[0]);
            List<KeyValuePair<string, Guid>> apps2 = await Helpers.SeedInitialPrograms(this.Context, 1, "TestProg2", new string[0]);


            TelemetryInitializeRequest request = new TelemetryInitializeRequest(apps[0].Value)
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")),
                TelimenaVersion = "1.0.0.0",
                UserInfo = userInfo
            };
            TelemetryInitializeResponse response = await sut.Initialize(request);
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program prg, out ClientAppUser usr);

            Assert.IsTrue(prg.Id > 0 && usr.Id > 0);
            Helpers.AssertRegistrationResponse(response, prg, usr, 1);

            Assert.AreEqual(request.ProgramInfo.PrimaryAssembly.Name, prg.PrimaryAssembly.Name);
            Assert.AreEqual("xyz", prg.PrimaryAssembly.Company);
            Assert.AreEqual(prg.Id, this.Context.ProgramAssemblies.FirstOrDefault(x => x.Name == request.ProgramInfo.PrimaryAssembly.Name).Program.Id);

            Assert.AreEqual(userInfo.UserName, usr.UserName);
            Assert.AreEqual(request.UserInfo.MachineName, usr.MachineName);
            Assert.AreEqual(request.UserInfo.UserName, usr.UserName);

            string prgName2 = Helpers.GetName("TestProg2");
            //now second app for same user
            request = new TelemetryInitializeRequest(apps2[0].Value) { ProgramInfo = Helpers.GetProgramInfo(prgName2), TelimenaVersion = "1.0.0.0", UserInfo = userInfo };

            response = await sut.Initialize(request);
            Program prg2 = await unit.Programs.SingleOrDefaultAsync(x => x.Name == prgName2);

            Helpers.AssertRegistrationResponse(response, prg2, usr, 1);

            Assert.IsTrue(prg2.Id > prg.Id);
        }

        //[Test]
        //public async Task TestRegistration_SkipUsageIncremet()
        //{
        //    TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
        //    TelemetryController sut = new TelemetryController(unit);
        //    UserInfo userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));
        //    var apps = await Helpers.SeedInitialPrograms(this.Context, 1, "TestProg", new string[0]);

        //    TelemetryInitializeRequest request = new TelemetryInitializeRequest(apps[0].Value)
        //    {
        //        ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg"))
        //        ,TelimenaVersion = "1.0.0.0"
        //        ,UserInfo = userInfo
        //        ,SkipUsageIncrementation = true
        //    };
        //    TelemetryInitializeResponse response = await sut.Initialize(request);
        //    Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program prg, out ClientAppUser usr);
        //    Helpers.AssertRegistrationResponse(response, prg, usr, 1);
        //    Assert.AreEqual(1, prg.TelemetrySummaries.Count);
        //    Assert.AreEqual(1, prg.TelemetrySummaries.Sum(x => x.GetTelemetryDetails().Count()));
        //}

        [Test]
        public async Task TestUpdateAction()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            TelemetryController sut = new TelemetryController(unit);
            List<KeyValuePair<string, Guid>> apps  = await Helpers.SeedInitialPrograms(this.Context, 4,"TestApp", new []{"Johnny Walker","Jim Beam", "Eric Cartman" });
          
            Helpers.AddHelperAssemblies(this.Context, 2, "TestApp");

            Helpers.GetProgramAndUser(this.Context, "TestApp3", "Jim Beam", out Program prg, out ClientAppUser usr);

            TelemetryItem telemetryItem = new TelemetryItem(Helpers.GetName("SomeView"), TelemetryItemTypes.View, new VersionData("1.2.3.4", "2.0.0.0"), new Dictionary<string, object>()
            {
                { "AKey", "AValue"},
                { "AKey2", "AValue2"},
            });

            TelemetryUpdateRequest request = new TelemetryUpdateRequest(prg.TelemetryKey)
            {
                DebugMode = true, UserId = usr.Guid, SerializedTelemetryUnits = new List<string>() { this.serializer.Serialize(telemetryItem) },
            };

            IHttpActionResult result = await sut.View(request);
            if (result is ExceptionResult ex)
            {
                Assert.Fail(ex.Exception.ToString());
            }
            Assert.IsInstanceOf<OkNegotiatedContentResult<TelemetryUpdateResponse>>(result);

            TelemetryUpdateResponse response = (result as OkNegotiatedContentResult<TelemetryUpdateResponse>).Content;

            Assert.IsTrue(prg.Id > 0 && usr.Id > 0);

            Helpers.AssertUpdateResponse(response, prg, usr, 1, "SomeView");

            View view = prg.Views.FirstOrDefault(x => x.Name == "SomeView");

            Assert.AreEqual(1, view.TelemetrySummaries.Count);
            ViewTelemetrySummary summary = view.TelemetrySummaries.SingleOrDefault(x => x.ClientAppUser.UserName == "TestUpdateAction_Jim Beam");
            Assert.AreEqual(1, summary.SummaryCount);

            Assert.AreEqual(summary.GetTelemetryDetails().Last().AssemblyVersionId, prg.PrimaryAssembly.GetVersion(telemetryItem.VersionData.Map()).Id);


            //run again
            response = (await sut.View(request) as OkNegotiatedContentResult<TelemetryUpdateResponse>).Content;

            Helpers.GetProgramAndUser(this.Context, "TestApp3", "Jim Beam", out prg, out usr);
            Helpers.AssertUpdateResponse(response, prg, usr, 2, "SomeView");
            view = prg.Views.FirstOrDefault(x => x.Name == "SomeView");

            Assert.AreEqual(1, view.TelemetrySummaries.Count);

            Assert.AreEqual(2, view.GetTelemetrySummary(this.GetUserByGuid(response.UserId).Id).SummaryCount);
            Assert.AreEqual(2, view.TelemetrySummaries.Single(x => x.ClientAppUser.UserName == "TestUpdateAction_Jim Beam").SummaryCount);

            Assert.AreEqual(2, view.GetTelemetryDetails(this.GetUserByGuid(response.UserId).Id).Count);
            Assert.AreEqual(summary.GetTelemetryDetails().Last().AssemblyVersionId, prg.PrimaryAssembly.GetVersion(telemetryItem.VersionData.Map()).Id);
        }

        private ClientAppUser GetUserByGuid(Guid id)
        {
            return this.Context.AppUsers.FirstOrDefault(x => x.Guid == id);
        }

        [Test]
        public async Task TestVersions()
        {


            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            TelemetryController sut = new TelemetryController(unit);
            List<KeyValuePair<string, Guid>> apps = await Helpers.SeedInitialPrograms(this.Context, 2, "TestApp", new[] { "Billy Jean", "Jack Black" });

            Helpers.GetProgramAndUser(this.Context, "TestApp", "Billy Jean", out Program prg, out ClientAppUser usr);
            Helpers.GetProgramAndUser(this.Context, "TestApp", "Jack Black", out prg, out ClientAppUser usr2);

            TelemetryInitializeRequest request = new TelemetryInitializeRequest(prg.TelemetryKey) {
                UserInfo = Helpers.GetUserInfo(Helpers.GetName("Billy Jean")),
                ProgramInfo = Helpers.GetProgramInfo("TestApp",version: new VersionData("1.2.3.4", "2.2.3.4")) };

            await sut.Initialize(request);
            Helpers.GetProgramAndUser(this.Context, "TestApp", "Billy Jean", out prg, out usr);


            Assert.AreEqual(1, this.Context.Versions.Count(x => x.ProgramAssembly.ProgramId == prg.Id));

            request = new TelemetryInitializeRequest(prg.TelemetryKey)
            {
                UserInfo = Helpers.GetUserInfo(Helpers.GetName("Billy Jean")),
                ProgramInfo = Helpers.GetProgramInfo("TestApp", version: new VersionData("2.0.0.0", "3.0.0.0"))
            };

            await sut.Initialize(request);
            Helpers.GetProgramAndUser(this.Context, "TestApp", "Billy Jean", out prg, out usr);

            Assert.AreEqual(2, this.Context.Versions.Count(x => x.ProgramAssembly.ProgramId == prg.Id));

        }

    }
}