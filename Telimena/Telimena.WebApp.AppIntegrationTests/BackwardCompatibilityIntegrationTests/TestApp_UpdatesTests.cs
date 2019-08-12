using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using SharedLogic;
using Telimena.TestUtilities.Base;
using Telimena.TestUtilities.Base.TestAppInteraction;
using Telimena.WebApp.AppIntegrationTests.Utilities;
using TelimenaClient;
using TelimenaClient.Model.Internal;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.WindowItems;

namespace Telimena.WebApp.AppIntegrationTests.BackwardCompatibilityIntegrationTests
{
  


    [TestFixture(), Timeout(3*60*1000)]
    public partial class _2_NonUiTests : IntegrationTestBase
    {
        [Test]
        public void ProductCodesTest_InstallersTestAppMsi3()
        {
            var code1 = this.GetCodeFromMsi(Apps.PackageNames.InstallersTestAppMsi3V1);
            NUnit.Framework.Assert.AreEqual(code1, Apps.ProductCodes.InstallersTestAppMsi3V1);

            var code2 = this.GetCodeFromMsi(Apps.PackageNames.InstallersTestAppMsi3V2);
            NUnit.Framework.Assert.AreEqual(code2, Apps.ProductCodes.InstallersTestAppMsi3V2);

            NUnit.Framework.Assert.AreNotEqual(code1, code2);
        }

