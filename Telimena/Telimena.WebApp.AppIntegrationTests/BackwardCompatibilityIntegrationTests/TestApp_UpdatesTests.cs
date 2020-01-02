using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
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
                var stamp = DateTime.UtcNow;
              
                this.LaunchPackageUpdaterTestsAppWithArgs(out FileInfo appFile, Apps.PackageNames.PackageUpdaterTestAppV1, SharedTestHelpers.GetMethodName(), waitForExit: false);

                Window updateNowMsgBox =
                    await WindowHelpers.WaitForWindowAsync(x => x.Equals("PackageTriggerUpdaterTestApp update download"), TimeSpan.FromMinutes(2)).ConfigureAwait(false);
                updateNowMsgBox.Get<Button>(SearchCriteria.ByText("Yes")).Click();
                Log("Clicked yes");

                var appDir = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                var executedFile = await GetExecutionSummaryFile(appDir, "Executed").ConfigureAwait(false);
                var executedLines = File.ReadAllLines(executedFile.FullName);
                Assert.IsTrue(stamp < DateTime.ParseExact(executedLines[0], "O", CultureInfo.InvariantCulture), $"Expected {executedLines[0]} to be newer than {stamp}");


                var finishedFile = await GetExecutionSummaryFile(appDir, "Finished").ConfigureAwait(false);
                var finishedLines = File.ReadAllLines(finishedFile.FullName);
                Assert.AreEqual(executedLines[0],finishedLines[0]);

                Assert.AreEqual("Killed other processes: True", finishedLines[1]);

                //do not check if app was updated, because we only care whether the updater was actually launched

            }
            catch (Exception ex)
            {
                 this.CleanupAndLog(ex);
                 throw;
            }

        }

        private static async Task<FileInfo> GetExecutionSummaryFile(DirectoryInfo appDir, string fileName, int timeoutMilliseconds = 20000)
        {
            var executedFile = appDir.GetFiles(fileName).FirstOrDefault();
            Log($"Looking for file [{executedFile}] in {appDir.FullName}");

            var sw = Stopwatch.StartNew();
            while (executedFile == null)
            {
                executedFile = appDir.GetFiles(fileName).FirstOrDefault();
                await Task.Delay(50);
                if (sw.ElapsedMilliseconds > timeoutMilliseconds)
                {
                    Assert.Fail($"[{fileName}] File was not created ");
                }
            }

            return executedFile;
        }

        [Test]
        public async Task _02_HandleUpdates_NonBeta()
        {
            try
            {

                VersionTuple initialVersions = this.GetVersionsFromApp(Apps.PackageNames.AutomaticTestsClientAppV1, SharedTestHelpers.GetMethodName());

                UpdateCheckResult _ = this.LaunchTestsAppNewInstanceAndGetResult<UpdateCheckResult>(out FileInfo appFile, Actions.HandleUpdates, Apps.Keys.AutomaticTestsClient, Apps.PackageNames.AutomaticTestsClientAppV1
                    , SharedTestHelpers.GetMethodName(), waitForExit: false);

                await this.PerformManualUpdate("AutomaticTestsClient", "2.");

                VersionTuple newVersions = this.GetVersionsFromApp(appFile);
                Log($"New versions: {newVersions.ToLog()}");

                this.AssertVersionAreCorrect(newVersions, initialVersions, appFile, "2.");

                Log("Checking update info is false");
                //now just assert that the update check result is empty next time
                var result = this.LaunchTestsAppAndGetResult<UpdateCheckResult>(appFile, Actions.CheckAndInstallUpdates, Apps.Keys.AutomaticTestsClient, waitForExit: true);

                this.AssertNoNonBetaUpdatesToInstall(result, true);

            }
            catch (Exception ex)
            {
                this.CleanupAndLog(ex);
                throw;
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

                VersionTuple newVersions = this.GetVersionsFromApp(appFile);
                Log($"New versions: {newVersions.ToLog()}");

                this.AssertVersionAreCorrect(newVersions, initialVersions, appFile, "2.");

                //now just assert that the update check result is empty next time
                Log("Checking update info is false");

                UpdateCheckResult result = this.LaunchTestsAppAndGetResult<UpdateCheckResult>(appFile, Actions.CheckAndInstallUpdates, Apps.Keys.AutomaticTestsClient, waitForExit: true);
                this.AssertNoNonBetaUpdatesToInstall(result, true);
            }
            catch (Exception ex)
            {
                this.CleanupAndLog(ex);
                throw;
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
                this.CleanupAndLog(ex);
                throw;
            }
        }

       

        [Test]
        public async Task _05_CheckAndInstallUpdates_Beta()
        {
            try
            {
                VersionTuple initialVersions = this.GetVersionsFromExtractedAppFile(Apps.PackageNames.AutomaticTestsClientAppV1, SharedTestHelpers.GetMethodName(), out FileInfo appFile);

                this.LaunchTestsAppAndGetResult<UpdateCheckResult>(appFile, Actions.HandleUpdatesWithBeta, Apps.Keys.AutomaticTestsClient, waitForExit: false);

                await this.PerformManualUpdate("AutomaticTestsClient", "3.");

                VersionTuple newVersions = this.GetVersionsFromFile(appFile);
                Log($"New versions: {newVersions.ToLog()}");

                this.AssertVersionAreCorrect(newVersions, initialVersions, appFile, "3.");

                //now just assert that the update check result is empty next time
                Log("Checking update info is false");
                UpdateCheckResult result = this.LaunchTestsAppAndGetResult<UpdateCheckResult>(appFile, Actions.CheckAndInstallUpdates, Apps.Keys.AutomaticTestsClient, waitForExit: true);

                this.AssertNoNonBetaUpdatesToInstall(result, false);

            }
            catch (Exception ex)
            {
                this.CleanupAndLog(ex);
                throw;
            }
        }

    }
}
