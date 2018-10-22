using System.Collections.Generic;
using AutomaticTestsClient;
using NUnit.Framework;
using Telimena.WebApp.UITests.Base;
using TelimenaClient;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Telimena.WebApp.UITests.IntegrationTests.BackwardCompatibilityIntegrationTests
{
    [TestFixture()]
    public partial class NonUiTests : PortalTestBase
    {

       
        [Test]
        public void HandleUpdatesNonBetaTests()
        {
            var result = this.LaunchTestsAppAndGetResult<UpdateCheckResult>(Actions.HandleUpdates);

         
        }



      
    }
}
