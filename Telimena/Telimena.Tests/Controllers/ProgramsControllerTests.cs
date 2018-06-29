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
            Helpers.SeedInitialPrograms(this.Context,2, "TestApp", "NewGuy");
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

            result = sut.GetLatestVersion(request.ProgramId).GetAwaiter().GetResult();
            Assert.AreEqual("2.2.0.0", (result as OkNegotiatedContentResult<string>).Content);


            request = new SetLatestVersionRequest()
            {
                ProgramId = prg.ProgramId,
                Version = "1.2.3.4"
            };
            result = sut.SetLatestVersion(request).GetAwaiter().GetResult();
            Assert.AreEqual("1.2.3.4", prg.PrimaryAssembly.LatestVersion.Version);
            Assert.IsInstanceOf<OkResult>(result);

            result = sut.GetLatestVersion(request.ProgramId).GetAwaiter().GetResult();
            Assert.AreEqual("1.2.3.4", (result as OkNegotiatedContentResult<string>).Content);
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

        [Test]
        public void TestRegisterProgram_PrgNotFound()
        {
            ProgramsUnitOfWork unit = new ProgramsUnitOfWork(this.Context);
            ProgramsController sut = new ProgramsController(unit);


            var result = sut.RegisterProgram("NoSuchApp").GetAwaiter().GetResult();
            Assert.IsInstanceOf<BadRequestErrorMessageResult>(result);
            Assert.AreEqual("Program [NoSuchApp] not found. Ensure it was used at least one time", (result as BadRequestErrorMessageResult).Message);
        }

        [Test]
        public void TestRegisterProgram_AllOk()
        {
            ProgramsUnitOfWork unit = new ProgramsUnitOfWork(this.Context);
            ProgramsController sut = new ProgramsController(unit);
            Helpers.SeedInitialPrograms(this.Context, 1, "TestApp", "TestUser");
            
            var teliUsr = Helpers.CreateTelimenaUser(this.Context, "look@me.now", "Some guys");
            var identity = new GenericIdentity(teliUsr.UserName);
            var principal = new GenericPrincipal(identity, new[] {TelimenaRoles.Developer});
            var usr = new ClaimsPrincipal(principal);


            sut.User = usr;


            var result = sut.RegisterProgram(Helpers.GetName("TestApp")).GetAwaiter().GetResult();
            Assert.IsInstanceOf<OkResult>(result);
           Helpers.GetProgramAndUser(this.Context, "TestApp", "TestUser", out Program prg, out ClientAppUser _);
            Assert.AreEqual(teliUsr.GetDeveloperAccountsLedByUser().Single(), prg.DeveloperAccount);
        }
    }
}