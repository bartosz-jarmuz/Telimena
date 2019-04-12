using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;
using DotNetLittleHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Remote;
using OpenQA.Selenium.Support.PageObjects;
using OpenQA.Selenium.Support.UI;
using Telimena.WebApp.UiStrings;
using Assert = NUnit.Framework.Assert;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;
using TestContext = NUnit.Framework.TestContext;

namespace Telimena.WebApp.UITests.Base
{
    [TestFixture]
    public abstract class UiTestBase : IntegrationTestBase
    {

        /// <summary>
        /// Start a download and wait for a file to appear
        /// https://stackoverflow.com/a/46440261/1141876
        /// </summary>
        /// <param name="expectedName">If we don't know the extension, Chrome creates a temp file in download folder and we think we have the file already</param>
        protected FileInfo ActAndWaitForFileDownload(
            Action action
            , string expectedName
            , TimeSpan maximumWaitTime
            , string downloadDirectory)
        {
            var expectedPath = Path.Combine(downloadDirectory, expectedName);
            try
            {
                File.Delete(expectedPath);
            }
            catch (Exception ex)
            {
                Log($"Could not delete file {expectedPath }, error: {ex}");
            }
            var stopwatch = Stopwatch.StartNew();
            Assert.IsFalse(File.Exists(expectedPath));
            action();

            var isTimedOut = false;
            string filePath = null;
            Func<bool> fileAppearedOrTimedOut = () =>
            {
                isTimedOut = stopwatch.Elapsed > maximumWaitTime;
                filePath = Directory.GetFiles(downloadDirectory)
                    .FirstOrDefault(x => Path.GetFileName(x) == expectedName);

                return filePath != null || isTimedOut;
            };

            do
            {
                Thread.Sleep(500);
            }
            while (!fileAppearedOrTimedOut());

            if (!string.IsNullOrEmpty(filePath))
            {
                Assert.AreEqual(expectedPath, filePath);
                return new FileInfo(filePath);
            }
            if (isTimedOut)
            {
                Assert.Fail("Failed to download");
            }

            return null;

        }

        protected void ClickOnProgramMenuButton(string appName, string buttonSuffix)
        {
            Retrier.RetryAsync(() =>
            {
                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));
                var prgMenu = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(appName + "_menu")));

                Actions actions = new Actions(this.Driver);
                actions.MoveToElement(prgMenu);
                actions.Perform();

