using System.Text;

namespace Telimena.Tests
{
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    using Client;
    using Effort;
    using Microsoft.VisualStudio.TestTools.UnitTesting;
    using Moq;
    using NUnit.Framework;
    using WebApi.Controllers;
    using WebApp.Core.Models;
    using WebApp.Infrastructure.Database;
    using WebApp.Infrastructure.Repository;
    using WebApp.Infrastructure.UnitOfWork;
    using WebApp.Infrastructure.UnitOfWork.Implementation;
    using Assert = NUnit.Framework.Assert;

    public class FakeStatsUnitOfWork : IStatisticsUnitOfWork
    {
        public FakeStatsUnitOfWork()
        {
            this.Programs = new FakeProgramRepo();
            this.ClientAppUsers = new FakeRepo<ClientAppUser>();
            this.Functions = new FakeFunctionsRepo();
        }

        public IRepository<ClientAppUser> ClientAppUsers { get; }
        public IProgramRepository Programs { get; }
        public IFunctionRepository Functions { get; }
        public Task CompleteAsync()
        {
            return Task.CompletedTask;
        }
    }

    [TestFixture]
    public class StatisticsControllerTests
    {
        [Test]
        public void TestMissingProgram()
        {
            StatisticsUpdateRequest request = new StatisticsUpdateRequest
            {
                ProgramId = 123123,
                UserId = 23
            };

            var unit = new FakeStatsUnitOfWork();

            var sut = new StatisticsController(unit);
            var response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            Assert.IsTrue(response.Error.Message.Contains($"Program [{request.ProgramId}] is null"));

        }

        [Test]
        public void TestMissingUser()
        {
            var request = new StatisticsUpdateRequest
            {
                ProgramId = 66,
                UserId = 23
            };
          
            var unit = new FakeStatsUnitOfWork();
            unit.Programs.Add(new Program()
            {
                Id = 66,
                Name = "SomeApp"
            });
         
            var sut = new StatisticsController(unit);
            var response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            Assert.IsTrue(response.Error.Message.Contains($"User [{request.UserId}] is null"));

        }

        [Test]
        public void TestUpdateAction()
         {
             var request = new StatisticsUpdateRequest
             {
                 ProgramId = 66,
                 UserId = 23
             };
             var unit = new FakeStatsUnitOfWork();
             unit.Programs.Add(new Program()
             {
                 Id = 66,
                 Name = "SomeApp"
             });
             unit.ClientAppUsers.Add(new ClientAppUser()
             {
                 Id = 23,
                 UserName = "Jim Beam"
             });
            var sut = new StatisticsController(unit);
            var response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
             Assert.IsNull(response.Error);
            Assert.AreEqual(1, response.Count);
            sut = new StatisticsController(unit);
            response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            Assert.AreEqual(2, response.Count);
            Assert.IsNull(response.Error);
        }

        [Test]
        public void AddProgramTest()
        {
            var conn = DbConnectionFactory.CreateTransient();
            var ctx = new TelimenaContext(conn);
            var sut = new StatisticsUnitOfWork(ctx);
            sut.Programs.Add(new Program()
            {
                Name = "TestProg",
                PrimaryAssembly = new PrimaryAssembly()
                {
                    Name = "TestAss"
                }
            });
            sut.CompleteAsync().GetAwaiter().GetResult();

            var prg = sut.Programs.FirstOrDefaultAsync(x=>x.Name == "TestProg").GetAwaiter().GetResult();
            Assert.AreEqual("TestAss", prg.PrimaryAssembly.Name);
            Assert.AreEqual(1, ctx.PrimaryAssemblies.FirstOrDefault(x=>x.Name == "TestAss").Program.Id);
            prg.Assemblies.Add(new ReferencedAssembly()
            {
                Name = "Helper1"
            });
            prg.Assemblies.Add(new ReferencedAssembly()
            {
                Name = "Helper2"
            });
            ctx.SaveChanges();

            prg = sut.Programs.FirstOrDefaultAsync(x => x.Name == "TestProg").GetAwaiter().GetResult();
            Assert.AreEqual(2, prg.Assemblies.Count);

            ctx.Programs.Remove(prg);

            ctx.SaveChangesAsync();

            Assert.AreEqual(0, ctx.PrimaryAssemblies.Count());
            Assert.AreEqual(0, ctx.ReferencedAssemblies.Count());
            Assert.AreEqual(0, ctx.Programs.Count());
        }
    }
}
