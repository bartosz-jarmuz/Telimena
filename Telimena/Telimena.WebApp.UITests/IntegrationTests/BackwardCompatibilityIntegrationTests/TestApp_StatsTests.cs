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
        public void ReportFunction()
        {
            FileInfo app;
            StatisticsUpdateResponse response = this.LaunchTestsAppAndGetResult<StatisticsUpdateResponse>(out app, Actions.ReportFunctionUsage, TestAppProvider.FileNames.TestAppV1, MethodBase.GetCurrentMethod().Name);

            Assert.IsTrue(response.ProgramId > 0);
            Assert.IsTrue(response.UserId > 0);
            Assert.IsTrue(response.Count > 0);
            Assert.IsTrue(response.FunctionId > 0);
            Assert.AreEqual("HandleReportFunctionUsage", response.FunctionName);

            StatisticsUpdateResponse responseNew = this.LaunchTestsAppAndGetResult<StatisticsUpdateResponse>(app, Actions.ReportFunctionUsage);

            Assert.AreEqual(responseNew.Count, response.Count + 1);
            Assert.AreEqual(responseNew.FunctionName, response.FunctionName);

            StatisticsUpdateResponse customFunctionResponse = this.LaunchTestsAppAndGetResult<StatisticsUpdateResponse>(app, Actions.ReportFunctionUsage, functionName: "UnitTestFunction");
            Assert.IsTrue(response.FunctionId < customFunctionResponse.FunctionId);
            Assert.AreEqual("UnitTestFunction", customFunctionResponse.FunctionName);
            Assert.IsTrue(customFunctionResponse.Count > 0);

        }



      
    }
}
