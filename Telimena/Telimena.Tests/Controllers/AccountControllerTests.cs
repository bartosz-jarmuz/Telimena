using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Web.Mvc;
using DbIntegrationTestHelpers;
using log4net;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using Telimena.WebApp.Controllers;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;
using Telimena.WebApp.Models.Account;
using Assert = NUnit.Framework.Assert;

namespace Telimena.Tests
{
    [TestClass]
    public class AccountControllerTests : IntegrationTestsContextSharedGlobally<TelimenaPortalContext>
    {
        protected override Action SeedAction =>
            () =>
            {
                TelimenaPortalDbInitializer.SeedUsers(this.Context);
                //   this.Context.Users.Add(new TelimenaUser() {UserId = "aa", Email = "aa@b.com", CreatedDate = DateTime.UtcNow});
                this.Context.SaveChanges();
            };

        private static void GetTwoUsers(AccountUnitOfWork unit, [CallerMemberName] string caller = null)
        {
            var result = unit.RegisterUserAsync(new TelimenaUser(caller + "Jim@Beam.com", caller + "Jim Beam"), "P@ssword", TelimenaRoles.Developer).GetAwaiter()
                .GetResult();
            if (!result.Item1.Succeeded)
            {
                var msg = result.Item1.Errors?.FirstOrDefault();
                if (msg != null && !msg.Contains("is already taken."))
                {
                    Assert.Fail($"Failed to register user. Error: {result.Item1.Errors?.FirstOrDefault()}");
                }
            }
            unit.Complete();

             result = unit.RegisterUserAsync(new TelimenaUser(caller + "Jack@Daniels.com", caller + "Jack Daniels"), "P@ssword", TelimenaRoles.Developer).GetAwaiter()
                .GetResult();
            if (!result.Item1.Succeeded)
            {
                var msg = result.Item1.Errors?.FirstOrDefault();
                if (msg != null && !msg.Contains("is already taken."))
                {
                    Assert.Fail($"Failed to register user. Error: {result.Item1.Errors?.FirstOrDefault()}");
                }
            }
            unit.Complete();
        }

        [Test]
        public void RegisterTest()
        {
            TelimenaUserManager manager = new TelimenaUserManager(new UserStore<TelimenaUser>(this.Context));
            AccountUnitOfWork unit = new AccountUnitOfWork(null, manager, this.Context);
            AccountController sut = new AccountController(unit, new Mock<ILog>().Object);

            RegisterViewModel model = new RegisterViewModel
            {
                Name = "Jim Beam"
                , ConfirmPassword = "P@ssword"
                , Password = "P@ssword"
                , Email = "Jim@Beam.com"
                , Role = TelimenaRoles.Developer
            };

            ViewResult result = sut.Register(model).GetAwaiter().GetResult() as ViewResult;
            Assert.AreEqual("WaitForActivationInfo", result.ViewName);

            TelimenaUser user = unit.UserManager.FindByNameAsync(model.Email).GetAwaiter().GetResult();

            Assert.AreEqual("Jim Beam", user.DisplayName);
            Assert.IsTrue(user.RegisteredDate.Date == DateTime.UtcNow.Date);
            //Assert.IsFalse(user.IsActivated);
            Assert.IsTrue(unit.UserManager.IsInRoleAsync(user.Id, TelimenaRoles.Developer).GetAwaiter().GetResult());
            Assert.IsTrue(unit.UserManager.IsInRoleAsync(user.Id, TelimenaRoles.Viewer).GetAwaiter().GetResult());

            DeveloperTeam developerTeam = unit.DeveloperRepository.SingleOrDefault(x => x.MainUser.Id == user.Id);
            Assert.AreEqual(user.DisplayName, developerTeam.MainUser.DisplayName);
            Assert.AreEqual(user.Email, developerTeam.MainEmail);

            Assert.AreEqual(developerTeam, user.GetDeveloperAccountsLedByUser().Single());
        }

        [Test]
        public void TestAddSecondUserToDeveloper()
        {
            TelimenaUserManager manager = new TelimenaUserManager(new UserStore<TelimenaUser>(this.Context));
            AccountUnitOfWork unit = new AccountUnitOfWork(null, manager, this.Context);

            GetTwoUsers(unit);

            TelimenaUser jim = unit.UserManager.FindByNameAsync(Helpers.GetName("Jim@Beam.com")).GetAwaiter().GetResult();
            TelimenaUser jack = unit.UserManager.FindByNameAsync(Helpers.GetName("Jack@Daniels.com")).GetAwaiter().GetResult();

            Assert.AreEqual(jim.Email, jim.AssociatedDeveloperAccounts.Single().MainEmail);

            DeveloperTeam jackDev = unit.DeveloperRepository.FirstOrDefault(x => x.MainUser.Id == jack.Id);
            Assert.AreEqual(jackDev.Id, jack.AssociatedDeveloperAccounts.Single().Id);
            DeveloperTeam jimDev = unit.DeveloperRepository.FirstOrDefault(x => x.MainUser.Id == jim.Id);
            Assert.AreEqual(jackDev.Id, jack.AssociatedDeveloperAccounts.Single().Id);

            jackDev.AssociateUser(jim);

            unit.Complete();
            jackDev = unit.DeveloperRepository.FirstOrDefault(x => x.MainUser.Id == jack.Id);

            Assert.AreEqual(2, jackDev.AssociatedUsers.Count());
            Assert.IsNotNull(jackDev.AssociatedUsers.Single(x => x.Id == jim.Id));
            Assert.IsNotNull(jackDev.AssociatedUsers.Single(x => x.Id == jack.Id));

            Assert.AreEqual(2, jim.AssociatedDeveloperAccounts.Count());
            Assert.IsNotNull(jim.AssociatedDeveloperAccounts.Single(x => x.Id == jackDev.Id));
            Assert.IsNotNull(jim.AssociatedDeveloperAccounts.Single(x => x.Id == jimDev.Id));

            jackDev.RemoveAssociatedUser(jim);
            unit.Complete();

            Assert.AreEqual(jack.Id, jackDev.AssociatedUsers.Single().Id);
            Assert.AreEqual(jimDev.Id, jim.AssociatedDeveloperAccounts.Single().Id);
            Assert.IsNull(jackDev.AssociatedUsers.SingleOrDefault(x => x.Id == jim.Id));
        }

