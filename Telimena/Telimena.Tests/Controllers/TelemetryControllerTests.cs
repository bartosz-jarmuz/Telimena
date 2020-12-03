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

        
       

    }
}