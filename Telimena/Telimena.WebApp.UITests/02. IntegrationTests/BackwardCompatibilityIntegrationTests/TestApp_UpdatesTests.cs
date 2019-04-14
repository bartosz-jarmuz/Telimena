using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using AutomaticTestsClient;
using NUnit.Framework;
using Telimena.WebApp.UITests.Base;
using Telimena.WebApp.UITests.Base.TestAppInteraction;
using TelimenaClient;
using TelimenaClient.Model;
using TelimenaClient.Model.Internal;
using TestStack.White;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.WindowItems;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Telimena.WebApp.UITests._02._IntegrationTests.BackwardCompatibilityIntegrationTests
{
  


    [TestFixture(), Timeout(3*60*1000)]
    public partial class _2_NonUiTests : IntegrationTestBase
    {

        [Test]
        public async Task _01_HandlePackageUpdates_NonBeta()
        {
            try
            {
              
                this.LaunchPackageUpdaterTestsAppWithArgs(out FileInfo appFile, Apps.PackageNames.PackageUpdaterTestAppV1, TestHelpers.GetMethodName(), waitForExit: false);

                Window updateNowMsgBox =
                    await TestHelpers.WaitForWindowAsync(x => x.Equals("PackageTriggerUpdaterTestApp update download"), TimeSpan.FromMinutes(2)).ConfigureAwait(false);
                updateNowMsgBox.Get<Button>(SearchCriteria.ByText("Yes")).Click();

                Window executed = await TestHelpers.WaitForWindowAsync(x => x.Equals("Updater executed"), TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                executed.Get<Button>(SearchCriteria.ByText("OK")).Click();

                Window doneMsg = await TestHelpers.WaitForWindowAsync(x => x.Equals("Updater finished"), TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                string text = GetTextFromMsgBox(doneMsg);
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

                VersionTuple initialVersions = await this.GetVersionsFromApp(Apps.PackageNames.AutomaticTestsClientAppV1, TestHelpers.GetMethodName()).ConfigureAwait(false);

                this.LaunchTestsAppNewInstanceAndGetResult<UpdateCheckResult>(out FileInfo appFile, Actions.HandleUpdates, Apps.Keys.AutomaticTestsClient, Apps.PackageNames.AutomaticTestsClientAppV1
                    , TestHelpers.GetMethodName(), waitForExit: false);
                Window updateNowMsgBox =
                    await TestHelpers.WaitForWindowAsync(x => x.Equals("AutomaticTestsClient update installation"), TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                updateNowMsgBox.Get<Button>(SearchCriteria.ByText("Yes")).Click();

                Window updater = await TestHelpers.WaitForWindowAsync(x => x.Contains("AutomaticTestsClient Updater"), TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                updater.Get<Button>(SearchCriteria.ByText("Install now!")).Click();

                Window doneMsg = await TestHelpers.WaitForMessageBoxAsync(updater, "Update complete", TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                doneMsg.Get<Button>(SearchCriteria.ByText("Yes")).Click();

                Window appWarning = await TestHelpers.WaitForWindowAsync(x => x.Equals("AutomaticTestsClient - This app requires arguments to run")
                    , TimeSpan.FromMinutes(2)).ConfigureAwait(false);
                VersionTuple newVersions = await this.GetVersionFromMsgBox(appWarning).ConfigureAwait(false);

                await this.AssertVersionAreCorrect(newVersions, initialVersions, appFile, "2.").ConfigureAwait(false);


                //now just assert that the update check result is empty next time
                UpdateCheckResult result = this.LaunchTestsAppAndGetResult<UpdateCheckResult>(appFile, Actions.CheckAndInstallUpdates, Apps.Keys.AutomaticTestsClient, waitForExit: true);

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

                VersionTuple initialVersions = this.GetVersionsFromExtractedAppFile(Apps.PackageNames.AutomaticTestsClientAppV1, TestHelpers.GetMethodName(), out FileInfo appFile);

                this.LaunchTestsAppAndGetResult<UpdateCheckResult>(appFile, Actions.CheckAndInstallUpdates, Apps.Keys.AutomaticTestsClient, waitForExit: false);

                Window updater = await TestHelpers.WaitForWindowAsync(x => x.Contains("AutomaticTestsClient Updater"), TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                updater.Get<Button>(SearchCriteria.ByText("Install now!")).Click();

                Window doneMsg = await TestHelpers.WaitForMessageBoxAsync(updater, "Update complete", TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                doneMsg.Get<Button>(SearchCriteria.ByText("Yes")).Click();

                Window appWarning = await TestHelpers.WaitForWindowAsync(x => x.Equals("AutomaticTestsClient - This app requires arguments to run")
                    , TimeSpan.FromMinutes(2)).ConfigureAwait(false);
                VersionTuple newVersions = await this.GetVersionFromMsgBox(appWarning).ConfigureAwait(false);

                await this.AssertVersionAreCorrect(newVersions, initialVersions, appFile, "2.").ConfigureAwait(false);

                //now just assert that the update check result is empty next time
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
            try
            {
                this.UninstallPackages(Apps.ProductCodes.InstallersTestAppMsi3V1, Apps.ProductCodes.InstallersTestAppMsi3V2);

                 VersionTuple initialVersions = this.GetVersionsFromMsiApp(Apps.PackageNames.InstallersTestAppMsi3V1, Apps.Paths.InstallersTestAppMsi3, Apps.ProductCodes.InstallersTestAppMsi3V1);

                 this.LaunchTestsAppAndGetResult<UpdateCheckResult>(Apps.Paths.InstallersTestAppMsi3, Actions.CheckAndInstallUpdates, Apps.Keys.InstallersTestAppMsi3);

                Window updater = await TestHelpers.WaitForWindowAsync(x => x.Contains("InstallersTestApp.Msi3Installer"), TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                updater.Get<Button>(SearchCriteria.ByText("OK")).Click();

                Thread.Sleep(1000);
                VersionTuple newVersions = this.GetVersionsFromFile(Apps.Paths.InstallersTestAppMsi3);

                Assert.IsTrue(newVersions.AssemblyVersion.IsNewerVersionThan(initialVersions.AssemblyVersion));
                Assert.IsTrue(newVersions.FileVersion.IsNewerVersionThan(initialVersions.FileVersion));
               

                //now just assert that the update check result is empty next time
                UpdateCheckResult result = this.LaunchTestsAppAndGetResult<UpdateCheckResult>(Apps.Paths.InstallersTestAppMsi3, Actions.CheckAndInstallUpdates, Apps.Keys.InstallersTestAppMsi3);

                this.AssertNoNonBetaUpdatesToInstall(result, false);

                this.UninstallPackages(Apps.ProductCodes.InstallersTestAppMsi3V1, Apps.ProductCodes.InstallersTestAppMsi3V2);

            }
            catch (Exception ex)
            {
                throw this.CleanupAndRethrow(ex);
            }
        }

        [Test]
        public async Task _05_CheckAndInstallUpdates_Beta()
        {
            try
            {
                VersionTuple initialVersions = this.GetVersionsFromExtractedAppFile(Apps.PackageNames.AutomaticTestsClientAppV1, TestHelpers.GetMethodName(), out FileInfo appFile);

                this.LaunchTestsAppAndGetResult<UpdateCheckResult>(appFile, Actions.HandleUpdatesWithBeta, Apps.Keys.AutomaticTestsClient, waitForExit: false);

                Window updateNowMsgBox =
                    await TestHelpers.WaitForWindowAsync(x => x.Equals("AutomaticTestsClient update installation"), TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                updateNowMsgBox.Get<Button>(SearchCriteria.ByText("Yes")).Click();

                Window updater = await TestHelpers.WaitForWindowAsync(x => x.Contains("AutomaticTestsClient Updater"), TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                updater.Get<Button>(SearchCriteria.ByText("Install now!")).Click();

                Window doneMsg = await TestHelpers.WaitForMessageBoxAsync(updater, "Update complete", TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                doneMsg.Get<Button>(SearchCriteria.ByText("No")).Click();

                VersionTuple newVersions = this.GetVersionsFromFile(appFile);

                await this.AssertVersionAreCorrect(newVersions, initialVersions, appFile, "3.").ConfigureAwait(false);

                //now just assert that the update check result is empty next time
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
