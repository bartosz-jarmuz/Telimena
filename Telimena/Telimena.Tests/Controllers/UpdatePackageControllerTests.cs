using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using DbIntegrationTestHelpers;
using Moq;
using NUnit.Framework;
using Telimena.Client;
using Telimena.WebApp.Controllers.Api;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.UnitOfWork;

namespace Telimena.Tests
{
    [TestFixture]
    public class UpdatePackageControllerTests : IntegrationTestsBase<TelimenaContext>
    {
        private Mock<IProgramsUnitOfWork> GetMockUnit(string prgVersion, List<UpdatePackageInfo> list)
        {
            Mock<IUpdatePackageRepository> pkgRepo = new Mock<IUpdatePackageRepository>();
            Mock<IProgramRepository> prgRepo = new Mock<IProgramRepository>();
            prgRepo.Setup(x => x.FirstOrDefaultAsync(It.IsAny<Expression<Func<Program, bool>>>())).Returns(Task.FromResult(new Program("prg") {Id = 1}));

            pkgRepo.Setup(x => x.GetAllPackagesNewerThan(1, prgVersion)).Returns(Task.FromResult(list));

            Mock<IProgramsUnitOfWork> unit = new Mock<IProgramsUnitOfWork>();
            unit.Setup(x => x.UpdatePackages).Returns(pkgRepo.Object);
            unit.Setup(x => x.Programs).Returns(prgRepo.Object);
            return unit;
        }

