using System;
using System.Diagnostics;
using System.IO;
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
    public class VersionTuple
    {
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }
    }



    [TestFixture(), Timeout(3*60*1000)]
    public partial class _2_NonUiTests : IntegrationTestBase
    {
        public async Task<VersionTuple> GetVersionsFromApp(string appName, string testSubfolderName)
        {
            FileInfo exe = TestAppProvider.ExtractApp(appName, testSubfolderName);
            return await this.GetVersionsFromApp(exe).ConfigureAwait(false);
        }

        public VersionTuple GetVersionsFromMsiApp(string installerName, FileInfo expectedProgramPath, Guid productCode)
        {
            this.UninstallMsi(productCode, expectedProgramPath);

            this.InstallMsi(TestAppProvider.GetFile(installerName), expectedProgramPath);
            Thread.Sleep(500);
            return this.GetVersionsFromFile(expectedProgramPath);
        }

        public async Task<VersionTuple> GetVersionsFromApp(FileInfo exe)
        {
            this.Log($"Starting process [{exe.FullName}]");
            Process process = Process.Start(exe.FullName);
            Application app = TestStack.White.Application.Attach(process);

            Window appWarning = await TestHelpers.WaitForWindowAsync(x => x.Equals("AutomaticTestsClient - This app requires arguments to run"), TimeSpan.FromMinutes(2)).ConfigureAwait(false);
            return await this.GetVersionFromMsgBox(appWarning).ConfigureAwait(false);
        }

        public VersionTuple GetVersionsFromFile(FileInfo exe)
        {
           string ass = TelimenaVersionReader.Read(exe, VersionTypes.AssemblyVersion);
           string file = TelimenaVersionReader.Read(exe, VersionTypes.FileVersion);
            return new VersionTuple() { AssemblyVersion = ass, FileVersion = file };

        }

        public string GetTextFromMsgBox(Window msgBox)
        {
            string text = msgBox.Get<Label>(SearchCriteria.Indexed(0)).Text;
            try
            {
                msgBox.Get<Button>(SearchCriteria.ByText("OK")).Click();
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    if (ex.Message.Contains("Window didn't respond") && ex.InnerException != null &&
                        ex.InnerException.Message.Contains("Process with an Id of"))
                    {
                        //that's weird but the message box got closed by something else
                    }
                    else
                    {
                        throw;
                    }
                }
            }

            return text;
        }

        public async Task<VersionTuple> GetVersionFromMsgBox(Window msgBox)
        {
            string text = msgBox.Get<Label>(SearchCriteria.Indexed(0)).Text;
            string[] versions = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            try
            {
                msgBox.Get<Button>(SearchCriteria.ByText("OK")).Click();
                await Task.Delay(500).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (!string.IsNullOrEmpty(text))
                {
                    if (ex.Message.Contains("Window didn't respond") && ex.InnerException != null &&
                        ex.InnerException.Message.Contains("Process with an Id of"))
                    {
                        //that's weird but the message box got closed by something else
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            
            return new VersionTuple() { AssemblyVersion = versions[0].Trim().Replace("AssemblyVersion: ",""), FileVersion = versions[1].Trim().Replace("FileVersion: ", "") };
        }

        [Test]
        public async Task _03_CheckAndInstallUpdatesNonBetaTests()
        {
            try
            {

                VersionTuple initialVersions = await this.GetVersionsFromApp(Apps.PackageNames.AutomaticTestsClientAppV1, MethodBase.GetCurrentMethod().Name).ConfigureAwait(false);

                FileInfo appFile;

                this.LaunchTestsAppNewInstanceAndGetResult<UpdateCheckResult>(out appFile, Actions.CheckAndInstallUpdates, Apps.Keys.AutomaticTestsClient,  Apps.PackageNames.AutomaticTestsClientAppV1
                    , MethodBase.GetCurrentMethod().Name, waitForExit: false);

                Window updater = await TestHelpers.WaitForWindowAsync(x => x.Contains("AutomaticTestsClient Updater"), TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                updater.Get<Button>(SearchCriteria.ByText("Install now!")).Click();

                Window doneMsg = await TestHelpers.WaitForMessageBoxAsync(updater, "Update complete", TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                doneMsg.Get<Button>(SearchCriteria.ByText("Yes")).Click();

                Window appWarning = await TestHelpers.WaitForWindowAsync(x => x.Equals("AutomaticTestsClient - This app requires arguments to run")
                    , TimeSpan.FromMinutes(2)).ConfigureAwait(false);
                VersionTuple newVersions = await this.GetVersionFromMsgBox(appWarning).ConfigureAwait(false);

                Assert.IsTrue(newVersions.AssemblyVersion.IsNewerVersionThan(initialVersions.AssemblyVersion));
                Assert.IsTrue(newVersions.FileVersion.IsNewerVersionThan(initialVersions.FileVersion));
                VersionTuple postUpdateVersions = await this.GetVersionsFromApp(appFile).ConfigureAwait(false);
                Assert.AreEqual(postUpdateVersions.AssemblyVersion, newVersions.AssemblyVersion);
                Assert.AreEqual(postUpdateVersions.FileVersion, newVersions.FileVersion);

                //now just assert that the update check result is empty next time
                UpdateCheckResult result = this.LaunchTestsAppAndGetResult<UpdateCheckResult>(appFile, Actions.CheckAndInstallUpdates, Apps.Keys.AutomaticTestsClient, waitForExit: true);

                Assert.IsFalse(result.IsUpdateAvailable);
                Assert.IsNull(result.Exception);
            }
            catch (Exception ex)
            {
                throw this.CleanupAndRethrow(ex);
            }
        }

       [Test]
        public async Task _03_MsiCheckAndInstallUpdatesTests()
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

                Assert.IsFalse(result.IsUpdateAvailable);
                Assert.IsNull(result.Exception);

                this.UninstallPackages(Apps.ProductCodes.InstallersTestAppMsi3V1, Apps.ProductCodes.InstallersTestAppMsi3V2);

            }
            catch (Exception ex)
            {
                throw this.CleanupAndRethrow(ex);
            }
        }



        [Test]
        public async Task _02_HandleUpdatesNonBetaTests()
        {
            try
            {

                VersionTuple initialVersions = await this.GetVersionsFromApp(Apps.PackageNames.AutomaticTestsClientAppV1, nameof(this._02_HandleUpdatesNonBetaTests)).ConfigureAwait(false);

                FileInfo appFile;

                this.LaunchTestsAppNewInstanceAndGetResult<UpdateCheckResult>(out appFile, Actions.HandleUpdates, Apps.Keys.AutomaticTestsClient, Apps.PackageNames.AutomaticTestsClientAppV1
                    , nameof(this._02_HandleUpdatesNonBetaTests), waitForExit: false);
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

                Assert.IsTrue(newVersions.AssemblyVersion.IsNewerVersionThan(initialVersions.AssemblyVersion));
                Assert.IsTrue(newVersions.FileVersion.IsNewerVersionThan(initialVersions.FileVersion));
                VersionTuple postUpdateVersions = await this.GetVersionsFromApp(appFile).ConfigureAwait(false);
                Assert.AreEqual(postUpdateVersions.AssemblyVersion, newVersions.AssemblyVersion);
                Assert.AreEqual(postUpdateVersions.FileVersion, newVersions.FileVersion);

                //now just assert that the update check result is empty next time
                UpdateCheckResult result = this.LaunchTestsAppAndGetResult<UpdateCheckResult>(appFile, Actions.HandleUpdates, Apps.Keys.AutomaticTestsClient, waitForExit:true);

                Assert.IsFalse(result.IsUpdateAvailable);
                Assert.IsNull(result.Exception);
            }
            catch (Exception ex)
            {
                throw this.CleanupAndRethrow(ex);
            }
        }

        [Test]
        public async Task _01_HandlePackageUpdatesNonBetaTests()
        {
            try
            {
                this.LaunchPackageUpdaterTestsAppWithArgs(out FileInfo appFile, Apps.PackageNames.PackageUpdaterTestAppV1, nameof(this._01_HandlePackageUpdatesNonBetaTests), waitForExit: false);

                Window updateNowMsgBox =
                    await TestHelpers.WaitForWindowAsync(x => x.Equals("PackageTriggerUpdaterTestApp update installation"), TimeSpan.FromMinutes(2)).ConfigureAwait(false);
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

        
    }
}
