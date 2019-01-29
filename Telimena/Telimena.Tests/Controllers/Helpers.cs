using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using DotNetLittleHelpers;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.TestFramework;
using Microsoft.AspNet.Identity.EntityFramework;
using Moq;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Api;
using Telimena.WebApp.Controllers.Api.V1;
using Telimena.WebApp.Core;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;
using TelimenaClient;
using TelimenaClient.Telemetry;

namespace Telimena.Tests
{
    public static class Helpers
    {
        public static TelemetryModule GetTelemetryModule(ICollection<ITelemetry> sentTelemetry, Guid telemetryKey)
        {
            TelemetryModule module = new TelemetryModule(new TelimenaProperties(new TelimenaStartupInfo(telemetryKey)));
            StubTelemetryChannel channel = new StubTelemetryChannel { OnSend = t => sentTelemetry.Add(t) };

#pragma warning disable 618
            module.InvokeMethod(nameof(TelemetryModule.InitializeTelemetryClient), channel);
#pragma warning restore 618

            Assert.IsInstanceOf<StubTelemetryChannel>(module.TelemetryClient.GetPropertyValue<TelemetryConfiguration>("TelemetryConfiguration").TelemetryChannel);
            return module;
        }

        public static void AddHelperAssemblies(TelimenaContext context, int assCount, string prgName, [CallerMemberName] string caller = "")
        {
            for (int i = 0; i < assCount; i++)
            {
                string programName = GetName(prgName, caller);
                string assName = $"HelperAss{i}_{programName}";
                Program prg = context.Programs.First(x => x.Name == programName);
                ProgramAssembly ass = new ProgramAssembly {Name = assName, Extension = ".dll"};
                prg.ProgramAssemblies.Add(ass);

                ass.AddVersion(new VersionData("0.0.1.0", "2.0.1.0"));
            }
        }

        public static void SetIp(ApiController controller, string ip)
        {
            Mock<HttpRequestBase> request = new Mock<HttpRequestBase>();
            // Not working - IsAjaxRequest() is static extension method and cannot be mocked
            // request.Setup(x => x.IsAjaxRequest()).Returns(true /* or false */);
            // use this
            request.SetupGet(x => x.UserHostAddress).Returns(ip);

            //var ctx = new Mock<HttpContextWrapper>();
            //ctx.SetupGet(x => x.Request).Returns(request.Object);

            Mock<HttpContextBase> mockContext = new Mock<HttpContextBase>();
            mockContext.SetupAllProperties();
            mockContext.Setup(c => c.Request).Returns(request.Object);

            TestingUtilities.SetReuqest(controller, HttpMethod.Post, new Dictionary<string, object>() { { "MS_HttpContext", mockContext.Object } });

        }
        public static void AssertRegistrationResponse(TelemetryInitializeResponse response, Program prg, ClientAppUser usr, int expectedCount, string funcName = null)
        {
            Assert.IsNull(response.Exception);
            Assert.AreEqual(usr.Guid, response.UserId);
        }

        public static void AssertUpdateResponse(List<TelemetrySummary> response, Program prg, ClientAppUser usr, int expectedSummariesCount, string funcName = null, int funcId = 0)
        {
            Assert.AreEqual(expectedSummariesCount, response.Count);
            Assert.AreEqual(1, response.Count(x => x.GetComponent().Name == funcName));
            if (funcId != 0)
            {
                Assert.AreEqual(1, response.Count(x => x.GetComponent().Id == funcId));
            }

            Assert.IsTrue(response.All(x=>x.GetComponent().Program.TelemetryKey == prg.TelemetryKey));
            Assert.IsTrue(response.All(x=>x.ClientAppUser.Guid == usr.Guid));
        }

        public static async Task<TelimenaUser> CreateTelimenaUser(TelimenaContext context, string email, string displayName = null, [CallerMemberName] string caller = "")
        {
            TelimenaUserManager manager = new TelimenaUserManager(new UserStore<TelimenaUser>(context));

            AccountUnitOfWork unit = new AccountUnitOfWork(null, manager, context);


            TelimenaUser user = new TelimenaUser(caller + "_" + email, caller + "_" + (displayName ?? email.ToUpper()));
           await  unit.RegisterUserAsync(user, "P@ssword", TelimenaRoles.Developer).ConfigureAwait(false);

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

        public static ProgramInfo GetProgramInfo(string name, string company = "xyz", string copyright = "Reserved", VersionData version = null)
        {
            if (version == null)
            {
                version = new VersionData("1.2.3.4", "2.2.3.4");
            }
            return new ProgramInfo
            {
                Name = name, PrimaryAssembly = new AssemblyInfo {Company = company, Copyright = copyright, Name = name, Extension = ".dll", VersionData = version}
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

            Mock<HttpRequestContext> requestContext = await SetupUserIntoRequestContext(context, devName, devEmail).ConfigureAwait(false);


            ProgramsUnitOfWork unit = new ProgramsUnitOfWork(context, new TelimenaUserManager(new UserStore<TelimenaUser>(context)), new AssemblyStreamVersionReader());

            ProgramsController programsController = new ProgramsController(unit, new Mock<IFileSaver>().Object, new Mock<IFileRetriever>().Object) {RequestContext = requestContext.Object};
            TelemetryController telemetryController = new TelemetryController(new TelemetryUnitOfWork(context, new AssemblyStreamVersionReader())) ;


           List<KeyValuePair<string, Guid>> list =  await SeedInitialPrograms(programsController,telemetryController, prgCount, GetName(getName, caller), userNames.Select(x=> GetName(x, caller)).ToList()).ConfigureAwait(false);
            await unit.CompleteAsync().ConfigureAwait(false);
            return list;
        }

        private static async Task<Mock<HttpRequestContext>> SetupUserIntoRequestContext(TelimenaContext context, string devName, string devEmail)
        {
            TelimenaUser teliUsr = context.Users.FirstOrDefault(x => x.Email == devEmail);
            if (teliUsr == null)
            {
                teliUsr = await CreateTelimenaUser(context, devEmail, devName).ConfigureAwait(false);
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
            List<KeyValuePair<string, Guid>> list = new List<KeyValuePair<string, Guid>>();
            IEnumerable<string> enumerable = userNames as string[] ?? userNames.ToArray();
            for (int i = 0; i < prgCount; i++)
            {
                string counter = i > 0 ? i.ToString() : "";
                KeyValuePair<string, Guid> pair = await SeedProgramAsync(programsController, prgName + counter).ConfigureAwait(false);

                foreach (string userName in enumerable)
                {
                    TelemetryInitializeRequest request = new TelemetryInitializeRequest(pair.Value)
                    {
                        UserInfo = GetUserInfo(userName),
                        ProgramInfo = GetProgramInfo(prgName)
                        ,TelimenaVersion = "1.0.0.0"
                    };

                    TelemetryInitializeResponse response = await telemetryController.Initialize(request).ConfigureAwait(false);
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
                Name = programName,
                TelemetryKey = Guid.NewGuid(),
                PrimaryAssemblyFileName = programName + ".dll"
            };
            RegisterProgramResponse response = await controller.Register(register).ConfigureAwait(false);
            if (response.Exception != null)
            {
                throw response.Exception;
            }

            return new KeyValuePair<string, Guid>(programName, response.TelemetryKey);
        }
    }
}