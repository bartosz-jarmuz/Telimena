using System;
using System.Linq;
using DbIntegrationTestHelpers;
using Moq;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Api.V1;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;
using static Telimena.Tests.TestingUtilities;

namespace Telimena.Tests
{
    [TestFixture]
    public class UpdaterUpdatesTests : IntegrationTestsContextNotShared<TelimenaPortalContext>
    {
        public Guid TestProgramTelemetryKey = Guid.Parse("dc13cced-30ea-4628-a81d-21d86f37df95");
        public Guid TestProgram2TelemetryKey = Guid.Parse("e0164bcd-f091-4b85-b88b-28b6efbebd2b");
        public Guid TestProgramWithDifferentUpdaterTelemetryKey = Guid.Parse("14b6e9b2-7886-4e4b-ba55-3541155c29ee");
        public Guid ProgramWhichChangesUpdaterTelemetryKey = Guid.Parse("ae21e6c3-131c-46ca-a91a-ee51f90d44cb");

        private IFileSaver saver = new Mock<IFileSaver>().Object;
        private IAssemblyStreamVersionReader assemblyStreamVersionReader;
        private string otherUpdaterInternalName = "SomeUpdater";
        private Program programWhichChangesUpdater;
        private Program programWithDifferentUpdater;
        private Program testProgram;
        private TelimenaTelemetryContext telemetryContext = new TelimenaTelemetryContext();

        protected override Action SeedAction =>
            () =>
            {
                TelimenaPortalDbInitializer.SeedUsers(this.Context);
                TelimenaPortalDbInitializer.SeedToolkit(this.Context);
                this.Prepare();

            };
        //The updater  can evolve independently of the Toolkit - some changes might be breaking the contracts, but most - should not
        //there were several updater updates (maybe changing UI?)
        //first two have no breaking changes - compatible with toolkit 0.0
        //3rd has breaking changes and requires toolkit 0.5 or above
        //4th has breaking changes and requires toolkit 0.9 or above
        //5th has no breaking changes, but is newer than 4th, so requires 0.9 or above
        //6th has no breaking changes, but is newer than 4th & 5th, so requires 0.9 or above
        //7th has breaking changes and requires toolkit 1.3 or above
        //8th has no breaking changes, but is newer than 7th so requires 1.3 or above

        //todo - When new updater is uploaded, it should specify what is the MINIMUM version of tookit that can use it
        //First updater will have version 0.0.0.0
        //It should be enforced automatically (OR SHOULD IT NOT?)
        //that if previous version of an updater introduced breaking changes on a certain toolkit version
        //then subsequent updaters should require at least that version
        // OR MAYBE IT SHOULD BE OVERRIDABLE? (consider fixing breaking changes?)
        // In any case, the decision whether an update should be downloaded & installed should be made in the web app
        // The Telimena.Client and the Updater should be 100% dumb and just download whatever is returned to them.

        //todo - how to automatically check if an updater is compatible with certain toolkit versions?

            private readonly Guid User1Guid = Guid.Parse("4e80652e-d0ba-4742-a78c-3a63de4a63f0");

