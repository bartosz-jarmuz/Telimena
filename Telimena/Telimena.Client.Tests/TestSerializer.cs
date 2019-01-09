using System;
using FluentAssertions;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using DotNetLittleHelpers;
using Newtonsoft.Json;
using NUnit.Framework;
using TelimenaClient.Serializer;

namespace TelimenaClient.Tests
{
    #region Using

    #endregion

    public class TestObject
    {
        public Dictionary<string, object> Dict { get; set; }
    }


    [TestFixture]
    public class TestSerializer
    {

        [Test]
        public void Test_TelemetryItems()
        {
            ITelimenaSerializer sut = new TelimenaSerializer();

            TelemetryItem originalItem = new TelemetryItem("Func1", TelemetryItemTypes.View, new VersionData("1.2.3.4", null), new Dictionary<string, object>()
            {
                { "AKey", "AValue"},
                { "AKeyDecimal", 23.2},
                { "AKeyInt", 23},
                { "AKeyDecimal2", 23M},
                { "AKeyBool", true},
                { "AKeyNull", null},
                { "DateUtc", DateTime.UtcNow},
                { "DateNowLocal", DateTime.Now},
                { "DateOffsetUtc", DateTimeOffset.UtcNow},
                { "DateLocal", new DateTime(2010,02,28,18,20,00,DateTimeKind.Local)},
            });

            string serialized = sut.Serialize(originalItem);
         
            TelemetryItem deserialized = sut.Deserialize<TelemetryItem>(serialized);
            originalItem.ShouldBeEquivalentTo(deserialized);


        }

        [Test]
        public void Test_Serializer()
        {
            UpdateRequest model = new UpdateRequest(Guid.Parse("dc13cced-30ea-4628-a81d-21d86f37df95"), new VersionData("1.2.0", "2.0.0"), Guid.Parse("4e80652e-d0ba-4742-a78c-3a63de4a63f0"), true, "3.2.1.3", "1.0.0.0");

            ITelimenaSerializer sut = new TelimenaSerializer();
            string stringified = sut.Serialize(model);

            UpdateRequest objectified = sut.Deserialize<UpdateRequest>(stringified);
            model.ShouldBeEquivalentTo(objectified);
        }
    }
}