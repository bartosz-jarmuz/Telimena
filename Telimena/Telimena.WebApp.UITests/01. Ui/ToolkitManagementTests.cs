using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Telimena.TestUtilities.Base;
using Telimena.TestUtilities.Base.TestAppInteraction;
using Telimena.WebApp.UiStrings;
using TelimenaClient;
using TelimenaClient.Model;

namespace Telimena.WebApp.UITests._01._Ui
{
    using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

    [TestFixture]
    [TestFixture()]
#if !DEBUG
    [Timeout(2 * 60 * 1000)]
#endif
    public partial class _1_WebsiteTests : WebsiteTestBase
    {

        [Test, Order(1), Retry(3)]
        public void _01_UploadToolkit()
        {
            try
            {
                this.GoToAdminHomePage();

                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

                this.Driver.FindElement(By.Id(Strings.Id.ToolkitManagementLink)).ClickWrapper(this.Driver);
                wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ToolkitManagementForm)));

                var file = TestAppProvider.GetFile("Telimena.Client.dll");


                this.Driver.FindElement(By.Id(Strings.Id.ToolkitPackageUploader)).SendKeys(file.FullName);


                wait.Until(x =>
                    x.FindElements(By.Id(Strings.Id.ToolkitUploadInfoBox))
                        .FirstOrDefault(e => e.Text.Contains(file.Name)));


                this.Driver.FindElement(By.Id(Strings.Id.SubmitToolkitUpload)).ClickWrapper(this.Driver);


                this.WaitForSuccessConfirmationWithText(wait
                    , z => (z.Contains("Uploaded package ") && z.Contains(" with ID ")));

            }
            catch (Exception ex)
            {
                this.HandleError(ex, SharedTestHelpers.GetMethodName());
            }
        }

        [Test, Order(2), Retry(3)]
        public void _02_UploadUpdaterExeTest()
        {
            try
            {
                this.UploadUpdater("Updater.exe", DefaultToolkitNames.UpdaterInternalName, null);
            }
            catch (Exception ex)
            {
                this.HandleError(ex, SharedTestHelpers.GetMethodName());
            }
        }

        [Test, Order(3), Retry(3)]
        public void _03_UploadUpdaterZipTest()
        {
            try
            {
                this.UploadUpdater("Updater.zip", DefaultToolkitNames.UpdaterInternalName, null);
            }
            catch (Exception ex)
            {
                this.HandleError(ex, SharedTestHelpers.GetMethodName());
            }

        }

        [Test, Order(3), Retry(3)]
        public void _03b_UploadPackageUpdaterZipTest()
        {
            try
            {
                this.UploadUpdater("PackageTriggerUpdater.zip", DefaultToolkitNames.PackageTriggerUpdaterInternalName, DefaultToolkitNames.PackageTriggerUpdaterFileName);
            }
            catch (Exception ex)
            {
                this.HandleError(ex, SharedTestHelpers.GetMethodName());
            }

        }

        [Test, Order(4), Retry(3)]
        public void _04a_SetDefaultUpdaterPublic()
        {
            try
            {

                this.SetUpdaterPublic(DefaultToolkitNames.UpdaterInternalName);
            }
            catch (Exception ex)
            {
                this.HandleError(ex, SharedTestHelpers.GetMethodName());
            }
        }

        [Test, Order(4), Retry(3)]
        public void _04b_SetPackageTriggerUpdaterPublic()
        {
            try
            {

                this.SetUpdaterPublic(DefaultToolkitNames.PackageTriggerUpdaterInternalName);
            }
            catch (Exception ex)
            {
                this.HandleError(ex, SharedTestHelpers.GetMethodName());
            }
        }

        private void SetUpdaterPublic(string internalName)
        {
            this.GoToAdminHomePage();
            this.Driver.FindElement(By.Id(Strings.Id.ToolkitManagementLink)).ClickWrapper(this.Driver);
            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

            var checkBox = this.GetIsPublicCheckbox(internalName);
            var isChecked = checkBox.GetAttribute("checked");

            if (isChecked == "true")
            {
                checkBox.ClickWrapper(this.Driver);
                Thread.Sleep(1000);

                this.WaitForErrorConfirmationWithText(wait, x=>x.Contains("Cannot change default updater"));
            }
            else
            {
                checkBox.ClickWrapper(this.Driver);
                Thread.Sleep(1000);
                this.Driver.FindElement(By.Id(Strings.Id.ToolkitManagementLink)).ClickWrapper(this.Driver);

                checkBox = this.GetIsPublicCheckbox(internalName);
                isChecked = checkBox.GetAttribute("checked");

                Assert.AreEqual("true", isChecked);
            }
          
        }

        private void UploadUpdater(string fileName, string internalName, string exeName)
        {
            this.GoToAdminHomePage();

            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

            this.Driver.FindElement(By.Id(Strings.Id.ToolkitManagementLink)).ClickWrapper(this.Driver);
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

            this.Driver.FindElement(By.Id(Strings.Id.SubmitUpdaterUpload)).ClickWrapper(this.Driver);


            this.WaitForSuccessConfirmationWithText(wait, x => x.Contains("Uploaded package "));

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