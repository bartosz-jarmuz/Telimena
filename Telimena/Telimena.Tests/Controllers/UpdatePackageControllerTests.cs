using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DbIntegrationTestHelpers;
using Moq;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Api;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.Repository.Implementation;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;
using TelimenaClient;
using TelimenaClient.Serializer;

namespace Telimena.Tests
{
    [TestFixture]
    public class UpdatePackageControllerTests : IntegrationTestsContextNotShared<TelimenaContext>
    {
        private readonly ITelimenaSerializer serializer = new TelimenaSerializer();

        private readonly Guid Program1Key = Guid.Parse("2f680cbe-2d27-44c1-9c1f-1812fe550f9f");
        private readonly Guid User1Guid = Guid.Parse("0717a970-990c-40f8-a053-3ff90fe48580");

        protected internal Guid  PrgPkg_1 = Guid.Parse("525b3edd-fa47-47d9-81b6-c9d05a77bcb0");
        protected internal Guid  PrgPkg_2 = Guid.Parse("92a9bf8a-a9b2-4778-93c0-fde44eacece6");
        protected internal Guid  PrgPkg_3 = Guid.Parse("0d9dd99e-242b-4b6c-ba24-1ded4bb9d87d");
        protected internal Guid  PrgPkg_4 = Guid.Parse("dca532d7-cb6f-4fe2-ac01-4ef83119e250");
        protected internal Guid  PrgPkg_5 = Guid.Parse("3fb61c5b-ece2-4b60-b659-85d94bfb39c0");
        protected internal Guid  PrgPkg_6 = Guid.Parse("871ae68f-63d2-4105-b2bb-9d2c28cf6523");
        private List<Guid> toolkitPackageGuids;

        private IProgramsUnitOfWork GetUnit(List<ProgramUpdatePackageInfo> list, List<TelimenaPackageInfo> toolkitPackages = null)
        {
            UpdatePackageRepository pkgRepo = new UpdatePackageRepository(this.Context, new AssemblyStreamVersionReader());
            ProgramRepository prgRepo = new ProgramRepository(this.Context);
            ProgramPackageRepository prgPkgRepo = new ProgramPackageRepository(this.Context, new AssemblyStreamVersionReader());
            prgPkgRepo.Add(new ProgramPackageInfo("Prg.zip", 1, 2222, "1.0.0.0"));
            prgRepo.Add(new Program("prg", this.Program1Key) { Id = 1 });

            Mock<IAssemblyStreamVersionReader> reader = TestingUtilities.GetMockVersionReader();
            this.AddToolkits(reader.Object, toolkitPackages);

            foreach (ProgramUpdatePackageInfo programUpdatePackageInfo in list)
            {
                pkgRepo.Add(programUpdatePackageInfo);
            }

            this.Context.SaveChanges();




            ProgramsUnitOfWork unit = new ProgramsUnitOfWork(this.Context, new Mock<ITelimenaUserManager>().Object, reader.Object);
            return unit;
        }

        private void AddToolkits(IAssemblyStreamVersionReader reader, List<TelimenaPackageInfo> toolkitPackages = null)
        {
            this.toolkitPackageGuids = new List<Guid>();
            var toolkitRepo = new ToolkitDataRepository(this.Context, reader);
            if (toolkitPackages == null)
            {
                toolkitPackageGuids .Add(toolkitRepo.StorePackageAsync(isBeta: false, introducesBreakingChanges: false, fileStream: TestingUtilities.GenerateStream("0.5.0.0"), fileSaver: new Mock<IFileSaver>().Object).GetAwaiter().GetResult().Guid);
                toolkitPackageGuids .Add(toolkitRepo.StorePackageAsync(isBeta: false, introducesBreakingChanges: false, fileStream: TestingUtilities.GenerateStream("0.7.0.0"), fileSaver: new Mock<IFileSaver>().Object).GetAwaiter().GetResult().Guid);
                toolkitPackageGuids .Add(toolkitRepo.StorePackageAsync(isBeta: false, introducesBreakingChanges: false, fileStream: TestingUtilities.GenerateStream("1.0.0.0"), fileSaver: new Mock<IFileSaver>().Object).GetAwaiter().GetResult().Guid);
                toolkitPackageGuids .Add(toolkitRepo.StorePackageAsync(isBeta: true, introducesBreakingChanges: false, fileStream: TestingUtilities.GenerateStream("1.2.0.0"), fileSaver: new Mock<IFileSaver>().Object).GetAwaiter().GetResult().Guid);
                toolkitPackageGuids .Add(toolkitRepo.StorePackageAsync(isBeta: false, introducesBreakingChanges: true, fileStream: TestingUtilities.GenerateStream("1.4.0.0"), fileSaver: new Mock<IFileSaver>().Object).GetAwaiter().GetResult().Guid);
                toolkitPackageGuids .Add(toolkitRepo.StorePackageAsync(isBeta: true, introducesBreakingChanges: false, fileStream: TestingUtilities.GenerateStream("1.6.0.0"), fileSaver: new Mock<IFileSaver>().Object).GetAwaiter().GetResult().Guid);
                toolkitPackageGuids .Add(toolkitRepo.StorePackageAsync(isBeta: true, introducesBreakingChanges: true, fileStream: TestingUtilities.GenerateStream("1.8.0.0"), fileSaver: new Mock<IFileSaver>().Object).GetAwaiter().GetResult().Guid);
                toolkitPackageGuids .Add(toolkitRepo.StorePackageAsync(isBeta: true, introducesBreakingChanges: false, fileStream: TestingUtilities.GenerateStream("2.0.0.0"), fileSaver: new Mock<IFileSaver>().Object).GetAwaiter().GetResult().Guid);
            }
        }

