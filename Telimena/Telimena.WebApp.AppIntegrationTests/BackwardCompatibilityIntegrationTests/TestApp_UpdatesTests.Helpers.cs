using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using DotNetLittleHelpers;
using FlaUI.Core.AutomationElements;
using FlaUI.Core.Conditions;
using FlaUI.Core.Definitions;
using NUnit.Framework;
using Telimena.TestUtilities.Base;
using Telimena.TestUtilities.Base.TestAppInteraction;
using Telimena.WebApp.AppIntegrationTests.Utilities;
using TelimenaClient;
using TelimenaClient.Model.Internal;


namespace Telimena.WebApp.AppIntegrationTests.BackwardCompatibilityIntegrationTests
{
    public class VersionTuple
    {
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }

        public string ToLog()
        {
            return $"AssemblyVersion: [{this.AssemblyVersion}]. FileVersion: [{this.FileVersion}]";
        }
    }



    public partial class _2_NonUiTests : IntegrationTestBase
    {
        public VersionTuple GetVersionsFromApp(string appName, string testSubfolderName)
        {
            FileInfo exe = TestAppProvider.ExtractApp(appName, testSubfolderName, x => IntegrationTestBase.Log(x));
            return this.GetVersionsFromApp(exe);
        }
        private void AssertNoNonBetaUpdatesToInstall(UpdateCheckResult result, bool betaUpdateMightStillBeAvailable)
        {
            Assert.IsNull(result.Exception);
            Assert.AreEqual(0, result.ProgramUpdatesToInstall.Count(x => !x.IsBeta));
            if (!betaUpdateMightStillBeAvailable)
            {
                Assert.AreEqual(0, result.ProgramUpdatesToInstall.Count);
                Assert.IsFalse(result.IsUpdateAvailable);
            }

        }
        public VersionTuple GetVersionsFromExtractedAppFile(string appName, string testSubfolderName, out FileInfo exe)
        {
            exe = TestAppProvider.ExtractApp(appName, testSubfolderName, x => IntegrationTestBase.Log(x));
            return this.GetVersionsFromFile(exe);
        }

        public VersionTuple GetVersionsFromMsiApp(string installerName, FileInfo expectedProgramPath, Guid productCode)
        {
            this.UninstallMsi(productCode, expectedProgramPath);

            this.InstallMsi(TestAppProvider.GetFile(installerName), expectedProgramPath);
            Thread.Sleep(500);
            return this.GetVersionsFromFile(expectedProgramPath);
        }
        private async Task PerformManualUpdate(string appName, string expectedVersion)
        {
            Window updateNowMsgBox = await WindowHelpers
                .WaitForWindowAsync(x => x.Equals($"{appName} update installation"), TimeSpan.FromMinutes(1))
                .ConfigureAwait(false);
            this.CheckProperUpdateVersionDownloadedInMessageBox(updateNowMsgBox, expectedVersion);

            WindowHelpers.ClickButtonByText(updateNowMsgBox, "Yes");

            Log("Clicked yes");
            Window updater = await WindowHelpers
                .WaitForWindowAsync(x => x.Contains($"{appName} Updater"), TimeSpan.FromMinutes(1))
                .ConfigureAwait(false);
            WindowHelpers.ClickButtonByText(updater, "Install now!");

            Log("Clicked Install now!");

            Window doneMsg = await WindowHelpers.WaitForMessageBoxAsync(updater, "Update complete", TimeSpan.FromMinutes(1))
                .ConfigureAwait(false);
            WindowHelpers.ClickButtonByText(doneMsg, "Yes");

            Log("Clicked yes");
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

                    Assert.IsTrue(TelimenaClient.Extensions.IsNewerVersionThan(newVersions.AssemblyVersion, initialVersions.AssemblyVersion)
                        , $"Version don't match. Initial AssVer: {initialVersions.AssemblyVersion}. New {newVersions.AssemblyVersion}");
                    Assert.IsTrue(Extensions.IsNewerVersionThan(newVersions.FileVersion, initialVersions.FileVersion),
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

        public VersionTuple GetVersionsFromApp(FileInfo exe)
        {
            Log($"Reading versions from [{exe.FullName}]");
            var versionTuple = new VersionTuple();
            versionTuple.AssemblyVersion =TelimenaVersionReader.Read(exe, VersionTypes.AssemblyVersion).ToString();
            versionTuple.FileVersion =TelimenaVersionReader.Read(exe, VersionTypes.FileVersion).ToString();
            return versionTuple;
        }

        public VersionTuple GetVersionsFromFile(FileInfo exe)
        {
           string ass = TelimenaVersionReader.Read(exe, VersionTypes.AssemblyVersion);
           string file = TelimenaVersionReader.Read(exe, VersionTypes.FileVersion);
            return new VersionTuple() { AssemblyVersion = ass, FileVersion = file };

        }

        private void CheckProperUpdateVersionDownloadedInMessageBox(Window updateNowMsgBox, string expectedVersion)
        {
            var label = updateNowMsgBox.FindAllChildren(x => x.ByControlType(ControlType.Text))[0];
            var lbl = updateNowMsgBox.FindAllChildren();
            var text = label.Name;
            var version = text.Replace("An update to version ", "").Replace(" was downloaded", "").Replace("Would you like to install now?", "").Trim();

            StringAssert.StartsWith(expectedVersion, version, $"Update version {version} does not start with {expectedVersion}. Probably incorrect version of update was downloaded");
        }

        private void CheckProperUpdateVersionDownloadedInUpdater(Window updaterWindow, string expectedVersion)
        {
            var lbl = updaterWindow.FindAllChildren(x => x.ByControlType(ControlType.Text))[1];
            var text = lbl.Name;

            StringAssert.StartsWith(expectedVersion, text, $"Update version {text} does not start with {expectedVersion}. Probably incorrect version of update was downloaded");
        }
      

      

      
        private void AssertVersionAreCorrect(VersionTuple newVersions, VersionTuple initialVersions, FileInfo appFile, string newVersionStartingPart)
        {
            Log("Initial versions\r\n" + initialVersions.GetPropertyInfoString());
            Log("New versions\r\n" + newVersions.GetPropertyInfoString());
            Assert.IsTrue(Extensions.IsNewerVersionThan(newVersions.AssemblyVersion, initialVersions.AssemblyVersion), $"New assembly version {newVersions.AssemblyVersion} is not newer than initial assembly version {initialVersions.AssemblyVersion}");
            Assert.IsTrue(Extensions.IsNewerVersionThan(newVersions.FileVersion, initialVersions.FileVersion), $"New file version {newVersions.FileVersion} is not newer than initial file version {initialVersions.FileVersion}");
            VersionTuple postUpdateVersions = this.GetVersionsFromFile(appFile);
            Log("PostUpdate versions\r\n" + postUpdateVersions.GetPropertyInfoString());

            Assert.AreEqual(postUpdateVersions.AssemblyVersion, newVersions.AssemblyVersion, $"Post update assembly version {postUpdateVersions.AssemblyVersion} is equal to 'new' assembly version {newVersions.AssemblyVersion}");
            Assert.AreEqual(postUpdateVersions.FileVersion, newVersions.FileVersion, $"Post update file version {postUpdateVersions.FileVersion} is equal to 'new' file version {newVersions.FileVersion}");

            Assert.IsTrue(newVersions.AssemblyVersion.StartsWith(newVersionStartingPart), $"{newVersions.AssemblyVersion}.StartsWith({newVersionStartingPart})");
            Assert.IsTrue(newVersions.FileVersion.StartsWith(newVersionStartingPart), $"{newVersions.FileVersion}.StartsWith({newVersionStartingPart})");
        }

        
    }
}
