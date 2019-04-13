using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.Deployment.WindowsInstaller;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Telimena.WebApp.UiStrings;
using Telimena.WebApp.UITests.Base;
using Telimena.WebApp.UITests.Base.TestAppInteraction;
using TelimenaClient;
using TelimenaClient.Model;
using static System.Reflection.MethodBase;

namespace Telimena.WebApp.UITests._01._Ui
{
    using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial class _1_UiTests : UiTestBase
    {
        public void UploadUpdatePackage(string appName, string packageFileName)
        {
            try
            {

                this.LoginAdminIfNeeded();

                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

                this.ClickOnManageProgramMenu(appName);

                IWebElement form = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.UploadProgramUpdateForm)));
                FileInfo file = TestAppProvider.GetFile(packageFileName);

                IWebElement input = form.FindElements(By.TagName("input")).FirstOrDefault(x => x.GetAttribute("type") == "file");
                if (input == null)
                {
                    Assert.Fail("Input box not found");
                }

                input.SendKeys(file.FullName);

                wait.Until(x => form.FindElements(By.ClassName("info")).FirstOrDefault(e => e.Text.Contains(file.Name)));
                var notes = GetCurrentMethod().Name + DateTimeOffset.UtcNow.ToString("O");
                this.SetReleaseNotesOnPackageUpload(form,  notes);
                // ReSharper disable once PossibleNullReferenceException

                var btn = wait.Until(ExpectedConditions.ElementToBeClickable(form.FindElements(By.TagName("input"))
                    .FirstOrDefault(x => x.GetAttribute("type") == "submit")));

                btn.Click();
                this.WaitForSuccessConfirmationWithText(wait, x=>x.Contains("Uploaded package with ID"));
                this.Driver.Navigate().Refresh();
                
