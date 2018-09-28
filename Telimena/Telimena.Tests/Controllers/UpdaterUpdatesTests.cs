using System;
using System.Linq;
using DbIntegrationTestHelpers;
using Moq;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Api;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;
using Telimena.ToolkitClient;
using Telimena.ToolkitClient.Serializer;

namespace Telimena.Tests
{
    [TestFixture]
    public class UpdaterUpdatesTests : IntegrationTestsContextNotShared<TelimenaContext>
    {

        private ITelimenaSerializer serializer = new TelimenaSerializer();
        protected override Action SeedAction => () => this.Prepare();
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

        private IToolkitDataUnitOfWork Prepare()
        {
            ToolkitDataUnitOfWork unit = new ToolkitDataUnitOfWork(this.Context, new AssemblyVersionReader());
            unit.UpdaterRepository.Add(new UpdaterPackageInfo("1.0.0", 1000, "0.0.0.0"));
            unit.CompleteAsync().GetAwaiter().GetResult();
            unit.UpdaterRepository.Add(new UpdaterPackageInfo("1.1.0", 1000, "0.0.0.0") {IsBeta = true});
            unit.CompleteAsync().GetAwaiter().GetResult();
            unit.UpdaterRepository.Add(new UpdaterPackageInfo("1.2.0", 1000, "0.5.0.0"));
            unit.CompleteAsync().GetAwaiter().GetResult();
            unit.UpdaterRepository.Add(new UpdaterPackageInfo("1.3.0", 1000, "0.9.0.0"));
            unit.CompleteAsync().GetAwaiter().GetResult();
            unit.UpdaterRepository.Add(new UpdaterPackageInfo("1.4.0", 1000, "0.9.0.0"));
            unit.CompleteAsync().GetAwaiter().GetResult();
            unit.UpdaterRepository.Add(new UpdaterPackageInfo("1.5.0", 1000, "0.9.0.0"));
            unit.CompleteAsync().GetAwaiter().GetResult();
            unit.UpdaterRepository.Add(new UpdaterPackageInfo("1.6.0", 1000, "1.3.0.0"));
            unit.CompleteAsync().GetAwaiter().GetResult();
            unit.UpdaterRepository.Add(new UpdaterPackageInfo("1.7.0", 1000, "1.3.0.0") {IsBeta = true});
            unit.CompleteAsync().GetAwaiter().GetResult();
            return unit;
        }

        [Test]
        public void Test_LatestUpdaterIsCompatible()
        {
            ToolkitDataUnitOfWork unit = new ToolkitDataUnitOfWork(this.Context, new AssemblyVersionReader());
            UpdaterController controller = new UpdaterController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            var request = new UpdateRequest(programId: 1, programVersion: "0.0", userId: 1, acceptBeta: false, updaterVersion: "1.0", toolkitVersion: "1.3.0");

            UpdateResponse result = controller.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual("1.6.0", result.UpdatePackages.Single().Version);
            //   Assert.AreEqual("1.7.0", result.UpdatePackagesIncludingBeta.Single().Version);
        }

        [Test]
        public void Test_LatestUpdaterIsNotCompatible_BreakingChanges()
        {
            ToolkitDataUnitOfWork unit = new ToolkitDataUnitOfWork(context: this.Context, versionReader: new AssemblyVersionReader());
            UpdaterController controller = new UpdaterController(work: unit, serializer: this.serializer, fileSaver: new Mock<IFileSaver>().Object, fileRetriever: new Mock<IFileRetriever>().Object);
            var request = new UpdateRequest(programId: 1, programVersion: "0.0", userId: 1, acceptBeta: false, updaterVersion: "1.0", toolkitVersion: "0.2.0");

            UpdateResponse result = controller.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(expected: 0, actual: result.UpdatePackages.Count);
            //Assert.AreEqual("1.1.0", result.UpdatePackagesIncludingBeta.Single().Version);
             request = new UpdateRequest(programId: 1, programVersion: "0.0", userId: 1, acceptBeta: false, updaterVersion: "1.1.0", toolkitVersion: "0.9.0");

            result = controller.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(expected: "1.5.0", actual: result.UpdatePackages.Single().Version);
            //  Assert.AreEqual("1.5.0", result.UpdatePackagesIncludingBeta.Single().Version);
        }

        [Test]
        public void Test_LatestUpdaterIsUsed()
        {
            ToolkitDataUnitOfWork unit = new ToolkitDataUnitOfWork(this.Context, new AssemblyVersionReader());
            UpdaterController controller = new UpdaterController(unit, this.serializer, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            var request = new UpdateRequest(programId: 1, programVersion: "0.0", userId: 1, acceptBeta: false, updaterVersion: "1.1", toolkitVersion: "0.2.0");

            UpdateResponse result = controller.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(0, result.UpdatePackages.Count);
            request = new UpdateRequest(programId: 1, programVersion: "0.0", userId: 1, acceptBeta: false, updaterVersion: "1.5", toolkitVersion: "1.0");

            result = controller.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(0, result.UpdatePackages.Count);

            request = new UpdateRequest(programId: 1, programVersion: "0.0", userId: 1, acceptBeta: false, updaterVersion: "1.7", toolkitVersion: "2.0");

            result = controller.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(0, result.UpdatePackages.Count);
        }
    }
}