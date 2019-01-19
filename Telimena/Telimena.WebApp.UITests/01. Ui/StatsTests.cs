using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutomaticTestsClient;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Telimena.WebApp.UiStrings;
using Telimena.WebApp.UITests.Base;
using Telimena.WebApp.UITests.Base.TestAppInteraction;
using TelimenaClient;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace Telimena.WebApp.UITests._01._Ui
{
    [TestFixture]
    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial class _1_UiTests : UiTestBase
    {
        private async Task<DateTime> GetLatestUsageFromTable()
        {
            try
            {
                this.GoToAdminHomePage();

                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

                this.Driver.FindElement(By.Id(TestAppProvider.AutomaticTestsClientAppName + "_menu")).Click();
                IWebElement statLink = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(TestAppProvider.AutomaticTestsClientAppName + "_statsLink")));

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
                    return parsed;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException($"Cannot parse [{dateTime}] as DateTime", ex);
                }
            }
            catch (Exception ex)
            {
                this.HandleError(ex, this.outputs, this.errors);
                return default(DateTime);
            }
        }

        [Test]
        public async Task Test_AppUsageTable()
        {
            string viewName = nameof(this.Test_AppUsageTable);
            FileInfo app = this.LaunchTestsAppNewInstanceAndGetResult(out _, out TelemetryItem first, Actions.ReportViewUsage, TestAppProvider.FileNames.TestAppV1, nameof(this.Test_AppUsageTable)
                , viewName: viewName);
            Task.Delay(1000).GetAwaiter().GetResult();
            DateTime previous = await this.GetLatestUsageFromTable();
            Task.Delay(1500).GetAwaiter().GetResult();

            var second = this.LaunchTestsAppAndGetResult<TelemetryItem>(app, Actions.ReportViewUsage, viewName: viewName);

            DateTime current = await this.GetLatestUsageFromTable();

            Assert.IsTrue(current > previous, $"current {current} is not larger than previous {previous}");
        }
    }
}