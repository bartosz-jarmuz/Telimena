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
    [TestFixture]
    public class TelemetryControllerTests : IntegrationTestsContextSharedGlobally<TelimenaContext>
    {
        protected override Action SeedAction => () => TelimenaDbInitializer.SeedUsers(this.Context);

        private readonly TelimenaSerializer serializer = new TelimenaSerializer();

        [Test]
        public async Task TestMissingProgram()
        {
            TelemetryInitializeRequest request = new TelemetryInitializeRequest(Guid.NewGuid()) { };

            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());

            TelemetryController sut = new TelemetryController(unit);
            TelemetryInitializeResponse response = await sut.Initialize(request).ConfigureAwait(false);
            Assert.IsTrue(response.Exception.Message.Contains($"Program [{request.TelemetryKey}] is null"));
        }

        [Test]
        public async Task TestRegistration_SameAppEachTime()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            TelemetryController sut = new TelemetryController(unit);
            UserInfo userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));

            List<KeyValuePair<string, Guid>> apps = await Helpers.SeedInitialPrograms(this.Context, 4, "TestProg", new[] { "NewGuy" }).ConfigureAwait(false);

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

            TelemetryInitializeResponse response = await sut.Initialize(request).ConfigureAwait(false);
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
            response = await  sut.Initialize(request).ConfigureAwait(false);
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
            response = await sut.Initialize(request).ConfigureAwait(false);
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
            response = await sut.Initialize(request).ConfigureAwait(false);
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out prg, out usr);
            Helpers.AssertRegistrationResponse(response, prg, usr, 5);
            Assert.AreEqual(latestId, prg.PrimaryAssembly.GetLatestVersion().Id);
            Assert.AreEqual(latestId, prg.GetLatestVersion().Id);
            Assert.AreEqual(firstVersionId, prg.PrimaryAssembly.GetVersion(request.ProgramInfo.PrimaryAssembly.VersionData ).Id);

            Assert.AreEqual(2, prg.PrimaryAssembly.Versions.Count);
        }

        [Test]
        public async Task TestRegistration_SameAppEachTime_ValidateHelperAssemblies()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            TelemetryController sut = new TelemetryController(unit);
            List<KeyValuePair<string, Guid>> apps = await Helpers.SeedInitialPrograms(this.Context, 1, "TestProg", new string[0] ).ConfigureAwait(false);
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
            TelemetryInitializeResponse response = await sut.Initialize(request).ConfigureAwait(false);
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
            response = await sut.Initialize(request).ConfigureAwait(false);
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
            response = await sut.Initialize(request).ConfigureAwait(false);
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
            response =  await sut.Initialize(request).ConfigureAwait(false);
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
            List<KeyValuePair<string, Guid>> apps = await Helpers.SeedInitialPrograms(this.Context, 1, "TestProg", new string[0]).ConfigureAwait(false);

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

            TelemetryInitializeResponse response = await sut.Initialize(oldGuyRequest).ConfigureAwait(false);
            await sut.Initialize(newGuyRequest).ConfigureAwait(false);
            await sut.Initialize(oldGuyRequest).ConfigureAwait(false);
            await sut.Initialize(newGuyRequest).ConfigureAwait(false);
            await sut.Initialize(oldGuyRequest).ConfigureAwait(false);
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program prg, out ClientAppUser usr);

            Assert.AreEqual(2, prg.PrimaryAssembly.Versions.Count);
            Assert.AreEqual(1, prg.ProgramAssemblies.Count);
            Assert.IsTrue(prg.PrimaryAssembly.Versions.Any(x => x.AssemblyVersion == "1.2.3.4"));
            Assert.IsTrue(prg.PrimaryAssembly.Versions.Any(x => x.AssemblyVersion == "6.0"));
        }

        [Test]
        public async Task TestIp_Ok()
        {
            TelemetryUnitOfWork unit;
            TelemetryController sut;
            TelemetryInitializeRequest request = null;
            UserInfo userInfo;
            List<KeyValuePair<string, Guid>> apps = await Helpers.SeedInitialPrograms(this.Context, 1, "TestProg", new string[0]).ConfigureAwait(false);

            for (int i = 0; i <= 2; i++)
            {

                unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
                sut = new TelemetryController(unit);

                Helpers.SetIp(sut, "1.2.3.4");
                userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));

                request = new TelemetryInitializeRequest(apps[0].Value)
                {
                    ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")),
                    TelimenaVersion = "1.0.0.0",
                    UserInfo = userInfo
                };

                TelemetryInitializeResponse response = await sut.Initialize(request).ConfigureAwait(false);
                Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program prg, out ClientAppUser usr);

                Assert.AreEqual("1.2.3.4", usr.IpAddresses.Single());

            }

            unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            sut = new TelemetryController(unit);

            Helpers.SetIp(sut, "2.2.3.4");
            userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));


            await sut.Initialize(request).ConfigureAwait(false);
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
            List<KeyValuePair<string, Guid>> apps = await Helpers.SeedInitialPrograms(this.Context, 1, "TestProg", new string[0]).ConfigureAwait(false);
            List<KeyValuePair<string, Guid>> apps2 = await Helpers.SeedInitialPrograms(this.Context, 1, "TestProg2", new string[0]).ConfigureAwait(false);


            TelemetryInitializeRequest request = new TelemetryInitializeRequest(apps[0].Value)
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")),
                TelimenaVersion = "1.0.0.0",
                UserInfo = userInfo
            };
            TelemetryInitializeResponse response = await sut.Initialize(request).ConfigureAwait(false);
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

            response = await sut.Initialize(request).ConfigureAwait(false);
            Program prg2 = await unit.Programs.SingleOrDefaultAsync(x => x.Name == prgName2).ConfigureAwait(false);

            Helpers.AssertRegistrationResponse(response, prg2, usr, 1);

            Assert.IsTrue(prg2.Id > prg.Id);
        }

        [Test]
        public async Task TestVersions()
        {
            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            TelemetryController sut = new TelemetryController(unit);
            List<KeyValuePair<string, Guid>> apps = await Helpers.SeedInitialPrograms(this.Context, 2, "TestApp", new[] { "Billy Jean", "Jack Black" }).ConfigureAwait(false);

            Helpers.GetProgramAndUser(this.Context, "TestApp", "Billy Jean", out Program prg, out ClientAppUser usr);
            Helpers.GetProgramAndUser(this.Context, "TestApp", "Jack Black", out prg, out ClientAppUser usr2);

            TelemetryInitializeRequest request = new TelemetryInitializeRequest(prg.TelemetryKey) {
                UserInfo = Helpers.GetUserInfo(Helpers.GetName("Billy Jean")),
                ProgramInfo = Helpers.GetProgramInfo("TestApp",version: new VersionData("1.2.3.4", "2.2.3.4")) };

            await sut.Initialize(request).ConfigureAwait(false);
            Helpers.GetProgramAndUser(this.Context, "TestApp", "Billy Jean", out prg, out usr);


            Assert.AreEqual(1, this.Context.Versions.Count(x => x.ProgramAssembly.ProgramId == prg.Id));

            request = new TelemetryInitializeRequest(prg.TelemetryKey)
            {
                UserInfo = Helpers.GetUserInfo(Helpers.GetName("Billy Jean")),
                ProgramInfo = Helpers.GetProgramInfo("TestApp", version: new VersionData("2.0.0.0", "3.0.0.0"))
            };

            await sut.Initialize(request).ConfigureAwait(false);
            Helpers.GetProgramAndUser(this.Context, "TestApp", "Billy Jean", out prg, out usr);

            Assert.AreEqual(2, this.Context.Versions.Count(x => x.ProgramAssembly.ProgramId == prg.Id));

        }

    }
}