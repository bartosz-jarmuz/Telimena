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
    using System.Reflection;
    using Client;
    using Effort;
    using Effort.DataLoaders;
    using NUnit.Framework;
    using WebApi.Controllers;
    using WebApp.Core.Models;
    using WebApp.Infrastructure.Database;
    using WebApp.Infrastructure.UnitOfWork.Implementation;
    #endregion

    [TestFixture]
    public class StatisticsControllerTests : IntegrationsTestsBase
    {
        public StatisticsControllerTests()
        {
        }

        [Test]
        public void TestAddRemoveReferencedAssemblies()
        {
            StatisticsUnitOfWork unit = new StatisticsUnitOfWork(this.Context);
            StatisticsController sut = new StatisticsController(unit);
            RegistrationRequest request = new RegistrationRequest()
            {
                ProgramInfo = this.GetProgramInfo("TestProg"),
                TelimenaVersion = "1.0.0.0",
                UserInfo = this.GetUserInfo("NewGuy")
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
            Assert.IsTrue( prg.Id > 0);

         

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
        public void TestRegistration_SameUserTwoApps_HappyPath()
        {
            StatisticsUnitOfWork unit = new StatisticsUnitOfWork(this.Context);
            StatisticsController sut = new StatisticsController(unit);
            UserInfo userInfo = this.GetUserInfo("NewGuy");
            var prgName = "TestProg" + MethodBase.GetCurrentMethod().Name;
            RegistrationRequest request = new RegistrationRequest()
            {
                ProgramInfo = this.GetProgramInfo(prgName),
                TelimenaVersion = "1.0.0.0",
                UserInfo = userInfo
            };
            RegistrationResponse result = sut.RegisterClient(request).GetAwaiter().GetResult();
            Program prg = unit.Programs.SingleOrDefaultAsync(x => x.Name == prgName).GetAwaiter().GetResult();
            ClientAppUser usr = unit.ClientAppUsers.SingleOrDefaultAsync().GetAwaiter().GetResult();

            Assert.IsTrue(prg.Id > 0 && usr.Id > 0);
            Assert.AreEqual(prg.Id, result.ProgramId);
            Assert.AreEqual(usr.Id, result.UserId);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(null, result.Error);

            Assert.AreEqual(1, prg.Usages.Count());
            Assert.AreEqual(prgName + ".dll", prg.PrimaryAssembly.Name);
            Assert.AreEqual("xyz", prg.PrimaryAssembly.Company);
            Assert.AreEqual(prg.Id, this.Context.PrimaryAssemblies.FirstOrDefault(x => x.Name == prgName+".dll").Program.Id);

            Assert.AreEqual(userInfo.UserName, usr.UserName);
            Assert.AreEqual(request.UserInfo.MachineName, usr.MachineName);
            Assert.AreEqual(request.UserInfo.UserName, usr.UserName);
            Assert.AreEqual(usr.Id, prg.Usages.Single().ClientAppUserId);

            var prgName2 = "TestProg2" + MethodBase.GetCurrentMethod().Name;
            //now second app for same user
            request = new RegistrationRequest()
            {
                ProgramInfo = this.GetProgramInfo(prgName2),
                TelimenaVersion = "1.0.0.0",
                UserInfo = userInfo
            };

            result = sut.RegisterClient(request).GetAwaiter().GetResult();
            Program prg2 = unit.Programs.SingleOrDefaultAsync(x => x.Name == prgName2).GetAwaiter().GetResult();

            Assert.AreNotEqual(prg.Id, prg2.Id);
            Assert.AreEqual(prg2.Id, result.ProgramId);
            Assert.AreEqual(usr.Id, result.UserId);
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(null, result.Error);

            Assert.IsTrue(prg2.Id > prg.Id);
            Assert.AreEqual(usr.Id, prg2.Usages.Single().ClientAppUserId);
        }

        private void SeedInitialPrograms(StatisticsController controller, int progCount, string prgName, string userName)
        {
            for (int i = 0; i < progCount; i++)
            {
                this.SeedProgram(controller, prgName+i, userName);
            }
        }

        private void SeedProgram(StatisticsController controller, string programName, string userName)
        {
            RegistrationRequest register = new RegistrationRequest()
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
            StatisticsUnitOfWork unit = new StatisticsUnitOfWork(this.Context);
            StatisticsController sut = new StatisticsController(unit);
            this.SeedInitialPrograms(sut, 4, "PrgTestUpdateAction", "Johny Walker");
            this.SeedInitialPrograms(sut, 4, "PrgTestUpdateAction", "Jim Beam");
            var prg = this.Context.Programs.First(x => x.Name == "PrgTestUpdateAction3");
            var usr = this.Context.AppUsers.First(x => x.UserName == "Jim Beam");
            StatisticsUpdateRequest request = new StatisticsUpdateRequest
            {
                ProgramId = prg.Id,
                UserId = usr.Id
            };

            StatisticsUpdateResponse response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            Assert.IsTrue(prg.Id > 0 && usr.Id > 0);
            Assert.IsNull(response.Error);
            Assert.AreEqual(2, response.Count);
            Assert.AreEqual(null, response.FunctionName);
            Assert.AreEqual(prg.Id, response.ProgramId);
            Assert.AreEqual(usr.Id, response.UserId);
            Assert.AreEqual(2, prg.Usages.Count);
            Assert.AreEqual(2, prg.Usages.Single(x=>x.ClientAppUser.UserName == "Jim Beam").Count);

            //run again
            response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            Assert.IsNull(response.Error);
            Assert.AreEqual(3, response.Count);
            Assert.AreEqual(null, response.FunctionName);
            Assert.AreEqual(prg.Id, response.ProgramId);
            Assert.AreEqual(usr.Id, response.UserId);
            Assert.AreEqual(2, prg.Usages.Count);
            Assert.AreEqual(3, prg.GetProgramUsage(response.UserId).Count);
            Assert.AreEqual(3, prg.Usages.Single(x => x.ClientAppUser.UserName == "Jim Beam").Count);

            //now do functions
            request = new StatisticsUpdateRequest
            {
                ProgramId = prg.Id,
                FunctionName = "Func1",
                UserId = usr.Id
            };

            response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            Assert.IsNull(response.Error);
            Assert.AreEqual(1, response.Count);
            Assert.AreEqual("Func1", response.FunctionName);
            Assert.AreEqual(prg.Id, response.ProgramId);
            Assert.AreEqual(usr.Id, response.UserId);

            Assert.AreEqual(1, prg.Functions.Count);
            Function func1 = prg.Functions.Single();
            Assert.AreEqual("Func1", func1.Name);
            Assert.AreEqual(1, func1.Id);
            Assert.AreEqual(1, func1.Usages.Count);
            Assert.AreEqual(prg.Id, func1.ProgramId);
            Assert.AreEqual(1, func1.GetFunctionUsage(response.UserId).Count);

            //run again
            response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            Assert.AreEqual(2, response.Count);
            prg = unit.Programs.GetById(prg.Id);
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