        private IToolkitDataUnitOfWork Prepare()
        {
            this.assemblyStreamVersionReader = GetMockVersionReader().Object;
            ToolkitDataUnitOfWork unit = new ToolkitDataUnitOfWork(this.Context, this.telemetryContext, this.assemblyStreamVersionReader);

            var defaultUpdater = unit.UpdaterRepository.GetUpdater(DefaultToolkitNames.UpdaterInternalName).GetAwaiter().GetResult();
            var user = unit.Users.FirstOrDefault();
            var updaterOther = unit.UpdaterRepository.Add("Updater.msi", this.otherUpdaterInternalName, user);

            var ultraNewest = unit.UpdaterRepository.Add("UltraNewest", "UltraNewest", user);

            unit.CompleteAsync().GetAwaiter().GetResult();
            defaultUpdater = unit.UpdaterRepository.GetUpdater(DefaultToolkitNames.UpdaterInternalName).GetAwaiter().GetResult();
            Assert.IsTrue(defaultUpdater.Id > 0);

            this.InsertPrograms(unit, updaterOther);


            unit.UpdaterRepository.StorePackageAsync(defaultUpdater, "0.0.0.0", GenerateStream("1.0.0"), this.saver).GetAwaiter().GetResult();
            unit.CompleteAsync().GetAwaiter().GetResult();
            Assert.AreEqual(1, defaultUpdater.Packages.Count);

            var pkg = unit.UpdaterRepository.StorePackageAsync(defaultUpdater, "0.0.0.0", GenerateStream("1.1.0"), this.saver).GetAwaiter().GetResult();
            pkg.IsBeta = true;
            unit.CompleteAsync().GetAwaiter().GetResult();
            Assert.AreEqual(2, defaultUpdater.Packages.Count);

            unit.UpdaterRepository.StorePackageAsync(defaultUpdater, "0.5.0.0", GenerateStream("1.2.0"), this.saver).GetAwaiter().GetResult();
            unit.CompleteAsync().GetAwaiter().GetResult();
            Assert.AreEqual(3, defaultUpdater.Packages.Count);

            unit.UpdaterRepository.StorePackageAsync(defaultUpdater, "0.9.0.0", GenerateStream("1.3.0"), this.saver).GetAwaiter().GetResult();
            unit.CompleteAsync().GetAwaiter().GetResult();

            unit.UpdaterRepository.StorePackageAsync(defaultUpdater, "0.9.0.0", GenerateStream("1.4.0"), this.saver).GetAwaiter().GetResult();
            unit.CompleteAsync().GetAwaiter().GetResult();

            unit.UpdaterRepository.StorePackageAsync(defaultUpdater, "0.9.0.0", GenerateStream("1.5.0"), this.saver).GetAwaiter().GetResult();
            unit.CompleteAsync().GetAwaiter().GetResult();
            unit.UpdaterRepository.StorePackageAsync(defaultUpdater, "0.9.0.0", GenerateStream("1.5.0"), this.saver).GetAwaiter().GetResult();
            unit.CompleteAsync().GetAwaiter().GetResult();

            unit.UpdaterRepository.StorePackageAsync(defaultUpdater, "1.3.0.0", GenerateStream("1.6.0"), this.saver).GetAwaiter().GetResult();
            unit.CompleteAsync().GetAwaiter().GetResult();


            unit.UpdaterRepository.StorePackageAsync(updaterOther, "0.0.2.0", GenerateStream("1.6.5"), this.saver).GetAwaiter().GetResult();
            unit.CompleteAsync().GetAwaiter().GetResult();

            pkg = unit.UpdaterRepository.StorePackageAsync(defaultUpdater, "1.3.0.0", GenerateStream("1.7.0"), this.saver).GetAwaiter().GetResult();
            pkg.IsBeta = true;
            unit.CompleteAsync().GetAwaiter().GetResult();


            pkg = unit.UpdaterRepository.StorePackageAsync(updaterOther, "0.0.2.0", GenerateStream("1.8.5"), this.saver).GetAwaiter().GetResult();
            pkg.IsBeta = true;
            unit.CompleteAsync().GetAwaiter().GetResult();


            unit.UpdaterRepository.StorePackageAsync(ultraNewest, "0.0.0.0", GenerateStream("9.8.5"), this.saver).GetAwaiter().GetResult();
            unit.CompleteAsync().GetAwaiter().GetResult();

            Assert.IsTrue(defaultUpdater.Packages.Count > 4);
            return unit;
        }

