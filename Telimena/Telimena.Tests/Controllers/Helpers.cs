using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using Microsoft.AspNet.Identity.EntityFramework;
using Moq;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Api;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;
using TelimenaClient;

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

                ass.AddVersion("0.0.1.0", null);
            }
        }

        public static void AssertRegistrationResponse(TelemetryInitializeResponse response, Program prg, ClientAppUser usr, int expectedCount, string funcName = null)
        {
            Assert.IsNull(response.Exception);
            Assert.AreEqual(expectedCount, response.Count);
            Assert.AreEqual(prg.Id, response.ProgramId);
            Assert.AreEqual(usr.Id, response.UserId);
        }

        public static void AssertUpdateResponse(TelemetryUpdateResponse response, Program prg, ClientAppUser usr, int expectedCount, string funcName = null, int funcId = 0)
        {
            Assert.IsNull(response.Exception);
            Assert.AreEqual(expectedCount, response.Count);
            Assert.AreEqual(funcName, response.ComponentName);
            Assert.AreEqual(funcId, response.ComponentId);

            Assert.AreEqual(prg.Id, response.ProgramId);
            Assert.AreEqual(usr.Id, response.UserId);
        }

        public static async Task<TelimenaUser> CreateTelimenaUser(TelimenaContext context, string email, string displayName = null, [CallerMemberName] string caller = "")
        {
            TelimenaUserManager manager = new TelimenaUserManager(new UserStore<TelimenaUser>(context));

            AccountUnitOfWork unit = new AccountUnitOfWork(null, manager, context);


            TelimenaUser user = new TelimenaUser(caller + "_" + email, caller + "_" + (displayName ?? email.ToUpper()));
           await  unit.RegisterUserAsync(user, "P@ssword", TelimenaRoles.Developer);

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
                Name = name, PrimaryAssembly = new AssemblyInfo {Company = company, Copyright = copyright, Name = name + ".dll", AssemblyVersion = version}
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

        public static async Task<List<KeyValuePair<string, Guid>>> SeedInitialPrograms(TelimenaContext context, int prgCount, string getName, string[] userNames, string devName = "Some Developer", string devEmail = "some@dev.dev", [CallerMemberName] string caller = "")
        {

            Mock<HttpRequestContext> requestContext = await SetupUserIntoRequestContext(context, devName, devEmail);


            ProgramsUnitOfWork unit = new ProgramsUnitOfWork(context, new TelimenaUserManager(new UserStore<TelimenaUser>(context)), new AssemblyStreamVersionReader());

            ProgramsController programsController = new ProgramsController(unit) {RequestContext = requestContext.Object};
            TelemetryController telemetryController = new TelemetryController(new TelemetryUnitOfWork(context, new AssemblyStreamVersionReader())) ;


           var list =  await SeedInitialPrograms(programsController,telemetryController, prgCount, GetName(getName, caller), userNames.Select(x=> GetName(x, caller)).ToList());
            await unit.CompleteAsync();
            return list;
        }

        private static async Task<Mock<HttpRequestContext>> SetupUserIntoRequestContext(TelimenaContext context, string devName, string devEmail)
        {
            TelimenaUser teliUsr = context.Users.FirstOrDefault(x => x.Email == devEmail);
            if (teliUsr == null)
            {
                teliUsr = await CreateTelimenaUser(context, devEmail, devName);
            }

            GenericIdentity identity = new GenericIdentity(teliUsr.UserName);
            GenericPrincipal principal = new GenericPrincipal(identity, new[] {TelimenaRoles.Developer});
            ClaimsPrincipal usr = new ClaimsPrincipal(principal);

            Mock<HttpRequestContext> requestContext = new Mock<HttpRequestContext>();
            requestContext.Setup(x => x.Principal).Returns(usr);
            return requestContext;
        }

        private static async Task<List<KeyValuePair<string, Guid>>> SeedInitialPrograms(ProgramsController programsController, TelemetryController telemetryController, int prgCount, string prgName, IEnumerable<string> userNames)
        {
            var list = new List<KeyValuePair<string, Guid>>();
            IEnumerable<string> enumerable = userNames as string[] ?? userNames.ToArray();
            for (int i = 0; i < prgCount; i++)
            {
                string counter = i > 0 ? i.ToString() : "";
                var pair = await SeedProgramAsync(programsController, prgName + counter);

                foreach (string userName in enumerable)
                {
                    TelemetryInitializeRequest request = new TelemetryInitializeRequest(pair.Value)
                    {
                        UserInfo = GetUserInfo(userName),
                        ProgramInfo = GetProgramInfo(prgName)
                        ,TelimenaVersion = "1.0.0.0"
                    };

                    TelemetryInitializeResponse response = await telemetryController.Initialize(request);
                    if (response.Exception != null)
                    {
                        throw response.Exception;
                    }
                }
                list.Add(pair);
            }

            return list;
        }

        public static async Task<KeyValuePair<string, Guid>> SeedProgramAsync(ProgramsController controller, string programName)
        {
           

            RegisterProgramRequest register = new RegisterProgramRequest
            {
                Name = programName
            };
        RegisterProgramResponse response =   await  controller.Register(register);
            if (response.Exception != null)
            {
                throw response.Exception;
            }

            return new KeyValuePair<string, Guid>(programName, response.TelemetryKey);
        }
    }
}