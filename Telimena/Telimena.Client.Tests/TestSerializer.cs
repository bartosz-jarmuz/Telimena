// -----------------------------------------------------------------------
//  <copyright file="TestRegistrationRequests.cs" company="SDL plc">
//   Copyright (c) SDL plc. All rights reserved.
//  </copyright>
// -----------------------------------------------------------------------

using System;
using DotNetLittleHelpers;
using NUnit.Framework;
using TelimenaClient.Serializer;

namespace TelimenaClient.Tests
{
    #region Using

    #endregion

    [TestFixture]
    public class TestSerializer
    {
        [Test]
        public void Test_Serializer()
        {
            UpdateRequest model = new UpdateRequest(Guid.Parse("dc13cced-30ea-4628-a81d-21d86f37df95"), new VersionData("1.2.0", "2.0.0"), Guid.Parse("4e80652e-d0ba-4742-a78c-3a63de4a63f0"), true, "3.2.1.3", "1.0.0.0");

            ITelimenaSerializer sut = new TelimenaSerializer();
            string stringified = sut.Serialize(model);
            string escaped = sut.UrlEncodeJson(stringified);

            Assert.AreEqual("{\"TelemetryKey\":\"dc13cced-30ea-4628-a81d-21d86f37df95\",\"UserId\":\"4e80652e-d0ba-4742-a78c-3a63de4a63f0\",\"VersionData\":{\"AssemblyVersion\":\"1.2.0\",\"FileVersion\":\"2.0.0\"},\"ToolkitVersion\":\"3.2.1.3\",\"AcceptBeta\":true,\"UpdaterVersion\":\"1.0.0.0\"}", stringified);
            Assert.AreEqual(
                "%7B%22TelemetryKey%22%3A%22dc13cced-30ea-4628-a81d-21d86f37df95%22%2C%22UserId%22%3A%224e80652e-d0ba-4742-a78c-3a63de4a63f0%22%2C%22VersionData%22%3A%7B%22AssemblyVersion%22%3A%221.2.0%22%2C%22FileVersion%22%3A%222.0.0%22%7D%2C%22ToolkitVersion%22%3A%223.2.1.3%22%2C%22AcceptBeta%22%3Atrue%2C%22UpdaterVersion%22%3A%221.0.0.0%22%7D", escaped);

            string unescaped = sut.UrlDecodeJson(escaped);
            Assert.AreEqual(stringified, unescaped);

            UpdateRequest objectified = sut.Deserialize<UpdateRequest>(unescaped);

            Assert.IsTrue(model.PublicInstancePropertiesAreEqual(objectified, true));
        }
    }
}