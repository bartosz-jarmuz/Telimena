using System;
using System.Linq;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using Telimena.WebApp.UiStrings;
using Telimena.WebApp.UITests.Base;
using Telimena.WebApp.UITests.Base.TestAppInteraction;

namespace Telimena.WebApp.UITests.Ui
{
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
        public void UploadUpdaterExeTest()
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
        public void UploadUpdaterZipTest()
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
    }
}