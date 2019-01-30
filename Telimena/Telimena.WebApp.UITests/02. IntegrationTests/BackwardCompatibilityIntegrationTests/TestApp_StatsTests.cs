using System;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using AutomaticTestsClient;
using DotNetLittleHelpers;
using Newtonsoft.Json;
using NUnit.Framework;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Messages;
using Telimena.WebApp.UITests.Base;
using Telimena.WebApp.UITests.Base.TestAppInteraction;

namespace Telimena.WebApp.UITests._02._IntegrationTests.BackwardCompatibilityIntegrationTests
{
    [TestFixture]
    [Order(2)]
    [SuppressMessage("ReSharper", "ConsiderUsingConfigureAwait")]
    public partial class _2_NonUiTests : IntegrationTestBase
    {
        internal class DtoJsonConverter : JsonConverter
        {
            private static readonly string HttpContent = typeof(HttpContent).FullName;

            public override bool CanConvert(Type objectType)
            {
                if (objectType.FullName == HttpContent)
                {
                    return true;
                }

                return false;
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
            {
                if (objectType.FullName == HttpContent)
                {
                    return serializer.Deserialize(reader, typeof(HttpContent));
                }

                throw new NotSupportedException(string.Format("Type {0} unexpected.", objectType));
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, value);
            }
        }

        [Test]
        public void InitializeTest()
        {
            TelemetryInitializeResponse response = this.LaunchTestsAppAndGetResult<TelemetryInitializeResponse>(out _, Actions.Initialize
                , TestAppProvider.FileNames.TestAppV1, MethodBase.GetCurrentMethod().Name);

            Assert.IsNull(response.Exception);
            Assert.IsTrue(response.UserId != Guid.Empty);
            //Assert.IsTrue(response.Count> 0);

            TelemetryInitializeResponse responseNew = this.LaunchTestsAppAndGetResult<TelemetryInitializeResponse>(out _, Actions.Initialize
                , TestAppProvider.FileNames.TestAppV1, MethodBase.GetCurrentMethod().Name);

            //Assert.AreEqual(responseNew.Count , response.Count +1);
        }

        [Test]
        public async Task ReportView()
        {
            string viewName = nameof(this.ReportView);
            //seed the data
            DateTimeOffset timestamp = DateTimeOffset.UtcNow;
            FileInfo app = this.LaunchTestsAppNewInstance(out _, Actions.ReportViewUsage, TestAppProvider.FileNames.TestAppV1, "", viewName: viewName);


            TelemetryQueryRequest request = TelemetryQueryRequest.CreateFull(new Guid(AutomaticTestsClientTelemetryKey));
            TelemetryQueryResponse queryResponse = await this.CheckTelemetry(request);
            TelemetrySummaryDto summary = null;
            TelemetryAwareComponentDto viewComponent = null;

                viewComponent = queryResponse.TelemetryAware.First(x => x.ComponentKey == viewName);
                Assert.IsNotNull(viewComponent);
                summary = viewComponent.Summaries.FirstOrDefault(x => x.UserName == Environment.UserName);
                Assert.IsNotNull(summary);


            timestamp = DateTimeOffset.UtcNow;
            app= this.LaunchTestsAppNewInstance(out _, Actions.ReportViewUsage, TestAppProvider.FileNames.TestAppV1, "", viewName: viewName);
            //Assert.IsNull(response.Exception);
            //Assert.AreEqual(HttpStatusCode.Accepted, response.StatusCode);

            queryResponse = await this.CheckTelemetry(request);

            viewComponent = queryResponse.TelemetryAware.First(x => x.ComponentKey == viewName);

            if (summary == null) //first time ever the test is run, the summary is empty - let it do work and fail 
            {
                summary = viewComponent.Summaries.FirstOrDefault(x => x.UserName == Environment.UserName);
            }

            var summaryAfterUpdate = viewComponent.Summaries.FirstOrDefault(x => x.UserName== Environment.UserName);
            Assert.IsNotNull(summaryAfterUpdate);

            Assert.Greater(summaryAfterUpdate.SummaryCount, summary.SummaryCount);
            Assert.Greater(summaryAfterUpdate.LastReported, summary.LastReported);
            Assert.Greater(summaryAfterUpdate.Details.Count, summary.Details.Count);
            Assert.That(summaryAfterUpdate.Details.OrderByDescending(x => x.Timestamp).First().Timestamp, Is.EqualTo(timestamp).Within(TimeSpan.FromSeconds(3.0)));

        }
    }
}