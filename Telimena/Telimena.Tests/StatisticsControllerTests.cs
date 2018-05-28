using System.Text;

namespace Telimena.Tests
{
    using System.Collections.ObjectModel;
    using System.Reflection;
    using System.Threading.Tasks;
    using Client;
    using Moq;
    using NUnit.Framework;
    using WebApi.Controllers;
    using WebApp.Core.Models;
    using WebApp.Infrastructure.Database;
    using WebApp.Infrastructure.Repository;
    using WebApp.Infrastructure.UnitOfWork;
    using WebApp.Infrastructure.UnitOfWork.Implementation;

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
    }
}
