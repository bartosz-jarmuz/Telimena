using System;
using System.Collections.Generic;
using System.Linq;
using DbIntegrationTestHelpers;
using NUnit.Framework;
using Telimena.WebApp.Core.DTO;
using Telimena.WebApp.Core.DTO.MappableToClient;
using Telimena.WebApp.Core.Models;
using Telimena.WebApp.Core.Models.Telemetry;
using Telimena.WebApp.Infrastructure.Database;
using Telimena.WebApp.Infrastructure.Repository;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;

namespace Telimena.Tests.UnitOfWorkTests
{
    [TestFixture]
    public class ProgramsDashboardUnitOfWorkTests 
    {
        private readonly Guid id_1 = Guid.Parse("a6beaaf7-3be2-4a12-b434-5efbd5f81eaf");
        private readonly Guid id_2 = Guid.Parse("e04b141e-4879-4346-9220-5d364992839d");
        private readonly Guid id_3 = Guid.Parse("7e032499-6ec0-4b06-aed4-8f569e143a42");
        private readonly Guid id_4 = Guid.Parse("9324d1d1-bbea-4b22-b9d0-cf6b5e7a248a");
        private readonly Guid id_5 = Guid.Parse("3c9d5a54-d251-419c-b453-b7e71a22ba59");
        private readonly Guid id_6 = Guid.Parse("7185c626-ff32-412c-8572-3f8e04dfe535");
        private readonly Guid id_7 = Guid.Parse("d63af478-6fe7-4e77-8fc5-85c56755fabb");
        private readonly Guid id_8 = Guid.Parse("e336b9eb-3d7a-42cf-8997-0e3293e830f6");

      

        [Test]
        public void TestOrderingQuery_FailSafe()
        {
            List<ViewTelemetryDetail> data = new List<ViewTelemetryDetail>();

            data.Add(new ViewTelemetryDetail(this.id_1)
            {
                Timestamp = new DateTime(2000, 01, 01), UserIdentifier = "AAA", EntryKey = "ZZZ"
                , AssemblyVersion = "2.0"
                ,FileVersion = null
            });
            data.Add(new ViewTelemetryDetail(this.id_2)
            {
                Timestamp = new DateTime(2000, 01, 02),
                UserIdentifier = "BBB",
                EntryKey = "ZZZ"
                ,AssemblyVersion = "6.0"
                ,FileVersion = null
            });
            data.Add(new ViewTelemetryDetail(this.id_3)
            {
                Timestamp = new DateTime(2000, 01, 03)
                ,
                UserIdentifier = "CCC", EntryKey = "ZZZ"
                ,AssemblyVersion = "4.0"
                ,FileVersion = null
            });

            List<Tuple<string, bool>> sorts = new List<Tuple<string, bool>>();
            sorts.Add(new Tuple<string, bool>("WrongKey", false));

            List<ViewTelemetryDetail> ordered = ProgramsDashboardUnitOfWork.ApplyOrderingQuery(sorts, data.AsAsyncQueryable(), 0, 10).GetAwaiter().GetResult()
                .ToList();

            Assert.AreEqual(this.id_3, ordered[0].Id);
            Assert.AreEqual(this.id_2, ordered[1].Id);
            Assert.AreEqual(this.id_1, ordered[2].Id);
        }

        [Test]
        public void TestOrderingQuery_NoSorts()
        {
            List<ViewTelemetryDetail> data = new List<ViewTelemetryDetail>();

            data.Add(new ViewTelemetryDetail(this.id_3) {Timestamp = new DateTime(2000, 01, 03), UserIdentifier = "CCC", EntryKey = "ZZZ"});
            data.Add(new ViewTelemetryDetail(this.id_4) {Timestamp = new DateTime(2000, 01, 04), UserIdentifier = "CCC", EntryKey = "ZZZ"});
            data.Add(new ViewTelemetryDetail(this.id_1) {Timestamp = new DateTime(2000, 01, 01), UserIdentifier = "AAA", EntryKey = "ZZZ"});
            data.Add(new ViewTelemetryDetail(this.id_2) {Timestamp = new DateTime(2000, 01, 02), UserIdentifier = "BBB", EntryKey = "ZZZ"});


            List<Tuple<string, bool>> sorts = new List<Tuple<string, bool>>();

            List<ViewTelemetryDetail> ordered = ProgramsDashboardUnitOfWork.ApplyOrderingQuery(sorts, data.AsAsyncQueryable(), 0, 10).GetAwaiter().GetResult()
                .ToList();

            Assert.AreEqual(this.id_4, ordered[0].Id);
            Assert.AreEqual(this.id_3, ordered[1].Id);
            Assert.AreEqual(this.id_2, ordered[2].Id);
            Assert.AreEqual(this.id_1, ordered[3].Id);
        }

