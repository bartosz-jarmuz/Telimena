using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DotNetLittleHelpers;
using NUnit.Framework;
using Telimena.TestUtilities.Base;
using Telimena.TestUtilities.Base.TestAppInteraction;
using Telimena.WebApp.AppIntegrationTests.Utilities;
using TelimenaClient;
using TelimenaClient.Model.Internal;
using TestStack.White;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.WindowItems;


namespace Telimena.WebApp.AppIntegrationTests.BackwardCompatibilityIntegrationTests
{
    public class VersionTuple
    {
        public string AssemblyVersion { get; set; }
        public string FileVersion { get; set; }
    }



    public partial class _2_NonUiTests : IntegrationTestBase
    {
        public async Task<VersionTuple> GetVersionsFromApp(string appName, string testSubfolderName)
        {
            FileInfo exe = TestAppProvider.ExtractApp(appName, testSubfolderName, x => IntegrationTestBase.Log(x));
            return await this.GetVersionsFromApp(exe).ConfigureAwait(false);
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

        public async Task<VersionTuple> GetVersionsFromApp(FileInfo exe)
        {
            Log($"Starting process [{exe.FullName}]");
            Process process = Process.Start(exe.FullName);
            Application app = TestStack.White.Application.Attach(process);

            Window appWarning = await WindowHelpers.WaitForWindowAsync(x => x.Equals("AutomaticTestsClient - This app requires arguments to run"), TimeSpan.FromMinutes(2)).ConfigureAwait(false);
            return await this.GetVersionFromMsgBox(appWarning).ConfigureAwait(false);
        }

        public VersionTuple GetVersionsFromFile(FileInfo exe)
        {
           string ass = TelimenaVersionReader.Read(exe, VersionTypes.AssemblyVersion);
           string file = TelimenaVersionReader.Read(exe, VersionTypes.FileVersion);
            return new VersionTuple() { AssemblyVersion = ass, FileVersion = file };

        }

        private void CheckProperUpdateVersionDownloadedInMessageBo(Window updateNowMsgBox, string expectedVersion)
        {
            var label = updateNowMsgBox.Get<Label>(SearchCriteria.Indexed(0));
            var text = label.Text;
            var version = text.Replace("An update to version ", "").Replace(" was downloaded", "").Replace("Would you like to install now?", "").Trim();

            StringAssert.StartsWith(expectedVersion, version, $"Update version {version} does not start with {expectedVersion}. Probably incorrect version of update was downloaded");
        }

        private void CheckProperUpdateVersionDownloadedInUpdater(Window updaterWindow, string expectedVersion)
        {
            var versionText = updaterWindow.Get<Label>(SearchCriteria.Indexed(1));
            var text = versionText.Text;

            StringAssert.StartsWith(expectedVersion, text, $"Update version {text} does not start with {expectedVersion}. Probably incorrect version of update was downloaded");
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
