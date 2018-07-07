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
    using System.Web;
    using System.Web.Http.Results;
    using System.Web.Mvc;
    using Client;
    using DbIntegrationTestHelpers;
    using log4net;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Moq;
    using NUnit.Framework;
    using WebApi.Controllers;
    using WebApp.Controllers.Developer;
    using WebApp.Core.Interfaces;
    using WebApp.Core.Messages;
    using WebApp.Core.Models;
    using WebApp.Infrastructure.Database;
    using WebApp.Infrastructure.Identity;
    using WebApp.Infrastructure.UnitOfWork.Implementation;
    using WebApp.Models.Developer;
    #endregion
    public class MockHttpContext : HttpContextBase
    {
        public override IPrincipal User { get; set; }
    }

    [TestFixture]
    public class RegisterProgramControllerTests : StaticContextIntegrationTestsBase<TelimenaContext>
    {
        protected override Action SeedAction => () => TelimenaDbInitializer.SeedUsers(this.Context);


        [Test]
        public void TestRegisterProgram_PrgNotFound()
        {
            ProgramsUnitOfWork unit = new ProgramsUnitOfWork(this.Context, new TelimenaUserManager(new UserStore<TelimenaUser>(this.Context)));

            RegisterProgramController sut = new RegisterProgramController(unit, new Mock<ILog>().Object);


            var model = new RegisterProgramViewModel()
            {
                ProgramName = Helpers.GetName("NoSuchApp")
            };

            var result = sut.Register(model).GetAwaiter().GetResult();
            Assert.IsInstanceOf<ViewResult>(result);

            Assert.IsFalse(model.IsSuccess);
            Assert.IsFalse(sut.ModelState.IsValid);

            Assert.AreEqual("Program [TestRegisterProgram_PrgNotFound_NoSuchApp] not found. Ensure it was used at least one time", sut.ModelState[""].Errors[0].ErrorMessage);
        }

        [Test]
        public void TestRegisterProgram_AllOk()
        {
            ProgramsUnitOfWork unit = new ProgramsUnitOfWork(this.Context, new TelimenaUserManager(new UserStore<TelimenaUser>(this.Context)));
            RegisterProgramController sut = new RegisterProgramController(unit, new Mock<ILog>().Object);

            Helpers.SeedInitialPrograms(this.Context, 1, "TestApp", "TestUser");
            
            var teliUsr = Helpers.CreateTelimenaUser(this.Context, "look@me.now", "Some guys");
            var identity = new GenericIdentity(teliUsr.UserName);
            var principal = new GenericPrincipal(identity, new[] {TelimenaRoles.Developer});
            var usr = new ClaimsPrincipal(principal);
            

            sut.ControllerContext = new ControllerContext
            {
                HttpContext = new MockHttpContext()
                {
                    User = usr
                }
            };

            var model = new RegisterProgramViewModel()
            {
                ProgramName = Helpers.GetName("TestApp")
            };

            var result = sut.Register(model).GetAwaiter().GetResult();
            Assert.IsInstanceOf<ViewResult>(result);
            Assert.IsTrue(model.IsSuccess);
           Helpers.GetProgramAndUser(this.Context, "TestApp", "TestUser", out Program prg, out ClientAppUser _);
            Assert.AreEqual(teliUsr.GetDeveloperAccountsLedByUser().Single(), prg.DeveloperAccount);
        }
    }
}