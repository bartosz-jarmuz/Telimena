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

            var guid = AdminDashboardController.GetTelemetryKeyFromUrl(url);

            Assert.AreEqual(new Guid("637df89e-50bc-4bb2-aa90-1c664b056510"), guid.Value);
        }
        [Test]

        public void TestGetKeyFromUrlForAudit_MoreParams()
        {
            string url = "https://test.net/ProgramManagement?telemetryKey=637df89e-50bc-4bb2-aa90-1c664b056510?something=something";

            var guid = AdminDashboardController.GetTelemetryKeyFromUrl(url);

            Assert.AreEqual(new Guid("637df89e-50bc-4bb2-aa90-1c664b056510"), guid.Value);
        }

        [Test]
        public void TestGetKeyFromUrlForAudit_NoKey()
        {
            string url = "https://test.net/ProgramManagement?something=something";

            var guid = AdminDashboardController.GetTelemetryKeyFromUrl(url);

            Assert.IsNull(guid);
        }

        [Test]
        public void TestPackageKeyFromUrlForAudit_Simple()
        {
            string url = "https://test.net/api/v1/update-packages/0a331a06-1758-4d83-a769-1cd9658bff03";

            var guid = AdminDashboardController.GetUpdatePackageGuidFromUrl(url);

            Assert.AreEqual(new Guid("0a331a06-1758-4d83-a769-1cd9658bff03"), guid.Value);
        }

        [Test]
        public void TestPackageKeyFromUrlForAudit_MoreParams()
        {
            string url = "https://ttest.net/api/v1/update-packages/0a331a06-1758-4d83-a769-1cd9658bff03/something";

            var guid = AdminDashboardController.GetUpdatePackageGuidFromUrl(url);

            Assert.AreEqual(new Guid("0a331a06-1758-4d83-a769-1cd9658bff03"), guid.Value);
        }

        [Test]
        public void TestPackageKeyFromUrlForAudit_NoKey()
        {
            string url = "https://test.net/api/v1/something";

            var guid = AdminDashboardController.GetUpdatePackageGuidFromUrl(url);

            Assert.IsNull(guid);
        }
    }
}