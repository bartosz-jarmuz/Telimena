namespace Telimena.Tests
{
    using System.Linq;
    using System.Runtime.CompilerServices;
    using Client;
    using NUnit.Framework;
    using WebApi.Controllers;
    using WebApp.Core.Models;
    using WebApp.Infrastructure.Database;

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
            Assert.AreEqual(prg.Id, response.ProgramId);
            Assert.AreEqual(usr.Id, response.UserId);
        }

        public static void AssertUpdateResponse(StatisticsUpdateResponse response, Program prg, ClientAppUser usr, int expectedCount, string funcName = null)
        {
            Assert.IsNull(response.Error);
            Assert.AreEqual(expectedCount, response.Count);
            Assert.AreEqual(funcName, response.FunctionName);
            Assert.AreEqual(prg.Id, response.ProgramId);
            Assert.AreEqual(usr.Id, response.UserId);
        }

        public static string GetName(string name, [CallerMemberName] string methodIdentifier = "")
        {
            return $"{methodIdentifier}_{name}";
        }

        public static ProgramInfo GetProgramInfo(string name, string company = "xyz", string copyright = "Reserved")
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

        public static void SeedInitialPrograms(StatisticsController controller, int progCount, string prgName, string userName)
        {
            for (int i = 0; i < progCount; i++)
            {
                var counter = i > 0 ? i.ToString() : "";
                Helpers.SeedProgram(controller, prgName + counter, userName);
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
    }
}