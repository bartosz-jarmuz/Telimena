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
using Telimena.PackageTriggerUpdater;

namespace TelimenaClient.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class TestUpdateChecks
    {
        protected internal Guid PrgPkg_1 = Guid.Parse("525b3edd-fa47-47d9-81b6-c9d05a77bcb0");
        protected internal Guid PrgPkg_2 = Guid.Parse("92a9bf8a-a9b2-4778-93c0-fde44eacece6");
        protected internal Guid PrgPkg_3 = Guid.Parse("0d9dd99e-242b-4b6c-ba24-1ded4bb9d87d");
        protected internal Guid PrgPkg_4 = Guid.Parse("dca532d7-cb6f-4fe2-ac01-4ef83119e250");
        protected internal Guid PrgPkg_5 = Guid.Parse("3fb61c5b-ece2-4b60-b659-85d94bfb39c0");
        protected internal Guid PrgPkg_6 = Guid.Parse("871ae68f-63d2-4105-b2bb-9d2c28cf6523");

        private Mock<ITelimenaHttpClient> GetMockClientForCheckForUpdates(Guid propertiesTelemetryKey, object programUpdatesResponse, object updaterResponse)
        {
            Mock<ITelimenaHttpClient> client = new Mock<ITelimenaHttpClient>();
            client.Setup(x => x.GetAsync(It.IsRegex("^" + Regex.Escape(ApiRoutes.GetProgramUpdaterName(propertiesTelemetryKey))))).Returns((string uri) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();
                response.Content = new StringContent("MyUpdater.exe");
                return Task.FromResult(response);
            });
            client.Setup(x => x.PostAsync("api/v1/Telemetry/Initialize", It.IsAny<HttpContent>())).Returns((string uri, HttpContent requestContent) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();
                TelemetryInitializeResponse telemetryInitializeResponse = new TelemetryInitializeResponse {Count = 0, UserId = Guid.NewGuid()};
                response.Content = new StringContent(JsonConvert.SerializeObject(telemetryInitializeResponse));
                return Task.FromResult(response);
            });
            client.Setup(x => x.PostAsync(It.IsRegex(".*" + Regex.Escape(ApiRoutes.ProgramUpdateCheck)), It.IsAny<HttpContent>())).Returns((string uri, HttpContent cnt) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();

                response.Content = new StringContent(JsonConvert.SerializeObject(programUpdatesResponse));
                return Task.FromResult(response);
            });
            client.Setup(x => x.PostAsync(It.IsRegex(".*" + Regex.Escape(ApiRoutes.UpdaterUpdateCheck)), It.IsAny<HttpContent>())).Returns((string uri, HttpContent cnt) =>
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

            var si = new TelimenaStartupInfo(Guid.NewGuid()) {SuppressAllErrors = false};
            si.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");

            ITelimena sut = Telimena.Construct(si) ;

            Assert.AreEqual("Telimena.Client.Tests", sut.Properties.StaticProgramInfo.PrimaryAssembly.Name);

            UpdateResponse latestVersionResponse = new UpdateResponse
            {
                UpdatePackages = new List<UpdatePackageData>
                {
                    new UpdatePackageData {FileSizeBytes = 666, Guid = Guid.NewGuid(), IsBeta = true, Version = "3.1.0.0"}
                    , new UpdatePackageData {FileSizeBytes = 666, Guid = Guid.NewGuid(), Version = "3.0.0.0"}
                }
            };
            Helpers.SetupMockHttpClient(sut, this.GetMockClientForCheckForUpdates(sut.Properties.TelemetryKey, latestVersionResponse, new UpdateResponse()));

            UpdateCheckResult response = sut.Updates.Async.CheckForUpdates().GetAwaiter().GetResult();
            Assert.IsTrue(response.IsUpdateAvailable);
            Assert.AreEqual("MyUpdater.exe", sut.Properties.LiveProgramInfo.UpdaterName);
            Assert.AreEqual(2, response.ProgramUpdatesToInstall.Count);
            Assert.IsNotNull(response.ProgramUpdatesToInstall.SingleOrDefault(x => x.Version == "3.1.0.0" && x.IsBeta));
            Assert.IsNotNull(response.ProgramUpdatesToInstall.SingleOrDefault(x => x.Version == "3.0.0.0"));
            Assert.IsNull(response.UpdaterUpdate);
            Assert.IsNull(response.Exception);
        }

        [Test]
        public void Test_CheckForUpdates_Program_AndUpdater()
        {
            var si = new TelimenaStartupInfo(Guid.NewGuid()) {SuppressAllErrors = false};
            si.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");

            ITelimena sut = Telimena.Construct(si) ;

            Assert.AreEqual("Telimena.Client.Tests", sut.Properties.StaticProgramInfo.PrimaryAssembly.Name);
            UpdateResponse latestVersionResponse = new UpdateResponse
            {
                UpdatePackages = new List<UpdatePackageData>
                {
                    new UpdatePackageData {FileSizeBytes = 666, Guid = Guid.NewGuid(), IsBeta = true, Version = "3.1.0.0"}
                }
            };
            UpdateResponse updaterResponse = new UpdateResponse
            {
                UpdatePackages = new List<UpdatePackageData> {new UpdatePackageData {FileName = DefaultToolkitNames.UpdaterFileName, Version = "1.2"}}
            };
            Helpers.SetupMockHttpClient(sut, this.GetMockClientForCheckForUpdates(sut.Properties.TelemetryKey, latestVersionResponse, updaterResponse));

            UpdateCheckResult response = sut.Updates.Async.CheckForUpdates().GetAwaiter().GetResult();
            Assert.IsTrue(response.IsUpdateAvailable);
            Assert.AreEqual(1, response.ProgramUpdatesToInstall.Count);
            Assert.IsNotNull(response.ProgramUpdatesToInstall.SingleOrDefault(x => x.Version == "3.1.0.0" && x.IsBeta));
            Assert.AreEqual("1.2", response.UpdaterUpdate.Version);
            Assert.IsNull(response.Exception);
        }

        [Test]
        public void Test_NoUpdates()
        {
            ITelimena sut = Telimena.Construct(new TelimenaStartupInfo(Guid.NewGuid()) { SuppressAllErrors = false });

            Helpers.SetupMockHttpClient(sut, this.GetMockClientForCheckForUpdates(sut.Properties.TelemetryKey, new UpdateResponse(), new UpdateResponse()));

            UpdateCheckResult response = sut.Updates.Async.CheckForUpdates().GetAwaiter().GetResult();
            Assert.IsFalse(response.IsUpdateAvailable);
            Assert.AreEqual(0, response.ProgramUpdatesToInstall.Count);
            Assert.IsNull(response.UpdaterUpdate);
            Assert.IsNull(response.Exception);
        }

        [Test]
        public void Test_OnlyUpdaterUpdates()
        {
            ITelimena sut = Telimena.Construct(new TelimenaStartupInfo(Guid.NewGuid()) { SuppressAllErrors = false });


            UpdateResponse latestVersionResponse = new UpdateResponse
            {
                UpdatePackages = new List<UpdatePackageData> {new UpdatePackageData {FileName = DefaultToolkitNames.UpdaterFileName, Version = "1.2"}}
            };
            Helpers.SetupMockHttpClient(sut, this.GetMockClientForCheckForUpdates(sut.Properties.TelemetryKey, new UpdateResponse(), latestVersionResponse));

            UpdateCheckResult response = sut.Updates.Async.CheckForUpdates().GetAwaiter().GetResult();
            Assert.IsFalse(response.IsUpdateAvailable);
            Assert.AreEqual(0, response.ProgramUpdatesToInstall.Count);
            Assert.AreEqual("1.2", response.UpdaterUpdate.Version);
            Assert.IsNull(response.Exception);
        }

        [Test]
        public void Test_PackageSorting()
        {
            List<UpdatePackageData> packages = new List<UpdatePackageData>
            {
                new UpdatePackageData {Guid = this.PrgPkg_4, Version = "3.5"}
                , new UpdatePackageData {Guid = this.PrgPkg_2, Version = "2.0"}
                , new UpdatePackageData {Guid = this.PrgPkg_1, Version = "1.0"}
                , new UpdatePackageData {Guid = this.PrgPkg_3, Version = "3.0"}
            };

            List<UpdatePackageData> ordered = UpdateInstructionCreator.Sort(packages);
            Assert.AreEqual(this.PrgPkg_1, ordered[0].Guid);
            Assert.AreEqual(this.PrgPkg_2, ordered[1].Guid);
            Assert.AreEqual(this.PrgPkg_3, ordered[2].Guid);
            Assert.AreEqual(this.PrgPkg_4, ordered[3].Guid);
        }

        [Test]
        public void Test_UpdateInstructionCreator()
        {
            var locator = new Locator(new LiveProgramInfo(new ProgramInfo { Name = "App.exe" }) { UpdaterName = "MyUpdater.exe" }, @"C:\AppFolder\");

            List<UpdatePackageData> packages = new List<UpdatePackageData>
            {
                new UpdatePackageData   {Guid =   this.PrgPkg_4, Version = "3.5", FileName="Update v 3.5.zip"  }
                , new UpdatePackageData {Guid = this.PrgPkg_2, Version = "2.0",   FileName="Update v 2.0.zip"  }
                , new UpdatePackageData {Guid = this.PrgPkg_1, Version = "1.0",   FileName="Update v 1.0.zip"  }
                , new UpdatePackageData {Guid = this.PrgPkg_3, Version = "3.0",   FileName="Update v 3.0.zip"  }
            };
            DirectoryInfo currentUpdateSubfolder = locator.GetCurrentUpdateSubfolder(packages);


            packages.ForEach(x=>x.StoredFilePath = Locator.Static.BuildUpdatePackagePath(currentUpdateSubfolder, x).FullName);

            Tuple<XDocument, FileInfo> tuple = UpdateInstructionCreator.CreateXDoc(packages
                , new ProgramInfo {PrimaryAssembly = new AssemblyInfo {Location = @"C:\AppFolder\MyApp.exe"}});
            XDocument xDoc = tuple.Item1;
            FileInfo file = tuple.Item2;
            Assert.AreEqual($@"{currentUpdateSubfolder.FullName}\UpdateInstructions.xml", file.FullName);

            global::Telimena.Updater.UpdateInstructions updaterInstructions = global::Telimena.Updater.UpdateInstructionsReader.DeserializeDocument(xDoc);

            Assert.AreEqual(@"3.5", updaterInstructions.LatestVersion);
            Assert.AreEqual(@"C:\AppFolder\MyApp.exe", updaterInstructions.ProgramExecutableLocation);

            Assert.That(()=>updaterInstructions.Packages[0].Version, Is.EqualTo("1.0"));      
            Assert.That(()=>updaterInstructions.Packages[1].Version, Is.EqualTo("2.0"));      
            Assert.That(()=>updaterInstructions.Packages[2].Version, Is.EqualTo("3.0"));      
            Assert.That(()=>updaterInstructions.Packages[3].Version, Is.EqualTo("3.5"));      
            
            Assert.AreEqual($@"{currentUpdateSubfolder.FullName}\1.0\Update v 1.0.zip", updaterInstructions.Packages[0].Path);
            Assert.AreEqual($@"{currentUpdateSubfolder.FullName}\2.0\Update v 2.0.zip", updaterInstructions.Packages[1].Path);
            Assert.AreEqual($@"{currentUpdateSubfolder.FullName}\3.0\Update v 3.0.zip", updaterInstructions.Packages[2].Path);
            Assert.AreEqual($@"{currentUpdateSubfolder.FullName}\3.5\Update v 3.5.zip", updaterInstructions.Packages[3].Path);
            updaterInstructions = null;

            global::Telimena.PackageTriggerUpdater.UpdateInstructions packageUpdaterInstructions = global::Telimena.PackageTriggerUpdater.UpdateInstructionsReader.DeserializeDocument(xDoc);
            Assert.AreEqual(@"3.5", packageUpdaterInstructions.LatestVersion);
            Assert.AreEqual(@"C:\AppFolder\MyApp.exe", packageUpdaterInstructions.ProgramExecutableLocation);

            Assert.That(() => packageUpdaterInstructions.Packages[0].Version, Is.EqualTo("1.0"));
            Assert.That(() => packageUpdaterInstructions.Packages[1].Version, Is.EqualTo("2.0"));
            Assert.That(() => packageUpdaterInstructions.Packages[2].Version, Is.EqualTo("3.0"));
            Assert.That(() => packageUpdaterInstructions.Packages[3].Version, Is.EqualTo("3.5"));

            Assert.AreEqual($@"{currentUpdateSubfolder.FullName}\1.0\Update v 1.0.zip", packageUpdaterInstructions.Packages[0].Path);
            Assert.AreEqual($@"{currentUpdateSubfolder.FullName}\2.0\Update v 2.0.zip", packageUpdaterInstructions.Packages[1].Path);
            Assert.AreEqual($@"{currentUpdateSubfolder.FullName}\3.0\Update v 3.0.zip", packageUpdaterInstructions.Packages[2].Path);
            Assert.AreEqual($@"{currentUpdateSubfolder.FullName}\3.5\Update v 3.5.zip", packageUpdaterInstructions.Packages[3].Path);

        }



        [Test]
        public void Test_UpdaterPathFinder()
        {
            string temp = Path.GetTempPath();

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            
            Locator locator = new Locator(new LiveProgramInfo(new ProgramInfo {Name = "Test_UpdaterPathFinder"}) {UpdaterName = "MyUpdater.exe"}, temp);

            FileInfo updater = locator.GetUpdater();
            Assert.AreEqual($@"{appData}\Telimena\Test_UpdaterPathFinder\Updates\MyUpdater.exe", updater.FullName);
        }

        [Test]
        public void Test_UpdaterPathFinder_CannotAccessAppData()
        {
            List<UpdatePackageData> packages = new List<UpdatePackageData>
            {
                new UpdatePackageData   {Guid = this.PrgPkg_4, Version = "3.5", FileName = "Update.zip"}
                , new UpdatePackageData {Guid = this.PrgPkg_2, Version = "2.0", FileName = "Update.zip"}
                , new UpdatePackageData {Guid = this.PrgPkg_1, Version = "1.0", FileName = "Update.zip"}
                , new UpdatePackageData {Guid = this.PrgPkg_3, Version = "3.0", FileName = "Update.zip"}
            };
            string temp = Path.GetTempPath();

            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            DirectoryInfo dirInfo = new DirectoryInfo(Path.Combine(appData, "Telimena", "Test_UpdaterPathFinder_CannotAccessAppData"));
            dirInfo.Create();
            DirectorySecurity securityRules = new DirectorySecurity();
            securityRules.AddAccessRule(new FileSystemAccessRule("Users", FileSystemRights.Write, AccessControlType.Deny));

            dirInfo.SetAccessControl(securityRules);
            Locator locator =
                new Locator(new LiveProgramInfo(new ProgramInfo {Name = "Test_UpdaterPathFinder_CannotAccessAppData"}) {UpdaterName = "MyUpdater.exe"}, temp);

            FileInfo updater = locator.GetUpdater();
            Assert.AreEqual($@"{temp}Test_UpdaterPathFinder_CannotAccessAppData Updates\MyUpdater.exe", updater.FullName);

            DirectoryInfo subFolder = locator.GetCurrentUpdateSubfolder(packages);
            Assert.AreEqual($@"{temp}Test_UpdaterPathFinder_CannotAccessAppData Updates\3.5", subFolder.FullName);

            Assert.AreEqual($@"{temp}Test_UpdaterPathFinder_CannotAccessAppData Updates\3.5\3.5\Update.zip", locator.BuildUpdatePackagePath(subFolder, packages[0]).FullName);
            Assert.AreEqual($@"{temp}Test_UpdaterPathFinder_CannotAccessAppData Updates\3.5\2.0\Update.zip", locator.BuildUpdatePackagePath(subFolder, packages[1]).FullName);
        }


        [Test]
        public void TestLocator()
        {
            Locator locator =
                new Locator(new LiveProgramInfo(new ProgramInfo { Name = "TestApp" }) { UpdaterName = "MyUpdater.exe" });
            var pkg = new UpdatePackageData() { FileName = "Update.zip", Version = "1.2.0" };

            var parent = locator.GetCurrentUpdateSubfolder(new[] {pkg});


            var updatePackage = locator.BuildUpdatePackagePath(parent, pkg);
            Assert.AreEqual($@"{Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData)}\Telimena\TestApp\Updates\1.2.0\1.2.0\Update.zip", updatePackage.FullName);

        }

    }


}