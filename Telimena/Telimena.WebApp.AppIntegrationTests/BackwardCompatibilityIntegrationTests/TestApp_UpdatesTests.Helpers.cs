using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Telimena.TestUtilities.Base;
using Telimena.TestUtilities.Base.TestAppInteraction;
using Telimena.WebApp.AppIntegrationTests.Utilities;
using TelimenaClient;
using TelimenaClient.Model.Internal;
using TestStack.White;
using TestStack.White.UIItems;
using TestStack.White.UIItems.Finders;
using TestStack.White.UIItems.WindowItems;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

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
            FileInfo exe = TestAppProvider.ExtractApp(appName, testSubfolderName, this.Log);
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
            exe = TestAppProvider.ExtractApp(appName, testSubfolderName, this.Log);
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
            this.Log($"Starting process [{exe.FullName}]");
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
            Assert.IsTrue(newVersions.AssemblyVersion.IsNewerVersionThan(initialVersions.AssemblyVersion));
            Assert.IsTrue(newVersions.FileVersion.IsNewerVersionThan(initialVersions.FileVersion));
            VersionTuple postUpdateVersions = this.GetVersionsFromFile(appFile);
            Assert.AreEqual(postUpdateVersions.AssemblyVersion, newVersions.AssemblyVersion);
            Assert.AreEqual(postUpdateVersions.FileVersion, newVersions.FileVersion);

            Assert.IsTrue(newVersions.AssemblyVersion.StartsWith(newVersionStartingPart));
            Assert.IsTrue(newVersions.FileVersion.StartsWith(newVersionStartingPart));
        }

        
    }
}