        [Test]
        public void TestOrderingQuery_OneProp()
        {
            List<ViewTelemetryDetail> data = new List<ViewTelemetryDetail>();

            data.Add(new ViewTelemetryDetail(this.id_1) {Timestamp = new DateTime(2000, 01, 01), UserIdentifier = "AAA", EntryKey = "ZZZ" });
            data.Add(new ViewTelemetryDetail(this.id_2) {Timestamp = new DateTime(2000, 01, 02), UserIdentifier = "BBB", EntryKey = "ZZZ"});
            data.Add(new ViewTelemetryDetail(this.id_3) {Timestamp = new DateTime(2000, 01, 03), UserIdentifier = "CCC", EntryKey = "ZZZ"});
            data.Add(new ViewTelemetryDetail(this.id_4) {Timestamp = new DateTime(2000, 01, 04), UserIdentifier = "CCC", EntryKey = "ZZZ"});
            data.Add(new ViewTelemetryDetail(this.id_5) {Timestamp = new DateTime(2000, 01, 05), UserIdentifier = "AAA", EntryKey = "XXX"});
            data.Add(new ViewTelemetryDetail(this.id_6) {Timestamp = new DateTime(2000, 01, 06), UserIdentifier = "AAA", EntryKey = "YYY"});
            data.Add(new ViewTelemetryDetail(this.id_7) {Timestamp = new DateTime(2000, 01, 07), UserIdentifier = "BBB", EntryKey = "YYY"});
            data.Add(new ViewTelemetryDetail(this.id_8) {Timestamp = new DateTime(2000, 01, 08), UserIdentifier = "BBB", EntryKey = "XXX"});


            List<Tuple<string, bool>> sorts = new List<Tuple<string, bool>>();
            sorts.Add(new Tuple<string, bool>(nameof(DataTableTelemetryData.EntryKey), false));

            List<ViewTelemetryDetail> ordered = ProgramsDashboardUnitOfWork.ApplyOrderingQuery(sorts, data.AsAsyncQueryable(), 0, 10).GetAwaiter().GetResult()
                .ToList();

            Assert.AreEqual(this.id_5, ordered[0].Id);
            Assert.AreEqual(this.id_8, ordered[1].Id);
            Assert.AreEqual(this.id_6, ordered[2].Id);
            Assert.AreEqual(this.id_7, ordered[3].Id);
            Assert.AreEqual(this.id_1, ordered[4].Id);
            Assert.AreEqual(this.id_2, ordered[5].Id);
            Assert.AreEqual(this.id_3, ordered[6].Id);
            Assert.AreEqual(this.id_4, ordered[7].Id);
        }

