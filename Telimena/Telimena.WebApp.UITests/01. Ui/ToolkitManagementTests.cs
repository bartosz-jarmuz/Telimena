using System;
using System.Linq;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Telimena.WebApp.UiStrings;
using Telimena.WebApp.UITests.Base;
using Telimena.WebApp.UITests.Base.TestAppInteraction;
using TelimenaClient;
using TelimenaClient.Model;

namespace Telimena.WebApp.UITests._01._Ui
{
    using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

    [TestFixture]
    [TestFixture(), Timeout(1 * 60 * 1000)]
    public partial class _1_UiTests : UiTestBase
    {

        private void UploadUpdater(string fileName, string internalName, string exeName)
        {
            this.GoToAdminHomePage();

            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

            this.Driver.FindElement(By.Id(Strings.Id.ToolkitManagementLink)).Click();
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ToolkitManagementForm)));

            var updater = TestAppProvider.GetFile(fileName);
            this.Driver.FindElement(By.Id(Strings.Id.UpdaterInternalName)).Clear();
            this.Driver.FindElement(By.Id(Strings.Id.UpdaterInternalName)).SendKeys(internalName);

            if (exeName != null)
            {
                this.Driver.FindElement(By.Id(Strings.Id.UpdaterExecutableName)).Clear();
                this.Driver.FindElement(By.Id(Strings.Id.UpdaterExecutableName)).SendKeys(exeName);
            }

            this.Driver.FindElement(By.Id(Strings.Id.UpdaterPackageUploader)).SendKeys(updater.FullName);

            wait.Until(x => x.FindElements(By.Id(Strings.Id.UpdaterUploadInfoBox)).FirstOrDefault(e => e.Text.Contains(updater.Name)));

            this.Driver.FindElement(By.Id(Strings.Id.SubmitUpdaterUpload)).Click();

            var confirmationBox = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.UpdaterConfirmationBox)));

            Assert.IsTrue(confirmationBox.GetAttribute("class").Contains("label-success"));

            Assert.IsTrue(confirmationBox.Text.Contains("Uploaded package "));

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

            Assert.IsTrue(confirmationBox.Text.Contains("Uploaded package "));
            Assert.IsTrue(confirmationBox.Text.Contains(" with ID "));

        }

        [Test]
        public void _02_UploadUpdaterExeTest()
        {
            try
            {
                this.UploadUpdater("Updater.exe", DefaultToolkitNames.UpdaterInternalName, null);
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
            }
        }


        [Test]
        public void _03_UploadUpdaterZipTest()
        {
            try
            {
                this.UploadUpdater("Updater.zip", DefaultToolkitNames.UpdaterInternalName, null);
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
            }
        }

        [Test]
        public void _03b_UploadPackageUpdaterZipTest()
        {
            try
            {
                this.UploadUpdater("PackageTriggerUpdater.zip", DefaultToolkitNames.PackageTriggerUpdaterInternalName, DefaultToolkitNames.PackageTriggerUpdaterFileName);
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
            }
        }

        [Test]
        public void _04a_SetDefaultUpdaterPublic()
        {
            this.SetUpdaterPublic(DefaultToolkitNames.UpdaterInternalName);
        }

        [Test]
        public void _04b_SetPackageTriggerUpdaterPublic()
        {
            this.SetUpdaterPublic(DefaultToolkitNames.PackageTriggerUpdaterInternalName);
        }

        private void SetUpdaterPublic(string internalName)
        {
            try
            {
                this.GoToAdminHomePage();
                this.Driver.FindElement(By.Id(Strings.Id.ToolkitManagementLink)).Click();
                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

                var checkBox = this.GetIsPublicCheckbox(internalName);
                var isChecked = checkBox.GetAttribute("checked");

                if (isChecked == "true")
                {
                    checkBox.Click();
                    Thread.Sleep(1000);
                    var table = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.UpdaterPackagesTable)));

                    var label = table.FindElement(By.XPath("../../label"));
                    Assert.AreEqual(true, label.Displayed);
                    Assert.AreEqual("Cannot change default updater", label.Text);
                }
                else
                {
                    checkBox.Click();
                    Thread.Sleep(1000);
                    this.Driver.FindElement(By.Id(Strings.Id.ToolkitManagementLink)).Click();

                    checkBox = this.GetIsPublicCheckbox(internalName);
                    isChecked = checkBox.GetAttribute("checked");

                    Assert.AreEqual("true", isChecked);
                }
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
            }
        }

        private IWebElement GetIsPublicCheckbox(string internalName)
        {
            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));
            var table = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.UpdaterPackagesTable)));

            var row = table.FindElements(By.TagName("tr")).FirstOrDefault(x => x.Text.Contains(internalName));
            if (row == null)
            {
                Assert.Fail($"There is no row for the {internalName} updater in the system. Cannot set public");
            }

            var checkBox = row.FindElements(By.TagName("input")).FirstOrDefault(x => x.GetAttribute("name") == @Strings.Name.ToggleIsPublic);

            if (checkBox == null)
            {
                Assert.Fail("Failed to find checkbox. Cannot set public");
            }

            return checkBox;
        }

    }

}