                //wait for the reload and verify package uploaded and notes set OK
                this.VerifyReleaseNotesOnPkg( notes);

            }
            catch (Exception ex)
            {
                this.HandleError(ex);
            }
        }

        private IWebElement GetUpdatesTableTopRow(WebDriverWait wait)
        {

            var table = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ProgramUpdatePackagesTable)));

            var rows = table.FindElements(By.TagName("tr")).ToList();
            return rows[1];
        }

        private void VerifyReleaseNotesOnPkg(string expectedNotes)
        {
            var wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

            var row = this.GetUpdatesTableTopRow(wait);
            var id = row.FindElements(By.TagName("td"))[1].Text;


            var showBtn = this.TryFind(() => row.FindElement(By.ClassName("expand"))
                , TimeSpan.FromSeconds(15));
            Assert.IsNotNull(showBtn);
            Actions actions = new Actions(this.Driver);
            actions.MoveToElement(showBtn);
            actions.Perform();

            wait.Until(ExpectedConditions.ElementToBeClickable(showBtn));
            showBtn.Click();

            var nextRow = row.FindElement(By.XPath("following-sibling::*[1]"));
            IWebElement notesSection =
                this.TryFind(
                    () => nextRow.FindElements(By.TagName("pre")).ToList()
                        .FirstOrDefault(x => x.GetAttribute("data-pkg-id") == id), TimeSpan.FromSeconds(1));


            Assert.AreEqual(expectedNotes, notesSection.Text);
        }


        private void SetReleaseNotesOnExistingPkg(string notes)
        {
            var wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));
            var table = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ProgramUpdatePackagesTable)));


            var setBtn = this.TryFind(() => table.FindElements(By.TagName("tr"))[1].FindElement(By.ClassName(Strings.Css.PrepareReleaseNotesButton))
                , TimeSpan.FromSeconds(15));

            Actions actions = new Actions(this.Driver);
            actions.MoveToElement(setBtn);
            actions.Perform();

            wait.Until(ExpectedConditions.ElementToBeClickable(setBtn));

            setBtn.Click();

            this.FillInReleaseNotesModal(wait, notes);
        }

        private void SetReleaseNotesOnPackageUpload(IWebElement form, string notes)
        {
            var wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

            var btn = wait.Until(ExpectedConditions.ElementToBeClickable(
                form.FindElement(By.ClassName(@Strings.Css.PrepareReleaseNotesButton))));

            btn.Click();

            this.FillInReleaseNotesModal(wait, notes);
        }

        private void FillInReleaseNotesModal(WebDriverWait wait, string notes)
        {

            var modal = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.SetReleaseNotesModal)));

            var area = modal.FindElement(By.TagName("textarea"));
            area.Clear();
            area.SendKeys(notes);

            var submit = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(Strings.Id.SubmitReleaseNotesButton)));
            submit.Click();
        }

      
        public void SetPackageAsBeta()
        {
            var wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));
            var row = this.GetUpdatesTableTopRow(wait);

            var box = row.FindElement(By.ClassName(Strings.Css.PackageBetaToggle));

            if (box.Selected)
            {
                box.Click();

                this.WaitForSuccessConfirmationWithText(wait, x => x.Contains("Beta flag to: false"));
                this.SetPackageAsBeta();
            }
            else
            {
                box.Click();

                this.WaitForSuccessConfirmationWithText(wait, x => x.Contains("Beta flag to: true"));
                this.Driver.Navigate().Refresh();

                row = this.GetUpdatesTableTopRow(wait);

                box = row.FindElement(By.ClassName(Strings.Css.PackageBetaToggle));

                Assert.IsTrue(box.Selected);
            }

          

        }



        private void DeleteApp(string appName, bool maybeNotExists)
        {
            this.GoToAdminHomePage();

            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

            IWebElement element = this.TryFind(By.Id($"{appName}_menu"));
            if (element == null)
            {
                if (maybeNotExists)
                {
                    return;

                }
                else
                {
                    Assert.Fail("Failed to find app button");
                }
            }

            element.Click();
            IWebElement link = wait.Until(ExpectedConditions.ElementIsVisible(By.Id($"{appName}_manageLink")));

            link.Click();
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.DeleteProgramTab + "Id"))).Click();

            var deleteBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(Strings.Id.DeleteProgramButton)));
            deleteBtn.Click();
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

        private void RegisterApp(string name, Guid? key, string description, string assemblyName, bool canAlreadyExist, bool hasToExistAlready)
        {
            this.GoToAdminHomePage();

            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

            this.Driver.FindElement(By.Id(Strings.Id.RegisterApplicationLink)).Click();
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(@Strings.Id.RegisterApplicationForm)));
            string autoGeneratedGuid = null;
            if (key != null)
            {
                this.Driver.FindElement(By.Id(Strings.Id.TelemetryKeyInputBox)).Clear();
                this.Driver.FindElement(By.Id(Strings.Id.TelemetryKeyInputBox)).SendKeys(key.ToString());
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
                        Assert.AreEqual($"A program with name [{name}] was already registered by TelimenaSystemDevTeam", alert.Text);
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

            IWebElement programTable = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ProgramSummaryBox)));

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

        public void UploadProgramPackage(string appName, string packageFileName)
        {
            try
            {

                this.LoginAdminIfNeeded();

                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

                this.ClickOnManageProgramMenu(appName);

                IWebElement form = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.UploadFullProgramPackageForm)));
                FileInfo file = TestAppProvider.GetFile(packageFileName);

                IWebElement input = form.FindElements(By.TagName("input")).FirstOrDefault(x => x.GetAttribute("type") == "file");
                if (input == null)
                {
                    Assert.Fail("Input box not found");
                }

                input.SendKeys(file.FullName);
                Log($"Uploading {file.FullName}");
                wait.Until(x => form.FindElements(By.ClassName("info")).FirstOrDefault(e => e.Text.Contains(file.Name)));

                var btn = wait.Until(ExpectedConditions.ElementToBeClickable(form.FindElements(By.TagName("input"))
                    .FirstOrDefault(x => x.GetAttribute("type") == "submit")));

                btn.Click();
                Log($"Submitted upload of {file.FullName}");

                this.WaitForSuccessConfirmationWithText(wait, x => x.Contains("Uploaded package with ID"));
                this.Driver.Navigate().Refresh();

            }
            catch (Exception ex)
            {
                this.HandleError(ex);
            }
        }

        private void DownloadAndInstallMsiProgramPackage(string appName, string packageFileName)
        {
            try
            {

                this.LoginAdminIfNeeded();

                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

                this.ClickOnManageProgramMenu(appName);

                IWebElement link = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.DownloadProgramLink)));



                this.Log($"Clicking on download URL to store file in {DownloadPath}");

                FileInfo file = this.ActAndWaitForFileDownload(() => link.Click(), packageFileName, TimeSpan.FromSeconds(80)
                    , DownloadPath);

                this.Log($"File downloaded {file.FullName}. ");

                this.InstallMsi3AndVerify(file);

                this.Log($"Deleting downloaded file {file.FullName}. ");

                file.Delete();
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
            }
        }

        private void InstallMsi3AndVerify(FileInfo msi)
        {
            this.UninstallMsi(Apps.ProductCodes.InstallersTestAppMsi3V1, Apps.Paths.InstallersTestAppMsi3);

            Assert.IsFalse(File.Exists(Apps.Paths.InstallersTestAppMsi3.FullName));
            this.Log($"Installing {msi.FullName}.");

            this.InstallMsi(msi, Apps.Paths.InstallersTestAppMsi3);
            this.Log($"Finished installing {msi.FullName}.");

            Assert.IsTrue(File.Exists(Apps.Paths.InstallersTestAppMsi3.FullName));
            this.UninstallMsi(Apps.ProductCodes.InstallersTestAppMsi3V1, Apps.Paths.InstallersTestAppMsi3);
            Assert.IsFalse(File.Exists(Apps.Paths.InstallersTestAppMsi3.FullName));


        }
        private string GetUpdaterForApp(string appName)
        {
            try
            {
                this.LoginAdminIfNeeded();

                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

                this.ClickOnManageProgramMenu(appName);

                IWebElement input = this.Driver.FindElement(By.Id(Strings.Id.UpdaterSelectList));
                if (input == null)
                {
                    Assert.Fail("Select list box not found");
                }

                return new SelectElement(input).SelectedOption.Text;

            }
            catch (Exception ex)
            {
                this.HandleError(ex);
                throw;

            }
        }

        private void SetUpdaterForApp(string appName, string updaterName)
        {
            try
            {
                this.LoginAdminIfNeeded();

                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

                this.ClickOnManageProgramMenu(appName);

                IWebElement input = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.UpdaterSelectList)));

                if (input == null)
                {
                    Assert.Fail("Select list box not found");
                }

                new SelectElement(input).SelectByText(updaterName);
                this.Driver.FindElement(By.Id(Strings.Id.SubmitUpdaterChange)).Click();

                this.WaitForSuccessConfirmationWithText(wait, x=>x.Contains("Updater set to "+ updaterName));
            }
            catch (Exception ex)
            {
                this.HandleError(ex);
            }
        }
    }


}