        [Test]
        public async Task _01_HandlePackageUpdates_NonBeta()
        {
            try
            {
              
                this.LaunchPackageUpdaterTestsAppWithArgs(out FileInfo appFile, Apps.PackageNames.PackageUpdaterTestAppV1, SharedTestHelpers.GetMethodName(), waitForExit: false);

                Window updateNowMsgBox =
                    await WindowHelpers.WaitForWindowAsync(x => x.Equals("PackageTriggerUpdaterTestApp update download"), TimeSpan.FromMinutes(2)).ConfigureAwait(false);
                updateNowMsgBox.Get<Button>(SearchCriteria.ByText("Yes")).Click();
                Log("Clicked yes");

                Window executed = await WindowHelpers.WaitForWindowAsync(x => x.Equals("Updater executed"), TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                executed.Get<Button>(SearchCriteria.ByText("OK")).Click();
                Log("Clicked OK");

                Window doneMsg = await WindowHelpers.WaitForWindowAsync(x => x.Equals("Updater finished"), TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                string text = this.GetTextFromMsgBox(doneMsg);
                Assert.AreEqual("Killed other processes: True", text);

                //do not check if app was updated, because we only care whether the updater was actually launched

            }
            catch (Exception ex)
            {
                throw this.CleanupAndRethrow(ex);
            }

        }

        [Test]
        public async Task _02_HandleUpdates_NonBeta()
        {
            try
            {

                VersionTuple initialVersions = await this.GetVersionsFromApp(Apps.PackageNames.AutomaticTestsClientAppV1, SharedTestHelpers.GetMethodName()).ConfigureAwait(false);

                UpdateCheckResult result = this.LaunchTestsAppNewInstanceAndGetResult<UpdateCheckResult>(out FileInfo appFile, Actions.HandleUpdates, Apps.Keys.AutomaticTestsClient, Apps.PackageNames.AutomaticTestsClientAppV1
                    , SharedTestHelpers.GetMethodName(), waitForExit: false);

                Window updateNowMsgBox =
                    await WindowHelpers.WaitForWindowAsync(x => x.Equals("AutomaticTestsClient update installation"), TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                this.CheckProperUpdateVersionDownloadedInMessageBo(updateNowMsgBox, "2.");

                updateNowMsgBox.Get<Button>(SearchCriteria.ByText("Yes")).Click();
                Log("Clicked yes");
                Window updater = await WindowHelpers.WaitForWindowAsync(x => x.Contains("AutomaticTestsClient Updater"), TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                updater.Get<Button>(SearchCriteria.ByText("Install now!")).Click();
                Log("Clicked Install now!");

                Window doneMsg = await WindowHelpers.WaitForMessageBoxAsync(updater, "Update complete", TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                doneMsg.Get<Button>(SearchCriteria.ByText("Yes")).Click();
                Log("Clicked yes");

                Window appWarning = await WindowHelpers.WaitForWindowAsync(x => x.Equals("AutomaticTestsClient - This app requires arguments to run")
                    , TimeSpan.FromMinutes(2)).ConfigureAwait(false);
                Log("Reading versions");

                VersionTuple newVersions = await this.GetVersionFromMsgBox(appWarning).ConfigureAwait(false);

                this.AssertVersionAreCorrect(newVersions, initialVersions, appFile, "2.");

                Log("Checking update info is false");
                //now just assert that the update check result is empty next time
                 result = this.LaunchTestsAppAndGetResult<UpdateCheckResult>(appFile, Actions.CheckAndInstallUpdates, Apps.Keys.AutomaticTestsClient, waitForExit: true);

                this.AssertNoNonBetaUpdatesToInstall(result, true);

            }
            catch (Exception ex)
            {
                throw this.CleanupAndRethrow(ex);
            }
        }

        [Test]
        public async Task _03_CheckAndInstallUpdates_NonBeta()
        {
            try
            {
                VersionTuple initialVersions = this.GetVersionsFromExtractedAppFile(Apps.PackageNames.AutomaticTestsClientAppV1, SharedTestHelpers.GetMethodName(), out FileInfo appFile);
                var installationResult = this.LaunchTestsAppAndGetResult<UpdateInstallationResult>(appFile, Actions.CheckAndInstallUpdates, Apps.Keys.AutomaticTestsClient, waitForExit: false);

                if (installationResult?.Exception != null)
                {
                    throw installationResult.Exception;
                }

                Window updater = await WindowHelpers.WaitForWindowAsync(x => x.Contains("AutomaticTestsClient Updater"), TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                this.CheckProperUpdateVersionDownloadedInUpdater(updater, "2.");


                updater.Get<Button>(SearchCriteria.ByText("Install now!")).Click();
                Log("Clicked Install now!");

                Window doneMsg = await WindowHelpers.WaitForMessageBoxAsync(updater, "Update complete", TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                doneMsg.Get<Button>(SearchCriteria.ByText("Yes")).Click();
                Log("Clicked yes");

                Window appWarning = await WindowHelpers.WaitForWindowAsync(x => x.Equals("AutomaticTestsClient - This app requires arguments to run")
                    , TimeSpan.FromMinutes(2)).ConfigureAwait(false);
                VersionTuple newVersions = await this.GetVersionFromMsgBox(appWarning).ConfigureAwait(false);
                Log("Reading versions");

                this.AssertVersionAreCorrect(newVersions, initialVersions, appFile, "2.");

                //now just assert that the update check result is empty next time
                Log("Checking update info is false");

                UpdateCheckResult result = this.LaunchTestsAppAndGetResult<UpdateCheckResult>(appFile, Actions.CheckAndInstallUpdates, Apps.Keys.AutomaticTestsClient, waitForExit: true);
                this.AssertNoNonBetaUpdatesToInstall(result, true);
            }
            catch (Exception ex)
            {
                throw this.CleanupAndRethrow(ex);
            }
        }

        [Test]
        public async Task _04_MsiCheckAndInstallUpdates()
        {
            this.ProductCodesTest_InstallersTestAppMsi3(); //sanity check to save time
            try
            {

                this.UninstallPackages(Apps.ProductCodes.InstallersTestAppMsi3V1, Apps.ProductCodes.InstallersTestAppMsi3V2);

                VersionTuple initialVersions = this.GetVersionsFromMsiApp(Apps.PackageNames.InstallersTestAppMsi3V1, Apps.Paths.InstallersTestAppMsi3, Apps.ProductCodes.InstallersTestAppMsi3V1);
                Log("Read initial versions");

                UpdateCheckResult result =  this.LaunchTestsAppAndGetResult<UpdateCheckResult>(Apps.Paths.InstallersTestAppMsi3, Actions.CheckAndInstallUpdates, Apps.Keys.InstallersTestAppMsi3);
                Assert.IsNull(result.Exception);
                Window updater = await WindowHelpers.WaitForWindowAsync(x => x.Contains("InstallersTestApp.Msi3Installer"), TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                updater.Get<Button>(SearchCriteria.ByText("OK")).Click();
                Log("Clicked OK");


                this.VerifyVersionsAreUpdatedAfterInstallation(initialVersions);


                //now just assert that the update check result is empty next time
                Log("Checking update info is false");
                result = this.LaunchTestsAppAndGetResult<UpdateCheckResult>(Apps.Paths.InstallersTestAppMsi3, Actions.CheckAndInstallUpdates, Apps.Keys.InstallersTestAppMsi3);

                this.AssertNoNonBetaUpdatesToInstall(result, false);

                this.UninstallPackages(Apps.ProductCodes.InstallersTestAppMsi3V1, Apps.ProductCodes.InstallersTestAppMsi3V2);

            }
            catch (Exception ex)
            {
                throw this.CleanupAndRethrow(ex);
            }
        }

        private void VerifyVersionsAreUpdatedAfterInstallation(VersionTuple initialVersions)
        {
            int i = 0;
            while (true)
            {
                i++;
                Thread.Sleep(1000 * i * 2);
                VersionTuple newVersions = this.GetVersionsFromFile(Apps.Paths.InstallersTestAppMsi3);

                try
                {

                    Assert.IsTrue(newVersions.AssemblyVersion.IsNewerVersionThan(initialVersions.AssemblyVersion)
                        , $"Version don't match. Initial AssVer: {initialVersions.AssemblyVersion}. New {newVersions.AssemblyVersion}");
                    Assert.IsTrue(newVersions.FileVersion.IsNewerVersionThan(initialVersions.FileVersion), 
                        $"Version don't match. Initial FileVer: {initialVersions.AssemblyVersion}. New {newVersions.AssemblyVersion}");
                    return;
                }
                catch (Exception)
                {
                    if (i > 4)
                    {
                        throw;
                    }
                }
            }
           
        }

        [Test]
        public async Task _05_CheckAndInstallUpdates_Beta()
        {
            try
            {
                VersionTuple initialVersions = this.GetVersionsFromExtractedAppFile(Apps.PackageNames.AutomaticTestsClientAppV1, SharedTestHelpers.GetMethodName(), out FileInfo appFile);

                this.LaunchTestsAppAndGetResult<UpdateCheckResult>(appFile, Actions.HandleUpdatesWithBeta, Apps.Keys.AutomaticTestsClient, waitForExit: false);

                Window updateNowMsgBox =
                    await WindowHelpers.WaitForWindowAsync(x => x.Equals("AutomaticTestsClient update installation"), TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                updateNowMsgBox.Get<Button>(SearchCriteria.ByText("Yes")).Click();
                Log("Clicked yes");

                Window updater = await WindowHelpers.WaitForWindowAsync(x => x.Contains("AutomaticTestsClient Updater"), TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                updater.Get<Button>(SearchCriteria.ByText("Install now!")).Click();
                Log("Clicked Install now!");

                Window doneMsg = await WindowHelpers.WaitForMessageBoxAsync(updater, "Update complete", TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                doneMsg.Get<Button>(SearchCriteria.ByText("No")).Click();
                Log("Clicked no");

                VersionTuple newVersions = this.GetVersionsFromFile(appFile);

                this.AssertVersionAreCorrect(newVersions, initialVersions, appFile, "3.");

                //now just assert that the update check result is empty next time
                Log("Checking update info is false");
                UpdateCheckResult result = this.LaunchTestsAppAndGetResult<UpdateCheckResult>(appFile, Actions.CheckAndInstallUpdates, Apps.Keys.AutomaticTestsClient, waitForExit: true);

                this.AssertNoNonBetaUpdatesToInstall(result, false);

            }
            catch (Exception ex)
            {
                throw this.CleanupAndRethrow(ex);
            }
        }

    }
}
