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

        private IProgramsUnitOfWork GetUnit(List<ProgramUpdatePackageInfo> list, List<TelimenaPackageInfo> toolkitPackages = null)
        {
            UpdatePackageRepository pkgRepo = new UpdatePackageRepository(this.Context, new AssemblyVersionReader());
            ProgramRepository prgRepo = new ProgramRepository(this.Context);
            ProgramPackageRepository prgPkgRepo = new ProgramPackageRepository(this.Context, new AssemblyVersionReader());
            prgPkgRepo.Add(new ProgramPackageInfo("Prg.zip", 1, 2222, "1.0.0.0"));
            prgRepo.Add(new Program("prg") {Id = 1});

            Mock<IAssemblyVersionReader> reader = TestingUtilities.GetMockVersionReader();
            this.AddToolkits(reader.Object, toolkitPackages);

            foreach (ProgramUpdatePackageInfo programUpdatePackageInfo in list)
            {
                pkgRepo.Add(programUpdatePackageInfo);
            }

            this.Context.SaveChanges();

          


            ProgramsUnitOfWork unit = new ProgramsUnitOfWork(this.Context, new Mock<ITelimenaUserManager>().Object, reader.Object);
            return unit;
        }

        

        private void AddToolkits(IAssemblyVersionReader reader, List<TelimenaPackageInfo> toolkitPackages = null)
        {
          
            var toolkitRepo = new ToolkitDataRepository(this.Context, reader);
            if (toolkitPackages == null)
            {
                toolkitRepo.StorePackageAsync(isBeta: false, introducesBreakingChanges: false, fileStream: TestingUtilities.GenerateStream("0.5.0.0"), fileSaver: new Mock<IFileSaver>().Object ).GetAwaiter().GetResult();
                toolkitRepo.StorePackageAsync(isBeta: false, introducesBreakingChanges: false, fileStream: TestingUtilities.GenerateStream("0.7.0.0"), fileSaver: new Mock<IFileSaver>().Object ).GetAwaiter().GetResult();
                toolkitRepo.StorePackageAsync(isBeta: false, introducesBreakingChanges: false, fileStream: TestingUtilities.GenerateStream("1.0.0.0"), fileSaver: new Mock<IFileSaver>().Object ).GetAwaiter().GetResult();
                toolkitRepo.StorePackageAsync(isBeta: true, introducesBreakingChanges: false,  fileStream: TestingUtilities.GenerateStream("1.2.0.0"), fileSaver: new Mock<IFileSaver>().Object ).GetAwaiter().GetResult();
                toolkitRepo.StorePackageAsync(isBeta: false, introducesBreakingChanges: true,  fileStream: TestingUtilities.GenerateStream("1.4.0.0"), fileSaver: new Mock<IFileSaver>().Object ).GetAwaiter().GetResult();
                toolkitRepo.StorePackageAsync(isBeta: true, introducesBreakingChanges: false,  fileStream: TestingUtilities.GenerateStream("1.6.0.0"), fileSaver: new Mock<IFileSaver>().Object ).GetAwaiter().GetResult();
                toolkitRepo.StorePackageAsync(isBeta: true, introducesBreakingChanges: true,   fileStream: TestingUtilities.GenerateStream("1.8.0.0"), fileSaver: new Mock<IFileSaver>().Object ).GetAwaiter().GetResult();
                toolkitRepo.StorePackageAsync(isBeta: true, introducesBreakingChanges: false,  fileStream: TestingUtilities.GenerateStream("2.0.0.0"), fileSaver: new Mock<IFileSaver>().Object ).GetAwaiter().GetResult();
            }
        }

        [Test]
        public void Beta_NonStandaloneUpdateAvailable()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222, "1.0.0.0") {IsStandalone = true, Id = 1}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222, "1.0.0.0") {IsStandalone = false, Id = 2}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = false, Id = 3}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222, "1.0.0.0") {IsStandalone = true, IsBeta = true, Id = 4}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222, "1.0.0.0") {IsStandalone = false, Id = 5}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Id = 6}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "0.7.0.0", "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(5, result.UpdatePackages.Count());

            Assert.IsTrue(result.UpdatePackages.Last().Version == "1.0.0.0" && result.UpdatePackages.Last().FileName == "Telimena.Client.zip");
            Assert.AreEqual(5, result.UpdatePackages[0].Id);
            Assert.AreEqual(3, result.UpdatePackages[1].Id);
            Assert.AreEqual(2, result.UpdatePackages[2].Id);
            Assert.AreEqual(1, result.UpdatePackages[3].Id);


            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(6, result.UpdatePackages[0].Id);
            Assert.AreEqual(5, result.UpdatePackages[1].Id);
            Assert.AreEqual(4, result.UpdatePackages[2].Id);
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
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222, "1.0.0.0") {IsStandalone = true, IsBeta = true, Id = 1}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Id = 2}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Id = 3}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222, "1.0.0.0") {IsStandalone = true, IsBeta = true, Id = 4}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Id = 5}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Id = 6}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "2.0.0.0", "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(0, result.UpdatePackages.Count());
            Assert.IsNull(result.Exception);

            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackages.Count());
            Assert.AreEqual(6, result.UpdatePackages[0].Id);
            Assert.AreEqual("api/ProgramUpdates/Get?id=6", result.UpdatePackages[0].DownloadUrl);

            Assert.AreEqual(5, result.UpdatePackages[1].Id);
            Assert.AreEqual(4, result.UpdatePackages[2].Id);
            //no update for toolkit, because its already maxed
            Assert.IsFalse(result.UpdatePackages.Any(x => x.FileName == "Telimena.Client.zip"));

        }

        [Test]
        public void Beta_TestSimplestScenario()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.0", 2222, "1.0.0.0") {IsStandalone = true, Id = 1}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222, "1.0.0.0") {IsStandalone = true, Id = 2}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = true, IsBeta = true, Id = 3}
                , new ProgramUpdatePackageInfo("pkg.zip", 2, "1.2.0.4", 2222, "1.0.0.0") {IsStandalone = true, IsBeta = true, Id = 4} //different program
            });


            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "0.5.0.0", "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(2, result.UpdatePackages[0].Id);
            Assert.AreEqual(2, result.UpdatePackages.Count);
            //return the version that is higher than max supported, but does not introduce breaking changes
            Assert.IsTrue(result.UpdatePackages.Last().Version == "1.0.0.0" && result.UpdatePackages.Last().FileName == "Telimena.Client.zip");

            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(3, result.UpdatePackages[0].Id);
            Assert.AreEqual("api/ProgramUpdates/Get?id=3", result.UpdatePackages[0].DownloadUrl);
            Assert.AreEqual(2, result.UpdatePackages.Count);

            //return the version that is higher than max supported, but does not introduce breaking changes
            Assert.AreEqual("1.2.0.0", result.UpdatePackages.Single(x => x.FileName == "Telimena.Client.zip"
                                                                         && x.DownloadUrl == $"api/Toolkit/Get?id=4").Version);

        }

        [Test]
        public void BetaTest_OnlyBetaOnlyNonStandaloneUpdateAvailable()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Id = 1}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Id = 2}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Id = 3}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Id = 4}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "1.8.0.0", "1.0.0.0");
            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(0, result.UpdatePackages.Count());
            Assert.IsNull(result.Exception);

            request.AcceptBeta = true;

            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(5, result.UpdatePackages.Count());

            Assert.AreEqual(4, result.UpdatePackages[0].Id);
            Assert.AreEqual(3, result.UpdatePackages[1].Id);
            Assert.AreEqual(2, result.UpdatePackages[2].Id);
            Assert.AreEqual(1, result.UpdatePackages[3].Id);

            //return the version that is supported, because it does not introduce breaking changes
            Assert.AreEqual("2.0.0.0", result.UpdatePackages.Single(x => x.FileName == "Telimena.Client.zip").Version);


        }

        [Test]
        public void BetaTest_OnlyNonStandaloneUpdateAvailable()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Id = 1}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Id = 2}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222, "1.0.0.0") {IsStandalone = false, Id = 3}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222, "1.0.0.0") {IsStandalone = false, IsBeta = true, Id = 4}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "1.0.0.0", "1.0.0.0");
            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackages.Single().Id);
            //no toolkit update here, because 1.0 is the highest non beta that is already in use
            Assert.IsFalse(result.UpdatePackages.Any(x => x.FileName == "Telimena.Client.zip"));


            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();


            Assert.AreEqual(5, result.UpdatePackages.Count);

            Assert.AreEqual(4, result.UpdatePackages[0].Id);
            Assert.AreEqual(3, result.UpdatePackages[1].Id);
            Assert.AreEqual(2, result.UpdatePackages[2].Id);
            Assert.AreEqual(1, result.UpdatePackages[3].Id);


            //return the version that is higher than max supported, but does not introduce breaking changes
            Assert.AreEqual("1.2.0.0", result.UpdatePackages.Single(x => x.FileName == "Telimena.Client.zip").Version);


        }

        [Test]
        public void Test_NonStandaloneUpdateAvailable()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222, "1.0.0.0") {IsStandalone = true, Id = 1}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222, "1.0.0.0") {IsStandalone = true, Id = 2}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = false, Id = 3}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222, "1.0.0.0") {IsStandalone = true, Id = 4}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222, "1.0.0.0") {IsStandalone = false, Id = 5}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222, "1.0.0.0") {IsStandalone = false, Id = 6}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "1.0.0.0", "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackages.Count());

            Assert.AreEqual(6, result.UpdatePackages[0].Id);
            Assert.AreEqual(5, result.UpdatePackages[1].Id);
            Assert.AreEqual(4, result.UpdatePackages[2].Id);


            //no update here - the version 1.2 is beta, whereas 1.4 has breaking changes
            Assert.IsFalse(result.UpdatePackages.Any(x => x.FileName == "Telimena.Client.zip"));


            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(4, result.UpdatePackages.Count());

            Assert.AreEqual(6, result.UpdatePackages[0].Id);
            Assert.AreEqual(5, result.UpdatePackages[1].Id);
            Assert.AreEqual(4, result.UpdatePackages[2].Id);

            //return the version that is higher than max supported, but does not introduce breaking changes
            Assert.IsTrue(result.UpdatePackages.Last().Version == "1.2.0.0" && result.UpdatePackages.Last().FileName == "Telimena.Client.zip");

        }

        [Test]
        public void Test_NoUpdatesAvailable()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>());

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "1.4.0.0", "1.0.0.0");

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
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "0.9.0.0", "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(0, result.UpdatePackages.Count);
            //Assert.AreEqual("1.0.0.0", result.UpdatePackages.Single().Version);
            //Assert.AreEqual("Telimena.Client.zip", result.UpdatePackages.Single().FileName);
            Assert.IsNull(result.Exception);


            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(0, result.UpdatePackages.Count);

            //Assert.AreEqual("1.2.0.0", result.UpdatePackages.Single().Version);
            //Assert.AreEqual("Telimena.Client.zip", result.UpdatePackages.Single().FileName);

            Assert.IsNull(result.Exception);
        }


        [Test]
        public void Test_OnlyNonStandaloneUpdateAvailable()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = false, Id = 1}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222, "1.0.0.0") {IsStandalone = false, Id = 2}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222, "1.0.0.0") {IsStandalone = false, Id = 3}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222, "1.6.0.0") {IsStandalone = false, Id = 4}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "1.0.0.0", "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(5, result.UpdatePackages.Count());

            Assert.AreEqual(4, result.UpdatePackages[0].Id);
            Assert.AreEqual(3, result.UpdatePackages[1].Id);
            Assert.AreEqual(2, result.UpdatePackages[2].Id);
            Assert.AreEqual(1, result.UpdatePackages[3].Id);
            //one of the packages supports version 1.6, however 1.6 is beta. It is higher than 1.4 though, so 1.4 is OK
            Assert.AreEqual("1.4.0.0", result.UpdatePackages.Single(x => x.FileName == "Telimena.Client.zip").Version);


            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(5, result.UpdatePackages.Count());

            Assert.AreEqual(4, result.UpdatePackages[0].Id);
            Assert.AreEqual(3, result.UpdatePackages[1].Id);
            Assert.AreEqual(2, result.UpdatePackages[2].Id);
            Assert.AreEqual(1, result.UpdatePackages[3].Id);

            //one of the packages supports version 1.6
            Assert.AreEqual("1.6.0.0", result.UpdatePackages.Single(x => x.FileName == "Telimena.Client.zip").Version);

        }

        [Test]
        public void TestSimplestScenario()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222, "1.0.0.0") {IsStandalone = true, Id = 1}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222, "1.0.0.0") {IsStandalone = true, Id = 2}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = true, Id = 3}
            });


            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "0.0.0.1", "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackages[0].Id);
            Assert.AreEqual("1.0.0.0", result.UpdatePackages.Single(x => x.FileName == "Telimena.Client.zip").Version);


            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(3, result.UpdatePackages[0].Id);

            Assert.AreEqual("1.2.0.0", result.UpdatePackages.Single(x => x.FileName == "Telimena.Client.zip").Version);

        }

        [Test]
        public void TestSimplestScenario_PreviousWasBeta()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222, "1.0.0.0") {IsStandalone = true, Id = 1}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222, "1.0.0.0") {IsStandalone = true, IsBeta = true, Id = 2}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = true, Id = 3}
                , new ProgramUpdatePackageInfo("pkg.zip", 2, "1.2.0.4", 2222, "2.0.0.0") {IsStandalone = true, Id = 4} //different program
            });


            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "1.0.0.0", "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackages.Single().Id);
            //one of the packages accepts a higher version, but its a package for different program, so no updater update here

            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(3, result.UpdatePackages[0].Id);

            Assert.AreEqual("1.2.0.0", result.UpdatePackages.Single(x => x.FileName == "Telimena.Client.zip").Version);
        }
    }
}