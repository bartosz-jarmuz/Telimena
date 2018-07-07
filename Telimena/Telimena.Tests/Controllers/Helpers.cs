namespace Telimena.Tests
{
    using System;
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Client;
    using Microsoft.AspNet.Identity.EntityFramework;
    using NUnit.Framework;
    using WebApi.Controllers;
    using WebApp.Core.Interfaces;
    using WebApp.Core.Models;
    using WebApp.Infrastructure.Database;
    using WebApp.Infrastructure.Identity;
    using WebApp.Infrastructure.UnitOfWork;
    using WebApp.Infrastructure.UnitOfWork.Implementation;

    public static class Helpers
    {
        public static ClientAppUser GetUser(TelimenaContext context, string name, [CallerMemberName] string methodIdentifier = "")
        {
            string usrName = Helpers.GetName(name, methodIdentifier);
            return context.AppUsers.First(x => x.UserName == usrName);

        }

        public static void GetProgramAndUser(TelimenaContext context, string programName, string userName, out Program prg, out ClientAppUser usr, [CallerMemberName] string methodIdentifier = "")
        {
            string prgName = Helpers.GetName(programName, methodIdentifier);
            prg = context.Programs.First(x => x.Name == prgName);
            usr = Helpers.GetUser(context, userName, methodIdentifier);

        }

        public static void AssertRegistrationResponse(RegistrationResponse response, Program prg, ClientAppUser usr, int expectedCount, string funcName = null)
        {
            Assert.IsNull(response.Error);
            Assert.AreEqual(expectedCount, response.Count);
            Assert.AreEqual(prg.ProgramId, response.ProgramId);
            Assert.AreEqual(usr.Id, response.UserId);
        }

        public static void AssertUpdateResponse(StatisticsUpdateResponse response, Program prg, ClientAppUser usr, int expectedCount, string funcName = null)
        {
            Assert.IsNull(response.Error);
            Assert.AreEqual(expectedCount, response.Count);
            Assert.AreEqual(funcName, response.FunctionName);
            Assert.AreEqual(prg.ProgramId, response.ProgramId);
            Assert.AreEqual(usr.Id, response.UserId);
        }

        public static string GetName(string name, [CallerMemberName] string methodIdentifier = "")
        {
            return $"{methodIdentifier}_{name}";
        }

        public static ProgramInfo GetProgramInfo(string name, string company = "xyz", string copyright = "Reserved", string version = "1.2.3.4")
        {
            return new ProgramInfo()
            {
                Name = name,
                PrimaryAssembly = new AssemblyInfo()
                {
                    Company = company,
                    Copyright = copyright,
                    Name = name + ".dll",
                    Version = version
                }
            };
        }

        public static void SeedInitialPrograms(TelimenaContext context, int progCount, string getName, string userName, [CallerMemberName] string caller = "")
        {
            StatisticsUnitOfWork unit = new StatisticsUnitOfWork(context);
            StatisticsController controller = new StatisticsController(unit);
            SeedInitialPrograms(controller, progCount, GetName(getName, caller), Helpers.GetName(userName, caller));
        }

        public static void SeedInitialPrograms(StatisticsController controller, int progCount, string prgName, string userName)
        {
            for (int i = 0; i < progCount; i++)
            {
                var counter = i > 0 ? i.ToString() : "";
                Helpers.SeedProgram(controller, prgName + counter, userName);
            }
        }
        public static void AddHelperAssemblies(TelimenaContext context, int assCount, string prgName)
        {
            for (int i = 0; i < assCount; i++)
            {
                var assName = $"HelperAss{i}_{prgName}.dll";
                var prg = context.Programs.First(x => x.Name == prgName);
                var ass = new ProgramAssembly()
                {
                    Name = assName
                };
                prg.ProgramAssemblies.Add(ass);

                StatisticsHelperService.EnsureVersionIsRegistered(ass, "0.0.1.0");
            }
        }


        public static UserInfo GetUserInfo(string name, string machineName = "SomeMachine")
        {
            return new UserInfo()
            {
                UserName = name,
                MachineName = machineName
            };
        }

        public static void SeedProgram(StatisticsController controller, string programName, string userName)
        {
            RegistrationRequest register = new RegistrationRequest()
            {
                ProgramInfo = Helpers.GetProgramInfo(programName),
                TelimenaVersion = "1.0.0.0",
                UserInfo = Helpers.GetUserInfo(userName)
            };
            controller.RegisterClient(register).GetAwaiter().GetResult();
        }

        public static TelimenaUser CreateTelimenaUser(TelimenaContext context, string email, string displayName = null, [CallerMemberName] string caller = "")
        {
            var manager = new TelimenaUserManager(
                new UserStore<TelimenaUser>(context));

            var unit = new AccountUnitOfWork(null, manager, context);


            var user = new TelimenaUser(caller +"_"+ email, caller + "_" + displayName ??email.ToUpper());
            unit.RegisterUserAsync(user, "P@ssword", TelimenaRoles.Developer).GetAwaiter().GetResult();

            unit.Complete();
            return user;
        }

    }
}