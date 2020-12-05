using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using NUnit.Framework;
using SharedLogic;
using Telimena.TestUtilities.Base;
using Telimena.TestUtilities.Base.TestAppInteraction;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Messages;

namespace Telimena.WebApp.AppIntegrationTests.BackwardCompatibilityIntegrationTests
{
    [TestFixture]
    [Order(2)]
    [SuppressMessage("ReSharper", "ConsiderUsingConfigureAwait")]
    public partial class _2_NonUiTests : IntegrationTestBase
    {

        [Test]
        public void TestAppWithEmbeddedTeli()
        {
            this.outputs.Clear();
            this.errors.Clear();

            this.LaunchTestsAppNewInstance(out _, Actions.ReportViewUsage, Apps.Keys.AutomaticTestsClient, 
                Apps.PackageNames.EmbeddedAssemblyTestApp, SharedTestHelpers.GetMethodName());

            Assert.IsTrue(this.outputs.Any(x=>x == "Ended with no errors"));
            this.outputs.Clear();
            this.errors.Clear();

        }
#if DEBUG
        [Test]
#endif
        public async Task ReportView()
        {
            string viewName = nameof(this.ReportView);
            //seed the data
            DateTimeOffset timestamp = DateTimeOffset.UtcNow;
            FileInfo app = this.LaunchTestsAppNewInstance(out _, Actions.ReportViewUsage, Apps.Keys.AutomaticTestsClient, Apps.PackageNames.AutomaticTestsClientAppV1, "", viewName: viewName);


            TelemetryQueryRequest request = TelemetryQueryRequest.CreateFull(Apps.Keys.AutomaticTestsClient);
            TelemetryQueryResponse queryResponse = await this.CheckTelemetry(request);
            TelemetrySummaryDto summary = null;
            TelemetryAwareComponentDto viewComponent = null;



                viewComponent = queryResponse.TelemetryAware.First(x => x.ComponentKey == viewName);
                Assert.IsNotNull(viewComponent);
                summary = viewComponent.Summaries.OrderByDescending(x=>x.LastReported).FirstOrDefault();
                Assert.IsNotNull(summary);

            var userName = summary.UserName;

            timestamp = DateTimeOffset.UtcNow;

            app= this.LaunchTestsAppNewInstance(out _, Actions.ReportViewUsage, Apps.Keys.AutomaticTestsClient, Apps.PackageNames.AutomaticTestsClientAppV1, "", viewName: viewName);
            
            //Assert.IsNull(response.Exception);
            //Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);

            queryResponse = await this.CheckTelemetry(request);

            viewComponent = queryResponse.TelemetryAware.First(x => x.ComponentKey == viewName);

            if (summary == null) //first time ever the test is run, the summary is empty - let it do work and fail 
            {
                summary = viewComponent.Summaries.FirstOrDefault(x => x.UserName == userName);
            }

            var summaryAfterUpdate = viewComponent.Summaries.FirstOrDefault(x => x.UserName== userName);
            Assert.IsNotNull(summaryAfterUpdate);

            Assert.Greater(summaryAfterUpdate.SummaryCount, summary.SummaryCount);
            Assert.Greater(summaryAfterUpdate.LastReported, summary.LastReported);
            Assert.Greater(summaryAfterUpdate.Details.Count, summary.Details.Count);
            Assert.That(summaryAfterUpdate.Details.OrderByDescending(x => x.Timestamp).First().Timestamp, Is.EqualTo(timestamp).Within(TimeSpan.FromSeconds(5.0)));

        }


    }
}