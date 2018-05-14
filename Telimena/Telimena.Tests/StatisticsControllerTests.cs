using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telimena.Tests
{
    using System.Reflection;
    using Client;
    using NUnit.Framework;
    using WebApi.Controllers;
    using WebApp.Infrastructure.Repository;

    [TestFixture]
    public class StatisticsControllerTests
    {
        [Test]
        public void TestAction()
         {
            var request = new StatisticsUpdateRequest();
            request.ProgramInfo = new ProgramInfo()
            {
                MainAssembly = new AssemblyInfo(Assembly.GetExecutingAssembly()),
                Name = "TestProgram" + Guid.NewGuid(),
                Version = "1.0.0.0"
            };
            request.UserInfo = new UserInfo()
            {
                MachineName = "TestMachine",
                UserName = "TestUser"
            };

             var repo = new TelimenaRepository();

            var sut = new StatisticsController(repo);
           var response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
            Assert.AreEqual(1, response.Count);
            Assert.IsTrue(response.IsMessageSuccessful);
            sut = new StatisticsController(repo);
            response = sut.UpdateProgramStatistics(request).GetAwaiter().GetResult();
             Assert.AreEqual(2, response.Count);
             Assert.IsTrue(response.IsMessageSuccessful);
        }
    }
}
