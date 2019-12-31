using System;
using Telimena.WebApp.Core.DTO.MappableToClient;
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
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Interfaces;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Portal;
using Telimena.WebApp.Core.Models.Telemetry;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;
using TelimenaClient;
using ProgramInfo = Telimena.WebApp.Core.DTO.MappableToClient.ProgramInfo;
using TelemetryInitializeRequest = Telimena.WebApp.Core.DTO.MappableToClient.TelemetryInitializeRequest;
using TelemetryInitializeResponse = Telimena.WebApp.Core.DTO.MappableToClient.TelemetryInitializeResponse;
using UserInfo = Telimena.WebApp.Core.DTO.MappableToClient.UserInfo;
using VersionData = Telimena.WebApp.Core.DTO.MappableToClient.VersionData;

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
       
        public static void AssertUpdateResponse(List<TelemetrySummary> response, TelemetryRootObject prg, ClientAppUser usr,
            int expectedSummariesCount, string funcName = null, Guid funcId = default(Guid))
        {
            Assert.AreEqual(expectedSummariesCount, response.Count);
            Assert.AreEqual(1, response.Count(x => x.GetComponent().Name == funcName));
            if (funcId != Guid.Empty)
            {
                Assert.AreEqual(1, response.Count(x => x.GetComponent().Id == funcId));
            }

            Assert.IsTrue(response.All(x=>x.GetComponent().Program.TelemetryKey == prg.TelemetryKey));
            Assert.IsTrue(response.All(x=>x.ClientAppUser.Id == usr.Id));
        }

        public static async Task<TelimenaUser> CreateTelimenaUser(TelimenaPortalContext context, string email, string displayName = null, [CallerMemberName] string caller = "")
        {
            TelimenaUserManager manager = new TelimenaUserManager(new UserStore<TelimenaUser>(context));

            AccountUnitOfWork unit = new AccountUnitOfWork(null, manager, context);


            TelimenaUser user = new TelimenaUser(caller + "" + email, caller + "" + (displayName ?? email.ToUpper()));
      var result =     await  unit.RegisterUserAsync(user, "P@ssword", TelimenaRoles.Developer).ConfigureAwait(false);
      if (!result.Item1.Succeeded)
      {
          var msg = result.Item1.Errors?.FirstOrDefault();
          if (msg != null && !msg.Contains("is already taken."))
          {
              Assert.Fail($"Failed to register user {user.UserName}. Error: {result.Item1.Errors?.FirstOrDefault()}");
            }
        }
            unit.Complete();
            return user;
        }

        public static string GetName(string name, [CallerMemberName] string methodIdentifier = "")
        {
            return $"{methodIdentifier}{name}";
        }

        public static void GetProgramAndUser(TelemetryUnitOfWork unit, string programName, string userName, out TelemetryRootObject prg, out ClientAppUser usr
            , [CallerMemberName] string methodIdentifier = "")
        {
            string prgName = GetName(programName, methodIdentifier);
            var p = unit.Programs.GetByNames("SeedInitialProgramsSomeDeveloper", prgName).GetAwaiter().GetResult();
            prg = unit.GetMonitoredProgram(p.TelemetryKey).Result;
            usr = GetUser(unit, userName, methodIdentifier);
        }

        

        public static ClientAppUser GetUser(TelemetryUnitOfWork context, string name, [CallerMemberName] string methodIdentifier = "")
        {
            string usrName = GetName(name, methodIdentifier);
            return context.ClientAppUsers.FirstOrDefault(x => x.UserIdentifier == usrName);
        }

        public static async Task<List<KeyValuePair<string, Guid>>> SeedInitialPrograms(TelimenaPortalContext portalContext, TelimenaTelemetryContext telemetryContext, 
            int prgCount, string getName, string[] userNames, string devName = "SomeDeveloper", string devEmail = "some@dev.dev", [CallerMemberName] string caller = "")
        {
            Mock<HttpRequestContext> requestContext =
                await SetupUserIntoRequestContext(portalContext, devName, devEmail).ConfigureAwait(false);

            RegisterProgramUnitOfWork unit = new RegisterProgramUnitOfWork(telemetryContext, portalContext);

            RegisterProgramController programsController =
                new RegisterProgramController(unit) { RequestContext = requestContext.Object };


            List<KeyValuePair<string, Guid>> list = await SeedInitialPrograms(programsController, prgCount
                , GetName(getName, caller), userNames.Select(x => GetName(x, caller)).ToList()).ConfigureAwait(false);

            foreach (string userName in userNames)
           {
               telemetryContext.AppUsers.Add(new ClientAppUser() {UserIdentifier = GetName(userName, caller)});
           }

           telemetryContext.SaveChanges();
            return list;
        }


        public static async Task<Mock<HttpRequestContext>> SetupUserIntoRequestContext(TelimenaPortalContext context, string devName, string devEmail,
            [CallerMemberName] string caller = "")
        {
            TelimenaUser teliUsr = context.Users.FirstOrDefault(x => x.Email == devEmail);
            if (teliUsr == null)
            {
                teliUsr = await CreateTelimenaUser(context, devEmail, devName, caller).ConfigureAwait(false);
            }

            GenericIdentity identity = new GenericIdentity(teliUsr.UserName);
            GenericPrincipal principal = new GenericPrincipal(identity, new[] {TelimenaRoles.Developer});
            ClaimsPrincipal usr = new ClaimsPrincipal(principal);

            Mock<HttpRequestContext> requestContext = new Mock<HttpRequestContext>();
            requestContext.Setup(x => x.Principal).Returns(usr);
            return requestContext;
        }

        private static async Task<List<KeyValuePair<string, Guid>>> SeedInitialPrograms(RegisterProgramController programsController, int prgCount, 
            string prgName, IEnumerable<string> userNames)
        {
            List<KeyValuePair<string, Guid>> list = new List<KeyValuePair<string, Guid>>();
            IEnumerable<string> enumerable = userNames as string[] ?? userNames.ToArray();
            for (int i = 0; i < prgCount; i++)
            {
                string counter = i > 0 ? i.ToString() : "";
                KeyValuePair<string, Guid> pair = await SeedProgramAsync(programsController, prgName + counter).ConfigureAwait(false);
                
                list.Add(pair);
            }

            return list;
        }


        public static async Task<KeyValuePair<string, Guid>> SeedProgramAsync(RegisterProgramController controller, string programName)
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