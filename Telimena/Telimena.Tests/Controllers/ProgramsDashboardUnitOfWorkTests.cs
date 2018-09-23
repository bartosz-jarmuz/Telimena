using DbIntegrationTestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Telimena.Client;
using Telimena.WebApp.Controllers.Api;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;

namespace Telimena.Tests
{
    [TestClass]
    public class ProgramsDashboardUnitOfWorkTests : IntegrationTestsContextSharedPerClass<TelimenaContext>
    {
        //TODO!

        //private void AddPrograms()
        //{
        //    var statsCtrl = new StatisticsController(new StatisticsUnitOfWork(this.Context));
        //    statsCtrl.RegisterClient(new RegistrationRequest()
        //    {
        //        ProgramInfo = new ProgramInfo() { }
        //    }).GetAwaiter().GetResult();
        //}

        //[TestMethod()]
        //public void GetAllProgramsTest()
        //{
        //    var sut = new ProgramsDashboardUnitOfWork(this.Context);




        //    var summary = sut.GetAllProgramsSummaryCounts()
        //}
    }
}