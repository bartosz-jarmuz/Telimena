// -----------------------------------------------------------------------
//  <copyright file="StatisticsControllerTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Telimena.Tests
{
    #region Using
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Client;
    using DbIntegrationTestHelpers;
    using NUnit.Framework;
    using WebApi.Controllers;
    using WebApp.Core.Models;
    using WebApp.Infrastructure.Database;
    using WebApp.Infrastructure.UnitOfWork.Implementation;
    #endregion


    [TestFixture]
    public class StatisticsControllerTests : IntegrationTestsBase<TelimenaContext>
    {
        public StatisticsControllerTests()
        {
        }

        protected override Action SeedAction => () => TelimenaDbInitializer.SeedUsers(this.Context);


        [Test]
        public void TestAddRemoveReferencedAssemblies()
        {
            StatisticsUnitOfWork unit = new StatisticsUnitOfWork(this.Context);
            StatisticsController sut = new StatisticsController(unit);
            RegistrationRequest request = new RegistrationRequest()
            {
                ProgramInfo = Helpers.GetProgramInfo("TestProg"),
                TelimenaVersion = "1.0.0.0",
                UserInfo = Helpers.GetUserInfo("NewGuy")
            };
            RegistrationResponse result = sut.RegisterClient(request).GetAwaiter().GetResult();
            Program prg = unit.Programs.SingleOrDefaultAsync(x => x.Name == "TestProg").GetAwaiter().GetResult();

            prg.Assemblies.Add(new ReferencedAssembly()
            {
                Name = "Helper1"
            });
            prg.Assemblies.Add(new ReferencedAssembly()
            {
                Name = "Helper2"
            });
            this.Context.SaveChanges();

            prg = unit.Programs.FirstOrDefaultAsync(x => x.Name == "TestProg").GetAwaiter().GetResult();
            Assert.AreEqual(2, prg.Assemblies.Count);

            this.Context.Programs.Remove(prg);

            this.Context.SaveChanges();

            Assert.AreEqual(0, this.Context.PrimaryAssemblies.Count());
            Assert.AreEqual(0, this.Context.ReferencedAssemblies.Count());
            Assert.AreEqual(0, this.Context.Programs.Count());
        }

        [Test]
        public void TestFunctionUsages()
        {
            StatisticsUnitOfWork unit = new StatisticsUnitOfWork(this.Context);
            StatisticsController sut = new StatisticsController(unit);
            Helpers.SeedInitialPrograms(sut,
                2, Helpers.GetName("TestApp"), Helpers.GetName("Billy Jean"));
            Helpers.SeedInitialPrograms(sut,
                2, Helpers.GetName("TestApp"), Helpers.GetName("Jack Black"));

            Helpers.GetProgramAndUser(this.Context, "TestApp", "Billy Jean",  out Program prg, out ClientAppUser usr);
            StatisticsUpdateRequest request = new StatisticsUpdateRequest
            {
                ProgramId = prg.Id,
                FunctionName = "Func1",
                UserId = usr.Id
            };

            var response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();

            Helpers.AssertUpdateResponse(response, prg, usr, 1, "Func1");

            Function func1 = prg.Functions.Single();

            Assert.AreEqual(1, prg.Functions.Count);
            Assert.AreEqual("Func1", func1.Name);
            Assert.AreEqual(1, func1.Id);
            Assert.AreEqual(1, func1.UsageSummaries.Count);
            Assert.AreEqual(prg.Id, func1.ProgramId);

            var usage = func1.GetFunctionUsageSummary(response.UserId);
            Assert.AreEqual(usr.Id, func1.GetFunctionUsageDetails(response.UserId).Single().UsageSummary.ClientAppUserId);

            var otherUser = Helpers.GetUser(this.Context, "Jack Black");
            request = new StatisticsUpdateRequest
            {
                ProgramId = prg.Id,
                FunctionName = "Func1",
                UserId = otherUser.Id
            };

            //run again with different user
            response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            Assert.AreEqual(1, response.Count);
            prg = unit.Programs.GetById(prg.Id);
            Assert.AreEqual(1, prg.Functions.Count);
            func1 = prg.Functions.Single();
            Assert.AreEqual("Func1", func1.Name);
            Assert.AreEqual(2, func1.UsageSummaries.Count);
            Assert.AreEqual(1, func1.GetFunctionUsageSummary(response.UserId).SummaryCount);
            Assert.AreEqual(1, usage.UsageDetails.Count);

            request = new StatisticsUpdateRequest
            {
                ProgramId = prg.Id,
                FunctionName = "Func1",
                UserId = usr.Id
            };
            //run again with first user
            response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            func1 = prg.Functions.Single();
            Assert.AreEqual(2, func1.UsageSummaries.Count);
            Assert.AreEqual(2, func1.GetFunctionUsageSummary(response.UserId).SummaryCount);
            Assert.AreEqual(2, usage.UsageDetails.Count);

            var details = func1.GetFunctionUsageDetails(response.UserId).OrderBy(x=>x.Id).ToList();
            Assert.AreEqual(2, details.Count);
            Assert.IsTrue(details.All(x=>x.UsageSummary.ClientAppUserId == response.UserId));
            Assert.IsTrue(details.First().DateTime < details.Last().DateTime);

            Assert.AreEqual(3, this.Context.FunctionUsageDetails.ToList().Count);
            Assert.AreEqual(2, this.Context.FunctionUsageDetails.Count(x => x.UsageSummaryId == usage.Id));
        }

       

        [Test]
        public void TestMissingProgram()
        {
            StatisticsUpdateRequest request = new StatisticsUpdateRequest
            {
                ProgramId = 123123,
                UserId = 23
            };

            StatisticsUnitOfWork unit = new StatisticsUnitOfWork(this.Context);

            StatisticsController sut = new StatisticsController(unit);
            StatisticsUpdateResponse response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            Assert.IsTrue(response.Error.Message.Contains($"Program [{request.ProgramId}] is null"));
        }

        [Test]
        public void TestMissingUser()
        {
            StatisticsUnitOfWork unit = new StatisticsUnitOfWork(this.Context);

            unit.Programs.Add(new Program()
            {
                Name = "SomeApp",
                PrimaryAssembly = new PrimaryAssembly()
                {
                    Name = "SomeApp.dll",
                    Company = "SomeCompm"
                }
            });

            unit.CompleteAsync().GetAwaiter().GetResult();
            Program prg = unit.Programs.GetAsync(x => x.Name == "SomeApp").GetAwaiter().GetResult().FirstOrDefault();
            Assert.IsTrue(prg.Id > 0);

            StatisticsUpdateRequest request = new StatisticsUpdateRequest
            {
                ProgramId = prg.Id,
                UserId = 15646
            };

            StatisticsController sut = new StatisticsController(unit);
            StatisticsUpdateResponse response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            Assert.AreEqual($"User [{request.UserId}] is null", response.Error.Message);
        }

        [Test]
        public void TestRegistration_SameAppEachTime()
        {
            StatisticsUnitOfWork unit = new StatisticsUnitOfWork(this.Context);
            StatisticsController sut = new StatisticsController(unit);
            UserInfo userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));

            RegistrationRequest request = new RegistrationRequest()
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")),
                TelimenaVersion = "1.0.0.0",
                UserInfo = userInfo
            };

            RegistrationResponse response = sut.RegisterClient(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program prg, out ClientAppUser usr);
            Helpers.AssertRegistrationResponse(response, prg, 1,usr, 1);
            Assert.AreEqual(1, prg.PrimaryAssembly.Versions.Count);

            //second time
            response = sut.RegisterClient(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out prg, out usr);
            Helpers.AssertRegistrationResponse(response, prg,1, usr, 2);
            Assert.AreEqual(1, prg.PrimaryAssembly.Versions.Count);

            //third time - different version
            request = new RegistrationRequest()
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg"), version: "2.0.0.0"),
                TelimenaVersion = "1.0.0.0",
                UserInfo = userInfo
            };
            response = sut.RegisterClient(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out prg, out usr);
            Helpers.AssertRegistrationResponse(response, prg,2, usr, 3);
            Assert.AreEqual(2, response.VersionId);
            Assert.AreEqual(2, prg.PrimaryAssembly.Versions.Count);

            //fourth time - use first version again
            request = new RegistrationRequest()
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")),
                TelimenaVersion = "1.0.0.0",
                UserInfo = userInfo
            };
            response = sut.RegisterClient(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out prg, out usr);
            Helpers.AssertRegistrationResponse(response, prg,1, usr, 4);
            Assert.AreEqual(1, response.VersionId);
            Assert.AreEqual(2, prg.PrimaryAssembly.Versions.Count);



            Assert.AreEqual(1, prg.PrimaryAssembly.GetLatestVersion().Id);
        }

        [Test]
        public void TestRegistration_SameUserTwoApps_HappyPath()
        {
            StatisticsUnitOfWork unit = new StatisticsUnitOfWork(this.Context);
            StatisticsController sut = new StatisticsController(unit);
            UserInfo userInfo = Helpers.GetUserInfo(Helpers.GetName("NewGuy"));
            
            RegistrationRequest request = new RegistrationRequest()
            {
                ProgramInfo = Helpers.GetProgramInfo(Helpers.GetName("TestProg")),
                TelimenaVersion = "1.0.0.0",
                UserInfo = userInfo
            };
            RegistrationResponse response = sut.RegisterClient(request).GetAwaiter().GetResult();
            Helpers.GetProgramAndUser(this.Context, "TestProg", "NewGuy", out Program prg, out ClientAppUser usr);

            Assert.IsTrue(prg.Id > 0 && usr.Id > 0);
            Helpers.AssertRegistrationResponse(response, prg, 1,usr,1);

            Assert.AreEqual(1, prg.UsageSummaries.Count());
            Assert.AreEqual(request.ProgramInfo.PrimaryAssembly.Name, prg.PrimaryAssembly.Name);
            Assert.AreEqual("xyz", prg.PrimaryAssembly.Company);
            Assert.AreEqual(prg.Id, this.Context.PrimaryAssemblies.FirstOrDefault(x => x.Name == request.ProgramInfo.PrimaryAssembly.Name).Program.Id);

            Assert.AreEqual(userInfo.UserName, usr.UserName);
            Assert.AreEqual(request.UserInfo.MachineName, usr.MachineName);
            Assert.AreEqual(request.UserInfo.UserName, usr.UserName);
            Assert.AreEqual(usr.Id, prg.UsageSummaries.Single().ClientAppUserId);

            var prgName2 = Helpers.GetName("TestProg2");
            //now second app for same user
            request = new RegistrationRequest()
            {
                ProgramInfo = Helpers.GetProgramInfo(prgName2),
                TelimenaVersion = "1.0.0.0",
                UserInfo = userInfo
            };

            response = sut.RegisterClient(request).GetAwaiter().GetResult();
            Program prg2 = unit.Programs.SingleOrDefaultAsync(x => x.Name == prgName2).GetAwaiter().GetResult();

            Helpers.AssertRegistrationResponse(response, prg2,2, usr, 1);

            Assert.IsTrue(prg2.Id > prg.Id);
            Assert.AreEqual(usr.Id, prg2.UsageSummaries.Single().ClientAppUserId);
        }

        [Test]
        public void TestUpdateAction()
        {
            StatisticsUnitOfWork unit = new StatisticsUnitOfWork(this.Context);
            StatisticsController sut = new StatisticsController(unit);
            Helpers.SeedInitialPrograms(sut, 4, Helpers.GetName("TestApp"), Helpers.GetName("Johny Walker"));
            Helpers.SeedInitialPrograms(sut, 4, Helpers.GetName("TestApp"), Helpers.GetName("Jim Beam"));
            Helpers.SeedInitialPrograms(sut, 4, Helpers.GetName("TestApp"), Helpers.GetName("Eric Cartman"));

            Helpers.GetProgramAndUser(this.Context, "TestApp3", "Jim Beam", out Program prg, out ClientAppUser usr);
            StatisticsUpdateRequest request = new StatisticsUpdateRequest
            {
                ProgramId = prg.Id,
                UserId = usr.Id
            };

            StatisticsUpdateResponse response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            Assert.IsTrue(prg.Id > 0 && usr.Id > 0);

            Helpers.AssertUpdateResponse(response, prg, usr, 2, null);

            Assert.AreEqual(3, prg.UsageSummaries.Count);
            Assert.AreEqual(2, prg.UsageSummaries.Single(x => x.ClientAppUser.UserName == "TestUpdateAction_Jim Beam").SummaryCount);

            //run again
            response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();

            Helpers.AssertUpdateResponse(response, prg, usr, 3, null);

            Assert.AreEqual(3, prg.UsageSummaries.Count);
            Assert.AreEqual(3, prg.GetProgramUsageSummary(response.UserId).SummaryCount);
            Assert.AreEqual(3, prg.UsageSummaries.Single(x => x.ClientAppUser.UserName == "TestUpdateAction_Jim Beam").SummaryCount);

            Assert.AreEqual(3, prg.GetProgramUsageDetails(response.UserId).Count);
            Assert.AreEqual(5, this.Context.ProgramUsageDetails.Count(x=>x.UsageSummary.ProgramId == prg.Id));
        }
    }
}