// -----------------------------------------------------------------------
//  <copyright file="StatisticsControllerTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Telimena.Tests
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using System.Security.Claims;
    using System.Security.Principal;
    using System.Threading;
    using System.Web.Http.Results;
    using Client;
    using DbIntegrationTestHelpers;
    using NUnit.Framework;
    using WebApi.Controllers;
    using WebApp.Core.Interfaces;
    using WebApp.Core.Messages;
    using WebApp.Core.Models;
    using WebApp.Infrastructure.Database;
    using WebApp.Infrastructure.UnitOfWork.Implementation;
    #endregion


    [TestFixture]
    public class ProgramsControllerTests : StaticContextIntegrationTestsBase<TelimenaContext>
    {
        protected override Action SeedAction => () => TelimenaDbInitializer.SeedUsers(this.Context);

        [Test]
        public void TestGetSetLatestVersion_AllOk()
        {
            ProgramsUnitOfWork unit = new ProgramsUnitOfWork(this.Context);
            ProgramsController sut = new ProgramsController(unit);
            Helpers.SeedInitialPrograms(this.Context,1, "TestApp", "NewGuy");
            Helpers.AddHelperAssemblies(this.Context, 1, Helpers.GetName("TestApp"));

            Helpers.GetProgramAndUser(this.Context, "TestApp", "NewGuy", out Program prg, out ClientAppUser usr);
            Assert.AreEqual("1.2.3.4", prg.PrimaryAssembly.LatestVersion.Version);
            var request = new SetLatestVersionRequest()
            {
                ProgramId = prg.ProgramId,
                Version = "2.2.0.0"
            };

            var result = sut.SetLatestVersion(request).GetAwaiter().GetResult();
            Assert.AreEqual("2.2.0.0", prg.PrimaryAssembly.LatestVersion.Version);
            Assert.IsInstanceOf<OkResult>(result);

            var latestVersionResponse = sut.GetLatestVersionInfo(request.ProgramId).GetAwaiter().GetResult();
            Assert.AreEqual("2.2.0.0", latestVersionResponse.PrimaryAssemblyVersion.LatestVersion);
            Assert.IsNull(latestVersionResponse.Error);
            Assert.AreEqual("1.0.0.0", latestVersionResponse.LatestTelimenaVersion);
            Assert.AreEqual(prg.ProgramAssemblies.Single(x=>x.PrimaryOf == null).ProgramAssemblyId, latestVersionResponse.HelperAssemblyVersions.Single().AssemblyId);
            Assert.AreEqual("0.0.1.0", latestVersionResponse.HelperAssemblyVersions.Single().LatestVersion);


            request = new SetLatestVersionRequest()
            {
                ProgramId = prg.ProgramId,
                Version = "1.2.3.4"
            };
            result = sut.SetLatestVersion(request).GetAwaiter().GetResult();
            Assert.AreEqual("1.2.3.4", prg.PrimaryAssembly.LatestVersion.Version);
            Assert.IsInstanceOf<OkResult>(result);

            latestVersionResponse = sut.GetLatestVersionInfo(request.ProgramId).GetAwaiter().GetResult();
            Assert.AreEqual("1.2.3.4", latestVersionResponse.PrimaryAssemblyVersion.LatestVersion);
        }

        [Test]
        public void TestSetLatestVersion_RequestNull()
        {
            ProgramsUnitOfWork unit = new ProgramsUnitOfWork(this.Context);
            ProgramsController sut = new ProgramsController(unit);

            var result = sut.SetLatestVersion(null).GetAwaiter().GetResult();
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(result);
            Assert.AreEqual("SetLatestVersionRequest is invalid", (result as BadRequestErrorMessageResult).Message);
        }

        [Test]
        public void TestSetLatestVersion_PrgNotFound()
        {
            ProgramsUnitOfWork unit = new ProgramsUnitOfWork(this.Context);
            ProgramsController sut = new ProgramsController(unit);
           var request = new SetLatestVersionRequest()
            {
                ProgramId = 564265416,
                Version = "1.2.3.4"
            };


            var result = sut.SetLatestVersion(request).GetAwaiter().GetResult();
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(result);
            Assert.AreEqual("Program [564265416] not found", (result as BadRequestErrorMessageResult).Message);
        }

        

        [Test]
        public void TestSetLatestVersion_VersionFormatInvalid()
        {
            ProgramsUnitOfWork unit = new ProgramsUnitOfWork(this.Context);
            ProgramsController sut = new ProgramsController(unit);
            var request = new SetLatestVersionRequest()
            {
                ProgramId = 1,
                Version = "notValid"
            };

            var result = sut.SetLatestVersion(request).GetAwaiter().GetResult();
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(result);
            Assert.AreEqual("Version [notValid] is not in valid format. Expected e.g. 1.0.0.0", (result as BadRequestErrorMessageResult).Message);

            request = new SetLatestVersionRequest()
            {
                ProgramId = 1,
            };

            result = sut.SetLatestVersion(request).GetAwaiter().GetResult();
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(result);
            Assert.AreEqual("Version [] is not in valid format. Expected e.g. 1.0.0.0", (result as BadRequestErrorMessageResult).Message);

        }

        
    }
}