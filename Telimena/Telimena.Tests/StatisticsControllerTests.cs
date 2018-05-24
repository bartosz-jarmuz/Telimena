using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telimena.Tests
{
    using System.Reflection;
    using Client;
    using Moq;
    using NUnit.Framework;
    using WebApi.Controllers;
    using WebApp.Core.Models;
    using WebApp.Infrastructure.Repository;
    using WebApp.Infrastructure.UnitOfWork;
    using WebApp.Infrastructure.UnitOfWork.Implementation;

    [TestFixture]
    public class StatisticsControllerTests
    {
        [Test]
        public void TestUpdateAction()
         {
             var request = new StatisticsUpdateRequest
             {
                 ProgramId = 66,
                 UserId = 23
             };

             var userRepo = new Mock<IClientAppUserRepository>();
             userRepo.Setup(x => x.Get(request.UserId)).Returns(new ClientAppUser()
             {
                 Id = request.UserId
             });
             var programRepo = new Mock<IProgramRepository>();
             programRepo.Setup(x => x.Get(request.ProgramId)).Returns(new Program()
            {
                Id = request.UserId
            });

             var work = new Mock<IStatisticsUnitOfWork>();

             work.Setup(x => x.ClientAppUsers).Returns(userRepo.Object);
             work.Setup(x => x.Programs).Returns(programRepo.Object);
            var sut = new StatisticsController(work.Object);
            var response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            Assert.AreEqual(1, response.Count);
            Assert.IsNull(response.Error);
            sut = new StatisticsController(work);
            response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            Assert.AreEqual(2, response.Count);
            Assert.IsNull(response.Error);
        }
    }
}
