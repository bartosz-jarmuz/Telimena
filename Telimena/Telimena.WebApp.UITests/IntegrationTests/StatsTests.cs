using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutomaticTestsClient;
using NUnit.Framework;
using NUnit.Framework.Internal;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Telimena.WebApp.UiStrings;
using Telimena.WebApp.UITests.Base;
using Telimena.WebApp.UITests.IntegrationTests.TestAppInteraction;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace Telimena.WebApp.UITests.IntegrationTests
{
    [TestFixture()]
    public class StatsTests : PortalTestBase
    {

        List<string> errors = new List<string>();
        List<string> outputs = new List<string>();

        [SetUp]
        public void ResetLists()
        {
            errors = new List<string>();
            outputs = new List<string>();
        }
        [Test]
        public void Test_AppUsageTable()
        {

            try
            {
                this.LaunchTestsApp();
                Task.Delay(1000).GetAwaiter().GetResult();
                var previous = this.GetLatestUsageFromTable();
                this.LaunchTestsApp();
                var current = this.GetLatestUsageFromTable();

                Assert.IsTrue(current > previous);

            }
            catch (Exception ex)
            {
                this.HandlerError(ex);
            }

        }

        private DateTime GetLatestUsageFromTable()
        {
            try
            {
                this.GoToAdminHomePage();

                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

                this.Driver.FindElement(By.Id(TestAppProvider.AutomaticTestsClientAppName + "_menu")).Click();
                var statLink = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id(TestAppProvider.AutomaticTestsClientAppName + "_statsLink")));

                statLink.Click();

                var programsTable = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ProgramUsageTable)));

                var sorter = programsTable.FindElements(By.TagName("th")).FirstOrDefault(x => x.Text == "DateTime");
                if (sorter.GetAttribute("class") != "sorting_desc")
                {
                    sorter.Click();
                    wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ProgramUsageTable)));
                    if (sorter.GetAttribute("class") != "sorting_desc")
                    {
                        sorter.Click();
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ProgramUsageTable)));

                    }
                }

                var latestRow = programsTable.FindElement(By.TagName("tbody")).FindElements(By.TagName("tr")).FirstOrDefault();

                var dateTime = latestRow.FindElements(By.TagName("td")).FirstOrDefault()?.Text;
                var parsed = DateTime.Parse(dateTime);
                return parsed;
            }
            catch (Exception ex)
            {
                this.HandlerError(ex);
                return default(DateTime);
            }
        }

        private void LaunchTestsApp()
        {
            FileInfo exe = TestAppProvider.ExtractApp(TestAppProvider.FileNames.TestAppV1);

            Arguments args = new Arguments() {ApiUrl = this.BaseUrl, Action = Actions.Initialize};

       

            Process process = ProcessCreator.Create(exe, args, outputs, errors);
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            process.WaitForExit();
        }
    }
}
