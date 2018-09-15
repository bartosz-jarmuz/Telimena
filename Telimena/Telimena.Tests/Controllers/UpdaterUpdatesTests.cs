﻿using System;
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
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;

namespace Telimena.Tests
{
    [TestFixture]
    public class UpdaterUpdatesTests : IntegrationTestsContextNotShared<TelimenaContext>
    {
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
            var unit = new ToolkitDataUnitOfWork(this.Context);
            unit.UpdaterRepository.Add(new UpdaterPackageInfo("1.0.0", 1000, "0.0.0.0"));
            unit.CompleteAsync().GetAwaiter().GetResult();
            unit.UpdaterRepository.Add(new UpdaterPackageInfo("1.1.0", 1000, "0.0.0.0"));
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
            unit.UpdaterRepository.Add(new UpdaterPackageInfo("1.7.0", 1000, "1.3.0.0"){IsBeta = true});
            unit.CompleteAsync().GetAwaiter().GetResult();
            return unit;
        }


      

        [Test]
        public void Test_LatestUpdaterIsCompatible()
        {
            var unit = this.Prepare();
            var controller = new UpdaterController(unit, null, null);
            var result = controller.CheckForUpdate("1.0", "1.3.0").GetAwaiter().GetResult();
            Assert.AreEqual("1.6.0", result.UpdatePackages.Single().Version);
            Assert.AreEqual("1.7.0", result.UpdatePackagesIncludingBeta.Single().Version);
            //todo - a request for update is made from toolkit v1.0
            //there were several updates of the updater since that time
            //however no breaking changes
            //assert that the latest version of updater is returned
        }

        [Test]
        public void Test_LatestUpdaterIsNotCompatible_BreakingChanges()
        {
            //assert that the latest version of updater is returned
            //todo - assert that 2nd is returned for request from toolkit 0.4 
            //todo - assert that 8th is returned for request from toolkit 2.0
            //todo - assert that 6th is returned for request from toolkit 1.0
        }
    }
}