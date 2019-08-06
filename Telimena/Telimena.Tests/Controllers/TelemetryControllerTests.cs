// -----------------------------------------------------------------------
//  <copyright file="TelemetryControllerTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Results;
using DbIntegrationTestHelpers;
using DotNetLittleHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Api;
using Telimena.WebApp.Controllers.Api.V1;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;
using TelimenaClient.Serializer;
using Assert = NUnit.Framework.Assert;

namespace Telimena.Tests
{
    [TestFixture]
    public class TelemetryControllerTests : GlobalContextTestBase
    {

        [Test]
        public async Task TestMissingProgram()
        {
            TelemetryInitializeRequest request = new TelemetryInitializeRequest(Guid.NewGuid()) { };

            TelemetryUnitOfWork unit = new TelemetryUnitOfWork(this.TelemetryContext, this.PortalContext,new AssemblyStreamVersionReader());

            TelemetryController sut = new TelemetryController(unit);
            TelemetryInitializeResponse response = await sut.Initialize(request).ConfigureAwait(false);
            Assert.IsTrue(response.Exception.Message.Contains($"Program [{request.TelemetryKey}] is null"));
        }

        
        [Test]
        public async Task TestIp_Ok()
        {
            //TelemetryUnitOfWork unit;
            //TelemetryController sut;
            //TelemetryInitializeRequest request = null;
            //UserInfo userInfo;
            //List<KeyValuePair<string, Guid>> apps = await Helpers.SeedInitialPrograms(this.Context, 1, "TestProg", new string[0]).ConfigureAwait(false);

            //for (int i = 0; i <= 2; i++)
            //{

            //    unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            //    sut = new TelemetryController(unit);

            //    Helpers.SetIp(sut, "1.2.3.4");
            //    userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));

            //    request = new TelemetryInitializeRequest(apps[0].Value)
            //    {
            //        ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")),
            //        TelimenaVersion = "1.0.0.0",
            //        UserInfo = userInfo
            //    };

            //    TelemetryInitializeResponse response = await sut.Initialize(request).ConfigureAwait(false);
            //    Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out TelemetryMonitoredProgram prg, out ClientAppUser usr);

            //    Assert.AreEqual("1.2.3.4", usr.IpAddresses.Single());

            //}

            //unit = new TelemetryUnitOfWork(this.Context, new AssemblyStreamVersionReader());
            //sut = new TelemetryController(unit);

            //Helpers.SetIp(sut, "2.2.3.4");
            //userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));


            //await sut.Initialize(request).ConfigureAwait(false);
            //Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out TelemetryMonitoredProgram _, out ClientAppUser usr2);

            //Assert.AreEqual("1.2.3.4", usr2.IpAddresses[0]);
            //Assert.AreEqual("2.2.3.4", usr2.IpAddresses[1]);

        }

        [Test]
        public void TestVersions()
        {
            //todo - version recording should be handled in the update package controllers - after all, its there where you create new 'versions'
        }

    }
}