using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telimena.WebApp.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telimena.WebApp.Controllers.Tests
{
    using System.Data.Common;
    using System.Data.Entity;
    using System.Web.Mvc;
    using Core.Interfaces;
    using Core.Models;
    using Infrastructure.Database;
    using Infrastructure.Identity;
    using Infrastructure.Repository.Implementation;
    using Infrastructure.UnitOfWork.Implementation;
    using log4net;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Models.Account;
    using Moq;
    using NUnit.Framework;

    [TestClass()]
    public class AccountControllerTests
    {
        private DbConnection connection;
        private TelimenaContext context;

        public AccountControllerTests()
        {
            this.connection = Effort.DbConnectionFactory.CreateTransient();
            this.context = new TelimenaContext(this.connection);

        }

        [Test]
        public void TestRemoveUser()
        {
            var manager = new TelimenaUserManager(
                new UserStore<TelimenaUser>(this.context));
            var unit = new AccountUnitOfWork(null, manager, this.context);

            AccountControllerTests.GetTwoUsers(unit);
            var jack = unit.UserManager.FindByNameAsync("Jack@Daniels.com").GetAwaiter().GetResult();

            var jackDev = this.context.Developers.FirstOrDefaultAsync(x => x.MainUser.Id == jack.Id).GetAwaiter().GetResult();
            int jackDevId = jackDev.Id;

            unit.UserManager.DeleteAsync(jack).GetAwaiter().GetResult();
            unit.CompleteAsync().GetAwaiter().GetResult();
            Assert.IsNull(unit.UserManager.FindByNameAsync("Jack@Daniels.com").GetAwaiter().GetResult());

            jackDev = this.context.Developers.FirstOrDefaultAsync(x => x.Id == jackDevId).GetAwaiter().GetResult();

            Assert.IsNull(jackDev.MainUser);
            Assert.AreEqual("Jack@Daniels.com", jackDev.MainEmail);
        }


        [Test]
        public void TestAddSecondUserToDeveloper()
        {
            var manager = new TelimenaUserManager(
                new UserStore<TelimenaUser>(this.context));
            var unit = new AccountUnitOfWork(null, manager, this.context);

            AccountControllerTests.GetTwoUsers(unit);

            var jim = unit.UserManager.FindByNameAsync("Jim@Beam.com").GetAwaiter().GetResult();
            var jack = unit.UserManager.FindByNameAsync("Jack@Daniels.com").GetAwaiter().GetResult();

            var jackDev = this.context.Developers.FirstOrDefaultAsync(x => x.MainUser.Id == jack.Id).GetAwaiter().GetResult();
            jackDev.AssociatedUsers.Add(jim);
            unit.CompleteAsync().GetAwaiter().GetResult();
            jackDev = this.context.Developers.FirstOrDefaultAsync(x => x.MainUser.Id == jack.Id).GetAwaiter().GetResult();

            Assert.AreEqual(jim.Id, jackDev.AssociatedUsers.Single().Id);
            jackDev.AssociatedUsers.Remove(jim);

            unit.CompleteAsync().GetAwaiter().GetResult();
        
        }

        private static void GetTwoUsers(AccountUnitOfWork unit)
        {
            unit.RegisterUserAsync(new TelimenaUser()
            {
                UserName = "Jim@Beam.com",
                Email = "Jim@Beam.com",
                DisplayName = "Jim Beam"
            }, "P@ssword", TelimenaRoles.Developer).GetAwaiter().GetResult();

            unit.CompleteAsync().GetAwaiter().GetResult();

            unit.RegisterUserAsync(new TelimenaUser()
            {
                UserName = "Jack@Daniels.com",
                Email = "Jack@Daniels.com",
                DisplayName = "Jack Daniels"
            }, "P@ssword", TelimenaRoles.Developer).GetAwaiter().GetResult();

            unit.CompleteAsync().GetAwaiter().GetResult();
        }

        [Test]
        public void RegisterTest()
        {
            var manager = new TelimenaUserManager(
                new UserStore<TelimenaUser>(this.context));
            var unit = new AccountUnitOfWork(null, manager, this.context);
            var sut = new AccountController(unit, new Mock<ILog>().Object);

            var model = new RegisterViewModel()
            {
                Name = "Jim Beam",
                ConfirmPassword = "P@ssword",
                Password = "P@ssword",
                Email = "Jim@Beam.com",
                Role = TelimenaRoles.Developer
            };

            ViewResult result = sut.Register(model).GetAwaiter().GetResult() as ViewResult;
            Assert.AreEqual("WaitForActivationInfo", result.ViewName );

            var user = unit.UserManager.FindByNameAsync(model.Email).GetAwaiter().GetResult();

            Assert.AreEqual("Jim Beam", user.DisplayName);
            Assert.IsTrue(user.CreatedDate.Date == DateTime.Today);
            Assert.IsFalse(user.IsActivated);
            Assert.IsTrue(user.RoleNames.Contains(TelimenaRoles.Developer));
            Assert.IsTrue(user.RoleNames.Contains(TelimenaRoles.Viewer));

            Developer developer = this.context.Developers.SingleOrDefault(x => x.MainUser.Id == user.Id);
            Assert.AreEqual(user.DisplayName, developer.MainUser.DisplayName);
            Assert.AreEqual(user.Email, developer.MainEmail);
        }

      
    }
}