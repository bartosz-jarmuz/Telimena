using System;
using System.Collections.Generic;
using System.Linq;
using DbIntegrationTestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Api;
using Telimena.WebApp.Core;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Identity;
using Telimena.WebApp.Infrastructure.Repository;
using Telimena.WebApp.Infrastructure.UnitOfWork.Implementation;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Telimena.Tests
{
    [TestFixture]
    public class ProgramsDashboardUnitOfWorkTests : IntegrationTestsContextSharedGlobally<TelimenaContext>
    {
        [Test]
        public void TestOrderingQuery_TwoProps()
        {
            var data = new List<ViewTelemetryDetail>();

            data.Add(new ViewTelemetryDetail() {Id=1, TelemetrySummary = this.GetSummary("AAA", "ZZZ") });
            data.Add(new ViewTelemetryDetail() {Id=2, TelemetrySummary = this.GetSummary("BBB", "ZZZ") });
            data.Add(new ViewTelemetryDetail() {Id=3, TelemetrySummary = this.GetSummary("CCC", "ZZZ") });
            data.Add(new ViewTelemetryDetail() {Id=4, TelemetrySummary = this.GetSummary("CCC", "ZZZ") });
            data.Add(new ViewTelemetryDetail() {Id=5, TelemetrySummary = this.GetSummary("AAA", "XXX") });
            data.Add(new ViewTelemetryDetail() {Id=6, TelemetrySummary = this.GetSummary("AAA", "YYY") });
            data.Add(new ViewTelemetryDetail() {Id=7, TelemetrySummary = this.GetSummary("BBB", "YYY") });
            data.Add(new ViewTelemetryDetail() {Id=8, TelemetrySummary = this.GetSummary("BBB", "XXX") });


            var sorts = new List<Tuple<string, bool>>();
            sorts.Add(new Tuple<string, bool>(nameof(UsageData.UserName), false));
            sorts.Add(new Tuple<string, bool>(nameof(UsageData.ViewName), false));

            var ordered = ProgramsDashboardUnitOfWork.ApplyOrderingQuery(sorts, data.AsAsyncQueryable(),0,10).GetAwaiter().GetResult().ToList();

            Assert.AreEqual(5, ordered[0].Id);
            Assert.AreEqual(6, ordered[1].Id);
            Assert.AreEqual(1, ordered[2].Id);
            Assert.AreEqual(8, ordered[3].Id);
            Assert.AreEqual(7, ordered[4].Id);
            Assert.AreEqual(2, ordered[5].Id);
            Assert.AreEqual(3, ordered[6].Id);
            Assert.AreEqual(4, ordered[7].Id);

        }


        [Test]
        public void TestOrderingQuery_OneProp()
        {
            var data = new List<ViewTelemetryDetail>();

            data.Add(new ViewTelemetryDetail() { Id = 1, TelemetrySummary = this.GetSummary("AAA", "ZZZ") });
            data.Add(new ViewTelemetryDetail() { Id = 2, TelemetrySummary = this.GetSummary("BBB", "ZZZ") });
            data.Add(new ViewTelemetryDetail() { Id = 3, TelemetrySummary = this.GetSummary("CCC", "ZZZ") });
            data.Add(new ViewTelemetryDetail() { Id = 4, TelemetrySummary = this.GetSummary("CCC", "ZZZ") });
            data.Add(new ViewTelemetryDetail() { Id = 5, TelemetrySummary = this.GetSummary("AAA", "XXX") });
            data.Add(new ViewTelemetryDetail() { Id = 6, TelemetrySummary = this.GetSummary("AAA", "YYY") });
            data.Add(new ViewTelemetryDetail() { Id = 7, TelemetrySummary = this.GetSummary("BBB", "YYY") });
            data.Add(new ViewTelemetryDetail() { Id = 8, TelemetrySummary = this.GetSummary("BBB", "XXX") });


            var sorts = new List<Tuple<string, bool>>();
            sorts.Add(new Tuple<string, bool>(nameof(UsageData.ViewName), false));

            var ordered = ProgramsDashboardUnitOfWork.ApplyOrderingQuery(sorts, data.AsAsyncQueryable(), 0, 10).GetAwaiter().GetResult().ToList();

            Assert.AreEqual(5, ordered[0].Id);
            Assert.AreEqual(8, ordered[1].Id);
            Assert.AreEqual(6, ordered[2].Id);
            Assert.AreEqual(7, ordered[3].Id);
            Assert.AreEqual(1, ordered[4].Id);
            Assert.AreEqual(2, ordered[5].Id);
            Assert.AreEqual(3, ordered[6].Id);
            Assert.AreEqual(4, ordered[7].Id);

        }


        [Test]
        public void TestOrderingQuery_FailSafe()
        {
            var data = new List<ViewTelemetryDetail>();

            data.Add(new ViewTelemetryDetail() { Id = 1, TelemetrySummary = this.GetSummary("AAA", "ZZZ"), AssemblyVersion = new AssemblyVersionInfo(new VersionData("2.0", null) ) });
            data.Add(new ViewTelemetryDetail() { Id = 2, TelemetrySummary = this.GetSummary("BBB", "ZZZ"), AssemblyVersion = new AssemblyVersionInfo(new VersionData("6.0", null) ) });
            data.Add(new ViewTelemetryDetail() { Id = 3, TelemetrySummary = this.GetSummary("CCC", "ZZZ"), AssemblyVersion = new AssemblyVersionInfo(new VersionData("4.0", null) ) });
           
            var sorts = new List<Tuple<string, bool>>();
            sorts.Add(new Tuple<string, bool>("WrongKey", false));

            var ordered = ProgramsDashboardUnitOfWork.ApplyOrderingQuery(sorts, data.AsAsyncQueryable(), 0, 10).GetAwaiter().GetResult().ToList();

            Assert.AreEqual(3, ordered[0].Id);
            Assert.AreEqual(2, ordered[1].Id);
            Assert.AreEqual(1, ordered[2].Id);

        }

        [Test]
        public void TestOrderingQuery_Version()
        {
            var data = new List<ViewTelemetryDetail>();

            data.Add(new ViewTelemetryDetail() { Id = 1, TelemetrySummary = this.GetSummary("AAA", "ZZZ"),AssemblyVersion = new AssemblyVersionInfo(new VersionData("2.0", null))});
            data.Add(new ViewTelemetryDetail() { Id = 2, TelemetrySummary = this.GetSummary("BBB", "ZZZ"),AssemblyVersion = new AssemblyVersionInfo(new VersionData("6.0", null))});
            data.Add(new ViewTelemetryDetail() { Id = 3, TelemetrySummary = this.GetSummary("CCC", "ZZZ"),AssemblyVersion = new AssemblyVersionInfo(new VersionData("4.0", null))});
            data.Add(new ViewTelemetryDetail() { Id = 4, TelemetrySummary = this.GetSummary("CCC", "ZZZ"),AssemblyVersion = new AssemblyVersionInfo(new VersionData("4.0", null))});
            data.Add(new ViewTelemetryDetail() { Id = 5, TelemetrySummary = this.GetSummary("AAA", "XXX"),AssemblyVersion = new AssemblyVersionInfo(new VersionData("2.0", null))});
            data.Add(new ViewTelemetryDetail() { Id = 6, TelemetrySummary = this.GetSummary("AAA", "YYY"),AssemblyVersion = new AssemblyVersionInfo(new VersionData("2.0", null))});
            data.Add(new ViewTelemetryDetail() { Id = 7, TelemetrySummary = this.GetSummary("BBB", "YYY"),AssemblyVersion = new AssemblyVersionInfo(new VersionData("9.0", null))});
            data.Add(new ViewTelemetryDetail() { Id = 8, TelemetrySummary = this.GetSummary("BBB", "XXX"),AssemblyVersion = new AssemblyVersionInfo(new VersionData("1.0", null))});


            var sorts = new List<Tuple<string, bool>>();
            sorts.Add(new Tuple<string, bool>(nameof(UsageData.ProgramVersion), false));

            var ordered = ProgramsDashboardUnitOfWork.ApplyOrderingQuery(sorts, data.AsAsyncQueryable(), 0, 10).GetAwaiter().GetResult().ToList();

            Assert.AreEqual(8, ordered[0].Id);
            Assert.AreEqual(1, ordered[1].Id);
            Assert.AreEqual(5, ordered[2].Id);
            Assert.AreEqual(6, ordered[3].Id);
            Assert.AreEqual(3, ordered[4].Id);
            Assert.AreEqual(4, ordered[5].Id);
            Assert.AreEqual(2, ordered[6].Id);
            Assert.AreEqual(7, ordered[7].Id);

        }

        [Test]
        public void TestOrderingQuery_NoSorts()
        {
            var data = new List<ViewTelemetryDetail>();

            data.Add(new ViewTelemetryDetail() { Id = 3, TelemetrySummary = this.GetSummary("CCC", "ZZZ") });
            data.Add(new ViewTelemetryDetail() { Id = 4, TelemetrySummary = this.GetSummary("CCC", "ZZZ") });
            data.Add(new ViewTelemetryDetail() { Id = 1, TelemetrySummary = this.GetSummary("AAA", "ZZZ") });
            data.Add(new ViewTelemetryDetail() { Id = 2, TelemetrySummary = this.GetSummary("BBB", "ZZZ") });


            var sorts = new List<Tuple<string, bool>>();
            
            var ordered = ProgramsDashboardUnitOfWork.ApplyOrderingQuery(sorts, data.AsAsyncQueryable(), 0, 10).GetAwaiter().GetResult().ToList();

            Assert.AreEqual(4, ordered[0].Id);
            Assert.AreEqual(3, ordered[1].Id);
            Assert.AreEqual(2, ordered[2].Id);
            Assert.AreEqual(1, ordered[3].Id);

        }





        private ViewTelemetrySummary GetSummary(string userName, string viewName)
        {
           return  new ViewTelemetrySummary()
            {
                ClientAppUser = new ClientAppUser() { UserName = userName }
                ,
                View = new View() { Name = viewName }
            };
        }
    }
}