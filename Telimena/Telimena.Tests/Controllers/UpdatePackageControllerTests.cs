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
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.Repository.Implementation;
using Telimena.WebApp.Infrastructure.UnitOfWork;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;

namespace Telimena.Tests
{
    [TestFixture]
    public class UpdatePackageControllerTests :IntegrationTestsContextNotShared<TelimenaContext>
    {
        ITelimenaSerializer serializer = new TelimenaSerializer();
        private IProgramsUnitOfWork GetUnit(List<ProgramUpdatePackageInfo> list)
        {
            var pkgRepo = new UpdatePackageRepository(this.Context, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object);
            var prgRepo = new ProgramRepository(this.Context);
            prgRepo.Add(new Program("prg") { Id = 1 });

            foreach (ProgramUpdatePackageInfo programUpdatePackageInfo in list)
            {
                pkgRepo.Add(programUpdatePackageInfo);
            }

            this.Context.SaveChanges();
            var unit = new ProgramsUnitOfWork(this.Context, new Mock<ITelimenaUserManager>().Object);
            return unit;
        }

        [Test]
        public void TestSimplestScenario()
        {
            var unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222) {IsStandalone = true, Id = 1},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222) {IsStandalone = true, Id = 2},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222) {IsStandalone = true, Id = 3}
            });


            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer);
            var request = new UpdateRequest()
            {
                ProgramId = 1,
                ProgramVersion = "1.2.0.0"
            };
            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackagesIncludingBeta.Single().Id);
            Assert.AreEqual(3, result.UpdatePackages.Single().Id);
        }


        [Test]
        public void TestSimplestScenario_PreviousWasBeta()
        {
            var unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222) {IsStandalone = true, Id = 1},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222) {IsStandalone = true, IsBeta = true, Id = 2},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222) {IsStandalone = true, Id = 3},
                new ProgramUpdatePackageInfo("pkg.zip", 2, "1.2.0.4", 2222) {IsStandalone = true, Id = 4} //different program
            });


            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer);
            var request = new UpdateRequest()
            {
                ProgramId = 1,
                ProgramVersion = "1.2.0.0"
            };
            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackagesIncludingBeta.Single().Id);
            Assert.AreEqual(3, result.UpdatePackages.Single().Id);
        }


        [Test]
        public void Beta_TestSimplestScenario()
        {
            var unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.0", 2222) {IsStandalone = true, Id = 1},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222) {IsStandalone = true, Id = 2},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222) {IsStandalone = true, IsBeta = true, Id = 3},
                new ProgramUpdatePackageInfo("pkg.zip", 2, "1.2.0.4", 2222) {IsStandalone = true, IsBeta = true, Id = 4} //different program
            });


            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer);
            var request = new UpdateRequest()
            {
                ProgramId = 1,
                ProgramVersion = "1.2.0.0"
            };
            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackagesIncludingBeta.Single().Id);
            Assert.AreEqual(2, result.UpdatePackages.Single().Id);
        }


        [Test]
        public void Test_NonStandaloneUpdateAvailable()
        {
            var unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222) {IsStandalone = true, Id = 1},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222) {IsStandalone = true, Id = 2},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222) {IsStandalone = false, Id = 3},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222) {IsStandalone = true, Id = 4},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222) {IsStandalone = false, Id = 5},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222) {IsStandalone = false, Id = 6}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer);
            var request = new UpdateRequest()
            {
                ProgramId = 1,
                ProgramVersion = "1.2.0.0"
            };
            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackagesIncludingBeta.Count());
            Assert.AreEqual(3, result.UpdatePackages.Count());

            Assert.AreEqual(6, result.UpdatePackages.ElementAt(0).Id);
            Assert.AreEqual(5, result.UpdatePackages.ElementAt(1).Id);
            Assert.AreEqual(4, result.UpdatePackages.ElementAt(2).Id);

            Assert.AreEqual(6, result.UpdatePackagesIncludingBeta.ElementAt(0).Id);
            Assert.AreEqual(5, result.UpdatePackagesIncludingBeta.ElementAt(1).Id);
            Assert.AreEqual(4, result.UpdatePackagesIncludingBeta.ElementAt(2).Id);
        }


        [Test]
        public void Beta_NonStandaloneUpdateAvailable()
        {
            var unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222) {IsStandalone = true, Id = 1},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222) {IsStandalone = false, Id = 2},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222) {IsStandalone = false, Id = 3},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222) {IsStandalone = true, IsBeta = true, Id = 4},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222) {IsStandalone = false, Id = 5},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222) {IsStandalone = false, IsBeta = true,  Id = 6}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer);
            var request = new UpdateRequest()
            {
                ProgramId = 1,
                ProgramVersion = "1.2.0.0"
            };
            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackagesIncludingBeta.Count());
            Assert.AreEqual(4, result.UpdatePackages.Count());

            Assert.AreEqual(5, result.UpdatePackages.ElementAt(0).Id);
            Assert.AreEqual(3, result.UpdatePackages.ElementAt(1).Id);
            Assert.AreEqual(2, result.UpdatePackages.ElementAt(2).Id);
            Assert.AreEqual(1, result.UpdatePackages.ElementAt(3).Id);

            Assert.AreEqual(6, result.UpdatePackagesIncludingBeta.ElementAt(0).Id);
            Assert.AreEqual(5, result.UpdatePackagesIncludingBeta.ElementAt(1).Id);
            Assert.AreEqual(4, result.UpdatePackagesIncludingBeta.ElementAt(2).Id);
        }

        [Test]
        public void Beta_OnlyBetaAvailable()
        {
            var unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.1", 2222) {IsStandalone = true, IsBeta = true, Id = 1},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.2", 2222) {IsStandalone = false, IsBeta = true, Id = 2},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222) {IsStandalone = false, IsBeta = true, Id = 3},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222) {IsStandalone = true, IsBeta = true, Id = 4},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222) {IsStandalone = false, IsBeta = true, Id = 5},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222) {IsStandalone = false, IsBeta = true, Id = 6}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer);
            var request = new UpdateRequest()
            {
                ProgramId = 1,
                ProgramVersion = "1.2.0.0"
            };
            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(3, result.UpdatePackagesIncludingBeta.Count());
            Assert.IsNull(result.UpdatePackages);


            Assert.AreEqual(6, result.UpdatePackagesIncludingBeta.ElementAt(0).Id);
            Assert.AreEqual(5, result.UpdatePackagesIncludingBeta.ElementAt(1).Id);
            Assert.AreEqual(4, result.UpdatePackagesIncludingBeta.ElementAt(2).Id);
        }


        [Test]
        public void Test_OnlyNonStandaloneUpdateAvailable()
        {
            var unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222) {IsStandalone = false, Id = 1},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222) {IsStandalone = false, Id = 2},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222) {IsStandalone = false, Id = 3},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222) {IsStandalone = false, Id = 4}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer);
            var request = new UpdateRequest()
            {
                ProgramId = 1,
                ProgramVersion = "1.2.0.0"
            };
            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(4, result.UpdatePackagesIncludingBeta.Count());
            Assert.AreEqual(4, result.UpdatePackages.Count());

            Assert.AreEqual(4, result.UpdatePackages.ElementAt(0).Id);
            Assert.AreEqual(3, result.UpdatePackages.ElementAt(1).Id);
            Assert.AreEqual(2, result.UpdatePackages.ElementAt(2).Id);
            Assert.AreEqual(1, result.UpdatePackages.ElementAt(3).Id);


            Assert.AreEqual(4, result.UpdatePackagesIncludingBeta.ElementAt(0).Id);
            Assert.AreEqual(3, result.UpdatePackagesIncludingBeta.ElementAt(1).Id);
            Assert.AreEqual(2, result.UpdatePackagesIncludingBeta.ElementAt(2).Id);
            Assert.AreEqual(1, result.UpdatePackagesIncludingBeta.ElementAt(3).Id);
        }

        [Test]
        public void BetaTest_OnlyBetaOnlyNonStandaloneUpdateAvailable()
        {
            var unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222) {IsStandalone = false,IsBeta=true, Id = 1},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222) {IsStandalone = false,IsBeta=true, Id = 2},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222) {IsStandalone = false,IsBeta=true, Id = 3},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222) {IsStandalone = false,IsBeta=true, Id = 4}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer);
            var request = new UpdateRequest()
            {
                ProgramId = 1,
                ProgramVersion = "1.2.0.0"
            };
            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(4, result.UpdatePackagesIncludingBeta.Count());

            Assert.IsNull(result.UpdatePackages);

            Assert.AreEqual(4, result.UpdatePackagesIncludingBeta.ElementAt(0).Id);
            Assert.AreEqual(3, result.UpdatePackagesIncludingBeta.ElementAt(1).Id);
            Assert.AreEqual(2, result.UpdatePackagesIncludingBeta.ElementAt(2).Id);
            Assert.AreEqual(1, result.UpdatePackagesIncludingBeta.ElementAt(3).Id);
        }

        [Test]
        public void BetaTest_OnlyNonStandaloneUpdateAvailable()
        {
            var unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.3", 2222) {IsStandalone = false,IsBeta=true, Id = 1},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.4", 2222) {IsStandalone = false,IsBeta=true, Id = 2},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.5", 2222) {IsStandalone = false, Id = 3},
                new ProgramUpdatePackageInfo("pkg.zip", 1, "1.2.0.6", 2222) {IsStandalone = false,IsBeta=true, Id = 4}
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer);
            var request = new UpdateRequest()
            {
                ProgramId = 1,
                ProgramVersion = "1.2.0.0"
            };
            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.AreEqual(4, result.UpdatePackagesIncludingBeta.Count());

            Assert.AreEqual(3, result.UpdatePackages.Single().Id);


            Assert.AreEqual(4, result.UpdatePackagesIncludingBeta.ElementAt(0).Id);
            Assert.AreEqual(3, result.UpdatePackagesIncludingBeta.ElementAt(1).Id);
            Assert.AreEqual(2, result.UpdatePackagesIncludingBeta.ElementAt(2).Id);
            Assert.AreEqual(1, result.UpdatePackagesIncludingBeta.ElementAt(3).Id);
        }

        [Test]
        public void Test_NoUpdatesAvailable()
        {
            var unit = this.GetUnit(new List<ProgramUpdatePackageInfo>
            {
            });

            ProgramUpdatesController sut = new ProgramUpdatesController(unit, this.serializer);
            var request = new UpdateRequest()
            {
                ProgramId = 1,
                ProgramVersion = "1.2.0.0"
            };
            UpdateResponse result = sut.GetUpdateInfo(this.serializer.SerializeAndEncode(request)).GetAwaiter().GetResult();

            Assert.IsEmpty(result.UpdatePackagesIncludingBeta);
            Assert.IsEmpty(result.UpdatePackages);

        }
    }
}