        [Test]
        public void TestOrderingQuery_TwoProps()
        {
            List<ViewTelemetryDetail> data = new List<ViewTelemetryDetail>();

            data.Add(new ViewTelemetryDetail(this.id_1) {Timestamp = new DateTime(2000, 01, 01), UserIdentifier = "AAA", EntryKey= "ZZZ"});
            data.Add(new ViewTelemetryDetail(this.id_2) {Timestamp = new DateTime(2000, 01, 02), UserIdentifier = "BBB", EntryKey= "ZZZ"});
            data.Add(new ViewTelemetryDetail(this.id_3) {Timestamp = new DateTime(2000, 01, 03), UserIdentifier = "CCC", EntryKey= "ZZZ"});
            data.Add(new ViewTelemetryDetail(this.id_4) {Timestamp = new DateTime(2000, 01, 04), UserIdentifier = "CCC", EntryKey= "ZZZ"});
            data.Add(new ViewTelemetryDetail(this.id_5) {Timestamp = new DateTime(2000, 01, 05), UserIdentifier = "AAA", EntryKey= "XXX"});
            data.Add(new ViewTelemetryDetail(this.id_6) {Timestamp = new DateTime(2000, 01, 06), UserIdentifier = "AAA", EntryKey= "YYY"});
            data.Add(new ViewTelemetryDetail(this.id_7) {Timestamp = new DateTime(2000, 01, 07), UserIdentifier = "BBB", EntryKey= "YYY"});
            data.Add(new ViewTelemetryDetail(this.id_8) {Timestamp = new DateTime(2000, 01, 08), UserIdentifier = "BBB", EntryKey = "XXX"});


            List<Tuple<string, bool>> sorts = new List<Tuple<string, bool>>();
            sorts.Add(new Tuple<string, bool>(nameof(DataTableTelemetryData.UserName), false));
            sorts.Add(new Tuple<string, bool>(nameof(DataTableTelemetryData.EntryKey), false));

            List<ViewTelemetryDetail> ordered = ProgramsDashboardUnitOfWork.ApplyOrderingQuery(sorts, data.AsAsyncQueryable(), 0, 10).GetAwaiter().GetResult()
                .ToList();

            Assert.AreEqual(this.id_5, ordered[0].Id);
            Assert.AreEqual(this.id_6, ordered[1].Id);
            Assert.AreEqual(this.id_1, ordered[2].Id);
            Assert.AreEqual(this.id_8, ordered[3].Id);
            Assert.AreEqual(this.id_7, ordered[4].Id);
            Assert.AreEqual(this.id_2, ordered[5].Id);
            Assert.AreEqual(this.id_3, ordered[6].Id);
            Assert.AreEqual(this.id_4, ordered[7].Id);
        }

        [Test]
        public void TestOrderingQuery_Version()
        {
            List<ViewTelemetryDetail> data = new List<ViewTelemetryDetail>();

            data.Add(new ViewTelemetryDetail(this.id_1){Timestamp = new DateTime(2000, 01, 01), UserIdentifier = "AAA",EntryKey = "ZZZ", FileVersion = "2.0"});
            data.Add(new ViewTelemetryDetail(this.id_2){Timestamp = new DateTime(2000, 01, 02), UserIdentifier = "BBB",EntryKey = "ZZZ", FileVersion = "6.0"});
            data.Add(new ViewTelemetryDetail(this.id_3){Timestamp = new DateTime(2000, 01, 03), UserIdentifier = "CCC",EntryKey = "ZZZ", FileVersion = "4.0"});
            data.Add(new ViewTelemetryDetail(this.id_4){Timestamp = new DateTime(2000, 01, 04), UserIdentifier = "CCC",EntryKey = "ZZZ", FileVersion = "4.0"});
            data.Add(new ViewTelemetryDetail(this.id_5){Timestamp = new DateTime(2000, 01, 05), UserIdentifier = "AAA",EntryKey = "XXX", FileVersion = "2.0"});
            data.Add(new ViewTelemetryDetail(this.id_6){Timestamp = new DateTime(2000, 01, 06), UserIdentifier = "AAA",EntryKey = "YYY", FileVersion = "2.0"});
            data.Add(new ViewTelemetryDetail(this.id_7){Timestamp = new DateTime(2000, 01, 07), UserIdentifier = "BBB",EntryKey = "YYY", FileVersion = "9.0"});
            data.Add(new ViewTelemetryDetail(this.id_8){Timestamp = new DateTime(2000, 01, 08), UserIdentifier = "BBB",EntryKey = "XXX", FileVersion = "1.0"});


            List<Tuple<string, bool>> sorts = new List<Tuple<string, bool>>();
            sorts.Add(new Tuple<string, bool>(nameof(DataTableTelemetryData.ProgramVersion), false));

            List<ViewTelemetryDetail> ordered = ProgramsDashboardUnitOfWork.ApplyOrderingQuery(sorts, data.AsAsyncQueryable(), 0, 10).GetAwaiter().GetResult()
                .ToList();

            Assert.AreEqual(this.id_8, ordered[0].Id);
            Assert.AreEqual(this.id_1, ordered[1].Id);
            Assert.AreEqual(this.id_5, ordered[2].Id);
            Assert.AreEqual(this.id_6, ordered[3].Id);
            Assert.AreEqual(this.id_3, ordered[4].Id);
            Assert.AreEqual(this.id_4, ordered[5].Id);
            Assert.AreEqual(this.id_2, ordered[6].Id);
            Assert.AreEqual(this.id_7, ordered[7].Id);
        }
    }
}