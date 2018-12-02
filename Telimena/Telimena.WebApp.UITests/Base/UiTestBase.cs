using System;
using System.Collections.Generic;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web.Configuration;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;
using Telimena.WebApp.UiStrings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;

namespace Telimena.WebApp.UITests.Base
{
    [TestFixture]
    public abstract class UiTestBase : IntegrationTestBase
    {
        
        public readonly string AdminName = GetSetting<string>(ConfigKeys.AdminName);
        public readonly string UserName = GetSetting(ConfigKeys.UserName);
        public readonly string AdminPassword = GetSetting(ConfigKeys.AdminPassword);
        public readonly string UserPassword = GetSetting(ConfigKeys.UserPassword);

        internal static Lazy<RemoteWebDriver> RemoteDriver = new Lazy<RemoteWebDriver>(() => GetBrowser("Chrome"));

        private static RemoteWebDriver GetBrowser(string browser)
        {
            switch (browser)
            {
                case "Chrome":
                    var opt = new ChromeOptions();
#if DEBUG

#else
                opt.AddArgument("--headless");
#endif
                    return new ChromeDriver(opt);
                case "Firefox":
                    return new FirefoxDriver();
                case "IE":
                    return new InternetExplorerDriver();
                default:
                    return new ChromeDriver();
            }
        }


        internal IWebDriver Driver => RemoteDriver.Value;
        internal ITakesScreenshot Screenshooter => this.Driver as ITakesScreenshot;

        public void GoToAdminHomePage()
        {
            try
            {

                this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl(""));
                this.LoginAdminIfNeeded();
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("Error while logging in admin", ex);
            }
        }

        public string GetAbsoluteUrl(string relativeUrl)
        {
            return this.TestEngine.GetAbsoluteUrl(relativeUrl);
        }

        public void RecognizeAdminDashboardPage()
        {
            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));
            if (this.Driver.Url.Contains("ChangePassword"))
            {
                Log("Going from change password page to Admin dashboard");
                this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl(""));
            }
            wait.Until(x => x.FindElement(By.Id(Strings.Id.PortalSummary)));
        }

        public IWebElement TryFind(string nameOrId, int timeoutSeconds = 10)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(timeoutSeconds));
                return wait.Until(x => x.FindElement(By.Id(nameOrId)));
            }
            catch { }

            return null;
        }

        public IWebElement TryFind(By by, int timeoutSeconds = 10)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(timeoutSeconds));
                return wait.Until(x => x.FindElement(by));
            }
            catch { }

            return null;
        }

        protected  bool IsLoggedIn()
        {
            if (this.TryFind(Strings.Id.MainHeader) != null)
            {
                return true;
            }

            return false;
        }


        protected void HandleError(Exception ex, List<string> outputs = null, List<string> errors = null, [CallerMemberName] string memberName = "")
        {
            Screenshot screen = this.Screenshooter.GetScreenshot();
            var path = Common.CreatePngPath(memberName);
            screen.SaveAsFile(path, ScreenshotImageFormat.Png);
            var page = this.Driver.PageSource;

            string errorOutputs = "";
            if (errors != null)
            {
                errorOutputs = String.Join("\r\n", errors);
            }
            string normalOutputs = "";
            if (outputs != null)
            {
                normalOutputs = String.Join("\r\n", outputs);
            }
            var alert = this.Driver.WaitForAlert(500);
            alert?.Dismiss();
            throw new AssertFailedException($"{ex}\r\n\r\n{this.PresentParams()}\r\n\r\n{errorOutputs}\r\n\r\n{normalOutputs}\r\n\r\n{page}", ex);

            //this.TestContext.AddResultFile(path);
        }

        private string PresentParams()
        {
            var sb = new StringBuilder();
            sb.Append("Url: " + this.Driver.Url);
            sb.Append("TestContext Parameters: ");
            foreach (var testParameter in NUnit.Framework.TestContext.Parameters.Names)
            {
                sb.Append(testParameter + ": " + NUnit.Framework.TestContext.Parameters[testParameter] + " ");
            }

            return sb.ToString();
        }

        public void LoginAdminIfNeeded()
        {
            this.LoginIfNeeded(this.AdminName, this.AdminPassword);
        }

        private void LoginIfNeeded(string userName, string password)
        {
            if (!this.IsLoggedIn())
            {
                this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl("Account/Login"));
            }

            if (this.Driver.Url.IndexOf("Login", StringComparison.InvariantCultureIgnoreCase) != -1 && this.Driver.FindElement(new ByIdOrName(Strings.Id.LoginForm)) != null)
            {
                Log("Trying to log in...");
                IWebElement login = this.Driver.FindElement(new ByIdOrName(Strings.Id.Email));

                if (login != null)
                {
                    IWebElement pass = this.Driver.FindElement(new ByIdOrName(Strings.Id.Password));
                    login.SendKeys(userName);
                    pass.SendKeys(password);
                    IWebElement submit = this.Driver.FindElement(new ByIdOrName(Strings.Id.SubmitLogin));
                    submit.Click();
                    this.GoToAdminHomePage();
                    this.RecognizeAdminDashboardPage();

                }
            }
            else
            {
                Log("Skipping logging in");
            }
        }
    }
}