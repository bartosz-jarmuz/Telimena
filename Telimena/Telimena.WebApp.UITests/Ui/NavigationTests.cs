using System;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Telimena.WebApp.UiStrings;
using Telimena.WebApp.UITests.Base;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

namespace Telimena.WebApp.UITests.Ui
{
    [TestFixture]
    public partial class UiTests : UiTestBase
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

                this.Driver.FindElement(By.Id(Strings.Id.ApiDocsLink)).Click();
               var apiInfo =  wait.Until(ExpectedConditions.ElementIsVisible(By.Id("api_info")));

                Assert.AreEqual("Telimena.WebApp", apiInfo.Text);

            }
            catch (Exception ex)
            {
                this.HandlerError(ex);
            }
        }


    }
}
