// -----------------------------------------------------------------------
//  <copyright file="StatisticsControllerTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using DbIntegrationTestHelpers;
using log4net;
using Microsoft.AspNet.Identity.EntityFramework;
using Moq;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Developer;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;
using Telimena.WebApp.Models.Developer;

namespace Telimena.Tests
{
    #region Using

    #endregion

    public class MockHttpContext : HttpContextBase
    {
        public override IPrincipal User { get; set; }
    }

    [TestFixture]
    public class RegisterProgramControllerTests : IntegrationTestsContextSharedGlobally<TelimenaContext>
    {
        protected override Action SeedAction => () => TelimenaDbInitializer.SeedUsers(this.Context);

        [Test]
        public void TestRegisterProgram_AllOk()
        {
            ProgramsUnitOfWork unit = new ProgramsUnitOfWork(this.Context, new TelimenaUserManager(new UserStore<TelimenaUser>(this.Context)), new AssemblyVersionReader());
            RegisterProgramController sut = new RegisterProgramController(unit, new Mock<ILog>().Object);

            Helpers.SeedInitialPrograms(this.Context, 1, "TestApp", "TestUser");

            TelimenaUser teliUsr = Helpers.CreateTelimenaUser(this.Context, "look@me.now", "Some guys");
            GenericIdentity identity = new GenericIdentity(teliUsr.UserName);
            GenericPrincipal principal = new GenericPrincipal(identity, new[] {TelimenaRoles.Developer});
            ClaimsPrincipal usr = new ClaimsPrincipal(principal);


            sut.ControllerContext = new ControllerContext {HttpContext = new MockHttpContext {User = usr}};

            RegisterProgramViewModel model = new RegisterProgramViewModel {ProgramName = Helpers.GetName("TestApp")};

            ActionResult result = sut.Register(model).GetAwaiter().GetResult();
            Assert.IsInstanceOf<ViewResult>(result);
            Assert.IsTrue(model.IsSuccess);
            Helpers.GetProgramAndUser(this.Context, "TestApp", "TestUser", out Program prg, out ClientAppUser _);
            Assert.AreEqual(teliUsr.GetDeveloperAccountsLedByUser().Single(), prg.DeveloperAccount);
        }

        [Test]
        public void TestRegisterProgram_PrgNotFound()
        {
            ProgramsUnitOfWork unit = new ProgramsUnitOfWork(this.Context, new TelimenaUserManager(new UserStore<TelimenaUser>(this.Context)), new AssemblyVersionReader());

            RegisterProgramController sut = new RegisterProgramController(unit, new Mock<ILog>().Object);


            RegisterProgramViewModel model = new RegisterProgramViewModel {ProgramName = Helpers.GetName("NoSuchApp")};

            ActionResult result = sut.Register(model).GetAwaiter().GetResult();
            Assert.IsInstanceOf<ViewResult>(result);

            Assert.IsFalse(model.IsSuccess);
            Assert.IsFalse(sut.ModelState.IsValid);

            Assert.AreEqual("Program [TestRegisterProgram_PrgNotFound_NoSuchApp] not found. Ensure it was used at least one time"
                , sut.ModelState[""].Errors[0].ErrorMessage);
        }
    }
}