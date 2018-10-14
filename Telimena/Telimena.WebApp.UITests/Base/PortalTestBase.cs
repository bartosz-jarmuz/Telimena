using System;
using System.Configuration;
using System.Runtime.CompilerServices;
using System.Web.Configuration;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;

namespace Telimena.WebApp.UITests.Base
{
    [TestFixture]
    public abstract class PortalTestBase
    {
        protected PortalTestBase()
        {
        }

        protected static string GetSetting(string key)
        {
            return  WebConfigurationManager.AppSettings.Get(key);
        }
        protected static T GetSetting<T>(string key) 
        {
            string val = GetSetting(key);
            return (T)Convert.ChangeType(val, typeof(T));
        }

        public readonly string AdminName = GetSetting<string>(ConfigKeys.AdminName);
        public readonly string UserName = GetSetting(ConfigKeys.UserName);
        public readonly string AdminPassword = GetSetting(ConfigKeys.AdminPassword);
        public readonly string UserPassword = GetSetting(ConfigKeys.UserPassword);
        private readonly bool isLocalTestSetting = GetSetting<bool>(ConfigKeys.IsLocalTest);


        internal RemoteWebDriver RemoteDriver;
        internal IWebDriver Driver => this.RemoteDriver;
        internal ITakesScreenshot Screenshooter => this.Driver as ITakesScreenshot;

        protected ITestEngine TestEngine { get; set; }


        [OneTimeTearDown]
        public void TestCleanup()
        {
            this.TestEngine.BaseCleanup();
            this.RemoteDriver.Dispose();

        }

        [OneTimeSetUp]
        public void TestInitialize()
        {
            if (this.isLocalTestSetting)
            {
                this.TestEngine = new LocalHostTestEngine();
            }
            else
            {
                this.TestEngine = new DeployedTestEngine();
            }
            this.SetBrowser("Chrome");
            this.TestEngine.BaseInitialize();
        }

        public void SetBrowser(string browser)
        {
            switch (browser)
            {
                case "Chrome":
                    this.RemoteDriver = new ChromeDriver();
                    break;
                case "Firefox":
                    this.RemoteDriver = new FirefoxDriver();
                    break;
                case "IE":
                    this.RemoteDriver = new InternetExplorerDriver();
                    break;
                default:
                    this.RemoteDriver = new ChromeDriver();
                    break;
            }

        }



        public string GetAbsoluteUrl(string relativeUrl)
        {
            return this.TestEngine.GetAbsoluteUrl(relativeUrl);
        }

        public void RecognizeAdminDashboardPage()
        {
            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));
            wait.Until(x => x.FindElement(By.Id("portalSummary")));
        }

        protected void HandlerError(Exception ex, [CallerMemberName] string memberName = "")
        {
            Screenshot screen = this.Screenshooter.GetScreenshot();
            screen.SaveAsFile(Common.CreatePngPath(memberName), ScreenshotImageFormat.Png);
            throw ex;
        }

        private void LoginIfNeeded(string userName, string password)
        {
            if (this.Driver.Url.Contains("Login") && this.Driver.FindElement(new ByIdOrName("loginForm")) != null)
            {
                IWebElement login = this.Driver.FindElement(new ByIdOrName("login"));
                if (login != null)
                {
                    IWebElement pass = this.Driver.FindElement(new ByIdOrName("password"));
                    login.SendKeys(userName);
                    pass.SendKeys(password);
                    IWebElement submit = this.Driver.FindElement(new ByIdOrName("submit"));
                    submit.Click();
                    WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));
                    wait.Until(x => x.FindElement(new ByIdOrName("__AjaxAntiForgeryForm")));
                }
            }
        }
    }
}