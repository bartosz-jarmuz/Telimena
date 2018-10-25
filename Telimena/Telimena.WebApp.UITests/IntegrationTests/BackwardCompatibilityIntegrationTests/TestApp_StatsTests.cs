using System.Collections.Generic;
using AutomaticTestsClient;
using NUnit.Framework;
using Telimena.WebApp.UITests.Base;
using Telimena.WebApp.UITests.IntegrationTests.TestAppInteraction;
using TelimenaClient;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Telimena.WebApp.UITests.IntegrationTests.BackwardCompatibilityIntegrationTests
{
    [TestFixture()]
    public partial class NonUiTests : PortalTestBase
    {

        [SetUp]
        public void ResetLists()
        {
            this.errors = new List<string>();
            this.outputs = new List<string>();
        }


        [Test]
        public void InitializeTest()
        {
            RegistrationResponse response = this.LaunchTestsAppAndGetResult< RegistrationResponse>(Actions.Initialize, TestAppProvider.FileNames.TestAppV1);

            Assert.IsTrue(response.ProgramId > 0);
            Assert.IsTrue(response.UserId> 0);
            Assert.IsTrue(response.Count> 0);

            RegistrationResponse responseNew = this.LaunchTestsAppAndGetResult<RegistrationResponse>(Actions.Initialize, TestAppProvider.FileNames.TestAppV1);

            Assert.AreEqual(responseNew.Count , response.Count +1);
        }

        [Test]
        public void ReportFunction()
        {
            StatisticsUpdateResponse response = this.LaunchTestsAppAndGetResult<StatisticsUpdateResponse>(Actions.ReportFunctionUsage, TestAppProvider.FileNames.TestAppV1);

            Assert.IsTrue(response.ProgramId > 0);
            Assert.IsTrue(response.UserId > 0);
            Assert.IsTrue(response.Count > 0);
            Assert.IsTrue(response.FunctionId > 0);
            Assert.AreEqual("HandleReportFunctionUsage", response.FunctionName);

            StatisticsUpdateResponse responseNew = this.LaunchTestsAppAndGetResult<StatisticsUpdateResponse>(Actions.ReportFunctionUsage, TestAppProvider.FileNames.TestAppV1);

            Assert.AreEqual(responseNew.Count, response.Count + 1);
            Assert.AreEqual(responseNew.FunctionName, response.FunctionName);

            StatisticsUpdateResponse customFunctionResponse = this.LaunchTestsAppAndGetResult<StatisticsUpdateResponse>(Actions.ReportFunctionUsage, TestAppProvider.FileNames.TestAppV1, functionName: "UnitTestFunction");
            Assert.IsTrue(response.FunctionId < customFunctionResponse.FunctionId);
            Assert.AreEqual("UnitTestFunction", customFunctionResponse.FunctionName);
            Assert.IsTrue(customFunctionResponse.Count > 0);

        }



      
    }
}
