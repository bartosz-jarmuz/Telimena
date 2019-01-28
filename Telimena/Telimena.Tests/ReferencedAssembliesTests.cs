using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DbIntegrationTestHelpers;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Api.V1;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository.FileStorage;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;
using TelimenaClient.Serializer;

namespace Telimena.Tests
{
    [TestFixture]
    public class ReferencedAssembliesTests : IntegrationTestsContextSharedGlobally<TelimenaContext>
    {
        protected override Action SeedAction => () => TelimenaDbInitializer.SeedUsers(this.Context);

        private TelimenaSerializer serializer = new TelimenaSerializer();

        [Test]
        public async Task TestReferencedAssemblies_Add()
        {
            await Helpers.SeedInitialPrograms(this.Context, 4, "TestApp", new[] { "Johnny Walker" }).ConfigureAwait(false);

            Helpers.AddHelperAssemblies(this.Context, 2, "TestApp");
            Helpers.GetProgramAndUser(this.Context, "TestApp", "Johnny Walker", out Program prg, out ClientAppUser usr);

            Assert.AreEqual(3, prg.ProgramAssemblies.Count);
            Assert.AreEqual(Helpers.GetName("TestApp") + ".dll", prg.PrimaryAssembly.Name + prg.PrimaryAssembly.Extension);
            Assert.IsNotNull(prg.ProgramAssemblies.Single(x => x.Name == Helpers.GetName("TestApp")));
            Assert.IsNotNull(prg.ProgramAssemblies.Single(x => x.Name == "HelperAss0_" + Helpers.GetName("TestApp")));
            Assert.IsNotNull(prg.ProgramAssemblies.Single(x => x.Name == "HelperAss1_" + Helpers.GetName("TestApp")));
        }

        [Test]
        public async Task TestReferencedAssembliesAddRemove()
        {
            await Helpers.SeedInitialPrograms(this.Context, 4, "TestApp", new[] { "NewGuy" }).ConfigureAwait(false);
            Helpers.GetProgramAndUser(this.Context, "TestApp", "NewGuy", out Program prg, out ClientAppUser usr);

            prg.ProgramAssemblies.Add(new ProgramAssembly { Name = "Helper1" });
            prg.ProgramAssemblies.Add(new ProgramAssembly { Name = "Helper2" });
            this.Context.SaveChanges();

            Helpers.GetProgramAndUser(this.Context, "TestApp", "NewGuy", out prg, out usr);

            Assert.AreEqual(3, prg.ProgramAssemblies.Count);

            this.Context.Programs.Remove(prg);

            this.Context.SaveChanges();

            Assert.AreEqual(0, this.Context.ProgramAssemblies.Count(x => x.Program.Name == prg.Name));
            Assert.AreEqual(0, this.Context.Programs.Count(x => x.Name == prg.Name));
        }




    }
}