        [Test]
        public void TestAddSecondUserToDeveloperTwice()
        {
            TelimenaUserManager manager = new TelimenaUserManager(new UserStore<TelimenaUser>(this.Context));
            AccountUnitOfWork unit = new AccountUnitOfWork(null, manager, this.Context);

            GetTwoUsers(unit);

            TelimenaUser jim = unit.UserManager.FindByNameAsync(Helpers.GetName("Jim@Beam.com")).GetAwaiter().GetResult();
            TelimenaUser jack = unit.UserManager.FindByNameAsync(Helpers.GetName("Jack@Daniels.com")).GetAwaiter().GetResult();


            DeveloperTeam jackDev = unit.DeveloperRepository.FirstOrDefault(x => x.MainUser.Id == jack.Id);

            jackDev.AssociateUser(jim);
            unit.Complete();
            jackDev.AssociateUser(jim);
            unit.Complete();

            Assert.AreEqual(2, jackDev.AssociatedUsers.Count);
        }

        [Test]
        public void TestRemoveDeveloper()
        {
            TelimenaUserManager manager = new TelimenaUserManager(new UserStore<TelimenaUser>(this.Context));

            AccountUnitOfWork unit = new AccountUnitOfWork(null, manager, this.Context);

            GetTwoUsers(unit);
            TelimenaUser jack = unit.UserManager.FindByNameAsync(Helpers.GetName("Jack@Daniels.com")).GetAwaiter().GetResult();
            TelimenaUser jim = unit.UserManager.FindByNameAsync(Helpers.GetName("Jim@Beam.com")).GetAwaiter().GetResult();

            DeveloperTeam jackDev = unit.DeveloperRepository.FirstOrDefault(x => x.MainUser.Id == jack.Id);
            jackDev.AssociateUser(jim);

            unit.DeveloperRepository.Remove(jackDev);
            unit.Complete();
            Assert.IsNotNull(this.Context.Users.Single(x => x.Id == jack.Id));
            Assert.IsNotNull(this.Context.Users.Single(x => x.Id == jim.Id));
            Assert.IsNull(unit.DeveloperRepository.SingleOrDefault(x => x.Id == jackDev.Id));
        }

        [Test]
        public void TestRemoveUser()
        {
            TelimenaUserManager manager = new TelimenaUserManager(new UserStore<TelimenaUser>(this.Context));
            AccountUnitOfWork unit = new AccountUnitOfWork(null, manager, this.Context);

            GetTwoUsers(unit);
            TelimenaUser jack = unit.UserManager.FindByNameAsync(Helpers.GetName("Jack@Daniels.com")).GetAwaiter().GetResult();

            DeveloperTeam jackDev = unit.DeveloperRepository.FirstOrDefault(x => x.MainUser.Id == jack.Id);
            var jackDevId = jackDev.Id;
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
            TelimenaUserManager manager = new TelimenaUserManager(new UserStore<TelimenaUser>(this.Context));
            AccountUnitOfWork unit = new AccountUnitOfWork(null, manager, this.Context);

            GetTwoUsers(unit);

            TelimenaUser jim = unit.UserManager.FindByNameAsync(Helpers.GetName("Jim@Beam.com")).GetAwaiter().GetResult();
            TelimenaUser jack = unit.UserManager.FindByNameAsync(Helpers.GetName("Jack@Daniels.com")).GetAwaiter().GetResult();

            DeveloperTeam jackDev = unit.DeveloperRepository.FirstOrDefault(x => x.MainUser.Id == jack.Id);
        }

        [Test]
        public void TestSetUserAsMainForSecondDeveloper()
        {
            TelimenaUserManager manager = new TelimenaUserManager(new UserStore<TelimenaUser>(this.Context));
            AccountUnitOfWork unit = new AccountUnitOfWork(null, manager, this.Context);

            GetTwoUsers(unit);

            TelimenaUser jim = unit.UserManager.FindByNameAsync(Helpers.GetName("Jim@Beam.com")).GetAwaiter().GetResult();
            TelimenaUser jack = unit.UserManager.FindByNameAsync(Helpers.GetName("Jack@Daniels.com")).GetAwaiter().GetResult();

            DeveloperTeam jackDev = unit.DeveloperRepository.FirstOrDefault(x => x.MainUser.Id == jack.Id);
            DeveloperTeam jimDev = unit.DeveloperRepository.FirstOrDefault(x => x.MainUser.Id == jim.Id);
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

            Assert.IsNotNull(jack.GetDeveloperAccountsLedByUser().Single(x => x.Id == jackDev.Id));
            Assert.IsNotNull(jack.GetDeveloperAccountsLedByUser().Single(x => x.Id == jimDev.Id));
        }
    }
}