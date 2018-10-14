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
using Telimena.WebApp.UiStrings;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestContext = Microsoft.VisualStudio.TestTools.UnitTesting.TestContext;

namespace Telimena.WebApp.UITests.Base
{
    [TestFixture]
    public abstract class PortalTestBase
    {

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

        private readonly bool isLocalTestSetting = true;
        //private readonly bool isLocalTestSetting = GetSetting<bool>(ConfigKeys.IsLocalTest);


        internal static RemoteWebDriver RemoteDriver;
        internal IWebDriver Driver => RemoteDriver;
        internal ITakesScreenshot Screenshooter => this.Driver as ITakesScreenshot;

        protected ITestEngine TestEngine { get; set; }

        [OneTimeTearDown]
        public void TestCleanup()
        {
            this.TestEngine.BaseCleanup();

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
            this.TestEngine.BaseInitialize();
        }

      

        public void GoToAdminHomePage()
        {
            this.Driver.Navigate().GoToUrl(this.GetAbsoluteUrl(""));
            this.LoginAdminIfNeeded();
        }


        public string GetAbsoluteUrl(string relativeUrl)
        {
            return this.TestEngine.GetAbsoluteUrl(relativeUrl);
        }

        public void RecognizeAdminDashboardPage()
        {
            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));
            wait.Until(x => x.FindElement(By.Id(Strings.Id.PortalSummary)));
        }

        protected void HandlerError(Exception ex, [CallerMemberName] string memberName = "")
        {
            Screenshot screen = this.Screenshooter.GetScreenshot();
            var path = Common.CreatePngPath(memberName);
            screen.SaveAsFile(path, ScreenshotImageFormat.Png);
            //this.TestContext.AddResultFile(path);
            throw ex;
        }

        public void LoginAdminIfNeeded()
        {
            this.LoginIfNeeded(this.AdminName, this.AdminPassword);
        }

        private void LoginIfNeeded(string userName, string password)
        {
            if (this.Driver.Url.Contains("Login") && this.Driver.FindElement(new ByIdOrName(Strings.Id.LoginForm)) != null)
            {
                IWebElement login = this.Driver.FindElement(new ByIdOrName(Strings.Id.Email));

                if (login != null)
                {
                    IWebElement pass = this.Driver.FindElement(new ByIdOrName(Strings.Id.Password));
                    login.SendKeys(userName);
                    pass.SendKeys(password);
                    IWebElement submit = this.Driver.FindElement(new ByIdOrName(Strings.Id.SubmitLogin));
                    submit.Click();
                    this.RecognizeAdminDashboardPage();

                }
            }
        }
    }
}