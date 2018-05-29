using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telimena.WebApp.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Telimena.WebApp.Controllers.Tests
{
    using Infrastructure.Database;
    using Infrastructure.Repository;

    [TestClass()]
    public class AdminDashboardControllerTests
    {
        [TestMethod()]
        public void GetAllProgramsTest()
        {
            var ctrl = new AdminDashboardController(null, new AdminDashboardUnitOfWork(new TelimenaContext()));
            ctrl.GetAllPrograms().GetAwaiter().GetResult();

        }
    }
}