using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using SharedLogic;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Telimena.TestUtilities.Base;
using Telimena.TestUtilities.Base.TestAppInteraction;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.UiStrings;
using TelimenaClient;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace Telimena.WebApp.UITests._01._Ui
{
    [TestFixture]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial class _1_WebsiteTests : WebsiteTestBase
    {
        private Task<DateTime> GetLatestUsageFromTable([CallerMemberName] string caller = "")
        {
            try
            {
                this.GoToAdminHomePage();

                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

                this.Driver.FindElement(By.Id(Apps.Names.AutomaticTestsClient + "_menu")).Click();
                IWebElement statLink = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Apps.Names.AutomaticTestsClient + "_statsLink")));

                statLink.Click();

                IWebElement programsTable = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ViewUsageTable)));
                wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(Strings.Id.ViewUsageTable)));
                wait.Until(ExpectedConditions.InvisibilityOfElementLocated(By.Id("ProgramUsageTable_processing")));

                IWebElement latestRow = this.TryFind(() => programsTable.FindElement(By.TagName("tbody")).FindElements(By.TagName("tr")).FirstOrDefault()
                    , TimeSpan.FromSeconds(2));

                if (latestRow == null)
                {
                    throw new InvalidOperationException("Latest row is null");
                }

                string dateTime = "Not initialized";
                try
                {
                    dateTime = latestRow.FindElements(By.TagName("td")).FirstOrDefault()?.Text;
                    DateTime parsed = DateTime.ParseExact(dateTime, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    return Task.FromResult(parsed);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Cannot parse [{dateTime}] as DateTime", ex);
                }
            }
            catch (Exception ex)
            {
                this.HandleError(ex, caller, this.outputs, this.errors);
                return Task.FromResult(default(DateTime));
            }
        }

        [Test]
        [Timeout(3*60*1000), Retry(3)]
        public async Task Test_AppUsageTable()
        {
            string viewName = nameof(this.Test_AppUsageTable);
            FileInfo app = this.LaunchTestsAppNewInstanceAndGetResult(out _, out TelemetryItem first, Actions.ReportViewUsage, Apps.Keys.AutomaticTestsClient, Apps.PackageNames.AutomaticTestsClientAppV1, nameof(this.Test_AppUsageTable)
                , viewName: viewName);
            Task.Delay(1000).GetAwaiter().GetResult();
            DateTime previous = await this.GetLatestUsageFromTable().ConfigureAwait(false);
            Task.Delay(1500).GetAwaiter().GetResult();

            var second = this.LaunchTestsAppAndGetResult<TelemetryItem>(app, Actions.ReportViewUsage, Apps.Keys.AutomaticTestsClient, viewName: viewName);

            DateTime current = await this.GetLatestUsageFromTable().ConfigureAwait(false);
            if (current == previous)
            {
                await Task.Delay(2000);
            }
            current = await this.GetLatestUsageFromTable().ConfigureAwait(false);
            Assert.IsTrue(current > previous, $"current {current} is not larger than previous {previous}");
        }
    }
}