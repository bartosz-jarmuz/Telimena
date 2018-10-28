using System.Collections.Generic;
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
            RegistrationResponse response = this.LaunchTestsAppAndGetResult< RegistrationResponse>(Actions.Initialize, TestAppProvider.FileNames.TestAppV1,  MethodBase.GetCurrentMethod().Name, out _);

            Assert.IsTrue(response.ProgramId > 0);
            Assert.IsTrue(response.UserId> 0);
            Assert.IsTrue(response.Count> 0);

            RegistrationResponse responseNew = this.LaunchTestsAppAndGetResult<RegistrationResponse>(Actions.Initialize, TestAppProvider.FileNames.TestAppV1, MethodBase.GetCurrentMethod().Name, out _);

            Assert.AreEqual(responseNew.Count , response.Count +1);
        }

        [Test]
        public void ReportFunction()
        {
            StatisticsUpdateResponse response = this.LaunchTestsAppAndGetResult<StatisticsUpdateResponse>(Actions.ReportFunctionUsage, TestAppProvider.FileNames.TestAppV1, MethodBase.GetCurrentMethod().Name, out _);

            Assert.IsTrue(response.ProgramId > 0);
            Assert.IsTrue(response.UserId > 0);
            Assert.IsTrue(response.Count > 0);
            Assert.IsTrue(response.FunctionId > 0);
            Assert.AreEqual("HandleReportFunctionUsage", response.FunctionName);

            StatisticsUpdateResponse responseNew = this.LaunchTestsAppAndGetResult<StatisticsUpdateResponse>(Actions.ReportFunctionUsage, TestAppProvider.FileNames.TestAppV1, MethodBase.GetCurrentMethod().Name, out _);

            Assert.AreEqual(responseNew.Count, response.Count + 1);
            Assert.AreEqual(responseNew.FunctionName, response.FunctionName);

            StatisticsUpdateResponse customFunctionResponse = this.LaunchTestsAppAndGetResult<StatisticsUpdateResponse>(Actions.ReportFunctionUsage, TestAppProvider.FileNames.TestAppV1, MethodBase.GetCurrentMethod().Name, out _, functionName: "UnitTestFunction");
            Assert.IsTrue(response.FunctionId < customFunctionResponse.FunctionId);
            Assert.AreEqual("UnitTestFunction", customFunctionResponse.FunctionName);
            Assert.IsTrue(customFunctionResponse.Count > 0);

        }



      
    }
}
