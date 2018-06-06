// -----------------------------------------------------------------------
//  <copyright file="StatisticsControllerTests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

namespace Telimena.Tests
{
    #region Using
    using System.Data.Common;
    using System.Linq;
    using Client;
    using Effort;
    using NUnit.Framework;
    using WebApi.Controllers;
    using WebApp.Core.Models;
    using WebApp.Infrastructure.Database;
    using WebApp.Infrastructure.UnitOfWork.Implementation;
    #endregion

  

    [TestFixture]
    public class StatisticsControllerTests
    {
        public StatisticsControllerTests()
        {
        }

        private DbConnection connection;
        private TelimenaContext context;

        [SetUp]
        public void Setup()
        {
            this.connection = DbConnectionFactory.CreateTransient();
            this.context = new TelimenaContext(this.connection);
        }

        [Test]
        public void TestAddRemoveReferencedAssemblies()
        {
            var unit = new StatisticsUnitOfWork(this.context);
            var sut = new StatisticsController(unit);
            var request = new RegistrationRequest()
            {
                ProgramInfo = this.GetProgramInfo("TestProg"),
                TelimenaVersion = "1.0.0.0",
                UserInfo = this.GetUserInfo("NewGuy")
        };
            var result = sut.RegisterClient(request).GetAwaiter().GetResult();
            var prg = unit.Programs.SingleOrDefaultAsync(x => x.Name == "TestProg").GetAwaiter().GetResult();

            prg.Assemblies.Add(new ReferencedAssembly()
            {
                Name = "Helper1"
            });
            prg.Assemblies.Add(new ReferencedAssembly()
            {
                Name = "Helper2"
            });
            this.context.SaveChanges();

            prg = unit.Programs.FirstOrDefaultAsync(x => x.Name == "TestProg").GetAwaiter().GetResult();
            Assert.AreEqual(2, prg.Assemblies.Count);

            this.context.Programs.Remove(prg);

            this.context.SaveChangesAsync();

            Assert.AreEqual(0, this.context.PrimaryAssemblies.Count());
            Assert.AreEqual(0, this.context.ReferencedAssemblies.Count());
            Assert.AreEqual(0, this.context.Programs.Count());
        }

        [Test]
        public void TestMissingProgram()
        {
            StatisticsUpdateRequest request = new StatisticsUpdateRequest
            {
                ProgramId = 123123,
                UserId = 23
            };

            var unit = new StatisticsUnitOfWork(this.context);

            var sut = new StatisticsController(unit);
            var response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            Assert.IsTrue(response.Error.Message.Contains($"Program [{request.ProgramId}] is null"));
        }

        [Test]
        public void TestMissingUser()
        {
            var request = new StatisticsUpdateRequest
            {
                ProgramId = 1,
                UserId = 1
            };

            var unit = new StatisticsUnitOfWork(this.context);

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

            var sut = new StatisticsController(unit);
            var response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            Assert.IsTrue(response.Error.Message.Contains($"User [{request.UserId}] is null"));
        }

        [Test]
        public void TestRegistration_SameUserTwoApps_HappyPath()
        {
            var unit = new StatisticsUnitOfWork(this.context);
            var sut = new StatisticsController(unit);
            var userInfo = this.GetUserInfo("NewGuy");
            var request = new RegistrationRequest()
            {
                ProgramInfo = this.GetProgramInfo("TestProg"),
                TelimenaVersion = "1.0.0.0",
                UserInfo = userInfo
            };
            var result = sut.RegisterClient(request).GetAwaiter().GetResult();
            Assert.AreEqual(1, result.ProgramId);
            Assert.AreEqual(1, result.UserId);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(null, result.Error);

            var prg = unit.Programs.SingleOrDefaultAsync(x => x.Name == "TestProg").GetAwaiter().GetResult();
            Assert.AreEqual(1, prg.Id);
            Assert.AreEqual(1, prg.Usages.Single().Count);
            Assert.AreEqual("TestProg.dll", prg.PrimaryAssembly.Name);
            Assert.AreEqual("xyz", prg.PrimaryAssembly.Company);
            Assert.AreEqual(1, this.context.PrimaryAssemblies.FirstOrDefault(x => x.Name == "TestProg.dll").Program.Id);

            var usr = unit.ClientAppUsers.SingleOrDefaultAsync().GetAwaiter().GetResult();
            Assert.AreEqual(1, usr.Id);
            Assert.AreEqual(userInfo.UserName, usr.UserName);
            Assert.AreEqual(request.UserInfo.MachineName, usr.MachineName);
            Assert.AreEqual(request.UserInfo.UserName, usr.UserName);
            Assert.AreEqual(usr.Id, prg.Usages.Single().ClientAppUserId);

            Assert.AreEqual(1, unit.Programs.GetAsync().GetAwaiter().GetResult().Count());

            //now second app for same user
            request = new RegistrationRequest()
            {
                ProgramInfo = this.GetProgramInfo("TestProg2"),
                TelimenaVersion = "1.0.0.0",
                UserInfo = userInfo
            };

            result = sut.RegisterClient(request).GetAwaiter().GetResult();
            Assert.AreEqual(2, result.ProgramId);
            Assert.AreEqual(1, result.UserId);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(null, result.Error);

            Assert.AreEqual(2, unit.Programs.GetAsync().GetAwaiter().GetResult().Count());
            prg = unit.Programs.SingleOrDefaultAsync(x => x.Name == "TestProg2").GetAwaiter().GetResult();
            Assert.AreEqual(2, prg.Id);
            usr = unit.ClientAppUsers.SingleOrDefaultAsync().GetAwaiter().GetResult();
            Assert.AreEqual(1, usr.Id);
            Assert.AreEqual(usr.Id, prg.Usages.Single().ClientAppUserId);
        }

