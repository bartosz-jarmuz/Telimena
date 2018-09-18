// -----------------------------------------------------------------------
//  <copyright file="StatisticsControllerTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Web.Http;
using System.Web.Http.Results;
using DbIntegrationTestHelpers;
using Microsoft.AspNet.Identity.EntityFramework;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Api;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;

namespace Telimena.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class ProgramVersionsControllerTests : IntegrationTestsContextSharedGlobally<TelimenaContext>
    {
        protected override Action SeedAction => () => TelimenaDbInitializer.SeedUsers(this.Context);

        [Test]
        public void TestGetSetLatestVersion_AllOk()
        {
            ProgramsUnitOfWork unit = new ProgramsUnitOfWork(this.Context, new TelimenaUserManager(new UserStore<TelimenaUser>(this.Context)));

            ProgramVersionsController sut = new ProgramVersionsController(unit);
            Helpers.SeedInitialPrograms(this.Context, 1, "TestApp", "NewGuy");
            Helpers.AddHelperAssemblies(this.Context, 1, Helpers.GetName("TestApp"));

            Helpers.GetProgramAndUser(this.Context, "TestApp", "NewGuy", out Program prg, out ClientAppUser usr);
            Assert.AreEqual("1.2.3.4", prg.PrimaryAssembly.LatestVersion.Version);
            SetLatestVersionRequest request = new SetLatestVersionRequest {ProgramId = prg.Id, Version = "2.2.0.0"};

            IHttpActionResult result = sut.SetLatestVersion(request).GetAwaiter().GetResult();
            Assert.AreEqual("2.2.0.0", prg.PrimaryAssembly.LatestVersion.Version);
            Assert.IsInstanceOf<OkResult>(result);

            LatestVersionResponse latestVersionResponse = sut.GetLatestVersionInfo(request.ProgramId).GetAwaiter().GetResult();
            Assert.AreEqual("2.2.0.0", latestVersionResponse.PrimaryAssemblyVersion.LatestVersion);
            Assert.IsNull(latestVersionResponse.Error);
            // todo Assert.AreEqual("1.0.0.0", latestVersionResponse.LatestTelimenaVersion); 
            Assert.AreEqual(prg.ProgramAssemblies.Single(x => x.PrimaryOf == null).Id, latestVersionResponse.HelperAssemblyVersions.Single().AssemblyId);
            Assert.AreEqual("0.0.1.0", latestVersionResponse.HelperAssemblyVersions.Single().LatestVersion);


            request = new SetLatestVersionRequest {ProgramId = prg.Id, Version = "1.2.3.4"};
            result = sut.SetLatestVersion(request).GetAwaiter().GetResult();
            Assert.AreEqual("1.2.3.4", prg.PrimaryAssembly.LatestVersion.Version);
            Assert.IsInstanceOf<OkResult>(result);

            latestVersionResponse = sut.GetLatestVersionInfo(request.ProgramId).GetAwaiter().GetResult();
            Assert.AreEqual("1.2.3.4", latestVersionResponse.PrimaryAssemblyVersion.LatestVersion);
        }

        [Test]
        public void TestSetLatestVersion_PrgNotFound()
        {
            ProgramsUnitOfWork unit = new ProgramsUnitOfWork(this.Context, new TelimenaUserManager(new UserStore<TelimenaUser>(this.Context)));

            ProgramVersionsController sut = new ProgramVersionsController(unit);
            SetLatestVersionRequest request = new SetLatestVersionRequest {ProgramId = 564265416, Version = "1.2.3.4"};


            IHttpActionResult result = sut.SetLatestVersion(request).GetAwaiter().GetResult();
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(result);
            Assert.AreEqual("Program [564265416] not found", (result as BadRequestErrorMessageResult).Message);
        }

        [Test]
        public void TestSetLatestVersion_RequestNull()
        {
            ProgramsUnitOfWork unit = new ProgramsUnitOfWork(this.Context, new TelimenaUserManager(new UserStore<TelimenaUser>(this.Context)));

            ProgramVersionsController sut = new ProgramVersionsController(unit);

            IHttpActionResult result = sut.SetLatestVersion(null).GetAwaiter().GetResult();
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(result);
            Assert.AreEqual("SetLatestVersionRequest is invalid", (result as BadRequestErrorMessageResult).Message);
        }

        [Test]
        public void TestSetLatestVersion_VersionFormatInvalid()
        {
            ProgramsUnitOfWork unit = new ProgramsUnitOfWork(this.Context, new TelimenaUserManager(new UserStore<TelimenaUser>(this.Context)));

            ProgramVersionsController sut = new ProgramVersionsController(unit);
            SetLatestVersionRequest request = new SetLatestVersionRequest {ProgramId = 1, Version = "notValid"};

            IHttpActionResult result = sut.SetLatestVersion(request).GetAwaiter().GetResult();
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(result);
            Assert.AreEqual("Version [notValid] is not in valid format. Expected e.g. 1.0.0.0", (result as BadRequestErrorMessageResult).Message);

            request = new SetLatestVersionRequest {ProgramId = 1};

            result = sut.SetLatestVersion(request).GetAwaiter().GetResult();
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(result);
            Assert.AreEqual("Version [] is not in valid format. Expected e.g. 1.0.0.0", (result as BadRequestErrorMessageResult).Message);
        }
    }
}