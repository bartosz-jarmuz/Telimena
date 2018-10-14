using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using NUnit.Framework.Internal;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;
using Telimena.WebApp.UiStrings;
using Telimena.WebApp.UITests.Base;
using Assert = NUnit.Framework.Assert;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;

namespace Telimena.WebApp.UITests.Account
{



    [TestClass]
    public class LoginTests : PortalTestBase
    {

        //[Test]
        public void AdminLoginOk()
        {
            try
            {
                this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl(""));

                IWebElement login = this.Driver.FindElement(new ByIdOrName(Strings.Id.Email));
                IWebElement pass = this.Driver.FindElement(new ByIdOrName(Strings.Id.Password));
                login.SendKeys(this.AdminName);
                pass.SendKeys(this.AdminPassword);
                IWebElement submit = this.Driver.FindElement(new ByIdOrName(Strings.Id.SubmitLogin));
                submit.Click();

                if (this.Driver.FindElement(By.Id(Strings.Id.PasswordForm)) == null)
                {
                    this.RecognizeAdminDashboardPage();

                }
            }
            catch (Exception ex)
            {
                this.HandlerError(ex);
            }
        }

        [Test]
        public void AdminLogin_Failed()
        {
            try
            {
                this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl(""));

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
