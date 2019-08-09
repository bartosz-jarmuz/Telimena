using System;
using System.Reflection;
using System.Threading;
using DotNetLittleHelpers;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Telimena.TestUtilities.Base;
using Telimena.TestUtilities.Base.TestAppInteraction;
using Telimena.WebApp.UiStrings;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace Telimena.WebApp.UITests._01._Ui
{
    [TestFixture]
    public partial class _1_WebsiteTests : WebsiteTestBase
    {
        [Test]
        public void GoThroughAllPagesAsAdmin()
        {
            try
            {
                this.GoToAdminHomePage();

                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(GetSetting<int>(ConfigKeys.WaitTimeoutSeconds)));

                this.TryFind(By.Id(Strings.Id.ToolkitManagementLink)).ClickWrapper(this.Driver, this.Log);
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id(@Strings.Id.ToolkitManagementForm)));

                this.TryFind(By.Id(Strings.Id.PortalAdminDashboardLink)).ClickWrapper(this.Driver, this.Log);
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id(@Strings.Id.PortalSummary)));

                this.TryFind(By.Id(Strings.Id.PortalUsersLink)).ClickWrapper(this.Driver, this.Log);
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id(@Strings.Id.PortalUsersTable)));

                this.TryFind(By.Id(Strings.Id.DeveloperDashboardLink)).ClickWrapper(this.Driver, this.Log);
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id(@Strings.Id.DeveloperDashboard)));

                this.TryFind(By.Id(Strings.Id.TeamManagementLink)).ClickWrapper(this.Driver, this.Log);
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.TeamMembersList)));

                this.TryFind(By.Id(Strings.Id.RegisterApplicationLink)).ClickWrapper(this.Driver, this.Log);
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id(@Strings.Id.RegisterApplicationForm)));

                this.ValidateAppPages(Apps.Names.AutomaticTestsClient, wait);

                this.ValidateHelpPages(wait);

                //api should be last
                this.TryFind(By.Id(Strings.Id.ApiDocsLink)).ClickWrapper(this.Driver, this.Log);
                var apiInfo = wait.Until(ExpectedConditions.ElementIsVisible(By.Id("api_info")));
                Assert.IsTrue(apiInfo.Text.Contains("Telimena API"), "apiInfo.Text.Contains('Telimena API')");
            }
            catch (Exception ex)
            {
                this.HandleError(ex, MethodBase.GetCurrentMethod().Name);
            }
        }

        private void ValidateSequencePage(IWebElement table, WebDriverWait wait)
        {
            Retrier.RetryAsync(() =>
            {
                table = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ViewUsageTable)));
                var elms = table.FindElements(By.TagName("tr"));
                this.Log($"View usage table row count: {elms.Count}");
                Assert.IsTrue(elms.Count > 1, "View usage table does not have at least one row");

            },TimeSpan.FromMilliseconds(1000), 8).GetAwaiter().GetResult();
            var rows = table.FindElements(By.TagName("tr"));
            if (rows.Count > 1)
            {
                var cells = rows[1].FindElements(By.TagName("td"));

                var link = cells[1].FindElement(By.TagName("a"));

                link.ClickWrapper(this.Driver, this.Log);

                Retrier.RetryAsync(() =>
                {
                    var sequenceTable = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.SequenceHistoryTable)));
                    var sequenceRows = sequenceTable.FindElements(By.TagName("tr"));
                    Assert.IsTrue(sequenceRows.Count > 1, $"Sequence table does not contain sequence elements. [{sequenceTable.Text}]"); //includes header

                }, TimeSpan.FromMilliseconds(1000), 8).GetAwaiter().GetResult();
                
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

            this.ClickOnProgramMenuButton(appName, "_dashboardLink");
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ProgramDashboard)));

        }

        private void ValidateHelpPages(WebDriverWait wait)
        {
            this.TryFind(By.Id(Strings.Id.Help.LinkHelpOverview)).ClickWrapper(this.Driver, this.Log);
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.Help.HelpOverview)));

            this.TryFind(By.Id(Strings.Id.Help.LinkGettingStarted)).ClickWrapper(this.Driver, this.Log);
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.Help.AppRegistrationPanel)));

            this.TryFind(By.Id(Strings.Id.Help.LinkTelemetry)).ClickWrapper(this.Driver, this.Log);
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.Help.TelemetryOverviewPanel)));

            this.TryFind(By.Id(Strings.Id.Help.LinkLifecycleManagement)).ClickWrapper(this.Driver, this.Log);
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.Help.CheckingForUpdatesPanel)));

        }
    }
}
