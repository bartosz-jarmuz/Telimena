using System;
using System.Threading;
using DotNetLittleHelpers;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Telimena.WebApp.UiStrings;
using Telimena.WebApp.UITests.Base;
using Telimena.WebApp.UITests.Base.TestAppInteraction;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace Telimena.WebApp.UITests._01._Ui
{
    [TestFixture]
    public partial class _1_UiTests : UiTestBase
    {
        [Test]
        public void GoThroughAllPagesAsAdmin()
        {
            try
            {
                this.GoToAdminHomePage();

                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

                this.Driver.FindElement(By.Id(Strings.Id.AppsAdminDashboardLink)).Click();
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id(@Strings.Id.AppsSummary)));

                this.Driver.FindElement(By.Id(Strings.Id.ToolkitManagementLink)).Click();
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id(@Strings.Id.ToolkitManagementForm)));

                this.Driver.FindElement(By.Id(Strings.Id.PortalAdminDashboardLink)).Click();
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id(@Strings.Id.PortalSummary)));

                this.Driver.FindElement(By.Id(Strings.Id.PortalUsersLink)).Click();
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id(@Strings.Id.PortalUsersTable)));

                this.Driver.FindElement(By.Id(Strings.Id.DeveloperDashboardLink)).Click();
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id(@Strings.Id.DeveloperDashboard)));

                this.Driver.FindElement(By.Id(Strings.Id.RegisterApplicationLink)).Click();
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id(@Strings.Id.RegisterApplicationForm)));

                this.ValidateAppPages(TestAppProvider.AutomaticTestsClientAppName, wait);

                //api should be last
                this.Driver.FindElement(By.Id(Strings.Id.ApiDocsLink)).Click();
                var apiInfo = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("api_info")));
                Assert.IsTrue(apiInfo.Text.Contains("Telimena API"));
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
            }
        }

        private void ValidateSequencePage(IWebElement table, WebDriverWait wait)
        {
            Retrier.RetryAsync(() =>
            {
                table = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ViewUsageTable)));
                var elms = table.FindElements(By.TagName("tr"));
                Assert.IsTrue(elms.Count > 1);

            },TimeSpan.FromMilliseconds(250), 8).GetAwaiter().GetResult();
            var rows = table.FindElements(By.TagName("tr"));
            if (rows.Count > 1)
            {
                var cells = rows[1].FindElements(By.TagName("td"));

                var link = cells[1].FindElement(By.TagName("a"));

                link.Click();

                Retrier.RetryAsync(() =>
                {
                    var sequenceTable = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.SequenceHistoryTable)));
                    var sequenceRows = sequenceTable.FindElements(By.TagName("tr"));
                    Assert.IsTrue(sequenceRows.Count > 1, $"Sequence table does not contain sequence elements. [{sequenceTable.Text}]"); //includes header

                }, TimeSpan.FromMilliseconds(250), 8).GetAwaiter().GetResult();
                
            }
        }

        private void ValidateAppPages(string appName, WebDriverWait wait)
        {
            this.ClickOnManageProgramMenu(appName);
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.UpdaterSelectList)));

            this.ClickOnProgramMenuButton(appName, "_statsLink");
            var table = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ViewUsageTable)));

            this.ValidateSequencePage(table, wait);

            this.ClickOnProgramMenuButton(appName, "_exceptionsLink");
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ExceptionsTable)));

            this.ClickOnProgramMenuButton(appName, "_logsLink");
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.LogsTable)));

            this.ClickOnProgramMenuButton(appName, "_pivotTableLink");
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.EventTelemetryPivotTable)));

        }
    }
}
