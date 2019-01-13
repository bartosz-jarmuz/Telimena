using System;
using System.IO;
using System.Reflection;
using AutomaticTestsClient;
using NUnit.Framework;
using Telimena.WebApp.UITests.Base;
using Telimena.WebApp.UITests.Base.TestAppInteraction;
using TelimenaClient;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Telimena.WebApp.UITests._02._IntegrationTests.BackwardCompatibilityIntegrationTests
{
    [TestFixture, Order(2)]
    public partial class _2_NonUiTests : IntegrationTestBase
    {
        [Test]
        public void InitializeTest()
        {
            TelemetryInitializeResponse response = this.LaunchTestsAppAndGetResult< TelemetryInitializeResponse>(out _, Actions.Initialize, TestAppProvider.FileNames.TestAppV1, MethodBase.GetCurrentMethod().Name);

            Assert.IsNull(response.Exception);
            Assert.IsTrue(response.UserId != Guid.Empty);
            //Assert.IsTrue(response.Count> 0);

            TelemetryInitializeResponse responseNew = this.LaunchTestsAppAndGetResult<TelemetryInitializeResponse>(out _, Actions.Initialize, TestAppProvider.FileNames.TestAppV1, MethodBase.GetCurrentMethod().Name);

            //Assert.AreEqual(responseNew.Count , response.Count +1);
        }

        [Test]
        public void ReportView()
        {
            FileInfo app;
            TelemetryUpdateResponse response = this.LaunchTestsAppAndGetResult<TelemetryUpdateResponse>(out app, Actions.ReportViewUsage, TestAppProvider.FileNames.TestAppV1, "", viewName: MethodBase.GetCurrentMethod().Name);

            Assert.IsNull(response.Exception);
            Assert.IsTrue(response.TelemetryKey != Guid.Empty);
            Assert.IsTrue(response.UserId != Guid.Empty);
            Assert.IsTrue(response.Count > 0);
            Assert.IsTrue(response.ComponentId > 0);
            Assert.AreEqual("ReportView", response.ComponentName);

            TelemetryUpdateResponse responseNew = this.LaunchTestsAppAndGetResult<TelemetryUpdateResponse>(out app, Actions.ReportViewUsage, TestAppProvider.FileNames.TestAppV1, "", viewName: MethodBase.GetCurrentMethod().Name);

            Assert.AreEqual(responseNew.Count, response.Count + 1);
            Assert.AreEqual("ReportView", responseNew.ComponentName);

            TelemetryUpdateResponse customViewNameResponse = this.LaunchTestsAppAndGetResult<TelemetryUpdateResponse>(app, Actions.ReportViewUsage, viewName: "UnitTestView");
            Assert.IsTrue(response.ComponentId < customViewNameResponse.ComponentId);
            Assert.AreEqual("UnitTestView", customViewNameResponse.ComponentName);
            Assert.IsTrue(customViewNameResponse.Count > 0);

        }



      
    }
}
