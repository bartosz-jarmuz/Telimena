using System;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;
using Telimena.WebApp.UiStrings;
using Telimena.WebApp.UITests.Base;
using Assert = NUnit.Framework.Assert;

namespace Telimena.WebApp.UITests.Ui
{

    [TestFixture]
    public partial class _1_UiTests : UiTestBase
    {

        [Test, Order(1)]
        public void AdminLoginOk()
        {
            try
            {
                if (this.IsLoggedIn())
                {
                    this.LogOut();
                }
                this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl("Account/Login"));


                IWebElement login = this.Driver.FindElement(new ByIdOrName(Strings.Id.Email));
                IWebElement pass = this.Driver.FindElement(new ByIdOrName(Strings.Id.Password));
                login.SendKeys(this.AdminName);
                pass.SendKeys(this.AdminPassword);
                IWebElement submit = this.Driver.FindElement(new ByIdOrName(Strings.Id.SubmitLogin));
                submit.Click();

                if (this.TryFind(Strings.Id.PasswordForm) == null)
                {
                    this.RecognizeAdminDashboardPage();

                }
            }
            catch (Exception ex)
            {
                this.HandlerError(ex);
            }

        }

        private void LogOut()
        {
            this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl("Account/LogOff"));
            this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl(""));
        }


        [Test, Order(0)]
        public void AdminLogin_Failed()
        {
            try
            {
                if (this.IsLoggedIn())
                {
                    this.LogOut();
                }
                               this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl("Account/Login"));


                IWebElement login = this.Driver.FindElement(new ByIdOrName(Strings.Id.Email));
                IWebElement pass = this.Driver.FindElement(new ByIdOrName(Strings.Id.Password));
                login.SendKeys("Wrong");
                pass.SendKeys("Dude");
                IWebElement submit = this.Driver.FindElement(new ByIdOrName(Strings.Id.SubmitLogin));
                submit.Click();
                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));
                IWebElement element = wait.Until(x => x.FindElement(By.ClassName("validation-summary-errors")));

                Assert.AreEqual("Invalid username or password", element.Text);
            }
            catch (Exception ex)
            {
                this.HandlerError(ex);
            }
        }

    }
}