                prgMenu.Click();
                IWebElement link = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(appName + buttonSuffix)));
                actions = new Actions(this.Driver);
                actions.MoveToElement(link);
                actions.Perform();

                link.Click();
            }, TimeSpan.FromMilliseconds(500), 3).GetAwaiter().GetResult();


        }

        protected void ClickOnManageProgramMenu(string appName)
        {
            this.ClickOnProgramMenuButton(appName, "_manageLink");

        }

        public void WaitForSuccessConfirmationWithText(WebDriverWait wait, Func<string, bool> validateText)
        {
            this.WaitForConfirmationWithTextAndClass(wait, "success", validateText);
        }

        public void WaitForErrorConfirmationWithText(WebDriverWait wait, Func<string, bool> validateText)
        {
            this.WaitForConfirmationWithTextAndClass(wait, "danger", validateText);
        }

        public void WaitForConfirmationWithTextAndClass(WebDriverWait wait, string cssPart, Func<string, bool> validateText)
        {
            var confirmationBox = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.TopAlertBox)));

            Assert.IsTrue(confirmationBox.GetAttribute("class").Contains(cssPart), "The alert has incorrect class: " + confirmationBox.GetAttribute("class"));
            Assert.IsTrue(validateText(confirmationBox.Text), "Incorrect message: " + confirmationBox.Text);
        }

        internal static Lazy<RemoteWebDriver> RemoteDriver = new Lazy<RemoteWebDriver>(() =>
        {
            var browser = GetBrowser("Chrome");

          //  browser.Manage().Window.Maximize();
            browser.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            return browser;
        });

        private static RemoteWebDriver GetBrowser(string browser)
        {
            switch (browser)
            {
                case "Chrome":
                    ChromeOptions opt = new ChromeOptions();
                    opt.AddArgument("--safebrowsing-disable-download-protection");
                    opt.AddUserProfilePreference("safebrowsing", "enabled");
                    if (!ShowBrowser)
                    {
                      opt.AddArgument("--headless");
                    }
                    return new ChromeDriver(opt);
                case "Firefox":
                    FirefoxOptions options = new FirefoxOptions();
             //       options.AddArguments("--headless");
                    var ff = new FirefoxDriver(options);
                    return ff;
                case "IE":
                    return new InternetExplorerDriver();
                default:
                    return new ChromeDriver();
            }
        }

        internal IWebDriver Driver => RemoteDriver.Value;
        internal ITakesScreenshot Screenshooter => this.Driver as ITakesScreenshot;

        public void WaitForPageLoad(int maxWaitTimeInSeconds)
        {
            string state = string.Empty;
            try
            {
                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(maxWaitTimeInSeconds));

                //Checks every 500 ms whether predicate returns true if returns exit otherwise keep trying till it returns ture
                wait.Until(d =>
                {

                    try
                    {
                        state = ((IJavaScriptExecutor) this.Driver).ExecuteScript(@"return document.readyState").ToString();
                    }
                    catch (InvalidOperationException)
                    {
                        //Ignore
                    }
                    catch (NoSuchWindowException)
                    {
                        //when popup is closed, switch to last windows
                        this.Driver.SwitchTo().Window(this.Driver.WindowHandles.Last());
                    }

                    //In IE7 there are chances we may get state as loaded instead of complete
                    return (state.Equals("complete", StringComparison.InvariantCultureIgnoreCase) ||
                            state.Equals("loaded", StringComparison.InvariantCultureIgnoreCase));

                });
            }
            catch (TimeoutException)
            {
                //sometimes Page remains in Interactive mode and never becomes Complete, then we can still try to access the controls
                if (!state.Equals("interactive", StringComparison.InvariantCultureIgnoreCase))
                    throw;
            }
            catch (NullReferenceException)
            {
                //sometimes Page remains in Interactive mode and never becomes Complete, then we can still try to access the controls
                if (!state.Equals("interactive", StringComparison.InvariantCultureIgnoreCase))
                    throw;
            }
            catch (WebDriverException)
            {
                if (this.Driver.WindowHandles.Count == 1)
                {
                    this.Driver.SwitchTo().Window(this.Driver.WindowHandles[0]);
                }

                state = ((IJavaScriptExecutor) this.Driver).ExecuteScript(@"return document.readyState").ToString();
                if (!(state.Equals("complete", StringComparison.InvariantCultureIgnoreCase) ||
                      state.Equals("loaded", StringComparison.InvariantCultureIgnoreCase)))
                    throw;
            }
        }

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
                this.Log("Going from change password page to Admin dashboard");
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
            catch
            {
            }

            return null;
        }

        protected IWebElement TryFind(By by, int timeoutSeconds = 10)
        {
            try
            {
                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(timeoutSeconds));
                return wait.Until(x => x.FindElement(by));
            }
            catch
            {
            }

            return null;
        }

        protected IWebElement TryFind(Func<IWebElement> finderFunc, TimeSpan timeout)
        {
            Stopwatch sw = Stopwatch.StartNew();
            while (sw.ElapsedMilliseconds < timeout.TotalMilliseconds)
            {
                try
                {
                    IWebElement result = finderFunc();
                    if (result != null)
                    {
                        return result;
                    }
                }
                catch
                {
                    //np
                }
            }

            return null;
        }

        protected bool IsLoggedIn()
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
            string path = Common.CreatePngPath(memberName);
            screen.SaveAsFile(path, ScreenshotImageFormat.Png);
            string page = this.Driver.PageSource;

            string errorOutputs = "";
            if (errors != null)
            {
                errorOutputs = string.Join("\r\n", errors);
            }

            string normalOutputs = "";
            if (outputs != null)
            {
                normalOutputs = string.Join("\r\n", outputs);
            }

            IAlert alert = this.Driver.WaitForAlert(500);
            alert?.Dismiss();
            throw new AssertFailedException($"{ex}\r\n\r\n{this.PresentParams()}\r\n\r\n{errorOutputs}\r\n\r\n{normalOutputs}\r\n\r\n{page}", ex);

            //this.TestContext.AddResultFile(path);
        }

        private string PresentParams()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("Url: " + this.Driver.Url);
            sb.Append("TestContext Parameters: ");
            foreach (string testParameter in TestContext.Parameters.Names)
            {
                sb.Append(testParameter + ": " + TestContext.Parameters[testParameter] + " ");
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

            if (this.Driver.Url.IndexOf("Login", StringComparison.InvariantCultureIgnoreCase) != -1 &&
                this.Driver.FindElement(new ByIdOrName(Strings.Id.LoginForm)) != null)
            {
                this.Log("Trying to log in...");
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
                this.Log("Skipping logging in");
            }
        }
    }
}