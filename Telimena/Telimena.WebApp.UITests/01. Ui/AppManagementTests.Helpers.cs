using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using Telimena.TestUtilities;
using Telimena.TestUtilities.Base;
using Telimena.TestUtilities.Base.TestAppInteraction;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.UiStrings;
using TelimenaClient;
using TelimenaClient.Model;
using static System.Reflection.MethodBase;

namespace Telimena.WebApp.UITests._01._Ui
{
    using ExpectedConditions = SeleniumExtras.WaitHelpers.ExpectedConditions;

    [SuppressMessage("ReSharper", "InconsistentNaming")]
    public partial class _1_WebsiteTests : WebsiteTestBase
    {
        private void UploadUpdatePackage(string appName, string packageFileName, [CallerMemberName] string caller = null)
        {
            try
            {

                this.LoginAdminIfNeeded();

                WebDriverWait wait = new WebDriverWait(this.Driver, ConfigHelper.GetTimeout());

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

                btn.ClickWrapper(this.Driver);
                this.WaitForSuccessConfirmationWithText(wait, x => StringAssert.Contains( "Uploaded package with ID",x));
                this.Driver.Navigate().Refresh();
                
                //wait for the reload and verify package uploaded and notes set OK
                this.VerifyReleaseNotesOnPkg( notes, packageFileName);

            }
            catch (Exception ex)
            {
                this.HandleError(ex, caller);
            }
        }

