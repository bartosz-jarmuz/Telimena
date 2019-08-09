using System;
using System.Reflection;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using Telimena.TestUtilities.Base;
using Telimena.WebApp.UiStrings;

namespace Telimena.WebApp.UITests._01._Ui
{
    [TestFixture]
    public partial class _3_TeamsTests: WebsiteTestBase
    {
//        [Test]
        public void RegisterTestUsers()
        {
            this.RegisterUser(this.User1Name, this.User1Name, this.User1Password);
            this.RegisterUser(this.User2Name, this.User2Name, this.User2Password);
            this.RegisterUser(this.User3Name, this.User3Name, this.User3Password);

        }

        public void RegisterAppAsTestUser1_EnsureNotVisibleTo3()
        {
            this.LogOut();
            this.LoginIfNeeded(this.User1Name, this.User1Password);

          //  this.RegisterApp(Apps.Names.User3TestApp, Apps.Keys.User3TestApp, "", );
        }




        public void RegisterUser(string displayName, string email, string password)
        {
            try
            {
                if (this.IsLoggedIn())
                {
                    this.LogOut();
                }

                this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl("Account/Register"));


                IWebElement name = this.Driver.FindElement(new ByIdOrName(Strings.Id.Name));
                IWebElement login = this.Driver.FindElement(new ByIdOrName(Strings.Id.Email));
                IWebElement pass = this.Driver.FindElement(new ByIdOrName(Strings.Id.Password));
                IWebElement pass2 = this.Driver.FindElement(new ByIdOrName(Strings.Id.ConfirmPassword));
                name.SendKeys(displayName);
                login.SendKeys(email);
                pass.SendKeys(password);
                pass2.SendKeys(password);

                IWebElement submit = this.Driver.FindElement(new ByIdOrName(Strings.Id.Register));
                submit.ClickWrapper(this.Driver);

                
                this.VerifyUserAdded();
            }
            catch (Exception ex)
            {
                this.HandleError(ex, SharedTestHelpers.GetMethodName());
            }

        }

        private void VerifyUserAdded()
        {
            var element = this.Driver.TryFind(By.ClassName(Strings.Css.UserCreatedMessage));
            if (element != null)
            {
                if (element.Text.Contains("Account created successfully."))
                {
                    return;
                }
            }

            element = this.Driver.TryFind(By.ClassName("validation-summary-errors"));
            if (element != null)
            {
                if (element.Text.Contains("is already taken."))
                {
                    return;
                }
            }

            Assert.Fail("Unexpected user creation result");

        }
    }
}