        [Test]
        public void TestSimplestScenario()
        {
            Mock<IProgramsUnitOfWork> unit = this.GetMockUnit("1.2.0.0", new List<UpdatePackageInfo>
            {
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222) {IsStandalone = true, Id = 10001},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222) {IsStandalone = true, Id = 10002},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222) {IsStandalone = true, Id = 10003}
            });


            ProgramUpdatesController sut = new ProgramUpdatesController(unit.Object);

            UpdateResponse result = sut.GetUpdateInfo(1, "1.2.0.0").GetAwaiter().GetResult();

            Assert.AreEqual(10003, result.UpdatePackagesIncludingBeta.Single().Id);
            Assert.AreEqual(10003, result.UpdatePackages.Single().Id);
        }


        [Test]
        public void TestSimplestScenario_PreviousWasBeta()
        {
            Mock<IProgramsUnitOfWork> unit = this.GetMockUnit("1.2.0.0", new List<UpdatePackageInfo>
            {
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222) {IsStandalone = true, Id = 10001},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222) {IsStandalone = true, IsBeta = true, Id = 10002},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222) {IsStandalone = true, Id = 10003}
            });


            ProgramUpdatesController sut = new ProgramUpdatesController(unit.Object);

            UpdateResponse result = sut.GetUpdateInfo(1, "1.2.0.0").GetAwaiter().GetResult();

            Assert.AreEqual(10003, result.UpdatePackagesIncludingBeta.Single().Id);
            Assert.AreEqual(10003, result.UpdatePackages.Single().Id);
        }


        [Test]
        public void Beta_TestSimplestScenario()
        {
            Mock<IProgramsUnitOfWork> unit = this.GetMockUnit("1.2.0.0", new List<UpdatePackageInfo>
            {
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222) {IsStandalone = true, Id = 10001},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222) {IsStandalone = true, Id = 10002},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222) {IsStandalone = true, IsBeta = true, Id = 10003}
            });


            ProgramUpdatesController sut = new ProgramUpdatesController(unit.Object);

            UpdateResponse result = sut.GetUpdateInfo(1, "1.2.0.0").GetAwaiter().GetResult();

            Assert.AreEqual(10003, result.UpdatePackagesIncludingBeta.Single().Id);
            Assert.AreEqual(10002, result.UpdatePackages.Single().Id);
        }


        [Test]
        public void Test_NonStandaloneUpdateAvailable()
        {
            Mock<IProgramsUnitOfWork> unit = this.GetMockUnit("1.2.0.0", new List<UpdatePackageInfo>
            {
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222) {IsStandalone = true, Id = 10001},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222) {IsStandalone = true, Id = 10002},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222) {IsStandalone = false, Id = 10003},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222) {IsStandalone = true, Id = 10004},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222) {IsStandalone = false, Id = 10005},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222) {IsStandalone = false, Id = 10006}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit.Object);
            UpdateResponse result = sut.GetUpdateInfo(1, "1.2.0.0").GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackagesIncludingBeta.Count());
            Assert.AreEqual(3, result.UpdatePackages.Count());

            Assert.AreEqual(10006, result.UpdatePackages.ElementAt(0).Id);
            Assert.AreEqual(10005, result.UpdatePackages.ElementAt(1).Id);
            Assert.AreEqual(10004, result.UpdatePackages.ElementAt(2).Id);

            Assert.AreEqual(10006, result.UpdatePackagesIncludingBeta.ElementAt(0).Id);
            Assert.AreEqual(10005, result.UpdatePackagesIncludingBeta.ElementAt(1).Id);
            Assert.AreEqual(10004, result.UpdatePackagesIncludingBeta.ElementAt(2).Id);
        }


        [Test]
        public void Beta_NonStandaloneUpdateAvailable()
        {
            Mock<IProgramsUnitOfWork> unit = this.GetMockUnit("1.2.0.0", new List<UpdatePackageInfo>
            {
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222) {IsStandalone = true, Id = 10001},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222) {IsStandalone = false, Id = 10002},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222) {IsStandalone = false, Id = 10003},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222) {IsStandalone = true, IsBeta = true, Id = 10004},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222) {IsStandalone = false, Id = 10005},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222) {IsStandalone = false, IsBeta = true,  Id = 10006}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit.Object);
            UpdateResponse result = sut.GetUpdateInfo(1, "1.2.0.0").GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackagesIncludingBeta.Count());
            Assert.AreEqual(4, result.UpdatePackages.Count());

            Assert.AreEqual(10005, result.UpdatePackages.ElementAt(0).Id);
            Assert.AreEqual(10003, result.UpdatePackages.ElementAt(1).Id);
            Assert.AreEqual(10002, result.UpdatePackages.ElementAt(2).Id);
            Assert.AreEqual(10001, result.UpdatePackages.ElementAt(3).Id);

            Assert.AreEqual(10006, result.UpdatePackagesIncludingBeta.ElementAt(0).Id);
            Assert.AreEqual(10005, result.UpdatePackagesIncludingBeta.ElementAt(1).Id);
            Assert.AreEqual(10004, result.UpdatePackagesIncludingBeta.ElementAt(2).Id);
        }

        [Test]
        public void Beta_OnlyBetaAvailable()
        {
            Mock<IProgramsUnitOfWork> unit = this.GetMockUnit("1.2.0.0", new List<UpdatePackageInfo>
            {
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222) {IsStandalone = true, IsBeta = true, Id = 10001},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222) {IsStandalone = false, IsBeta = true, Id = 10002},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222) {IsStandalone = false, IsBeta = true, Id = 10003},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222) {IsStandalone = true, IsBeta = true, Id = 10004},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222) {IsStandalone = false, IsBeta = true, Id = 10005},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222) {IsStandalone = false, IsBeta = true, Id = 10006}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit.Object);
            UpdateResponse result = sut.GetUpdateInfo(1, "1.2.0.0").GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackagesIncludingBeta.Count());
            Assert.IsNull(result.UpdatePackages);


            Assert.AreEqual(10006, result.UpdatePackagesIncludingBeta.ElementAt(0).Id);
            Assert.AreEqual(10005, result.UpdatePackagesIncludingBeta.ElementAt(1).Id);
            Assert.AreEqual(10004, result.UpdatePackagesIncludingBeta.ElementAt(2).Id);
        }


        [Test]
        public void Test_OnlyNonStandaloneUpdateAvailable()
        {
            Mock<IProgramsUnitOfWork> unit = this.GetMockUnit("1.2.0.0", new List<UpdatePackageInfo>
            {
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222) {IsStandalone = false, Id = 10003},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222) {IsStandalone = false, Id = 10004},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222) {IsStandalone = false, Id = 10005},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222) {IsStandalone = false, Id = 10006}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit.Object);
            UpdateResponse result = sut.GetUpdateInfo(1, "1.2.0.0").GetAwaiter().GetResult();

            Assert.AreEqual(4, result.UpdatePackagesIncludingBeta.Count());
            Assert.AreEqual(4, result.UpdatePackages.Count());

            Assert.AreEqual(10006, result.UpdatePackages.ElementAt(0).Id);
            Assert.AreEqual(10005, result.UpdatePackages.ElementAt(1).Id);
            Assert.AreEqual(10004, result.UpdatePackages.ElementAt(2).Id);
            Assert.AreEqual(10003, result.UpdatePackages.ElementAt(3).Id);


            Assert.AreEqual(10006, result.UpdatePackagesIncludingBeta.ElementAt(0).Id);
            Assert.AreEqual(10005, result.UpdatePackagesIncludingBeta.ElementAt(1).Id);
            Assert.AreEqual(10004, result.UpdatePackagesIncludingBeta.ElementAt(2).Id);
            Assert.AreEqual(10003, result.UpdatePackagesIncludingBeta.ElementAt(3).Id);
        }

        [Test]
        public void BetaTest_OnlyBetaOnlyNonStandaloneUpdateAvailable()
        {
            Mock<IProgramsUnitOfWork> unit = this.GetMockUnit("1.2.0.0", new List<UpdatePackageInfo>
            {
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222) {IsStandalone = false,IsBeta=true, Id = 10003},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222) {IsStandalone = false,IsBeta=true, Id = 10004},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222) {IsStandalone = false,IsBeta=true, Id = 10005},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222) {IsStandalone = false,IsBeta=true, Id = 10006}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit.Object);
            UpdateResponse result = sut.GetUpdateInfo(1, "1.2.0.0").GetAwaiter().GetResult();

            Assert.AreEqual(4, result.UpdatePackagesIncludingBeta.Count());

            Assert.IsNull(result.UpdatePackages);

            Assert.AreEqual(10006, result.UpdatePackagesIncludingBeta.ElementAt(0).Id);
            Assert.AreEqual(10005, result.UpdatePackagesIncludingBeta.ElementAt(1).Id);
            Assert.AreEqual(10004, result.UpdatePackagesIncludingBeta.ElementAt(2).Id);
            Assert.AreEqual(10003, result.UpdatePackagesIncludingBeta.ElementAt(3).Id);
        }

        [Test]
        public void BetaTest_OnlyNonStandaloneUpdateAvailable()
        {
            Mock<IProgramsUnitOfWork> unit = this.GetMockUnit("1.2.0.0", new List<UpdatePackageInfo>
            {
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222) {IsStandalone = false,IsBeta=true, Id = 10003},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222) {IsStandalone = false,IsBeta=true, Id = 10004},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222) {IsStandalone = false, Id = 10005},
                new UpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222) {IsStandalone = false,IsBeta=true, Id = 10006}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit.Object);
            UpdateResponse result = sut.GetUpdateInfo(1, "1.2.0.0").GetAwaiter().GetResult();

            Assert.AreEqual(4, result.UpdatePackagesIncludingBeta.Count());

            Assert.AreEqual(10005, result.UpdatePackages.Single().Id);


            Assert.AreEqual(10006, result.UpdatePackagesIncludingBeta.ElementAt(0).Id);
            Assert.AreEqual(10005, result.UpdatePackagesIncludingBeta.ElementAt(1).Id);
            Assert.AreEqual(10004, result.UpdatePackagesIncludingBeta.ElementAt(2).Id);
            Assert.AreEqual(10003, result.UpdatePackagesIncludingBeta.ElementAt(3).Id);
        }

        [Test]
        public void Test_NoUpdatesAvailable()
        {
            Mock<IProgramsUnitOfWork> unit = this.GetMockUnit("1.2.0.0", new List<UpdatePackageInfo>
            {
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit.Object);
            UpdateResponse result = sut.GetUpdateInfo(1, "1.2.0.0").GetAwaiter().GetResult();

            Assert.IsNull(result.UpdatePackagesIncludingBeta);
            Assert.IsNull(result.UpdatePackages);

        }
    }
}