        private IWebElement GetUpdatesTableTopRow(WebDriverWait wait, string packageName = null)
        {

            var table = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ProgramUpdatePackagesTable)));
            var rows = table.FindElements(By.TagName("tr")).ToList();

            if (packageName == null)
            {
                return rows[1];

            }
            else
            {
                for (int index = 1; index < rows.Count; index++) //skip header row
                {
                    IWebElement webElement = rows[index];
                    var cells = webElement.FindElements(By.TagName("td"));
                    if (cells.Any(x => x.Text == packageName))
                    {
                        return webElement;
                    }
                }

                return null;
            }
        }

        private void VerifyReleaseNotesOnPkg(string expectedNotes, string packageName = null)
        {
            var wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

            var row = this.GetUpdatesTableTopRow(wait, packageName);
            var id = row.FindElements(By.TagName("td"))[1].Text;


            var showBtn = DOMExtensions.TryFind(() => row.FindElement(By.ClassName("expand"))
                , TimeSpan.FromSeconds(15));
            Assert.IsNotNull(showBtn);
            Actions actions = new Actions(this.Driver);
            actions.MoveToElement(showBtn);
            actions.Perform();

            wait.Until(ExpectedConditions.ElementToBeClickable(showBtn));
            showBtn.ClickWrapper(this.Driver);

            var nextRow = row.FindElement(By.XPath("following-sibling::*[1]"));
            IWebElement notesSection =
                DOMExtensions.TryFind(
                    () => nextRow.FindElements(By.TagName("pre")).ToList()
                        .FirstOrDefault(x => x.GetAttribute("data-pkg-id") == id), TimeSpan.FromSeconds(1));


            Assert.AreEqual(expectedNotes, notesSection.Text);
        }

        private async Task SendBasicTelemetry(Guid guid)
        {

            await this.SendBasicTelemetry(guid, new BasicTelemetryItem()
            {
                Metrics = new Dictionary<string, double>()
                {
                    {"Something", 123.3 }
                },
                ProgramVersion = "1.2.3.4",
            }).ConfigureAwait(false);
            await this.SendBasicTelemetry(guid, new BasicTelemetryItem()
            {
                EventName = "SomeViewEvent",
                TelemetryItemType = TelemetryItemTypes.View,
                Metrics = new Dictionary<string, double>()
                {
                    {"SomethingForView", 123.3 }
                },
                ProgramVersion = "1.2.3.4",
            }).ConfigureAwait(false);
            await this.SendBasicTelemetry(guid, new BasicTelemetryItem()
            {
                TelemetryItemType = TelemetryItemTypes.LogMessage,
                LogMessage = "Hello, world",
            }).ConfigureAwait(false);

            TelemetryQueryRequest request = TelemetryQueryRequest.CreateFull(guid);
            TelemetryQueryResponse queryResponse = await this.CheckTelemetry(request).ConfigureAwait(false);
            var viewTelemetry = queryResponse.TelemetryAware.Single(x => x.ComponentKey == "SomeViewEvent");
            var eventTelemetry = queryResponse.TelemetryAware.Single(x => x.ComponentKey == "DefaultEvent");
            Assert.AreEqual("1.2.3.4", viewTelemetry.Summaries.Single().Details.Single().FileVersion);
            Assert.AreEqual("1.2.3.4", eventTelemetry.Summaries.Single().Details.Single().FileVersion);
        }

        private void SetReleaseNotesOnExistingPkg(string notes)
        {
            var wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));
            var table = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.ProgramUpdatePackagesTable)));


            var setBtn = DOMExtensions.TryFind(() => table.FindElements(By.TagName("tr"))[1].FindElement(By.ClassName(Strings.Css.PrepareReleaseNotesButton))
                , TimeSpan.FromSeconds(15));

            Actions actions = new Actions(this.Driver);
            actions.MoveToElement(setBtn);
            actions.Perform();

            wait.Until(ExpectedConditions.ElementToBeClickable(setBtn));

            setBtn.ClickWrapper(this.Driver);

            this.FillInReleaseNotesModal(wait, notes);
        }

        private void SetReleaseNotesOnPackageUpload(IWebElement form, string notes)
        {
            var wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

            var btn = wait.Until(ExpectedConditions.ElementToBeClickable(
                form.FindElement(By.ClassName(@Strings.Css.PrepareReleaseNotesButton))));

            btn.ClickWrapper(this.Driver);

            this.FillInReleaseNotesModal(wait, notes);
        }

        private void FillInReleaseNotesModal(WebDriverWait wait, string notes)
        {

            var modal = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.SetReleaseNotesModal)));

            var area = modal.FindElement(By.TagName("textarea"));
            area.Clear();
            area.SendKeys(notes);

            var submit = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(Strings.Id.SubmitReleaseNotesButton)));
            submit.ClickWrapper(this.Driver);
        }
      
        private void SetPackageAsBeta(string packageName)
        {
            var wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));
            var row = this.GetUpdatesTableTopRow(wait, packageName);

            Log($"Setting update as beta in top row: {row.Text}");


            var box = row.FindElement(By.ClassName(Strings.Css.PackageBetaToggle));

            if (box.Selected)
            {
                Log("Box was initially selected.");
                box.ClickWrapper(this.Driver);

                this.WaitForSuccessConfirmationWithText(wait, x => StringAssert.Contains( "Beta flag to: false", x));
                Log("Box deselected. Rerunning function.");
                this.SetPackageAsBeta( packageName);
            }
            else
            {
                Log("Box is not selected");

                box.ClickWrapper(this.Driver);

                this.WaitForSuccessConfirmationWithText(wait, x => StringAssert.Contains( "Beta flag to: true", x));
                this.Driver.Navigate().Refresh();
                Log("Refreshed page");

                row = this.GetUpdatesTableTopRow(wait, packageName);
                Log($"Setting update as bet in top row: {row.Text}");

                box = row.FindElement(By.ClassName(Strings.Css.PackageBetaToggle));

                Assert.IsTrue(box.Selected);
                Log("Box is selected");
            }
        }


        private void SetPackageAsNonBeta(string packageName)
        {
            var wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));
            var row = this.GetUpdatesTableTopRow(wait, packageName);

            Log($"Setting update as non-beta in top row: {row.Text}");


            var box = row.FindElement(By.ClassName(Strings.Css.PackageBetaToggle));

            if (!box.Selected)
            {
                Log("Box was initially deselected.");
                box.ClickWrapper(this.Driver);

                this.WaitForSuccessConfirmationWithText(wait, x => StringAssert.Contains( "Beta flag to: true", x));
                Log("Box selected. Rerunning function.");
                this.SetPackageAsNonBeta(packageName);
            }
            else
            {
                Log("Box is selected");

                box.ClickWrapper(this.Driver);

                this.WaitForSuccessConfirmationWithText(wait, x => StringAssert.Contains( "Beta flag to: false", x));
                this.Driver.Navigate().Refresh();
                Log("Refreshed page");

                row = this.GetUpdatesTableTopRow(wait, packageName);
                Log($"Setting update as non-beta in top row: {row.Text}");

                box = row.FindElement(By.ClassName(Strings.Css.PackageBetaToggle));

                Assert.IsFalse(box.Selected);
                Log("Box is deselected");
            }
        }

        private void DeleteApp(string appName, bool maybeNotExists)
        {
            this.GoToAdminHomePage();

            WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(45));

            IWebElement element = this.Driver.TryFind(By.Id($"{appName}_menu"));
            if (element == null)
            {
                if (maybeNotExists)
                {
                    Log("Temp test app did not exist");

                    return;

                }
                else
                {
                    Assert.Fail("Failed to find app button");
                }
            }

            element.ClickWrapper(this.Driver);
            IWebElement link = wait.Until(ExpectedConditions.ElementIsVisible(By.Id($"{appName}_manageLink")));

            link.ClickWrapper(this.Driver);
            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.DeleteProgramTab + "Id"))).ClickWrapper(this.Driver);

            var deleteBtn = wait.Until(ExpectedConditions.ElementToBeClickable(By.Id(Strings.Id.DeleteProgramButton)));
            deleteBtn.ClickWrapper(this.Driver);
            Log("Delete button clicked");
            var alert = this.Driver.WaitForAlert(10000, true);
            alert.Accept();
            Log("Are you sure alert accepted");

            Thread.Sleep(2000);
            alert = this.Driver.WaitForAlert(10000, true);
            alert.SendKeys(appName);
            alert.Accept();
            Log("Provide app name alert accepted");

            Thread.Sleep(2000);
            this.WaitForSuccessConfirmationWithText(wait, x => StringAssert.Contains( "deleted successfully", x));
            Log("Success alert displayed");

            this.Driver.Navigate().Refresh();
            Log("Refresh clicked");

            wait.Until(ExpectedConditions.ElementIsVisible(By.Id(@Strings.Id.PortalSummary)));
            Log("Temp test app deleted");
        }

        private void UploadProgramPackage(string appName, string packageFileName, [CallerMemberName] string caller = null)
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

                btn.ClickWrapper(this.Driver);
                Log($"Submitted upload of {file.FullName}");

                this.WaitForSuccessConfirmationWithText(wait, x => StringAssert.Contains( "Uploaded package with ID", x));
                this.Driver.Navigate().Refresh();

            }
            catch (Exception ex)
            {
                this.HandleError(ex, caller);
            }
        }

        private void DownloadAndInstallMsiProgramPackage(string appName, string packageFileName, [CallerMemberName] string caller = null)
        {
            try
            {

                this.LoginAdminIfNeeded();

                WebDriverWait wait = new WebDriverWait(this.Driver, TimeSpan.FromSeconds(15));

                this.ClickOnManageProgramMenu(appName);

                IWebElement link = wait.Until(ExpectedConditions.ElementIsVisible(By.Id(Strings.Id.DownloadProgramLink)));



                IntegrationTestBase.Log($"Clicking on download URL to store file in {DownloadPath}");

                FileInfo file = this.ActAndWaitForFileDownload(() => link.ClickWrapper(this.Driver), packageFileName, TimeSpan.FromSeconds(80)
                    , DownloadPath);

                IntegrationTestBase.Log($"File downloaded {file.FullName}. ");

                this.InstallMsi3AndVerify(file);

                IntegrationTestBase.Log($"Deleting downloaded file {file.FullName}. ");

                file.Delete();
            }
            catch (Exception ex)
            {
                this.HandleError(ex, caller);
            }
        }

        private void InstallMsi3AndVerify(FileInfo msi)
        {
            this.UninstallMsi(Apps.ProductCodes.InstallersTestAppMsi3V1, Apps.Paths.InstallersTestAppMsi3);

            Assert.IsFalse(File.Exists(Apps.Paths.InstallersTestAppMsi3.FullName));
            IntegrationTestBase.Log($"Installing {msi.FullName}.");

            this.InstallMsi(msi, Apps.Paths.InstallersTestAppMsi3);
            IntegrationTestBase.Log($"Finished installing {msi.FullName}.");

            Assert.IsTrue(File.Exists(Apps.Paths.InstallersTestAppMsi3.FullName));
            this.UninstallMsi(Apps.ProductCodes.InstallersTestAppMsi3V1, Apps.Paths.InstallersTestAppMsi3);
            Assert.IsFalse(File.Exists(Apps.Paths.InstallersTestAppMsi3.FullName));


        }
        private string GetUpdaterForApp(string appName, [CallerMemberName] string caller = null)
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
                this.HandleError(ex, caller);
                throw;

            }
        }

        private void SetUpdaterForApp(string appName, string updaterName, [CallerMemberName] string caller = null)
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
                this.Driver.FindElement(By.Id(Strings.Id.SubmitUpdaterChange)).ClickWrapper(this.Driver);

                this.WaitForSuccessConfirmationWithText(wait, x => StringAssert.Contains( "Updater set to " + updaterName, x));
            }
            catch (Exception ex)
            {
                this.HandleError(ex, caller);
            }
        }
    }


}