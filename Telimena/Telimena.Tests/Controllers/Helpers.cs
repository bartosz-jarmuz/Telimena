using System.Linq;
using System.Runtime.CompilerServices;
using Microsoft.AspNet.Identity.EntityFramework;
using NUnit.Framework;
using Telimena.Client;
using Telimena.WebApp.Controllers.Api;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;

namespace Telimena.Tests
{
    public static class Helpers
    {
        public static void AddHelperAssemblies(TelimenaContext context, int assCount, string prgName)
        {
            for (int i = 0; i < assCount; i++)
            {
                string assName = $"HelperAss{i}_{prgName}.dll";
                Program prg = context.Programs.First(x => x.Name == prgName);
                ProgramAssembly ass = new ProgramAssembly {Name = assName};
                prg.ProgramAssemblies.Add(ass);

                StatisticsHelperService.EnsureVersionIsRegistered(ass, "0.0.1.0");
            }
        }

        public static void AssertRegistrationResponse(RegistrationResponse response, Program prg, ClientAppUser usr, int expectedCount, string funcName = null)
        {
            Assert.IsNull(response.Exception);
            Assert.AreEqual(expectedCount, response.Count);
            Assert.AreEqual(prg.Id, response.ProgramId);
            Assert.AreEqual(usr.Id, response.UserId);
        }

        public static void AssertUpdateResponse(StatisticsUpdateResponse response, Program prg, ClientAppUser usr, int expectedCount, string funcName = null)
        {
            Assert.IsNull(response.Exception);
            Assert.AreEqual(expectedCount, response.Count);
            Assert.AreEqual(funcName, response.FunctionName);
            Assert.AreEqual(prg.Id, response.ProgramId);
            Assert.AreEqual(usr.Id, response.UserId);
        }

        public static TelimenaUser CreateTelimenaUser(TelimenaContext context, string email, string displayName = null, [CallerMemberName] string caller = "")
        {
            TelimenaUserManager manager = new TelimenaUserManager(new UserStore<TelimenaUser>(context));

            AccountUnitOfWork unit = new AccountUnitOfWork(null, manager, context);


            TelimenaUser user = new TelimenaUser(caller + "_" + email, caller + "_" + displayName ?? email.ToUpper());
            unit.RegisterUserAsync(user, "P@ssword", TelimenaRoles.Developer).GetAwaiter().GetResult();

            unit.Complete();
            return user;
        }

        public static string GetName(string name, [CallerMemberName] string methodIdentifier = "")
        {
            return $"{methodIdentifier}_{name}";
        }

        public static void GetProgramAndUser(TelimenaContext context, string programName, string userName, out Program prg, out ClientAppUser usr
            , [CallerMemberName] string methodIdentifier = "")
        {
            string prgName = GetName(programName, methodIdentifier);
            prg = context.Programs.First(x => x.Name == prgName);
            usr = GetUser(context, userName, methodIdentifier);
        }

        public static ProgramInfo GetProgramInfo(string name, string company = "xyz", string copyright = "Reserved", string version = "1.2.3.4")
        {
            return new ProgramInfo
            {
                Name = name, PrimaryAssembly = new AssemblyInfo {Company = company, Copyright = copyright, Name = name + ".dll", Version = version}
            };
        }

        public static ClientAppUser GetUser(TelimenaContext context, string name, [CallerMemberName] string methodIdentifier = "")
        {
            string usrName = GetName(name, methodIdentifier);
            return context.AppUsers.First(x => x.UserName == usrName);
        }

        public static UserInfo GetUserInfo(string name, string machineName = "SomeMachine")
        {
            return new UserInfo {UserName = name, MachineName = machineName};
        }

        public static void SeedInitialPrograms(TelimenaContext context, int progCount, string getName, string userName, [CallerMemberName] string caller = "")
        {
            StatisticsUnitOfWork unit = new StatisticsUnitOfWork(context, new AssemblyVersionReader());
            StatisticsController controller = new StatisticsController(unit);
            SeedInitialPrograms(controller, progCount, GetName(getName, caller), GetName(userName, caller));
        }

        public static void SeedInitialPrograms(StatisticsController controller, int progCount, string prgName, string userName)
        {
            for (int i = 0; i < progCount; i++)
            {
                string counter = i > 0 ? i.ToString() : "";
                SeedProgram(controller, prgName + counter, userName);
            }
        }

        public static void SeedProgram(StatisticsController controller, string programName, string userName)
        {
            RegistrationRequest register = new RegistrationRequest
            {
                ProgramInfo = GetProgramInfo(programName), TelimenaVersion = "1.0.0.0", UserInfo = GetUserInfo(userName)
            };
            controller.RegisterClient(register).GetAwaiter().GetResult();
        }
    }
}