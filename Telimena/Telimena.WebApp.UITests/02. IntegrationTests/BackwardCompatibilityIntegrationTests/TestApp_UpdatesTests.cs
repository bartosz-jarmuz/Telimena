using System;
using System.Diagnostics;
using System.IO;
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
        public async Task<VersionTuple> GetVersionsFromApp(FileInfo exe)
        {
            this.Log($"Starting process [{exe.FullName}]");
            var process = Process.Start(exe.FullName);
            Application app = TestStack.White.Application.Attach(process);

            Window appWarning = await TestHelpers.WaitForWindowAsync(x => x.Equals("AutomaticTestsClient - This app requires arguments to run"), TimeSpan.FromMinutes(2)).ConfigureAwait(false);
            return await this.GetVersionFromMsgBox(appWarning).ConfigureAwait(false);
        }

        public async Task<VersionTuple> GetVersionFromMsgBox(Window msgBox)
        {
            string text = msgBox.Get<Label>(SearchCriteria.Indexed(0)).Text;
            string[] versions = text.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            msgBox.Get<Button>(SearchCriteria.ByText("OK")).Click();
            await Task.Delay(500).ConfigureAwait(false);
            return new VersionTuple() { AssemblyVersion = versions[0].Trim().Replace("AssemblyVersion: ",""), FileVersion = versions[1].Trim().Replace("FileVersion: ", "") };
        }

         [Test]
        public async Task _02_HandleUpdatesNonBetaTests()
        {
            try
            {

                VersionTuple initialVersions = await this.GetVersionsFromApp(TestAppProvider.FileNames.TestAppV1, nameof(this._02_HandleUpdatesNonBetaTests)).ConfigureAwait(false);

                FileInfo appFile;

                this.LaunchTestsAppAndGetResult<UpdateCheckResult>(out appFile, Actions.HandleUpdates, TestAppProvider.FileNames.TestAppV1
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
                var result = this.LaunchTestsAppAndGetResult<UpdateCheckResult>(appFile, Actions.HandleUpdates, waitForExit:true);

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
                this.LaunchPackageUpdaterTestsAppWithArgs(out FileInfo appFile, TestAppProvider.FileNames.PackageUpdaterTestAppV1, nameof(this._01_HandlePackageUpdatesNonBetaTests), waitForExit: false);

                Window updateNowMsgBox =
                    await TestHelpers.WaitForWindowAsync(x => x.Equals("PackageTriggerUpdaterTestApp update installation"), TimeSpan.FromMinutes(2)).ConfigureAwait(false);
                updateNowMsgBox.Get<Button>(SearchCriteria.ByText("Yes")).Click();

                Window executed = await TestHelpers.WaitForWindowAsync(x => x.Equals("Updater executed"), TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                executed.Get<Button>(SearchCriteria.ByText("OK")).Click();

                Window doneMsg = await TestHelpers.WaitForWindowAsync(x => x.Equals("Updater finished"), TimeSpan.FromMinutes(1)).ConfigureAwait(false);
                var text = doneMsg.Get<Label>();
                Assert.AreEqual("Killed other processes: True", text.Text);
                var btn = doneMsg.Get<Button>(SearchCriteria.ByText("OK"));
                btn.Click();

                //do not check if app was updated, because we only care whether the updater was actually launched

            }
            catch (Exception ex)
            {
                throw this.CleanupAndRethrow(ex);
            }
            
        }
    }
}
