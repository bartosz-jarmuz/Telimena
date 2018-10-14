using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;
using Telimena.WebApp.UITests.Base;

namespace Telimena.WebApp.UITests.Account
{
    public class LoginTests : PortalTestBase
    {



        [Test]
        public void AdminLoginOk()
        {
            try
            {
                this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl(""));

                IWebElement login = this.Driver.FindElement(new ByIdOrName("email"));
                IWebElement pass = this.Driver.FindElement(new ByIdOrName("password"));
                login.SendKeys(this.AdminName);
                pass.SendKeys(this.AdminPassword);
                IWebElement submit = this.Driver.FindElement(new ByIdOrName("submitLogin"));
                submit.Click();
                this.RecognizeAdminDashboardPage();
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

                IWebElement login = this.Driver.FindElement(new ByIdOrName("email"));
                IWebElement pass = this.Driver.FindElement(new ByIdOrName("password"));
                login.SendKeys("Wrong");
                pass.SendKeys("Dude");
                IWebElement submit = this.Driver.FindElement(new ByIdOrName("submitLogin"));
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
