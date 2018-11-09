using System;
using System.Collections.Generic;
using System.Linq;
using DbIntegrationTestHelpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using NUnit.Framework;
using Telimena.WebApp.Controllers.Api;
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
            var data = new List<FunctionUsageDetail>();

            data.Add(new FunctionUsageDetail() {Id=1, UsageSummary = this.GetSummary("AAA", "ZZZ") });
            data.Add(new FunctionUsageDetail() {Id=2, UsageSummary = this.GetSummary("BBB", "ZZZ") });
            data.Add(new FunctionUsageDetail() {Id=3, UsageSummary = this.GetSummary("CCC", "ZZZ") });
            data.Add(new FunctionUsageDetail() {Id=4, UsageSummary = this.GetSummary("CCC", "ZZZ") });
            data.Add(new FunctionUsageDetail() {Id=5, UsageSummary = this.GetSummary("AAA", "XXX") });
            data.Add(new FunctionUsageDetail() {Id=6, UsageSummary = this.GetSummary("AAA", "YYY") });
            data.Add(new FunctionUsageDetail() {Id=7, UsageSummary = this.GetSummary("BBB", "YYY") });
            data.Add(new FunctionUsageDetail() {Id=8, UsageSummary = this.GetSummary("BBB", "XXX") });


            var sorts = new List<Tuple<string, bool>>();
            sorts.Add(new Tuple<string, bool>(nameof(UsageData.UserName), false));
            sorts.Add(new Tuple<string, bool>(nameof(UsageData.FunctionName), false));

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
            var data = new List<FunctionUsageDetail>();

            data.Add(new FunctionUsageDetail() { Id = 1, UsageSummary = this.GetSummary("AAA", "ZZZ") });
            data.Add(new FunctionUsageDetail() { Id = 2, UsageSummary = this.GetSummary("BBB", "ZZZ") });
            data.Add(new FunctionUsageDetail() { Id = 3, UsageSummary = this.GetSummary("CCC", "ZZZ") });
            data.Add(new FunctionUsageDetail() { Id = 4, UsageSummary = this.GetSummary("CCC", "ZZZ") });
            data.Add(new FunctionUsageDetail() { Id = 5, UsageSummary = this.GetSummary("AAA", "XXX") });
            data.Add(new FunctionUsageDetail() { Id = 6, UsageSummary = this.GetSummary("AAA", "YYY") });
            data.Add(new FunctionUsageDetail() { Id = 7, UsageSummary = this.GetSummary("BBB", "YYY") });
            data.Add(new FunctionUsageDetail() { Id = 8, UsageSummary = this.GetSummary("BBB", "XXX") });


            var sorts = new List<Tuple<string, bool>>();
            sorts.Add(new Tuple<string, bool>(nameof(UsageData.FunctionName), false));

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
            var data = new List<FunctionUsageDetail>();

            data.Add(new FunctionUsageDetail() { Id = 1, UsageSummary = this.GetSummary("AAA", "ZZZ"), AssemblyVersion = new AssemblyVersion("2.0", null ) });
            data.Add(new FunctionUsageDetail() { Id = 2, UsageSummary = this.GetSummary("BBB", "ZZZ"), AssemblyVersion = new AssemblyVersion("6.0", null ) });
            data.Add(new FunctionUsageDetail() { Id = 3, UsageSummary = this.GetSummary("CCC", "ZZZ"), AssemblyVersion = new AssemblyVersion("4.0", null ) });
           
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
            var data = new List<FunctionUsageDetail>();

            data.Add(new FunctionUsageDetail() { Id = 1, UsageSummary = this.GetSummary("AAA", "ZZZ"),AssemblyVersion = new AssemblyVersion("2.0", null)});
            data.Add(new FunctionUsageDetail() { Id = 2, UsageSummary = this.GetSummary("BBB", "ZZZ"),AssemblyVersion = new AssemblyVersion("6.0", null)});
            data.Add(new FunctionUsageDetail() { Id = 3, UsageSummary = this.GetSummary("CCC", "ZZZ"),AssemblyVersion = new AssemblyVersion("4.0", null)});
            data.Add(new FunctionUsageDetail() { Id = 4, UsageSummary = this.GetSummary("CCC", "ZZZ"),AssemblyVersion = new AssemblyVersion("4.0", null)});
            data.Add(new FunctionUsageDetail() { Id = 5, UsageSummary = this.GetSummary("AAA", "XXX"),AssemblyVersion = new AssemblyVersion("2.0", null)});
            data.Add(new FunctionUsageDetail() { Id = 6, UsageSummary = this.GetSummary("AAA", "YYY"),AssemblyVersion = new AssemblyVersion("2.0", null)});
            data.Add(new FunctionUsageDetail() { Id = 7, UsageSummary = this.GetSummary("BBB", "YYY"),AssemblyVersion = new AssemblyVersion("9.0", null)});
            data.Add(new FunctionUsageDetail() { Id = 8, UsageSummary = this.GetSummary("BBB", "XXX"),AssemblyVersion = new AssemblyVersion("1.0", null)});


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
            var data = new List<FunctionUsageDetail>();

            data.Add(new FunctionUsageDetail() { Id = 3, UsageSummary = this.GetSummary("CCC", "ZZZ") });
            data.Add(new FunctionUsageDetail() { Id = 4, UsageSummary = this.GetSummary("CCC", "ZZZ") });
            data.Add(new FunctionUsageDetail() { Id = 1, UsageSummary = this.GetSummary("AAA", "ZZZ") });
            data.Add(new FunctionUsageDetail() { Id = 2, UsageSummary = this.GetSummary("BBB", "ZZZ") });


            var sorts = new List<Tuple<string, bool>>();
            
            var ordered = ProgramsDashboardUnitOfWork.ApplyOrderingQuery(sorts, data.AsAsyncQueryable(), 0, 10).GetAwaiter().GetResult().ToList();

            Assert.AreEqual(4, ordered[0].Id);
            Assert.AreEqual(3, ordered[1].Id);
            Assert.AreEqual(2, ordered[2].Id);
            Assert.AreEqual(1, ordered[3].Id);

        }





        private FunctionUsageSummary GetSummary(string userName, string functionName)
        {
           return  new FunctionUsageSummary()
            {
                ClientAppUser = new ClientAppUser() { UserName = userName }
                ,
                Function = new Function() { Name = functionName }
            };
        }
    }
}