        private void SeedInitialPrograms(StatisticsController controller, int progCount, string userName)
        {
            for (int i = 0; i < progCount; i++)
            {
                this.SeedProgram(controller, "TestProg"+i, userName);
            }
        }

        private void SeedProgram(StatisticsController controller, string programName, string userName)
        {
            var register = new RegistrationRequest()
            {
                ProgramInfo = this.GetProgramInfo(programName),
                TelimenaVersion = "1.0.0.0",
                UserInfo = this.GetUserInfo(userName)
            };
            controller.RegisterClient(register).GetAwaiter().GetResult();
        }

        [Test]
        public void TestUpdateAction()
        {
            var unit = new StatisticsUnitOfWork(this.context);
            var sut = new StatisticsController(unit);
            this.SeedInitialPrograms(sut, 4, "Johny Walker");
            this.SeedInitialPrograms(sut, 4, "Jim Beam");
            var request = new StatisticsUpdateRequest
            {
                ProgramId = 4,
                UserId = 2
            };

            StatisticsUpdateResponse response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            Assert.IsNull(response.Error);
            Assert.AreEqual(2, response.Count);
            Assert.AreEqual(null, response.FunctionName);
            Assert.AreEqual(4, response.ProgramId);
            Assert.AreEqual(2, response.UserId);
            var prg = unit.Programs.GetById(4);
            Assert.AreEqual(2, prg.Usages.Count);
            Assert.AreEqual(2, prg.Usages.Single(x=>x.ClientAppUser.UserName == "Jim Beam").Count);

            //run again
            response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            Assert.IsNull(response.Error);
            Assert.AreEqual(3, response.Count);
            Assert.AreEqual(null, response.FunctionName);
            Assert.AreEqual(4, response.ProgramId);
            Assert.AreEqual(2, response.UserId);
            prg = unit.Programs.GetById(4);
            Assert.AreEqual(2, prg.Usages.Count);
            Assert.AreEqual(3, prg.GetProgramUsage(response.UserId).Count);
            Assert.AreEqual(3, prg.Usages.Single(x => x.ClientAppUser.UserName == "Jim Beam").Count);

            //now do functions
            request = new StatisticsUpdateRequest
            {
                ProgramId = 4,
                FunctionName = "Func1",
                UserId = 2
            };

            response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            Assert.IsNull(response.Error);
            Assert.AreEqual(1, response.Count);
            Assert.AreEqual("Func1", response.FunctionName);
            Assert.AreEqual(4, response.ProgramId);
            Assert.AreEqual(2, response.UserId);
            prg = unit.Programs.GetById(4);
            Assert.AreEqual(1, prg.Functions.Count);
            var func1 = prg.Functions.Single();
            Assert.AreEqual("Func1", func1.Name);
            Assert.AreEqual(1, func1.Id);
            Assert.AreEqual(1, func1.Usages.Count);
            Assert.AreEqual(4, func1.ProgramId);
            Assert.AreEqual(1, func1.GetFunctionUsage(response.UserId).Count);

            //run again
            response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            Assert.AreEqual(2, response.Count);
            prg = unit.Programs.GetById(4);
            Assert.AreEqual(1, prg.Functions.Count);
            func1 = prg.Functions.Single();
            Assert.AreEqual("Func1", func1.Name);
            Assert.AreEqual(1, func1.Usages.Count);
            Assert.AreEqual(2, func1.GetFunctionUsage(response.UserId).Count);
        }

        private ProgramInfo GetProgramInfo(string name, string company = "xyz", string copyright = "Reserved")
        {
            return new ProgramInfo()
            {
                Name = name,
                PrimaryAssembly = new AssemblyInfo()
                {
                    Company = company,
                    Copyright = copyright,
                    Name = name + ".dll"
                }
            };
        }

        private UserInfo GetUserInfo(string name, string machineName = "SomeMachine")
        {
            return new UserInfo()
            {
                UserName = name,
                MachineName = machineName
            };
        }
    }
}