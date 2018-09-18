using System.Collections.Generic;
using System.Linq;
using DbIntegrationTestHelpers;
using Moq;
using NUnit.Framework;
using Telimena.Client;
using Telimena.WebApp.Controllers.Api;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.Repository.Implementation;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;

namespace Telimena.Tests
{
    [TestFixture]
    public class UpdatePackageControllerTests : IntegrationTestsContextNotShared<TelimenaContext>
    {
        private readonly ITelimenaSerializer serializer = new TelimenaSerializer();

        private IProgramsUnitOfWork GetUnit(List<ProgramUpdatePackageInfo> list)
        {
            UpdatePackageRepository pkgRepo = new UpdatePackageRepository(this.Context, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            ProgramRepository prgRepo = new ProgramRepository(this.Context);
            ProgramPackageRepository prgPkgRepo = new ProgramPackageRepository(this.Context, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            prgPkgRepo.Add(new ProgramPackageInfo("Prg.zip", 1, 2222, "1.0.0.0"));
            prgRepo.Add(new Program("prg") {Id = 1});

            foreach (ProgramUpdatePackageInfo programUpdatePackageInfo in list)
            {
                pkgRepo.Add(programUpdatePackageInfo);
            }

            this.Context.SaveChanges();
            ProgramsUnitOfWork unit = new ProgramsUnitOfWork(this.Context, new Mock<ITelimenaUserManager>().Object);
            return unit;
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

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer);
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(4, result.UpdatePackages.Count());

            Assert.AreEqual(5, result.UpdatePackages.ElementAt(0).Id);
            Assert.AreEqual(3, result.UpdatePackages.ElementAt(1).Id);
            Assert.AreEqual(2, result.UpdatePackages.ElementAt(2).Id);
            Assert.AreEqual(1, result.UpdatePackages.ElementAt(3).Id);


            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(6, result.UpdatePackages.ElementAt(0).Id);
            Assert.AreEqual(5, result.UpdatePackages.ElementAt(1).Id);
            Assert.AreEqual(4, result.UpdatePackages.ElementAt(2).Id);
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

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer);
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(0, result.UpdatePackages.Count());
            Assert.IsNull(result.Exception);

            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackages.Count());

            Assert.AreEqual(6, result.UpdatePackages.ElementAt(0).Id);
            Assert.AreEqual(5, result.UpdatePackages.ElementAt(1).Id);
            Assert.AreEqual(4, result.UpdatePackages.ElementAt(2).Id);
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


            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer);
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(2, result.UpdatePackages.Single().Id);

            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(3, result.UpdatePackages.Single().Id);
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

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer);
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "1.0.0.0");
            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(0, result.UpdatePackages.Count());
            Assert.IsNull(result.Exception);

            request.AcceptBeta = true;

            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(4, result.UpdatePackages.Count());

            Assert.AreEqual(4, result.UpdatePackages.ElementAt(0).Id);
            Assert.AreEqual(3, result.UpdatePackages.ElementAt(1).Id);
            Assert.AreEqual(2, result.UpdatePackages.ElementAt(2).Id);
            Assert.AreEqual(1, result.UpdatePackages.ElementAt(3).Id);
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

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer);
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "1.0.0.0");
            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackages.Single().Id);


            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();


            Assert.AreEqual(4, result.UpdatePackages.Count);

            Assert.AreEqual(4, result.UpdatePackages.ElementAt(0).Id);
            Assert.AreEqual(3, result.UpdatePackages.ElementAt(1).Id);
            Assert.AreEqual(2, result.UpdatePackages.ElementAt(2).Id);
            Assert.AreEqual(1, result.UpdatePackages.ElementAt(3).Id);
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

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer);
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackages.Count());

            Assert.AreEqual(6, result.UpdatePackages.ElementAt(0).Id);
            Assert.AreEqual(5, result.UpdatePackages.ElementAt(1).Id);
            Assert.AreEqual(4, result.UpdatePackages.ElementAt(2).Id);

            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackages.Count());

            Assert.AreEqual(6, result.UpdatePackages.ElementAt(0).Id);
            Assert.AreEqual(5, result.UpdatePackages.ElementAt(1).Id);
            Assert.AreEqual(4, result.UpdatePackages.ElementAt(2).Id);
        }

        [Test]
        public void Test_NoUpdatesAvailable()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>());

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer);
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.IsEmpty(result.UpdatePackages);
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
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222, "1.0.0.0") {IsStandalone = false, Id = 4}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer);
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(4, result.UpdatePackages.Count());

            Assert.AreEqual(4, result.UpdatePackages.ElementAt(0).Id);
            Assert.AreEqual(3, result.UpdatePackages.ElementAt(1).Id);
            Assert.AreEqual(2, result.UpdatePackages.ElementAt(2).Id);
            Assert.AreEqual(1, result.UpdatePackages.ElementAt(3).Id);

            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(4, result.UpdatePackages.Count());

            Assert.AreEqual(4, result.UpdatePackages.ElementAt(0).Id);
            Assert.AreEqual(3, result.UpdatePackages.ElementAt(1).Id);
            Assert.AreEqual(2, result.UpdatePackages.ElementAt(2).Id);
            Assert.AreEqual(1, result.UpdatePackages.ElementAt(3).Id);
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


            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer);
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackages.Single().Id);
            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(3, result.UpdatePackages.Single().Id);
        }

        [Test]
        public void TestSimplestScenario_PreviousWasBeta()
        {
            IProgramsUnitOfWork unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222, "1.0.0.0") {IsStandalone = true, Id = 1}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222, "1.0.0.0") {IsStandalone = true, IsBeta = true, Id = 2}
                , new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222, "1.0.0.0") {IsStandalone = true, Id = 3}
                , new ProgramUpdatePackageInfo("pkg.zip", 2, "1.2.0.4", 2222, "1.0.0.0") {IsStandalone = true, Id = 4} //different program
            });


            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer);
            UpdateRequest request = new UpdateRequest(1, "1.2.0.0", 666, false, "1.0.0.0");

            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackages.Single().Id);

            request.AcceptBeta = true;
            result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();
            Assert.AreEqual(3, result.UpdatePackages.Single().Id);
        }
    }
}