        private void InsertPrograms(ToolkitDataUnitOfWork unit, WebApp.Core.Models.Portal.Updater updaterOther)
        {
            var prg = new Program("Test Program", this.TestProgram2TelemetryKey);
            unit.Programs.Add(prg);

            this.testProgram = new Program("Test Program", this.TestProgramTelemetryKey);
            unit.Programs.Add(this.testProgram);

            this.programWithDifferentUpdater = new Program("Program with different updater", this.TestProgramWithDifferentUpdaterTelemetryKey);
            this.programWithDifferentUpdater.Updater = updaterOther;
            unit.Programs.Add(this.programWithDifferentUpdater);

            this.programWhichChangesUpdater = new Program("Program which changes updater", this.ProgramWhichChangesUpdaterTelemetryKey);
            unit.Programs.Add(this.programWhichChangesUpdater);

            unit.CompleteAsync().GetAwaiter().GetResult();

            this.testProgram = this.Context.Programs.First(x => x.Name == this.testProgram.Name);
            this.programWithDifferentUpdater = this.Context.Programs.First(x => x.Name == this.programWithDifferentUpdater.Name);
            this.programWhichChangesUpdater = this.Context.Programs.First(x => x.Name == this.programWhichChangesUpdater.Name);

        }

        [Test]
        public void Test_MissingMinimumToolkitInfo()
        {
            ToolkitDataUnitOfWork unit = new ToolkitDataUnitOfWork(this.Context, this.telemetryContext, this.assemblyStreamVersionReader);
            var defaultUpdater = unit.UpdaterRepository.GetUpdater(DefaultToolkitNames.UpdaterInternalName).GetAwaiter().GetResult();
            // assert that empty min version does not break the query
            unit.UpdaterRepository.StorePackageAsync(defaultUpdater, "", GenerateStream("2.1.8.5"), this.saver).GetAwaiter().GetResult();
            unit.CompleteAsync().GetAwaiter().GetResult();

            UpdatersController controller = new UpdatersController(unit, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            var request = new UpdateRequest(telemetryKey: this.TestProgram2TelemetryKey, programVersion: new VersionData("0.0", ""), 
                userId: this.User1Guid, acceptBeta: false, 
                updaterVersion: "1.0"
                , toolkitVersion: "1.3.0");

            UpdateResponse result = controller.UpdateCheck(request).GetAwaiter().GetResult();
            Assert.AreEqual("2.1.8.5", result.UpdatePackages.Single().Version);
            Assert.IsNull(result.Exception);
        }




        [Test]
        public void Test_LatestUpdaterIsCompatible()
        {
            ToolkitDataUnitOfWork unit = new ToolkitDataUnitOfWork(this.Context, this.telemetryContext, this.assemblyStreamVersionReader);
            UpdatersController controller = new UpdatersController(unit, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            var request = new UpdateRequest(telemetryKey: this.TestProgramTelemetryKey, programVersion: new VersionData("0.0", ""), userId: this.User1Guid, acceptBeta: false, updaterVersion: "1.0"
                , toolkitVersion: "1.3.0");

            UpdateResponse result = controller.UpdateCheck(request).GetAwaiter().GetResult();
            Assert.AreEqual("1.6.0", result.UpdatePackages.Single().Version);
            Assert.AreEqual($"api/v1/updaters/{result.UpdatePackages.Single().PublicId}", result.UpdatePackages.Single().DownloadUrl);

            request = new UpdateRequest(telemetryKey: this.TestProgramWithDifferentUpdaterTelemetryKey, programVersion: new VersionData("0.0", ""), userId: this.User1Guid, acceptBeta: false
                , updaterVersion: "1.0", toolkitVersion: "1.3.0");

            result = controller.UpdateCheck(request).GetAwaiter().GetResult();
            Assert.AreEqual("1.6.5", result.UpdatePackages.Single().Version);
            Assert.AreNotEqual(Guid.Empty, result.UpdatePackages[0].PublicId);
            Assert.AreEqual($"api/v1/updaters/{result.UpdatePackages[0].PublicId}", result.UpdatePackages[0].DownloadUrl);
        }


        [Test]
        public void Test_UpdaterChange()
        {
            ToolkitDataUnitOfWork unit = new ToolkitDataUnitOfWork(this.Context, this.telemetryContext, this.assemblyStreamVersionReader);
            UpdatersController controller = new UpdatersController(unit, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            var request = new UpdateRequest(telemetryKey: this.ProgramWhichChangesUpdaterTelemetryKey, programVersion: new VersionData("0.0", ""), userId: this.User1Guid, acceptBeta: false
                , updaterVersion: "1.0", toolkitVersion: "1.3.0");

            UpdateResponse result = controller.UpdateCheck(request).GetAwaiter().GetResult();
            Assert.AreEqual("1.6.0", result.UpdatePackages.Single().Version);
            this.programWhichChangesUpdater.Updater = unit.UpdaterRepository.GetUpdater("UltraNewest").GetAwaiter().GetResult();
            unit.CompleteAsync().GetAwaiter().GetResult();
            request = new UpdateRequest(telemetryKey: this.ProgramWhichChangesUpdaterTelemetryKey, programVersion: new VersionData("0.0", ""), userId: this.User1Guid, acceptBeta: false
                , updaterVersion: "1.0", toolkitVersion: "1.3.0");

            result = controller.UpdateCheck(request).GetAwaiter().GetResult();
            Assert.AreEqual("9.8.5", result.UpdatePackages.Single().Version);
        }

        [Test]
        public void Test_LatestUpdaterIsNotCompatible_BreakingChanges()
        {
            ToolkitDataUnitOfWork unit = new ToolkitDataUnitOfWork( this.Context, this.telemetryContext, versionReader: new AssemblyStreamVersionReader());
            UpdatersController controller = new UpdatersController(work: unit, fileSaver: new Mock<IFileSaver>().Object
                , fileRetriever: new Mock<IFileRetriever>().Object);
            var request = new UpdateRequest(telemetryKey: this.TestProgramTelemetryKey, programVersion: new VersionData("0.0", ""), userId: this.User1Guid, acceptBeta: false, updaterVersion: "1.0"
                , toolkitVersion: "0.2.0");

            UpdateResponse result = controller.UpdateCheck(request).GetAwaiter().GetResult();

            Assert.AreEqual(expected: 0, actual: result.UpdatePackages.Count);
            request = new UpdateRequest(telemetryKey: this.TestProgramTelemetryKey, programVersion: new VersionData("0.0", ""), userId: this.User1Guid, acceptBeta: false, updaterVersion: "1.1.0"
                , toolkitVersion: "0.9.0");

            result = controller.UpdateCheck(request).GetAwaiter().GetResult();

            Assert.AreEqual(expected: "1.5.0", actual: result.UpdatePackages.Single().Version);
        }




        [Test]
        public void Test_LatestUpdaterIsUsed()
        {
            ToolkitDataUnitOfWork unit = new ToolkitDataUnitOfWork(this.Context, this.telemetryContext, new AssemblyStreamVersionReader());
            UpdatersController controller = new UpdatersController(unit, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            var request = new UpdateRequest(telemetryKey: this.TestProgramTelemetryKey, programVersion: new VersionData("0.0", ""), userId: this.User1Guid, acceptBeta: false, updaterVersion: "1.1"
                , toolkitVersion: "0.2.0");

            UpdateResponse result = controller.UpdateCheck(request).GetAwaiter().GetResult();

            Assert.AreEqual(0, result.UpdatePackages.Count);
            Assert.IsNull(result.Exception);
            request = new UpdateRequest(telemetryKey: this.TestProgramTelemetryKey, programVersion: new VersionData("0.0", ""), userId: this.User1Guid, acceptBeta: false, updaterVersion: "1.5"
                , toolkitVersion: "1.0");

            result = controller.UpdateCheck(request).GetAwaiter().GetResult();
            Assert.AreEqual(0, result.UpdatePackages.Count);
            Assert.IsNull(result.Exception);

            request = new UpdateRequest(telemetryKey: this.TestProgramTelemetryKey, programVersion: new VersionData("0.0", ""), userId: this.User1Guid, acceptBeta: false, updaterVersion: "1.7"
                , toolkitVersion: "2.0");

            result = controller.UpdateCheck(request).GetAwaiter().GetResult();

            Assert.IsNull(result.Exception);
            Assert.AreEqual(0, result.UpdatePackages.Count);

        }
    }
}