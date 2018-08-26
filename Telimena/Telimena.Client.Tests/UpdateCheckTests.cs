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
using System.Threading.Tasks;
using System.Xml.Linq;
using Moq;
using Newtonsoft.Json;
using NUnit.Framework;
using Telimena.Updater;

namespace Telimena.Client.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class TestUpdateChecks
    {
        private Mock<ITelimenaHttpClient> GetMockClientForCheckForUpdates(object responseObj)
        {
            Mock<ITelimenaHttpClient> client = new Mock<ITelimenaHttpClient>();
            client.Setup(x => x.PostAsync("api/Statistics/RegisterClient", It.IsAny<HttpContent>())).Returns((string uri, HttpContent requestContent) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();
                RegistrationResponse registrationResponse = new RegistrationResponse
                {
                    Count = 0,
                    ProgramId = 1,
                    UserId = 2
                };
                response.Content = new StringContent(JsonConvert.SerializeObject(registrationResponse));
                return Task.FromResult(response);
            });
            client.Setup(x => x.GetAsync(It.IsAny<string>())).Returns((string uri) =>
            {
                HttpResponseMessage response = new HttpResponseMessage();

                response.Content = new StringContent(JsonConvert.SerializeObject(responseObj));
                return Task.FromResult(response);
            });
            return client;
        }


        [Test]
        public void Test_CheckForUpdates()
        {
            Client.Telimena sut = new Client.Telimena
            {
                SuppressAllErrors = false
            };
            Assert.AreEqual("Telimena.Client", sut.ProgramInfo.PrimaryAssembly.Name);

            sut.LoadHelperAssembliesByName("Telimena.Client.Tests.dll", "Moq.dll");

            UpdateResponse latestVersionResponse = new UpdateResponse
            {
                UpdatePackages = new List<UpdatePackageData>
                {
                    new UpdatePackageData {FileSizeBytes = 666, Id = 10001, IsStandalone = true},
                    new UpdatePackageData {FileSizeBytes = 666, Id = 10002}
                }
            };
            latestVersionResponse.UpdatePackagesIncludingBeta = new List<UpdatePackageData>(latestVersionResponse.UpdatePackages);
            Helpers.SetupMockHttpClient(sut, this.GetMockClientForCheckForUpdates(latestVersionResponse));

            UpdateCheckResult response = sut.CheckForUpdates().GetAwaiter().GetResult();
            Assert.IsTrue(response.IsUpdateAvailable);
            //Assert.AreEqual("3.1.0.0", response.PrimaryAssemblyUpdateInfo.LatestVersionInfo.LatestVersion);
            //Assert.AreEqual("3.1.0.1", response.HelperAssembliesToUpdate.Single().LatestVersionInfo.LatestVersion);
        }


        [Test]
        public void Test_PackageSorting()
        {
            List<UpdatePackageData> packages = new List<UpdatePackageData>
            {
                new UpdatePackageData {Id = 4, Version = "3.5"},
                new UpdatePackageData {Id = 2, Version = "2.0"},
                new UpdatePackageData {Id = 1, Version = "1.0"},
                new UpdatePackageData {Id = 3, Version = "3.0"}
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
                new UpdatePackageData {Id = 4, Version = "3.5", StoredFilePath = @"C:\AppFolder\Updates\Update v 3.5\Update v 3.5.zip"},
                new UpdatePackageData {Id = 2, Version = "2.0", StoredFilePath = @"C:\AppFolder\Updates\Update v 3.5\Update v 2.0.zip"},
                new UpdatePackageData {Id = 1, Version = "1.0", StoredFilePath = @"C:\AppFolder\Updates\Update v 3.5\Update v 1.0.zip"},
                new UpdatePackageData {Id = 3, Version = "3.0", StoredFilePath = @"C:\AppFolder\Updates\Update v 3.5\Update v 3.0.zip"}
            };

            Tuple<XDocument, FileInfo> tuple = UpdateInstructionCreator.CreateXDoc(packages, new ProgramInfo(){PrimaryAssembly = new AssemblyInfo(){Location = @"C:\AppFolder\MyApp.exe"}});
            XDocument xDoc = tuple.Item1;
            FileInfo file = tuple.Item2;
            Assert.AreEqual(@"C:\AppFolder\Updates\Update v 3.5\UpdateInstructions.xml", file.FullName);

            var instructions = UpdateInstructionsReader.ParseDocument(xDoc);

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
                new UpdatePackageData {Id = 4, Version = "3.5"},
                new UpdatePackageData {Id = 2, Version = "2.0"},
                new UpdatePackageData {Id = 1, Version = "1.0"},
                new UpdatePackageData {Id = 3, Version = "3.0"}
            };

            var parentFolder = UpdateHandler.PathFinder.GetUpdatesParentFolder(@"C:\AppFolder", "App Updates");
            Assert.AreEqual(@"C:\AppFolder\App Updates", parentFolder.FullName);

            var updater = UpdateHandler.PathFinder.GetUpdaterExecutable(@"C:\AppFolder", "App Updates");
            Assert.AreEqual(@"C:\AppFolder\App Updates\Updater.exe", updater.FullName);
            
            var subFolder = UpdateHandler.PathFinder.GetUpdatesSubfolder(@"C:\AppFolder", "App Updates",packages);
            Assert.AreEqual(@"C:\AppFolder\App Updates\3.5", subFolder.FullName);

        }


    }
}