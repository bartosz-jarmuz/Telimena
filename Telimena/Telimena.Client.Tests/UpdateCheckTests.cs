// -----------------------------------------------------------------------
//  <copyright file="ClientTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Security.AccessControl;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Telimena.Updater;
using Telimena.WebApp.Core.Models;

namespace TelimenaClient.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class TestUpdateChecks
    {
        private Mock<ITelimenaHttpClient> GetMockClientForCheckForUpdates(object programUpdatesResponse, object updaterResponse)
        {
            Mock<ITelimenaHttpClient> client = new Mock<ITelimenaHttpClient>();
            client.Setup(x => x.GetAsync(It.IsRegex("^" + Regex.Escape(ApiRoutes.GetProgramUpdaterName)))).Returns((string uri) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();
                response.Content = new StringContent("MyUpdater.exe");
                return Task.FromResult(response);
            });
            client.Setup(x => x.PostAsync("api/Statistics/RegisterClient", It.IsAny<HttpContent>())).Returns((string uri, HttpContent requestContent) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();
                TelemetryInitializeResponse telemetryInitializeResponse = new TelemetryInitializeResponse {Count = 0, ProgramId = 1, UserId = 2};
                response.Content = new StringContent(JsonConvert.SerializeObject(telemetryInitializeResponse));
                return Task.FromResult(response);
            });
            client.Setup(x => x.GetAsync(It.IsRegex(".*" + Regex.Escape(ApiRoutes.GetProgramUpdateInfo)))).Returns((string uri) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();

                response.Content = new StringContent(JsonConvert.SerializeObject(programUpdatesResponse));
                return Task.FromResult(response);
            });
            client.Setup(x => x.GetAsync(It.IsRegex(".*" + Regex.Escape(ApiRoutes.GetUpdaterUpdateInfo)))).Returns((string uri) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();

                response.Content = new StringContent(JsonConvert.SerializeObject(updaterResponse));
                return Task.FromResult(response);
            });
            return client;
        }

        [Test]
        public void Test_CheckForUpdates_OnlyProgram()
        {
            TelimenaClient.Telimena sut = new TelimenaClient.Telimena(Guid.Empty) {SuppressAllErrors = false};
            Assert.AreEqual("Telimena.Client.Tests", sut.StaticProgramInfo.PrimaryAssembly.Name);

            sut.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");

            UpdateResponse latestVersionResponse = new UpdateResponse
            {
                UpdatePackages = new List<UpdatePackageData>
                {
                    new UpdatePackageData {FileSizeBytes = 666, Id = 10001, IsBeta = true, Version = "3.1.0.0"}
                    , new UpdatePackageData {FileSizeBytes = 666, Id = 10002, Version = "3.0.0.0"}
                }
            };
            Helpers.SetupMockHttpClient(sut, this.GetMockClientForCheckForUpdates(latestVersionResponse, new UpdateResponse()));

            UpdateCheckResult response = sut.CheckForUpdatesAsync().GetAwaiter().GetResult();
            Assert.IsTrue(response.IsUpdateAvailable);
            Assert.AreEqual("MyUpdater.exe", sut.LiveProgramInfo.UpdaterName);
            Assert.AreEqual(2, response.ProgramUpdatesToInstall.Count);
            Assert.IsNotNull(response.ProgramUpdatesToInstall.SingleOrDefault(x=>x.Version == "3.1.0.0" && x.IsBeta == true));
            Assert.IsNotNull(response.ProgramUpdatesToInstall.SingleOrDefault(x=>x.Version == "3.0.0.0"));
            Assert.IsNull(response.UpdaterUpdate);
            Assert.IsNull(response.Exception);

        }

        [Test]
        public void Test_CheckForUpdates_Program_AndUpdater()
        {
            TelimenaClient.Telimena sut = new TelimenaClient.Telimena(Guid.Empty) { SuppressAllErrors = false };
            Assert.AreEqual("Telimena.Client.Tests", sut.StaticProgramInfo.PrimaryAssembly.Name);

            sut.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");

            UpdateResponse latestVersionResponse = new UpdateResponse
            {
                UpdatePackages = new List<UpdatePackageData>
                {
                    new UpdatePackageData {FileSizeBytes = 666, Id = 10001, IsBeta = true, Version = "3.1.0.0"}
            }};
            UpdateResponse updaterResponse = new UpdateResponse
            {
                UpdatePackages = new List<UpdatePackageData>
                {
                    new UpdatePackageData {FileName = DefaultToolkitNames.UpdaterFileName, Version = "1.2"}
                }
            };
            Helpers.SetupMockHttpClient(sut, this.GetMockClientForCheckForUpdates(latestVersionResponse, updaterResponse));

            UpdateCheckResult response = sut.CheckForUpdatesAsync().GetAwaiter().GetResult();
            Assert.IsTrue(response.IsUpdateAvailable);
            Assert.AreEqual(1, response.ProgramUpdatesToInstall.Count);
            Assert.IsNotNull(response.ProgramUpdatesToInstall.SingleOrDefault(x => x.Version == "3.1.0.0" && x.IsBeta == true));
            Assert.AreEqual("1.2", response.UpdaterUpdate.Version);
            Assert.IsNull(response.Exception);

        }

        [Test]
        public void Test_OnlyUpdaterUpdates()
        {
            TelimenaClient.Telimena sut = new TelimenaClient.Telimena(Guid.Empty) { SuppressAllErrors = false };

            UpdateResponse latestVersionResponse = new UpdateResponse
            {
                UpdatePackages = new List<UpdatePackageData>
                {
                    new UpdatePackageData {FileName = DefaultToolkitNames.UpdaterFileName, Version = "1.2"}
                }
            };
            Helpers.SetupMockHttpClient(sut, this.GetMockClientForCheckForUpdates(new UpdateResponse(), latestVersionResponse));

            UpdateCheckResult response = sut.CheckForUpdatesAsync().GetAwaiter().GetResult();
            Assert.IsFalse(response.IsUpdateAvailable);
            Assert.AreEqual(0, response.ProgramUpdatesToInstall.Count);
            Assert.AreEqual("1.2", response.UpdaterUpdate.Version);
            Assert.IsNull(response.Exception);


        }

        [Test]
        public void Test_NoUpdates()
        {
            TelimenaClient.Telimena sut = new TelimenaClient.Telimena(Guid.Empty) { SuppressAllErrors = false };

          
            Helpers.SetupMockHttpClient(sut, this.GetMockClientForCheckForUpdates(new UpdateResponse(),new UpdateResponse()));

            UpdateCheckResult response = sut.CheckForUpdatesAsync().GetAwaiter().GetResult();
            Assert.IsFalse(response.IsUpdateAvailable);
            Assert.AreEqual(0, response.ProgramUpdatesToInstall.Count);
            Assert.IsNull(response.UpdaterUpdate);
            Assert.IsNull(response.Exception);

        }

        [Test]
        public void Test_PackageSorting()
        {
            List<UpdatePackageData> packages = new List<UpdatePackageData>
            {
                new UpdatePackageData {Id = 4, Version = "3.5"}
                , new UpdatePackageData {Id = 2, Version = "2.0"}
                , new UpdatePackageData {Id = 1, Version = "1.0"}
                , new UpdatePackageData {Id = 3, Version = "3.0"}
            };

            List<UpdatePackageData> ordered = UpdateInstructionCreator.Sort(packages);
            Assert.AreEqual(1, ordered[0].Id);
            Assert.AreEqual(2, ordered[1].Id);
            Assert.AreEqual(3, ordered[2].Id);
            Assert.AreEqual(4, ordered[3].Id);
        }

        [Test]
        public void Test_UpdateInstructionCreator()
        {
            List<UpdatePackageData> packages = new List<UpdatePackageData>
            {
                new UpdatePackageData {Id = 4, Version = "3.5", StoredFilePath = @"C:\AppFolder\Updates\Update v 3.5\Update v 3.5.zip"}
                , new UpdatePackageData {Id = 2, Version = "2.0", StoredFilePath = @"C:\AppFolder\Updates\Update v 3.5\Update v 2.0.zip"}
                , new UpdatePackageData {Id = 1, Version = "1.0", StoredFilePath = @"C:\AppFolder\Updates\Update v 3.5\Update v 1.0.zip"}
                , new UpdatePackageData {Id = 3, Version = "3.0", StoredFilePath = @"C:\AppFolder\Updates\Update v 3.5\Update v 3.0.zip"}
            };

            Tuple<XDocument, FileInfo> tuple = UpdateInstructionCreator.CreateXDoc(packages
                , new ProgramInfo {PrimaryAssembly = new AssemblyInfo {Location = @"C:\AppFolder\MyApp.exe"}});
            XDocument xDoc = tuple.Item1;
            FileInfo file = tuple.Item2;
            Assert.AreEqual(@"C:\AppFolder\Updates\Update v 3.5\UpdateInstructions.xml", file.FullName);

            UpdateInstructions instructions = UpdateInstructionsReader.ParseDocument(xDoc);

            Assert.AreEqual(@"3.5", instructions.LatestVersion);
            Assert.AreEqual(@"C:\AppFolder\MyApp.exe", instructions.ProgramExecutableLocation);
            Assert.AreEqual(@"C:\AppFolder\Updates\Update v 3.5\Update v 1.0.zip", instructions.PackagePaths[0]);
            Assert.AreEqual(@"C:\AppFolder\Updates\Update v 3.5\Update v 2.0.zip", instructions.PackagePaths[1]);
            Assert.AreEqual(@"C:\AppFolder\Updates\Update v 3.5\Update v 3.0.zip", instructions.PackagePaths[2]);
            Assert.AreEqual(@"C:\AppFolder\Updates\Update v 3.5\Update v 3.5.zip", instructions.PackagePaths[3]);
        }

        [Test]
        public void Test_UpdaterPathFinder()
        {
            List<UpdatePackageData> packages = new List<UpdatePackageData>
            {
                new UpdatePackageData {Id = 4, Version = "3.5"}
                , new UpdatePackageData {Id = 2, Version = "2.0"}
                , new UpdatePackageData {Id = 1, Version = "1.0"}
                , new UpdatePackageData {Id = 3, Version = "3.0"}
            };
            var temp = Path.GetTempPath();
            var locator = new Locator(new LiveProgramInfo(new ProgramInfo()
            {
                Name = "MyApp"
            })
            {
                UpdaterName = "MyUpdater.exe"
            }, temp);

            DirectoryInfo parentFolder = locator.GetUpdatesParentFolder();
            Assert.AreEqual($@"{temp}MyApp Updates", parentFolder.FullName);

            FileInfo updater = locator.GetUpdater();
            Assert.AreEqual($@"{temp}MyApp Updates\MyUpdater.exe", updater.FullName);

            DirectoryInfo subFolder = locator.GetCurrentUpdateSubfolder(packages);
            Assert.AreEqual($@"{temp}MyApp Updates\3.5", subFolder.FullName);
        }

        [Test]
        public void Test_UpdaterPathFinder_NonWritable()
        {
            var temp = Path.GetTempPath() + "Locked";
            DirectorySecurity securityRules = new DirectorySecurity();
            securityRules.AddAccessRule(new FileSystemAccessRule("Users", FileSystemRights.Write, AccessControlType.Deny));

            Directory.CreateDirectory(temp, securityRules);

            var myDocs = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            var locator = new Locator(new LiveProgramInfo(new ProgramInfo() {Name = "MyApp" })
            {
                UpdaterName = "MyUpdater.exe"
            }, temp);

            DirectoryInfo parentFolder = locator.GetUpdatesParentFolder();
            Assert.AreEqual($@"{myDocs}\Telimena\MyApp\MyApp Updates", parentFolder.FullName);

            FileInfo updater = locator.GetUpdater();
            Assert.AreEqual($@"{myDocs}\Telimena\MyApp\MyApp Updates\MyUpdater.exe", updater.FullName);
        }
    }
}