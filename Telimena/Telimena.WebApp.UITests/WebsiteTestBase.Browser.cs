using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.IE;
using OpenQA.Selenium.Remote;
using Telimena.TestUtilities;
using Telimena.TestUtilities.Base;
using TestContext = NUnit.Framework.TestContext;

namespace Telimena.WebApp.UITests
{
    [TestFixture]
    public abstract partial class WebsiteTestBase : IntegrationTestBase
    {
        public static string TestOutputFolderPathBase => Path.Combine(new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.Parent.Parent.FullName, "Tests");

        public static string ScreenshotOutputFolderPath = Path.Combine(TestOutputFolderPathBase, "Selenium Screenshots");

        public static Lazy<RemoteWebDriver> RemoteDriver = new Lazy<RemoteWebDriver>(() =>
        {
            var browser = GetBrowser("Chrome");
            browser.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(5);
            return browser;
        });

        public static bool ShowBrowser
        {
            get
            {
                try
                {
#if DEBUG
                    return true;
#endif
#pragma warning disable 162
                    return ConfigHelper.GetSetting<bool>(ConfigKeys.ShowBrowser);
#pragma warning restore 162
                }
                catch
                {
                    return false;
                }
            }
        }

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
                    var driverService = ChromeDriverService.CreateDefaultService();
                    var driver = new ChromeDriver(driverService, opt);
                    driver.Manage().Window.Maximize();
                    driver.Manage().Window.Size = new Size(ConfigHelper.GetSetting<int>(ConfigKeys.BrowserWidth, 3840), ConfigHelper.GetSetting<int>(ConfigKeys.BrowserHeight, 2160));

                    Task.Run(() => AllowHeadlessDownload(driver, driverService));
                    return driver;
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

        protected static string DownloadPath => Environment.GetEnvironmentVariable("USERPROFILE") + "\\Downloads";

        static async Task AllowHeadlessDownload(IWebDriver driver,  ChromeDriverService driverService)
        {
            var jsonContent = new JObject(
                new JProperty("cmd", "Page.setDownloadBehavior"),
                new JProperty("params",
                    new JObject(new JObject(
                        new JProperty("behavior", "allow"),
                        new JProperty("downloadPath", DownloadPath)))));
            var content = new StringContent(jsonContent.ToString(), Encoding.UTF8, "application/json");
            var sessionIdProperty = typeof(ChromeDriver).GetProperty("SessionId");
            var sessionId = sessionIdProperty.GetValue(driver, null) as SessionId;

            using (var client = new HttpClient())
            {
                client.BaseAddress = driverService.ServiceUrl;
                var result = await client.PostAsync("session/" + sessionId.ToString() + "/chromium/send_command", content);
                var resultContent = await result.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }

        public IWebDriver Driver => RemoteDriver.Value;
        internal ITakesScreenshot Screenshooter => this.Driver as ITakesScreenshot;

        public string GetAbsoluteUrl(string relativeUrl)
        {
            return this.TestEngine.GetAbsoluteUrl(relativeUrl);
        }
        
        private string TakeScreenshot(string screenshotName)
        {
            int attempt = 0;
            while (attempt < 5)
            {
                attempt++;
                IntegrationTestBase.Log($"Saving error screenshot: {screenshotName} for test run {TestRunTimestamp}");
                string path = CreatePngPath(screenshotName, this.GetType().Name);
                IntegrationTestBase.Log($"{path}");
                try
                {
                    Screenshot screen = this.Screenshooter.GetScreenshot();
                    screen.SaveAsFile(path, ScreenshotImageFormat.Png);
                    TestContext.AddTestAttachment(path);
                    return path;

                }
                catch (UnhandledAlertException alertException)
                {
                    IntegrationTestBase.Log($"Attempt {attempt} - Unexpected alert {screenshotName} for test run {TestRunTimestamp}.\r\n{alertException}");

                    IAlert unexpectedAlert = this.Driver.SwitchTo().Alert();
                    IntegrationTestBase.Log($"Attempt {attempt} - Alert text: [{unexpectedAlert?.Text}]");
                    unexpectedAlert?.Dismiss();
                    try
                    {
                        Screenshot screen = this.Screenshooter.GetScreenshot();
                        screen.SaveAsFile(path, ScreenshotImageFormat.Png);
                    }
                    catch (Exception screenshotError)
                    {
                        IntegrationTestBase.Log($"Attempt {attempt} - Error while saving screenshot after dismissing alert: {screenshotError}");
                    }
                }
                catch (Exception screenshotError)
                {
                    IntegrationTestBase.Log($"Attempt {attempt} - Error while saving screenshot: {screenshotError}");
                }

            }

            return "Failed to take screenshot";
        }

        public static string CreatePngPath(string fileName, string callerClassName)
        {
            var path = Path.Combine(ScreenshotOutputFolderPath, IntegrationTestBase.TestRunTimestamp + " " + callerClassName);
            Directory.CreateDirectory(path);
            return Path.Combine(path, fileName + ".png");
        }

        protected void HandleError(Exception ex, string screenshotName, List<string> outputs = null, List<string> errors = null)
        {
            Log($"Handling error {ex.Message}.");
            var screenshotPath = this.TakeScreenshot(screenshotName);

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

            Log($"{ex.ToString()}.");

            IAlert alert = this.Driver.WaitForAlert(500);
            alert?.Dismiss();
            throw new TelimenaTestException($"{ex}\r\n\r\n{this.PresentParams()}\r\n\r\n{errorOutputs}\r\n\r\n{normalOutputs}\r\n\r\nScreenshot Path:\r\n\r\n{screenshotPath}", ex);

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
                IntegrationTestBase.Log($"Could not delete file {expectedPath }, error: {ex}");
            }
            var stopwatch = Stopwatch.StartNew();
            Assert.IsFalse(File.Exists(expectedPath));
            action();
            IntegrationTestBase.Log($"Action executed");

            var isTimedOut = false;
            string filePath = null;
            Func<bool> fileAppearedOrTimedOut = () =>
            {
                isTimedOut = stopwatch.Elapsed > maximumWaitTime;
                filePath = Directory.GetFiles(downloadDirectory)
                    .FirstOrDefault(x => Path.GetFileName(x) == expectedName);
                if (filePath != null)
                {
                    IntegrationTestBase.Log($"File stored at {filePath}");
                }
                else
                {
                    IntegrationTestBase.Log($"File not ready yet. Elapsed: {stopwatch.Elapsed}");
                }

                return filePath != null || isTimedOut;
            };

            do
            {
                Thread.Sleep(500);
            }
            while (!fileAppearedOrTimedOut());

            if (!String.IsNullOrEmpty(filePath))
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
    }
}