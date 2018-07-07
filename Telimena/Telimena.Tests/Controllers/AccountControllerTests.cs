namespace Telimena.Tests
{
    using System;
    using System.Data.Common;
    using System.Data.Entity;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using System.Web.Mvc;
    using DbIntegrationTestHelpers;
    using log4net;
    using Microsoft.AspNet.Identity.EntityFramework;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using NUnit.Framework;
    using WebApp.Controllers;
    using WebApp.Core.Interfaces;
    using WebApp.Core.Models;
    using WebApp.Infrastructure.Database;
    using WebApp.Infrastructure.Identity;
    using WebApp.Infrastructure.UnitOfWork.Implementation;
    using WebApp.Models.Account;
    using Assert = NUnit.Framework.Assert;

    [TestClass()]
    public class AccountControllerTests : StaticContextIntegrationTestsBase<TelimenaContext>
    {

        public AccountControllerTests()
        {
        }
        protected override Action SeedAction => () =>
        {
            TelimenaDbInitializer.SeedUsers(this.Context);
         //   this.Context.Users.Add(new TelimenaUser() {UserName = "aa", Email = "aa@b.com", CreatedDate = DateTime.UtcNow});
            this.Context.SaveChanges();
        };

        [Test]
        public void TestRemoveDeveloper()
        {
            var manager = new TelimenaUserManager(
                new UserStore<TelimenaUser>(this.Context));
            
            var unit = new AccountUnitOfWork(null, manager, this.Context);

            AccountControllerTests.GetTwoUsers(unit);
            var jack = unit.UserManager.FindByNameAsync(Helpers.GetName("Jack@Daniels.com")).GetAwaiter().GetResult();
            var jim = unit.UserManager.FindByNameAsync(Helpers.GetName("Jim@Beam.com")).GetAwaiter().GetResult();

            var jackDev = unit.DeveloperRepository.FirstOrDefault(x => x.MainUser.Id == jack.Id);
            jackDev.AssociateUser(jim);

            unit.DeveloperRepository.Remove(jackDev);
            unit.Complete();
            Assert.IsNotNull(this.Context.Users.Single(x=>x.Id == jack.Id));
            Assert.IsNotNull(this.Context.Users.Single(x=>x.Id == jim.Id));
            Assert.IsNull(unit.DeveloperRepository.SingleOrDefault(x=>x.Id == jackDev.Id));

        }

        [Test]
        public void TestRemoveUser()
        {
            var manager = new TelimenaUserManager(
                new UserStore<TelimenaUser>(this.Context));
            var unit = new AccountUnitOfWork(null, manager, this.Context);

            AccountControllerTests.GetTwoUsers(unit);
            var jack = unit.UserManager.FindByNameAsync(Helpers.GetName("Jack@Daniels.com")).GetAwaiter().GetResult();

            var jackDev = unit.DeveloperRepository.FirstOrDefault(x => x.MainUser.Id == jack.Id);
            int jackDevId = jackDev.Id;
            Assert.AreEqual(jackDevId, jack.AssociatedDeveloperAccounts.Single().Id);

            unit.UserManager.DeleteAsync(jack).GetAwaiter().GetResult();
            unit.Complete();
            Assert.IsNull(unit.UserManager.FindByNameAsync(jack.UserName).GetAwaiter().GetResult());

            jackDev = unit.DeveloperRepository.FirstOrDefault(x => x.Id == jackDevId);

            Assert.IsNull(jackDev.MainUser);
            Assert.AreEqual(Helpers.GetName("Jack@Daniels.com"), jackDev.MainEmail);
        }
        [Test]
        public void TestRemoveUserFromDeveloper()
        {
            var manager = new TelimenaUserManager(
                new UserStore<TelimenaUser>(this.Context));
            var unit = new AccountUnitOfWork(null, manager, this.Context);

            AccountControllerTests.GetTwoUsers(unit);

            var jim = unit.UserManager.FindByNameAsync(Helpers.GetName("Jim@Beam.com")).GetAwaiter().GetResult();
            var jack = unit.UserManager.FindByNameAsync(Helpers.GetName("Jack@Daniels.com")).GetAwaiter().GetResult();

            var jackDev = unit.DeveloperRepository.FirstOrDefault(x => x.MainUser.Id == jack.Id);



        }


        [Test]
        public void TestSetUserAsMainForSecondDeveloper()
        {
            var manager = new TelimenaUserManager(
                new UserStore<TelimenaUser>(this.Context));
            var unit = new AccountUnitOfWork(null, manager, this.Context);

            AccountControllerTests.GetTwoUsers(unit);

            var jim = unit.UserManager.FindByNameAsync(Helpers.GetName("Jim@Beam.com")).GetAwaiter().GetResult();
            var jack = unit.UserManager.FindByNameAsync(Helpers.GetName("Jack@Daniels.com")).GetAwaiter().GetResult();

            var jackDev = unit.DeveloperRepository.FirstOrDefault(x => x.MainUser.Id == jack.Id);
            var jimDev = unit.DeveloperRepository.FirstOrDefault(x => x.MainUser.Id == jim.Id);
            Assert.AreEqual(jackDev.Id, jack.GetDeveloperAccountsLedByUser().Single().Id);
            Assert.AreEqual(jimDev.Id, jim.GetDeveloperAccountsLedByUser().Single().Id);

            jimDev.SetMainUser(jack);
            unit.Complete();

            Assert.IsNotNull(jimDev.AssociatedUsers.Single(x => x.Id == jim.Id));
            Assert.IsNotNull(jimDev.AssociatedUsers.Single(x => x.Id == jack.Id));

            Assert.AreEqual(1, jim.AssociatedDeveloperAccounts.Count());
            Assert.AreEqual(jack, jimDev.MainUser);

            Assert.AreEqual(0, jim.GetDeveloperAccountsLedByUser().Count());
            Assert.AreEqual(2, jack.GetDeveloperAccountsLedByUser().Count());

            Assert.IsNotNull(jack.GetDeveloperAccountsLedByUser().Single(x=>x.Id == jackDev.Id));
            Assert.IsNotNull(jack.GetDeveloperAccountsLedByUser().Single(x=>x.Id == jimDev.Id));
   
        }

        [Test]
        public void TestAddSecondUserToDeveloperTwice()
        {
            var manager = new TelimenaUserManager(
                new UserStore<TelimenaUser>(this.Context));
            var unit = new AccountUnitOfWork(null, manager, this.Context);

            AccountControllerTests.GetTwoUsers(unit);

            var jim = unit.UserManager.FindByNameAsync(Helpers.GetName("Jim@Beam.com")).GetAwaiter().GetResult();
            var jack = unit.UserManager.FindByNameAsync(Helpers.GetName("Jack@Daniels.com")).GetAwaiter().GetResult();


            var jackDev = unit.DeveloperRepository.FirstOrDefault(x => x.MainUser.Id == jack.Id);

            jackDev.AssociateUser(jim);
            unit.Complete();
            jackDev.AssociateUser(jim);
            unit.Complete();

            Assert.AreEqual(2, jackDev.AssociatedUsers.Count);
        }


        [Test]
        public void TestAddSecondUserToDeveloper()
        {
            var manager = new TelimenaUserManager(
                new UserStore<TelimenaUser>(this.Context));
            var unit = new AccountUnitOfWork(null, manager, this.Context);

            AccountControllerTests.GetTwoUsers(unit);

            var jim = unit.UserManager.FindByNameAsync(Helpers.GetName("Jim@Beam.com")).GetAwaiter().GetResult();
            var jack = unit.UserManager.FindByNameAsync(Helpers.GetName("Jack@Daniels.com")).GetAwaiter().GetResult();

            Assert.AreEqual(jim.Email, jim.AssociatedDeveloperAccounts.Single().MainEmail);

            var jackDev = unit.DeveloperRepository.FirstOrDefault(x => x.MainUser.Id == jack.Id);
            Assert.AreEqual(jackDev.Id, jack.AssociatedDeveloperAccounts.Single().Id);
            var jimDev = unit.DeveloperRepository.FirstOrDefault(x => x.MainUser.Id == jim.Id);
            Assert.AreEqual(jackDev.Id, jack.AssociatedDeveloperAccounts.Single().Id);

            jackDev.AssociateUser(jim);

            unit.Complete();
            jackDev = unit.DeveloperRepository.FirstOrDefault(x => x.MainUser.Id == jack.Id);

            Assert.AreEqual(2, jackDev.AssociatedUsers.Count());
            Assert.IsNotNull(jackDev.AssociatedUsers.Single(x=>x.Id == jim.Id));
            Assert.IsNotNull(jackDev.AssociatedUsers.Single(x=>x.Id == jack.Id));

            Assert.AreEqual(2, jim.AssociatedDeveloperAccounts.Count());
            Assert.IsNotNull(jim.AssociatedDeveloperAccounts.Single(x => x.Id == jackDev.Id));
            Assert.IsNotNull(jim.AssociatedDeveloperAccounts.Single(x => x.Id == jimDev.Id));

            jackDev.RemoveAssociatedUser(jim);
            unit.Complete();

            Assert.AreEqual(jack.Id, jackDev.AssociatedUsers.Single().Id);
            Assert.AreEqual(jimDev.Id, jim.AssociatedDeveloperAccounts.Single().Id);
            Assert.IsNull(jackDev.AssociatedUsers.SingleOrDefault(x=>x.Id == jim.Id));

        }

        private static void GetTwoUsers(AccountUnitOfWork unit, [CallerMemberName] string caller = null)
        {
            unit.RegisterUserAsync(new TelimenaUser(caller + "_Jim@Beam.com", caller + "_Jim Beam"), "P@ssword", TelimenaRoles.Developer).GetAwaiter().GetResult();

            unit.Complete();

            unit.RegisterUserAsync(new TelimenaUser(caller + "_Jack@Daniels.com", caller + "_Jack Daniels"), "P@ssword", TelimenaRoles.Developer).GetAwaiter().GetResult();

            unit.Complete();
        }

        [Test]
        public void RegisterTest()
        {
            var manager = new TelimenaUserManager(
                new UserStore<TelimenaUser>(this.Context));
            var unit = new AccountUnitOfWork(null, manager, this.Context);
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
            Assert.AreEqual("WaitForActivationInfo", result.ViewName);

            var user = unit.UserManager.FindByNameAsync(model.Email).GetAwaiter().GetResult();

            Assert.AreEqual("Jim Beam", user.DisplayName);
            Assert.IsTrue(user.RegisteredDate.Date == DateTime.UtcNow.Date);
            Assert.IsFalse(user.IsActivated);
            Assert.IsTrue(unit.UserManager.IsInRoleAsync(user.Id, TelimenaRoles.Developer).GetAwaiter().GetResult());
            Assert.IsTrue(unit.UserManager.IsInRoleAsync(user.Id, TelimenaRoles.Viewer).GetAwaiter().GetResult());

            DeveloperAccount developerAccount = unit.DeveloperRepository.SingleOrDefault(x => x.MainUser.Id == user.Id);
            Assert.AreEqual(user.DisplayName, developerAccount.MainUser.DisplayName);
            Assert.AreEqual(user.Email, developerAccount.MainEmail);

            Assert.AreEqual(developerAccount, user.GetDeveloperAccountsLedByUser().Single());
        }


    }
}