        [Test]
        public void Beta_NonStandaloneUpdateAvailable()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222, "1.0.0.0") {IsStandalone = true,    Guid = this.PrgPkg_1}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222, "1.0.0.0") {IsStandalone = false, Guid = this.PrgPkg_2}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = false, Guid = this.PrgPkg_3}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222, "1.0.0.0") {IsStandalone = true, IsBeta = true, Guid = this.PrgPkg_4}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222, "1.0.0.0") {IsStandalone = false, Guid = this.PrgPkg_5}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Guid = this.PrgPkg_6}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(this.Program1Key, new VersionData("", "1.2.0.0"), this.User1Guid, false, "0.7.0.0", "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(5, result.UpdatePackages.Count());

            Assert.IsTrue(result.UpdatePackages.Last().Version == "1.0.0.0" && result.UpdatePackages.Last().FileName == "Telimena.Client.zip");
            Assert.AreEqual(this.PrgPkg_5, result.UpdatePackages[0].Guid);
            Assert.AreEqual(this.PrgPkg_3, result.UpdatePackages[1].Guid);
            Assert.AreEqual(this.PrgPkg_2, result.UpdatePackages[2].Guid);
            Assert.AreEqual(this.PrgPkg_1, result.UpdatePackages[3].Guid);


            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(this.PrgPkg_6, result.UpdatePackages[0].Guid);
            Assert.AreEqual(this.PrgPkg_5, result.UpdatePackages[1].Guid);
            Assert.AreEqual(this.PrgPkg_4, result.UpdatePackages[2].Guid);
            //return the version that is higher than max supported, but does not introduce breaking changes
            Assert.AreEqual("1.2.0.0", result.UpdatePackages.Single(x => x.FileName == "Telimena.Client.zip").Version);
            //also check its the last package 
            Assert.IsTrue(result.UpdatePackages.Last().FileName == "Telimena.Client.zip");

        }

        [Test]
        public void Beta_OnlyBetaAvailable()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222, "1.0.0.0") {IsStandalone = true, IsBeta = true,    Guid = this.PrgPkg_1}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Guid = this.PrgPkg_2}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Guid = this.PrgPkg_3}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222, "1.0.0.0") {IsStandalone = true, IsBeta = true,  Guid = this.PrgPkg_4}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Guid = this.PrgPkg_5}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Guid = this.PrgPkg_6}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(this.Program1Key, new VersionData("2.0.0.0", "1.2.0.0"), this.User1Guid, false, "2.0.0.0", "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(0, result.UpdatePackages.Count());
            Assert.IsNull(result.Exception);

            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackages.Count());
            Assert.AreEqual(this.PrgPkg_6, result.UpdatePackages[0].Guid);
            Assert.AreEqual($"api/ProgramUpdates/Get?id={this.PrgPkg_6}", result.UpdatePackages[0].DownloadUrl);

            Assert.AreEqual(this.PrgPkg_5, result.UpdatePackages[1].Guid);
            Assert.AreEqual(this.PrgPkg_4, result.UpdatePackages[2].Guid);
            //no update for toolkit, because its already maxed
            Assert.IsFalse(result.UpdatePackages.Any(x => x.FileName == "Telimena.Client.zip"));

        }

        [Test]
        public void Beta_TestSimplestScenario()
        {
            
            
            
            
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.0", 2222, "1.0.0.0") {IsStandalone = true,                  Guid = this.PrgPkg_1}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222, "1.0.0.0") {IsStandalone = true,                Guid = this.PrgPkg_2}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = true, IsBeta = true, Guid = this.PrgPkg_3}
                , new ProgramUpdatePackageInfo("pkg.zip", 2, "1.2.0.4", 2222, "1.0.0.0") {IsStandalone = true, IsBeta = true, Guid = this.PrgPkg_4} //different program
            });


            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(this.Program1Key, new VersionData("2.0.0.0", "1.2.0.0"), this.User1Guid, false, "0.5.0.0", "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(this.PrgPkg_2, result.UpdatePackages[0].Guid);
            Assert.AreEqual(2, result.UpdatePackages.Count);
            //return the version that is higher than max supported, but does not introduce breaking changes
            Assert.IsTrue(result.UpdatePackages.Last().Version == "1.0.0.0" && result.UpdatePackages.Last().FileName == "Telimena.Client.zip");

            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(this.PrgPkg_3, result.UpdatePackages[0].Guid);
            Assert.AreEqual("api/ProgramUpdates/Get?id=0d9dd99e-242b-4b6c-ba24-1ded4bb9d87d", result.UpdatePackages[0].DownloadUrl);
            Assert.AreEqual(2, result.UpdatePackages.Count);

            //return the version that is higher than max supported, but does not introduce breaking changes
            Assert.AreEqual("1.2.0.0", result.UpdatePackages.Single(x => x.FileName == "Telimena.Client.zip"
                                                                         && x.DownloadUrl.Contains($"api/Toolkit/Get?id=")).Version);

        }

        [Test]
        public void BetaTest_OnlyBetaOnlyNonStandaloneUpdateAvailable()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true,   Guid = this.PrgPkg_1}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Guid = this.PrgPkg_2}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Guid = this.PrgPkg_3}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Guid = this.PrgPkg_4}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(this.Program1Key, new VersionData( "0.0.0.0", "1.2.0.0"), this.User1Guid, false, "1.8.0.0", "1.0.0.0");
            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(0, result.UpdatePackages.Count());
            Assert.IsNull(result.Exception);

            request.AcceptBeta = true;

            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(5, result.UpdatePackages.Count());

            Assert.AreEqual(this.PrgPkg_4, result.UpdatePackages[0].Guid);
            Assert.AreEqual(this.PrgPkg_3, result.UpdatePackages[1].Guid);
            Assert.AreEqual(this.PrgPkg_2, result.UpdatePackages[2].Guid);
            Assert.AreEqual(this.PrgPkg_1, result.UpdatePackages[3].Guid);

            //return the version that is supported, because it does not introduce breaking changes
            Assert.AreEqual("2.0.0.0", result.UpdatePackages.Single(x => x.FileName == "Telimena.Client.zip").Version);


            request.VersionData.FileVersion = "3.0.0.0"; //version too high
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(0, result.UpdatePackages.Count());

        }

        [Test]
        public void BetaTest_OnlyNonStandaloneUpdateAvailable()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true,   Guid = this.PrgPkg_1}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Guid = this.PrgPkg_2}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222, "1.0.0.0") {IsStandalone = false,                Guid = this.PrgPkg_3}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Guid = this.PrgPkg_4}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(this.Program1Key, new VersionData("2.0.0.0", "1.2.0.0"), this.User1Guid, false, "1.0.0.0", "1.0.0.0");
            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(this.PrgPkg_3, result.UpdatePackages.Single().Guid);
            //no toolkit update here, because 1.0 is the highest non beta that is already in use
            Assert.IsFalse(result.UpdatePackages.Any(x => x.FileName == "Telimena.Client.zip"));


            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();


            Assert.AreEqual(5, result.UpdatePackages.Count);

            Assert.AreEqual(this.PrgPkg_4, result.UpdatePackages[0].Guid);
            Assert.AreEqual(this.PrgPkg_3, result.UpdatePackages[1].Guid);
            Assert.AreEqual(this.PrgPkg_2, result.UpdatePackages[2].Guid);
            Assert.AreEqual(this.PrgPkg_1, result.UpdatePackages[3].Guid);


            //return the version that is higher than max supported, but does not introduce breaking changes
            Assert.AreEqual("1.2.0.0", result.UpdatePackages.Single(x => x.FileName == "Telimena.Client.zip").Version);


        }

        [Test]
        public void Test_NonStandaloneUpdateAvailable()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222, "1.0.0.0") {IsStandalone = true,    Guid = this.PrgPkg_1}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222, "1.0.0.0") {IsStandalone = true,  Guid = this.PrgPkg_2}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = false, Guid = this.PrgPkg_3}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222, "1.0.0.0") {IsStandalone = true,  Guid = this.PrgPkg_4}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222, "1.0.0.0") {IsStandalone = false, Guid = this.PrgPkg_5}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222, "1.0.0.0") {IsStandalone = false, Guid = this.PrgPkg_6}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(this.Program1Key, new VersionData("2.0.0.0", "1.2.0.0"), this.User1Guid, false, "1.0.0.0", "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackages.Count());

            Assert.AreEqual(this.PrgPkg_6, result.UpdatePackages[0].Guid);
            Assert.AreEqual(this.PrgPkg_5, result.UpdatePackages[1].Guid);
            Assert.AreEqual(this.PrgPkg_4, result.UpdatePackages[2].Guid);


            //no update here - the version 1.2 is beta, whereas 1.4 has breaking changes
            Assert.IsFalse(result.UpdatePackages.Any(x => x.FileName == "Telimena.Client.zip"));


            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(4, result.UpdatePackages.Count());

            Assert.AreEqual(this.PrgPkg_6, result.UpdatePackages[0].Guid);
            Assert.AreEqual(this.PrgPkg_5, result.UpdatePackages[1].Guid);
            Assert.AreEqual(this.PrgPkg_4, result.UpdatePackages[2].Guid);

            //return the version that is higher than max supported, but does not introduce breaking changes
            Assert.IsTrue(result.UpdatePackages.Last().Version == "1.2.0.0" && result.UpdatePackages.Last().FileName == "Telimena.Client.zip");

        }

        [Test]
        public void Test_NoUpdatesAvailable()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>());

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(this.Program1Key, new VersionData("2.0.0.0", "1.2.0.0"), this.User1Guid, false, "1.4.0.0", "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.IsEmpty(result.UpdatePackages);
            Assert.IsNull(result.Exception);
        }

        [Test]
        public void Test_OnlyToolkitUpdateAvailable()
        {

            //ACTUALLY - CHANGE THE BEHAVIOUR AT LEAST FOR NOW
            // SEEMS THIS SCENARIO IS NOT DESIRED!
            // the challenge is whether it makes sense to update the toolkit (where there are no breaking changes) to make use of new telemetry features
            // versus
            // do not change anything in the client app unless the developer has made an update (and that seems safer)
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>());

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(this.Program1Key, new VersionData("2.0.0.0", "1.2.0.0"), this.User1Guid, false, "0.9.0.0", "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(0, result.UpdatePackages.Count);
            //Assert.AreEqual("1.0.0.0", result.UpdatePackages.Single().AssemblyVersion);
            //Assert.AreEqual("Telimena.Client.zip", result.UpdatePackages.Single().FileName);
            Assert.IsNull(result.Exception);


            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(0, result.UpdatePackages.Count);

            //Assert.AreEqual("1.2.0.0", result.UpdatePackages.Single().AssemblyVersion);
            //Assert.AreEqual("Telimena.Client.zip", result.UpdatePackages.Single().FileName);

            Assert.IsNull(result.Exception);
        }


        [Test]
        public void Test_OnlyNonStandaloneUpdateAvailable()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = false,   Guid = this.PrgPkg_1}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222, "1.0.0.0") {IsStandalone = false, Guid = this.PrgPkg_2}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222, "1.0.0.0") {IsStandalone = false, Guid = this.PrgPkg_3}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222, "1.6.0.0") {IsStandalone = false, Guid = this.PrgPkg_4}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(this.Program1Key, new VersionData("2.0.0.0", "1.2.0.0"), this.User1Guid, false, "1.0.0.0", "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(5, result.UpdatePackages.Count());

            Assert.AreEqual(this.PrgPkg_4, result.UpdatePackages[0].Guid);
            Assert.AreEqual(this.PrgPkg_3, result.UpdatePackages[1].Guid);
            Assert.AreEqual(this.PrgPkg_2, result.UpdatePackages[2].Guid);
            Assert.AreEqual(this.PrgPkg_1, result.UpdatePackages[3].Guid);
            //one of the packages supports version 1.6, however 1.6 is beta. It is higher than 1.4 though, so 1.4 is OK
            Assert.AreEqual("1.4.0.0", result.UpdatePackages.Single(x => x.FileName == "Telimena.Client.zip").Version);


            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(5, result.UpdatePackages.Count());

            Assert.AreEqual(this.PrgPkg_4, result.UpdatePackages[0].Guid);
            Assert.AreEqual(this.PrgPkg_3, result.UpdatePackages[1].Guid);
            Assert.AreEqual(this.PrgPkg_2, result.UpdatePackages[2].Guid);
            Assert.AreEqual(this.PrgPkg_1, result.UpdatePackages[3].Guid);

            //one of the packages supports version 1.6
            Assert.AreEqual("1.6.0.0", result.UpdatePackages.Single(x => x.FileName == "Telimena.Client.zip").Version);

        }

        [Test]
        public void TestSimplestScenario()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222, "1.0.0.0") {IsStandalone = true,   Guid =this.PrgPkg_1}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222, "1.0.0.0") {IsStandalone = true, Guid =this.PrgPkg_2}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = true, Guid =this.PrgPkg_3}
            });


            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(this.Program1Key, new VersionData("2.0.0.0", "1.2.0.0"), this.User1Guid, false, "0.0.0.1", "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(this.PrgPkg_3, result.UpdatePackages[0].Guid);
            Assert.AreEqual("1.0.0.0", result.UpdatePackages.Single(x => x.FileName == "Telimena.Client.zip").Version);


            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(this.PrgPkg_3, result.UpdatePackages[0].Guid);

            Assert.AreEqual("1.2.0.0", result.UpdatePackages.Single(x => x.FileName == "Telimena.Client.zip").Version);

        }

        [Test]
        public void TestSimplestScenario_MultiplePackagesSameVersion()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222, "1.0.0.0") { IsStandalone  = false, Guid =this.PrgPkg_1}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222, "1.0.0.0") {IsStandalone = false, Guid =this.PrgPkg_2}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = false, Guid =Guid.NewGuid()}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = false, Guid =Guid.NewGuid()}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = false, Guid =Guid.NewGuid()}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = false, Guid =Guid.NewGuid()}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = false, Guid =this.PrgPkg_3}
            });


            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(this.Program1Key, new VersionData("3.0.0.0", "1.2.0.1"), this.User1Guid, false, "0.0.0.1", "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(this.PrgPkg_3, result.UpdatePackages[0].Guid);
            Assert.AreEqual(3, result.UpdatePackages.Count);
            Assert.AreEqual("1.0.0.0", result.UpdatePackages.Single(x => x.FileName == "Telimena.Client.zip").Version);


            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(this.PrgPkg_3, result.UpdatePackages[0].Guid);

            Assert.AreEqual("1.2.0.0", result.UpdatePackages.Single(x => x.FileName == "Telimena.Client.zip").Version);

        }

        [Test]
        public void TestSimplestScenario_PreviousWasBeta()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222, "1.0.0.0") {IsStandalone = true, Guid = this.PrgPkg_1}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222, "1.0.0.0") {IsStandalone = true, IsBeta = true, Guid = this.PrgPkg_2}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = true, Guid = this.PrgPkg_3}
                , new ProgramUpdatePackageInfo("pkg.zip", 2, "1.2.0.4", 2222, "2.0.0.0") {IsStandalone = true, Guid = this.PrgPkg_4} //different program
            });


            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(this.Program1Key, new VersionData("2.0.0.0", "1.2.0.0"), this.User1Guid, false, "1.0.0.0", "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(this.PrgPkg_3, result.UpdatePackages.Single().Guid);
            //one of the packages accepts a higher version, but its a package for different program, so no updater update here

            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(this.PrgPkg_3, result.UpdatePackages[0].Guid);

            Assert.AreEqual("1.2.0.0", result.UpdatePackages.Single(x => x.FileName == "Telimena.Client.zip").Version);
        }
    }
}