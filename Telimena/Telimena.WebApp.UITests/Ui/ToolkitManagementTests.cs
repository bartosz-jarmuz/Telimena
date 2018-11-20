using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Telimena.WebApp.UiStrings;
using Telimena.WebApp.UITests.Base;
using Telimena.WebApp.UITests.Base.TestAppInteraction;

namespace Telimena.WebApp.UITests.Ui
{
    using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

    [TestFixture]
    public partial class _1_UiTests : UiTestBase
    {

        private void UploadUpdater(string fileName)
        {
            this.GoToAdminHomePage();

            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

            this.Driver.FindElement(By.Id(Strings.Id.ToolkitManagementLink)).Click();
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ToolkitManagementForm)));

            var updater = TestAppProvider.GetFile(fileName);


            this.Driver.FindElement(By.Id(Strings.Id.UpdaterPackageUploader)).SendKeys(updater.FullName);


            wait.Until(x => x.FindElements(By.Id(Strings.Id.UpdaterUploadInfoBox)).FirstOrDefault(e => e.Text.Contains(updater.Name)));


            this.Driver.FindElement(By.Id(Strings.Id.SubmitUpdaterUpload)).Click();

            var confirmationBox = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.UpdaterConfirmationBox)));

            Assert.IsTrue(confirmationBox.GetAttribute("class").Contains("label-success"));

            Assert.IsTrue(confirmationBox.Text.Contains("Uploaded package 1.8.0.0 with ID "));

        }



        [Test]
        public void _01_UploadToolkit()
        {
            this.GoToAdminHomePage();

            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

            this.Driver.FindElement(By.Id(Strings.Id.ToolkitManagementLink)).Click();
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ToolkitManagementForm)));

            var file = TestAppProvider.GetFile("Telimena.Client.dll");


            this.Driver.FindElement(By.Id(Strings.Id.ToolkitPackageUploader)).SendKeys(file.FullName);


            wait.Until(x => x.FindElements(By.Id(Strings.Id.ToolkitUploadInfoBox)).FirstOrDefault(e => e.Text.Contains(file.Name)));


            this.Driver.FindElement(By.Id(Strings.Id.SubmitToolkitUpload)).Click();

            var confirmationBox = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ToolkitConfirmationBox)));

            Assert.IsTrue(confirmationBox.GetAttribute("class").Contains("label-success"));

            Assert.IsTrue(confirmationBox.Text.Contains("Uploaded package 2.0.0.0 with ID "));

        }

        [Test]
        public void _02_UploadUpdaterExeTest()
        {
            try
            {
                UploadUpdater("Updater.exe");
            }
            catch (Exception ex)
            {
                this.HandlerError(ex);
            }
        }


        [Test]
        public void _03_UploadUpdaterZipTest()
        {
            try
            {
                UploadUpdater("Updater.zip");
            }
            catch (Exception ex)
            {
                this.HandlerError(ex);
            }
        }

        [Test]
        public void _04_RegisterAutomaticTestsClient()
        {
            try
            {
                this.RegisterApp("AutomaticTestsClient", AutomaticTestsClientTelemetryKey, "Telimena system tests app", "AutomaticTestsClient.exe", true, false);
            }
            catch (Exception ex)
            {
                this.HandlerError(ex);
            }
              
        }

        [Test]
        public void RegisterTempTestApp()
        {
            try
            {
                this.RegisterApp("Unit test app", null, "To be deleted", "Auto test TestPlugin.dll", true, false);

                this.RegisterApp("Unit test app", null, "To be deleted", "Auto test TestPlugin.dll", true, true);

                this.DeleteApp("Unit test app");
            }
            catch (Exception ex)
            {
                this.HandlerError(ex);
            }
            finally
            {
                var alert = this.Driver.WaitForAlert(500);
                alert?.Dismiss();
            }
        }

        private void DeleteApp(string appName)
        {
            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

            this.Driver.FindElement(By.Id($"{appName}_menu")).Click();
            IWebElement link = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id($"{appName}_manageLink")));

            link.Click();
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ProgramSummaryBox)));
            this.Driver.FindElement(By.Id(Strings.Id.DeleteProgramButton)).Click();
            var alert = this.Driver.WaitForAlert(10000, true);
            alert.Accept();
            Thread.Sleep(2000);
            alert = this.Driver.WaitForAlert(10000, true);
            alert.SendKeys(appName);
            alert.Accept();

            Thread.Sleep(2000);

            this.Driver.Navigate().Refresh();
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(@Strings.Id.PortalSummary)));
        }

        private void RegisterApp(string name, string key, string description, string assemblyName, bool canAlreadyExist, bool hasToExistAlready)
        {
            this.GoToAdminHomePage();

            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

            this.Driver.FindElement(By.Id(Strings.Id.RegisterApplicationLink)).Click();
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(@Strings.Id.RegisterApplicationForm)));
            string autoGeneratedGuid = null;
            if (key != null)
            {
                this.Driver.FindElement(By.Id(Strings.Id.TelemetryKeyInputBox)).Clear();
                this.Driver.FindElement(By.Id(Strings.Id.TelemetryKeyInputBox)).SendKeys(key);
            }
            else
            {
                IWebElement ele = this.Driver.FindElement(By.Id(Strings.Id.TelemetryKeyInputBox));

                autoGeneratedGuid = ele.GetAttribute("value");
                Assert.AreNotEqual(Guid.Empty, Guid.Parse(autoGeneratedGuid));
            }

            this.Driver.FindElement(By.Id(Strings.Id.ProgramNameInputBox)).SendKeys(name);
            this.Driver.FindElement(By.Id(Strings.Id.ProgramDescriptionInputBox)).SendKeys(description);
            this.Driver.FindElement(By.Id(Strings.Id.PrimaryAssemblyNameInputBox)).SendKeys(assemblyName);

            this.Driver.FindElement(By.Id(Strings.Id.SubmitAppRegistration)).Click();

            
                IAlert alert = this.Driver.WaitForAlert(10000);
            if (alert != null)
            {
                if (canAlreadyExist)
                {
                    if (alert.Text != "Use different telemetry key")
                    {
                        Assert.AreEqual($"A program with name [{name}] was already registered", alert.Text);
                    }
                    alert.Accept();
                    return;
                }
                else
                {
                    Assert.Fail("Test scenario expects that the app does not exist");
                }
            }
            else
            {
                if (hasToExistAlready)
                {
                    Assert.Fail("The app should already exist and the error was expected");
                }
            }

            IWebElement programTable = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ProgramSummaryBox)));

            var infoElements = programTable.FindElements(By.ClassName(Strings.Css.ProgramInfoElement));

            Assert.AreEqual(name, infoElements[0].Text);
            Assert.AreEqual(description, infoElements[1].Text);
            if (key != null)
            {
                Assert.AreEqual(key, infoElements[2].Text);
            }
            else
            {
                Assert.AreEqual(autoGeneratedGuid, infoElements[2].Text);
            }
            Assert.AreEqual(assemblyName, infoElements[3].Text);
        }
    }
}