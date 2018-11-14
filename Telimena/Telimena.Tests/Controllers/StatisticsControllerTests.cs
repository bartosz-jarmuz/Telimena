// -----------------------------------------------------------------------
//  <copyright file="StatisticsControllerTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;
using DbIntegrationTestHelpers;
using DotNetLittleHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Api;
using Telimena.WebApp.Core.Models;
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
    public class StatisticsControllerTests : IntegrationTestsContextSharedGlobally<TelimenaContext>
    {
        protected override Action SeedAction => () => TelimenaDbInitializer.SeedUsers(this.Context);

   
        [Test]
        public void TestViewUsages()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            StatisticsController sut = new StatisticsController(unit);
            Helpers.SeedInitialPrograms(sut, 2, Helpers.GetName("TestApp"), Helpers.GetName("Billy Jean"));
            Helpers.SeedInitialPrograms(sut, 2, Helpers.GetName("TestApp"), Helpers.GetName("Jack Black"));
            Helpers.GetProgramAndUser(this.Context, "TestApp", "Billy Jean", out Program prg, out ClientAppUser usr);

            StatisticsUpdateRequest request = new StatisticsUpdateRequest {ProgramId = prg.Id, ComponentName = "Func1", UserId = usr.Id, AssemblyVersion = "1.2.3.4", TelemetryData = new Dictionary<string, string>()
            {
                { "AKey", "AValue"},
                { "AKey2", "AValue2"},
            }};

            StatisticsUpdateResponse response = sut.Update(request).GetAwaiter().GetResult();

            Helpers.AssertUpdateResponse(response, prg, usr, 1, "Func1",1);

            View view = prg.Views.Single();

            Assert.AreEqual(1, prg.Views.Count);
            Assert.AreEqual("Func1", view.Name);
            Assert.AreEqual(1, view.Id);
            Assert.AreEqual(1, view.TelemetrySummaries.Count);
            Assert.AreEqual(prg.Id, view.ProgramId);

            var summary = view.GetTelemetrySummary(response.UserId);
            Assert.AreEqual(usr.Id, summary.ClientAppUserId);
            
            var detail = view.GetTelemetryDetails(response.UserId).Single();
            Assert.AreEqual(detail.GetTelemetrySummary().Id, summary.Id);
            Assert.AreEqual(usr.Id, detail.GetTelemetrySummary().ClientAppUserId);
            Assert.AreEqual(2, detail.GetTelemetryUnits().Count());
            Assert.AreEqual("AKey", detail.GetTelemetryUnits().ElementAt(0).Key);
            Assert.AreEqual("AValue", detail.GetTelemetryUnits().ElementAt(0).Value);
            Assert.AreEqual("AKey2", detail.GetTelemetryUnits().ElementAt(1).Key);
            Assert.AreEqual("AValue2", detail.GetTelemetryUnits().ElementAt(1).Value);
          

            ClientAppUser otherUser = Helpers.GetUser(this.Context, "Jack Black");

            //run again with different user
            request = new StatisticsUpdateRequest {ProgramId = prg.Id, ComponentName = "Func1", UserId = otherUser.Id, AssemblyVersion = "1.2.3.4",
                TelemetryData = new Dictionary<string, string>()
                {
                    { "AKey3", "AValue3"},
                    { "AKey4", "AValue4"},
                    { "AKey5", "AValue5"},
                }
            };

            response = sut.Update(request).GetAwaiter().GetResult();
            Assert.AreEqual(1, response.Count);
            prg = unit.Programs.FirstOrDefaultAsync(x => x.Id == prg.Id).GetAwaiter().GetResult();
            Assert.AreEqual(1, prg.Views.Count);
            view = prg.Views.Single();
            Assert.AreEqual("Func1", view.Name);
            Assert.AreEqual(2, view.TelemetrySummaries.Count);
            Assert.AreEqual(1, view.GetTelemetrySummary(response.UserId).SummaryCount);
            Assert.AreEqual(1, summary.GetTelemetryDetails().Count());

            var otherSummary = view.GetTelemetrySummary(response.UserId);
            Assert.AreEqual(otherUser.Id, otherSummary.ClientAppUserId);

            var otherUserDetail = view.GetTelemetryDetails(response.UserId).Single();
            Assert.AreEqual(otherUser.Id, otherUserDetail.GetTelemetrySummary().ClientAppUserId);
            Assert.AreEqual(3, otherUserDetail.GetTelemetryUnits().Count());
            Assert.AreEqual("AKey3", otherUserDetail.GetTelemetryUnits().ElementAt(0).Key);
            Assert.AreEqual("AValue3", otherUserDetail.GetTelemetryUnits().ElementAt(0).Value);
            Assert.AreEqual("AKey4", otherUserDetail.GetTelemetryUnits().ElementAt(1).Key);
            Assert.AreEqual("AValue4", otherUserDetail.GetTelemetryUnits().ElementAt(1).Value);

            request = new StatisticsUpdateRequest {ProgramId = prg.Id, ComponentName = "Func1", UserId = usr.Id, AssemblyVersion = "1.2.3.4"/*, TelemetryData = serialized*/};
            //run again with first user
            response = sut.Update(request).GetAwaiter().GetResult();
            view = prg.Views.Single();
            Assert.AreEqual(2, view.TelemetrySummaries.Count);
            Assert.AreEqual(2, view.GetTelemetrySummary(response.UserId).SummaryCount);
            Assert.AreEqual(2, summary.GetTelemetryDetails().Count());
      

            List<ViewTelemetryDetail> details = view.GetTelemetryDetails(response.UserId).OrderBy(x => x.Id).Cast<ViewTelemetryDetail>().ToList();
            Assert.AreEqual(2, details.Count);
            Assert.IsTrue(details.All(x => x.TelemetrySummary.ClientAppUserId == response.UserId));
            Assert.IsTrue(details.First().DateTime < details.Last().DateTime);
      //      Assert.AreEqual(10100, new TelimenaSerializer().Deserialize<CustomDataObject>(details.Last().CustomUsageData.Data).SomeValue);

            Assert.AreEqual(3, this.Context.ViewUsageDetails.ToList().Count);
            Assert.AreEqual(2, this.Context.ViewUsageDetails.Count(x => x.TelemetrySummaryId == summary.Id));
        }

        [Test]
        public void TestMissingProgram()
        {
            StatisticsUpdateRequest request = new StatisticsUpdateRequest {ProgramId = 123123, UserId = 23};

            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());

            StatisticsController sut = new StatisticsController(unit);
            StatisticsUpdateResponse response = sut.Update(request).GetAwaiter().GetResult();
            Assert.IsTrue(response.Exception.Message.Contains($"Program [{request.ProgramId}] is null"));
        }

        [Test]
        public void TestMissingUser()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());

            unit.Programs.Add(new Program("SomeApp") {PrimaryAssembly = new ProgramAssembly {Name = "SomeApp.dll", Company = "SomeCompm"}});

            unit.CompleteAsync().GetAwaiter().GetResult();
            Program prg = unit.Programs.GetAsync(x => x.Name == "SomeApp").GetAwaiter().GetResult().FirstOrDefault();
            Assert.IsTrue(prg.Id > 0);

            StatisticsUpdateRequest request = new StatisticsUpdateRequest {ProgramId = prg.Id, UserId = 15646};

            StatisticsController sut = new StatisticsController(unit);
            StatisticsUpdateResponse response = sut.Update(request).GetAwaiter().GetResult();
            Assert.AreEqual($"User [{request.UserId}] is null", response.Exception.Message);
        }

        [Test]
        public void TestReferencedAssemblies_Add()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            StatisticsController sut = new StatisticsController(unit);
            Helpers.SeedInitialPrograms(sut, 4, Helpers.GetName("TestApp"), Helpers.GetName("Johny Walker"));
            Helpers.AddHelperAssemblies(this.Context, 2, Helpers.GetName("TestApp"));
            Helpers.GetProgramAndUser(this.Context, "TestApp", "Johny Walker", out Program prg, out ClientAppUser usr);

            Assert.AreEqual(3, prg.ProgramAssemblies.Count);
            Assert.AreEqual(Helpers.GetName("TestApp") + ".dll", prg.PrimaryAssembly.Name);
            Assert.IsNotNull(prg.ProgramAssemblies.Single(x => x.Name == Helpers.GetName("TestApp") + ".dll"));
            Assert.IsNotNull(prg.ProgramAssemblies.Single(x => x.Name == "HelperAss0_" + Helpers.GetName("TestApp") + ".dll"));
            Assert.IsNotNull(prg.ProgramAssemblies.Single(x => x.Name == "HelperAss1_" + Helpers.GetName("TestApp") + ".dll"));
        }

        [Test]
        public void TestReferencedAssembliesAddRemove()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            StatisticsController sut = new StatisticsController(unit);
            RegistrationRequest request = new RegistrationRequest
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg"))
                , TelimenaVersion = "1.0.0.0"
                , UserInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"))
            };
            RegistrationResponse result = sut.RegisterClient(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program prg, out ClientAppUser usr);

            prg.ProgramAssemblies.Add(new ProgramAssembly {Name = "Helper1"});
            prg.ProgramAssemblies.Add(new ProgramAssembly {Name = "Helper2"});
            this.Context.SaveChanges();

            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out prg, out usr);

            Assert.AreEqual(3, prg.ProgramAssemblies.Count);

            this.Context.Programs.Remove(prg);

            this.Context.SaveChanges();

            Assert.AreEqual(0, this.Context.ProgramAssemblies.Count(x => x.Program.Name == prg.Name));
            Assert.AreEqual(0, this.Context.Programs.Count(x => x.Name == prg.Name));
        }

        [Test]
        public void TestRegistration_DevIdProvided_DevFound()
        {
            TelimenaUser teliUser = new TelimenaUser("test12@me.now", Helpers.GetName("DevAccount Name"));
            this.Context.Users.Add(teliUser);
            this.Context.Developers.Add(new DeveloperAccount(teliUser));
            this.Context.SaveChanges();
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            int devId = unit.Developers.FirstOrDefault(x => x.MainEmail == teliUser.Email).Id;
            Assert.IsTrue(devId > 0);

            StatisticsController sut = new StatisticsController(unit);
            UserInfo userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));

            RegistrationRequest request = new RegistrationRequest
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")), TelimenaVersion = "1.0.0.0", UserInfo = userInfo
            };

            request.ProgramInfo.DeveloperId = devId;
            RegistrationResponse response = sut.RegisterClient(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program prg, out ClientAppUser usr);
            Helpers.AssertRegistrationResponse(response, prg, usr, 1);
            Assert.AreEqual(devId, prg.DeveloperAccount.Id);
            Assert.AreEqual(teliUser.Email, prg.DeveloperAccount.MainEmail);
        }

        [Test]
        public void TestRegistration_DevIdProvided_DevNotFound()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            StatisticsController sut = new StatisticsController(unit);
            UserInfo userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));

            RegistrationRequest request = new RegistrationRequest
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")), TelimenaVersion = "1.0.0.0", UserInfo = userInfo
            };
            request.ProgramInfo.DeveloperId = 1234123412;
            RegistrationResponse response = sut.RegisterClient(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program prg, out ClientAppUser usr);
            Helpers.AssertRegistrationResponse(response, prg, usr, 1);
            Assert.IsNull(prg.DeveloperAccount);
        }

        [Test]
        public void TestRegistration_SameAppEachTime()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            StatisticsController sut = new StatisticsController(unit);
            UserInfo userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));

            RegistrationRequest request = new RegistrationRequest
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")), TelimenaVersion = "1.0.0.0", UserInfo = userInfo
            };
            request.ProgramInfo.HelperAssemblies = new List<AssemblyInfo>
            {
                new AssemblyInfo {Name = "Helper_" + Helpers.GetName("TestProg") + ".dll", Version = "0.0.0.1"}
            };
            RegistrationResponse response = sut.RegisterClient(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program prg, out ClientAppUser usr);
            Helpers.AssertRegistrationResponse(response, prg, usr, 1);
            Assert.AreEqual(1, prg.PrimaryAssembly.Versions.Count);
            Assert.AreEqual(2, prg.ProgramAssemblies.Count);
            ProgramAssembly helper = prg.ProgramAssemblies.Single(x => x.Name == "Helper_" + Helpers.GetName("TestProg") + ".dll");
            Assert.AreEqual("0.0.0.1", helper.GetLatestVersion().Version);
            Assert.AreEqual(1, helper.Versions.Count);

            Assert.AreEqual("1.2.3.4", prg.PrimaryAssembly.GetLatestVersion().Version);
            Assert.AreEqual("1.2.3.4", prg.GetLatestVersion().Version);
            Assert.AreEqual(unit.Versions.Single(x => x.ProgramAssemblyId == prg.PrimaryAssembly.Id).Id, prg.PrimaryAssembly.GetLatestVersion().Id);
            int firstVersionId = prg.PrimaryAssembly.GetLatestVersion().Id;

            //second time
            response = sut.RegisterClient(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out prg, out usr);
            Helpers.AssertRegistrationResponse(response, prg, usr, 2);
            Assert.AreEqual(1, prg.PrimaryAssembly.Versions.Count);
            Assert.AreEqual("1.2.3.4", prg.PrimaryAssembly.GetLatestVersion().Version);
            Assert.AreEqual(unit.Versions.Single(x => x.ProgramAssemblyId == prg.PrimaryAssembly.Id).Id, prg.PrimaryAssembly.GetLatestVersion().Id);


            //third time - different version
            request = new RegistrationRequest
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg"), version: "2.0.0.0"), TelimenaVersion = "1.0.0.0", UserInfo = userInfo
            };
            response = sut.RegisterClient(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out prg, out usr);
            Helpers.AssertRegistrationResponse(response, prg, usr, 3);
            Assert.AreEqual("2.0.0.0", prg.PrimaryAssembly.GetLatestVersion().Version);
            //Assert.AreEqual("1.2.3.4", prg.PrimaryAssembly.GetLatestVersion().Version);
            Assert.AreEqual(unit.Versions.Single(x => x.ProgramAssemblyId == prg.PrimaryAssembly.Id && x.Version == "2.0.0.0").Id
                , prg.PrimaryAssembly.GetLatestVersion().Id);


            int latestId = prg.PrimaryAssembly.GetLatestVersion().Id;

            Assert.AreEqual(2, prg.PrimaryAssembly.Versions.Count);

            //fourth time - use first version again
            request = new RegistrationRequest
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")), TelimenaVersion = "1.0.0.0", UserInfo = userInfo
            };
            response = sut.RegisterClient(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out prg, out usr);
            Helpers.AssertRegistrationResponse(response, prg, usr, 4);
            Assert.AreEqual(latestId, prg.PrimaryAssembly.GetLatestVersion().Id);
            Assert.AreEqual(latestId, prg.GetLatestVersion().Id);
            Assert.AreEqual(firstVersionId, prg.PrimaryAssembly.GetVersion(request.ProgramInfo.PrimaryAssembly.Version, null).Id);

            Assert.AreEqual(2, prg.PrimaryAssembly.Versions.Count);
        }

        [Test]
        public void TestRegistration_SameAppEachTime_ValidateHelperAssemblies()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            StatisticsController sut = new StatisticsController(unit);
            UserInfo userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));

            RegistrationRequest request = new RegistrationRequest
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")), TelimenaVersion = "1.0.0.0", UserInfo = userInfo
            };
            request.ProgramInfo.HelperAssemblies = new List<AssemblyInfo>
            {
                new AssemblyInfo {Name = "Helper_" + Helpers.GetName("TestProg") + ".dll", Version = "0.0.0.1"}
            };
            RegistrationResponse response = sut.RegisterClient(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program prg, out ClientAppUser usr);
            Helpers.AssertRegistrationResponse(response, prg, usr, 1);
            Assert.AreEqual(2, prg.ProgramAssemblies.Count);
            ProgramAssembly helper = prg.ProgramAssemblies.Single(x => x.Name == "Helper_" + Helpers.GetName("TestProg") + ".dll");
            Assert.AreEqual("0.0.0.1", helper.GetLatestVersion().Version);
            Assert.AreEqual(1, helper.Versions.Count);


            //second time
            request.ProgramInfo.HelperAssemblies = new List<AssemblyInfo>
            {
                new AssemblyInfo {Name = "Helper2_" + Helpers.GetName("TestProg") + ".dll", Version = "0.0.2.2"}
            };
            response = sut.RegisterClient(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out prg, out usr);
            Helpers.AssertRegistrationResponse(response, prg, usr, 2);
            Assert.AreEqual(3, prg.ProgramAssemblies.Count);
            helper = prg.ProgramAssemblies.Single(x => x.Name == "Helper_" + Helpers.GetName("TestProg") + ".dll");
            ProgramAssembly helper2 = prg.ProgramAssemblies.Single(x => x.Name == "Helper2_" + Helpers.GetName("TestProg") + ".dll");
            Assert.AreEqual("0.0.0.1", helper.GetLatestVersion().Version);
            Assert.AreEqual(1, helper.Versions.Count);
            Assert.AreEqual("0.0.2.2", helper2.GetLatestVersion().Version);
            Assert.AreEqual(1, helper2.Versions.Count);


            //third time - different version
            request = new RegistrationRequest
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg"), version: "2.0.0.0"), TelimenaVersion = "1.0.0.0", UserInfo = userInfo
            };
            request.ProgramInfo.HelperAssemblies = new List<AssemblyInfo>
            {
                new AssemblyInfo {Name = "Helper_" + Helpers.GetName("TestProg") + ".dll", Version = "0.3.0.1"} //newer version of helper!
            };
            response = sut.RegisterClient(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out prg, out usr);
            Helpers.AssertRegistrationResponse(response, prg, usr, 3);

            Assert.AreEqual(3, prg.ProgramAssemblies.Count);
            helper = prg.ProgramAssemblies.Single(x => x.Name == "Helper_" + Helpers.GetName("TestProg") + ".dll");
            helper2 = prg.ProgramAssemblies.Single(x => x.Name == "Helper2_" + Helpers.GetName("TestProg") + ".dll");
            Assert.AreEqual("0.3.0.1", helper.GetLatestVersion().Version);
            Assert.AreEqual(2, helper.Versions.Count);
            Assert.AreEqual("0.0.2.2", helper2.GetLatestVersion().Version);
            Assert.AreEqual(1, helper2.Versions.Count);
            Assert.AreEqual("2.0.0.0", prg.GetLatestVersion().Version);


            //fourth time - do not register helpers anymore
            request = new RegistrationRequest
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")), TelimenaVersion = "1.0.0.0", UserInfo = userInfo
            };
            response = sut.RegisterClient(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out prg, out usr);
            Helpers.AssertRegistrationResponse(response, prg, usr, 4);
            Assert.AreEqual(3, prg.ProgramAssemblies.Count);

            helper = prg.ProgramAssemblies.Single(x => x.Name == "Helper_" + Helpers.GetName("TestProg") + ".dll");
            helper2 = prg.ProgramAssemblies.Single(x => x.Name == "Helper2_" + Helpers.GetName("TestProg") + ".dll");
            Assert.AreEqual("0.3.0.1", helper.GetLatestVersion().Version);
            Assert.AreEqual(2, helper.Versions.Count);
            Assert.AreEqual("0.0.2.2", helper2.GetLatestVersion().Version);
            Assert.AreEqual(1, helper2.Versions.Count);
            Assert.AreEqual("2.0.0.0", prg.GetLatestVersion().Version);
        }

        [Test]
        public void TestRegistration_SameAppTwoUsers_DifferentVersions()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            StatisticsController sut = new StatisticsController(unit);
            UserInfo oldGuyUserInfo = Helpers.GetUserInfo(Helpers.GetName("OldGuy"));
            UserInfo newGuyUserInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));

            RegistrationRequest oldGuyRequest = new RegistrationRequest
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")), TelimenaVersion = "1.0.0.0", UserInfo = oldGuyUserInfo
            };
            RegistrationRequest newGuyRequest = new RegistrationRequest
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg"), version: "6.0"), TelimenaVersion = "1.0.0.0", UserInfo = newGuyUserInfo
            };

            RegistrationResponse response = sut.RegisterClient(oldGuyRequest).GetAwaiter().GetResult();
            sut.RegisterClient(newGuyRequest).GetAwaiter().GetResult();
            sut.RegisterClient(oldGuyRequest).GetAwaiter().GetResult();
            sut.RegisterClient(newGuyRequest).GetAwaiter().GetResult();
            sut.RegisterClient(oldGuyRequest).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program prg, out ClientAppUser usr);

            Assert.AreEqual(2, prg.PrimaryAssembly.Versions.Count);
            Assert.AreEqual(1, prg.ProgramAssemblies.Count);
            Assert.IsTrue(prg.PrimaryAssembly.Versions.Any(x => x.Version == "1.2.3.4"));
            Assert.IsTrue(prg.PrimaryAssembly.Versions.Any(x => x.Version == "6.0"));
            Assert.AreEqual(5, prg.UsageSummaries.Sum(x => x.SummaryCount));
            Assert.AreEqual(2, prg.UsageSummaries.Count);
            Assert.AreEqual(5, prg.UsageSummaries.Sum(x => x.GetTelemetryDetails().Count()));
        }

        private void SetIp(ApiController controller, string ip)
        {
            var request = new Mock<HttpRequestBase>();
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
        public void TestIp_Ok()
        {
            TelemetryUnitOfWork unit;
            StatisticsController sut;
            RegistrationRequest request = null;
            UserInfo userInfo;
            for (int i = 0; i <= 2; i++)
            {

                unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
                 sut = new StatisticsController(unit);

                SetIp(sut, "1.2.3.4");
                userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));

                request = new RegistrationRequest
                {
                    ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")), TelimenaVersion = "1.0.0.0", UserInfo = userInfo
                };

                RegistrationResponse response = sut.RegisterClient(request).GetAwaiter().GetResult();
                Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program prg, out ClientAppUser usr);

                Assert.AreEqual("1.2.3.4", usr.IpAddresses.Single());

            }

            unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            sut = new StatisticsController(unit);

            SetIp(sut, "2.2.3.4");
            userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));


            sut.RegisterClient(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program _, out ClientAppUser usr2);

            Assert.AreEqual("1.2.3.4", usr2.IpAddresses[0]);
            Assert.AreEqual("2.2.3.4", usr2.IpAddresses[1]);

        }

        [Test]
        public void TestRegistration_SameUserTwoApps_HappyPath()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            StatisticsController sut = new StatisticsController(unit);
            UserInfo userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));

            RegistrationRequest request = new RegistrationRequest
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")), TelimenaVersion = "1.0.0.0", UserInfo = userInfo
            };
            RegistrationResponse response = sut.RegisterClient(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program prg, out ClientAppUser usr);

            Assert.IsTrue(prg.Id > 0 && usr.Id > 0);
            Helpers.AssertRegistrationResponse(response, prg, usr, 1);

            Assert.AreEqual(1, prg.UsageSummaries.Count());
            Assert.AreEqual(request.ProgramInfo.PrimaryAssembly.Name, prg.PrimaryAssembly.Name);
            Assert.AreEqual("xyz", prg.PrimaryAssembly.Company);
            Assert.AreEqual(prg.Id, this.Context.ProgramAssemblies.FirstOrDefault(x => x.Name == request.ProgramInfo.PrimaryAssembly.Name).Program.Id);

            Assert.AreEqual(userInfo.UserName, usr.UserName);
            Assert.AreEqual(request.UserInfo.MachineName, usr.MachineName);
            Assert.AreEqual(request.UserInfo.UserName, usr.UserName);
            Assert.AreEqual(usr.Id, prg.UsageSummaries.Single().ClientAppUserId);

            string prgName2 = Helpers.GetName("TestProg2");
            //now second app for same user
            request = new RegistrationRequest {ProgramInfo = Helpers.GetProgramInfo(prgName2), TelimenaVersion = "1.0.0.0", UserInfo = userInfo};

            response = sut.RegisterClient(request).GetAwaiter().GetResult();
            Program prg2 = unit.Programs.SingleOrDefaultAsync(x => x.Name == prgName2).GetAwaiter().GetResult();

            Helpers.AssertRegistrationResponse(response, prg2, usr, 1);

            Assert.IsTrue(prg2.Id > prg.Id);
            Assert.AreEqual(usr.Id, prg2.UsageSummaries.Single().ClientAppUserId);
        }

        [Test]
        public void TestRegistration_SkipUsageIncremet()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            StatisticsController sut = new StatisticsController(unit);
            UserInfo userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));

            RegistrationRequest request = new RegistrationRequest
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg"))
                , TelimenaVersion = "1.0.0.0"
                , UserInfo = userInfo
                , SkipUsageIncrementation = true
            };
            RegistrationResponse response = sut.RegisterClient(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program prg, out ClientAppUser usr);
            Helpers.AssertRegistrationResponse(response, prg, usr, 0);
            Assert.AreEqual(1, prg.UsageSummaries.Count);
            Assert.AreEqual(0, prg.UsageSummaries.Sum(x => x.GetTelemetryDetails().Count()));
        }

        [Test]
        public void TestUpdateAction()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            StatisticsController sut = new StatisticsController(unit);
            Helpers.SeedInitialPrograms(sut, 4, Helpers.GetName("TestApp"), Helpers.GetName("Johny Walker"));
            Helpers.SeedInitialPrograms(sut, 4, Helpers.GetName("TestApp"), Helpers.GetName("Jim Beam"));
            Helpers.SeedInitialPrograms(sut, 4, Helpers.GetName("TestApp"), Helpers.GetName("Eric Cartman"));
            Helpers.AddHelperAssemblies(this.Context, 2, Helpers.GetName("TestApp"));

            Helpers.GetProgramAndUser(this.Context, "TestApp3", "Jim Beam", out Program prg, out ClientAppUser usr);
            StatisticsUpdateRequest request = new StatisticsUpdateRequest {ProgramId = prg.Id, UserId = usr.Id, AssemblyVersion = "1.2.3.4"};

            StatisticsUpdateResponse response = sut.Update(request).GetAwaiter().GetResult();
            Assert.IsTrue(prg.Id > 0 && usr.Id > 0);

            Helpers.AssertUpdateResponse(response, prg, usr, 2, null);

            Assert.AreEqual(3, prg.UsageSummaries.Count);
            var summary = prg.UsageSummaries.SingleOrDefault(x => x.ClientAppUser.UserName == "TestUpdateAction_Jim Beam");
            Assert.AreEqual(2, summary.SummaryCount);

            Assert.AreEqual(summary.GetTelemetryDetails().Last().AssemblyVersionId, prg.PrimaryAssembly.GetVersion(request.AssemblyVersion, null).Id);


            //run again
            response = sut.Update(request).GetAwaiter().GetResult();

            Helpers.AssertUpdateResponse(response, prg, usr, 3, null);

            Assert.AreEqual(3, prg.UsageSummaries.Count);
            Assert.AreEqual(3, prg.GetProgramUsageSummary(response.UserId).SummaryCount);
            Assert.AreEqual(3, prg.UsageSummaries.Single(x => x.ClientAppUser.UserName == "TestUpdateAction_Jim Beam").SummaryCount);

            Assert.AreEqual(3, prg.GetProgramUsageDetails(response.UserId).Count);
            Assert.AreEqual(5, this.Context.ProgramUsageDetails.Count(x => x.TelemetrySummary.ProgramId == prg.Id));
            Assert.AreEqual(summary.GetTelemetryDetails().Last().AssemblyVersionId, prg.PrimaryAssembly.GetVersion(request.AssemblyVersion, null).Id);
        }

        [Test]
        public void TestUsageDetailsAndVersions()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            StatisticsController sut = new StatisticsController(unit);
            Helpers.SeedInitialPrograms(sut, 2, Helpers.GetName("TestApp"), Helpers.GetName("Billy Jean"));
            Helpers.SeedInitialPrograms(sut, 2, Helpers.GetName("TestApp"), Helpers.GetName("Jack Black"));

            Helpers.GetProgramAndUser(this.Context, "TestApp", "Billy Jean", out Program prg, out ClientAppUser usr);
            Helpers.GetProgramAndUser(this.Context, "TestApp", "Jack Black", out prg, out ClientAppUser usr2);

            ProgramTelemetryDetail detail = prg.GetLatestUsageDetail();
            Assert.AreEqual(usr2.Id, detail.TelemetrySummary.ClientAppUserId);
            //Assert.AreEqual(null, detail.CustomUsageData);
            Assert.AreEqual("1.2.3.4", detail.AssemblyVersion.Version);

            StatisticsUpdateRequest request = new StatisticsUpdateRequest {ProgramId = prg.Id, UserId = usr.Id, AssemblyVersion = "1.2.3.4"};

            StatisticsUpdateResponse response = sut.Update(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestApp", "Billy Jean", out prg, out usr);

            Assert.AreEqual(3, prg.UsageSummaries.Sum(x => x.GetTelemetryDetails().Count()));
            Assert.AreEqual(2, prg.GetProgramUsageDetails(usr.Id).Count);
            Assert.AreEqual(1, prg.GetProgramUsageDetails(usr2.Id).Count);
            Assert.IsTrue(prg.GetProgramUsageDetails(usr.Id).All(x => x.AssemblyVersion.Version == "1.2.3.4"));
            Assert.IsTrue(prg.GetProgramUsageDetails(usr2.Id).All(x => x.AssemblyVersion.Version == "1.2.3.4"));

            Assert.AreEqual(1, this.Context.Versions.Count(x => x.ProgramAssembly.ProgramId == prg.Id));

            detail = prg.GetLatestUsageDetail();
            Assert.AreEqual(usr.Id, detail.TelemetrySummary.ClientAppUserId);
            Assert.AreEqual("1.2.3.4", detail.AssemblyVersion.Version);

            request = new StatisticsUpdateRequest {ProgramId = prg.Id, UserId = usr.Id, AssemblyVersion = "2.0.0.0"};
            response = sut.Update(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestApp", "Billy Jean", out prg, out usr);

            Assert.AreEqual(4, prg.UsageSummaries.Sum(x => x.GetTelemetryDetails().Count()));
            Assert.AreEqual(3, prg.GetProgramUsageDetails(usr.Id).Count);
            Assert.AreEqual(1, prg.GetProgramUsageDetails(usr2.Id).Count);
            Assert.AreEqual(2, prg.GetProgramUsageDetails(usr.Id).Count(x => x.AssemblyVersion.Version == "1.2.3.4"));
            Assert.AreEqual(1, prg.GetProgramUsageDetails(usr.Id).Count(x => x.AssemblyVersion.Version == "2.0.0.0"));
            Assert.IsTrue(prg.GetProgramUsageDetails(usr2.Id).All(x => x.AssemblyVersion.Version == "1.2.3.4"));

            Assert.AreEqual(2, this.Context.Versions.Count(x => x.ProgramAssembly.ProgramId == prg.Id));


            detail = prg.GetLatestUsageDetail();
            Assert.AreEqual(usr.Id, detail.TelemetrySummary.ClientAppUserId);
            Assert.AreEqual("2.0.0.0", detail.AssemblyVersion.Version);
        }


    
    }
}