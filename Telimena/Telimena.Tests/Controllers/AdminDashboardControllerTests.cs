using System;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Admin;

namespace Telimena.Tests
{
    [TestFixture]
    public class AdminDashboardControllerTests 
    {
        [Test]
        public void TestGetKeyFromUrlForAudit_Simple()
        {
            string url = "https://test.net/ProgramManagement?telemetryKey=637df89e-50bc-4bb2-aa90-1c664b056510";

            var guid = AdminDashboardController.GetGuidFromUrl(url);

            Assert.AreEqual(new Guid("637df89e-50bc-4bb2-aa90-1c664b056510"), guid.Value);
        }
        [Test]

        public void TestGetKeyFromUrlForAudit_MoreParams()
        {
            string url = "https://test.net/ProgramManagement?telemetryKey=637df89e-50bc-4bb2-aa90-1c664b056510?something=something";

            var guid = AdminDashboardController.GetGuidFromUrl(url);

            Assert.AreEqual(new Guid("637df89e-50bc-4bb2-aa90-1c664b056510"), guid.Value);
        }
        [Test]

        public void TestGetKeyFromUrlForAudit_NoKey()
        {
            string url = "https://test.net/ProgramManagement?something=something";

            var guid = AdminDashboardController.GetGuidFromUrl(url);

            Assert.IsNull(guid);
        }
    }
}