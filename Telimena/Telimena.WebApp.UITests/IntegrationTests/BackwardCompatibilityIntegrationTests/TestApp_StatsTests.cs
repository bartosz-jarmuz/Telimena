using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using AutomaticTestsClient;
using NUnit.Framework;
using Telimena.WebApp.UITests.Base;
using Telimena.WebApp.UITests.Base.TestAppInteraction;
using TelimenaClient;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Telimena.WebApp.UITests.IntegrationTests.BackwardCompatibilityIntegrationTests
{
    [TestFixture]
    public partial class _2_NonUiTests : IntegrationTestBase
    {
        [Test]
        public void InitializeTest()
        {
            RegistrationResponse response = this.LaunchTestsAppAndGetResult< RegistrationResponse>(out _, Actions.Initialize, TestAppProvider.FileNames.TestAppV1, MethodBase.GetCurrentMethod().Name);

            Assert.IsTrue(response.ProgramId > 0);
            Assert.IsTrue(response.UserId> 0);
            Assert.IsTrue(response.Count> 0);

            RegistrationResponse responseNew = this.LaunchTestsAppAndGetResult<RegistrationResponse>(out _, Actions.Initialize, TestAppProvider.FileNames.TestAppV1, MethodBase.GetCurrentMethod().Name);

            Assert.AreEqual(responseNew.Count , response.Count +1);
        }

        [Test]
        public void ReportView()
        {
            FileInfo app;
            StatisticsUpdateResponse response = this.LaunchTestsAppAndGetResult<StatisticsUpdateResponse>(out app, Actions.ReportViewUsage, TestAppProvider.FileNames.TestAppV1, MethodBase.GetCurrentMethod().Name);

            Assert.IsTrue(response.ProgramId > 0);
            Assert.IsTrue(response.UserId > 0);
            Assert.IsTrue(response.Count > 0);
            Assert.IsTrue(response.ComponentId > 0);
            Assert.AreEqual("ReportView", response.ComponentName);

            StatisticsUpdateResponse responseNew = this.LaunchTestsAppAndGetResult<StatisticsUpdateResponse>(app, Actions.ReportViewUsage);

            Assert.AreEqual(responseNew.Count, response.Count + 1);
            Assert.AreEqual(responseNew.ComponentName, response.ComponentName);

            StatisticsUpdateResponse customViewNameResponse = this.LaunchTestsAppAndGetResult<StatisticsUpdateResponse>(app, Actions.ReportViewUsage, viewName: "UnitTestView");
            Assert.IsTrue(response.ComponentId < customViewNameResponse.ComponentId);
            Assert.AreEqual("UnitTestView", customViewNameResponse.ComponentName);
            Assert.IsTrue(customViewNameResponse.Count > 0);

        }



      
    }
}
